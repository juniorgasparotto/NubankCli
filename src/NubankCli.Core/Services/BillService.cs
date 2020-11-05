using NubankCli.Core.Extensions;
using NubankCli.Core.Repositories.Api;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NubankCli.Core.Services
{
    public class BillService
    {
        private readonly NubankRepository client;
        private IEnumerable<Bill> _bills;

        private IEnumerable<Bill> Bills
        {
            get
            {
                if (_bills == null)
                    _bills = client.GetBills();

                return _bills;
            }
        }

        public BillService(NubankRepository nubankClient)
        {
            this.client = nubankClient;
        }

        public IEnumerable<BillTransaction> GetBillItemsByMonth(DateTime? startFilter, DateTime? endFilter)
        {
            var start = (startFilter ?? Bills.Min(f => f.Summary.OpenDate)).Date.GetDateBeginningOfMonth();
            var end = (endFilter ?? Bills.Max(f => f.Summary.CloseDate)).Date.GetDateBeginningOfMonth();

            /*
                Eu quero todas as transações do mês 2020-07 até 2020-09 considerando que o dia de corte é 17
                
                Boleto 2020-06 (Open=2020-06-17, Close=2020-07-17)
                  Aqui você encontra transações do mês 6 e 7
                  O mês 6 vai de 2020-06-17 até 2020-06-30
                  O mês 7 vai de 2020-07-01 até 2020-07-17
                Boleto 2020-07 (Open=2020-07-17, Close=2020-08-17)
                  Aqui você encontra transações do mês 7 e 8
                  O mês 7 vai de 2020-07-17 até 2020-07-31
                  O mês 8 vai de 2020-08-01 até 2020-08-17
                Boleto 2020-08 (Open=2020-08-17, Close=2020-09-17)
                  Aqui você encontra transações do mês 8 e 9
                  O mês 8 vai de 2020-08-17 até 2020-08-31
                  O mês 9 vai de 2020-09-01 até 2020-09-17
                Boleto 2020-09 (Open=2020-09-17, Close=2020-10-17)
                  Aqui você encontra transações do mês 9 e 10
                  O mês 9  vai de 2020-09-17 até 2020-09-31
                  O mês 10 vai de 2020-10-01 até 2020-10-17
               
            -> Regra de seleção dos boletos:

                Boletos	     2020-06	               2020-09		
                Close_Date = 2020-07-17 && Open_Date = 2020-09-17
                         01..17                    17..31
            */

            var transactions = new List<BillTransaction>();
            var selecteds = Bills
                .Where(f => f.Summary.CloseDate.GetDateBeginningOfMonth() >= start && f.Summary.OpenDate.GetDateBeginningOfMonth() <= end)
                .OrderBy(f => f.Summary.OpenDate)
                .ToList();

            foreach (var b in selecteds)
            {
                var response = client.GetBill(b.Links.Self.Href);
                if (response != null)
                {
                    b.LineItems = response.LineItems;
                    transactions.AddRange(response.LineItems);
                }
            }

            var selectedsTrans = transactions.Where(f => f.PostDate.GetDateBeginningOfMonth() >= start && f.PostDate.GetDateBeginningOfMonth() <= end).ToList();
            return selectedsTrans;
        }
        
        public IEnumerable<Bill> GetBills(DateTime? startFilter, DateTime? endFilter)
        {
            var start = (startFilter ?? Bills.Min(f => f.Summary.OpenDate)).Date.GetDateBeginningOfMonth();
            var end = (endFilter ?? Bills.Max(f => f.Summary.CloseDate)).Date.GetDateBeginningOfMonth();

            var selecteds = Bills
                .Where(f => f.Summary.OpenDate.GetDateBeginningOfMonth() >= start && f.Summary.OpenDate.GetDateBeginningOfMonth() <= end)
                .OrderBy(f => f.Summary.OpenDate)
                .ToList();

            foreach (var b in selecteds)
            {
                var response = client.GetBill(b.Links.Self.Href);
                if (response != null)
                    b.LineItems = response.LineItems;
            }

            return selecteds;
        }

        public void PopulateEvents(IEnumerable<Bill> bills, IEnumerable<Event> events)
        {
            foreach (var b in bills)
                PopulateEvents(b.LineItems, events);
        }

        public void PopulateEvents(IEnumerable<BillTransaction> billTransactions, IEnumerable<Event> events)
        {
            // Obtem as informações do evento de cada compra
            // Pagamentos recebidos e Compras do Rewards não tem links, então não existe referencia dentro do eventos
            // Compras com mais de uma parcela vão compartilhar o mesmo evento uma vez que o nubank só mantem o evento da primeira compra
            // mas é bom as outras parcelas terem a referencia do mesmo evento para ter acesso a latitude e longitude, mas devem tomar 
            // cuidado para não usar a mesma data, pois apenas a primeira compra reflete a data real.
            foreach (var t in billTransactions)
                if (t.Href != null)
                    t.Event = events.First(f => f.Href == t.Href);
        }
    }
}
