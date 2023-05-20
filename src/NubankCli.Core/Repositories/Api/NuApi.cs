using NubankSharp.Entities;
using NubankSharp.Extensions;
using NubankSharp.Repositories.Api.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NubankSharp.Repositories.Api
{
    public class NuApi
    {
        private readonly EndPointApi _endPointRepository;
        private readonly IGqlQueryRepository _queryRepository;

        private NuHttpClient RestClient { get; }

        public NuApi(NuHttpClient restClient, EndPointApi endPointRepository, IGqlQueryRepository queryRepository = null)
        {
            RestClient = restClient;
            this._endPointRepository = endPointRepository;
            this._queryRepository = queryRepository ?? new GqlQueryRepository();
        }

        public IEnumerable<SavingFeed> GetSavingsAccountFeed()
        {
            var response = RestClient.Post<GetSavingsAccountFeedResponse>(nameof(_endPointRepository.GraphQl), _endPointRepository.GraphQl, new { query = _queryRepository.GetGql("savings") }, null, out _);
            return response.Data.Viewer.SavingsAccount.Feed;
        }

        public decimal GetBalance()
        {
            var response = RestClient.Post<GetBalanceResponse>($"{nameof(_endPointRepository.GraphQl)}-{nameof(GetBalance)}", _endPointRepository.GraphQl, new { query = _queryRepository.GetGql("balance") }, null, out _);
            return response.Data.Viewer.SavingsAccount.NetAmountBalance.NetAmount;
        }

        public string ExecuteQuery(string query)
        {
            var response = RestClient.Post($"{nameof(_endPointRepository.GraphQl)}-{nameof(ExecuteQuery)}", _endPointRepository.GraphQl, new { query }, null, out _);
            return response;
        }

        public IEnumerable<Event> GetEvents()
        {
            var response = RestClient.Get<GetEventsResponse>(nameof(_endPointRepository.Events), _endPointRepository.Events, null, out _);
            return response.Events;
        }

        public IEnumerable<Bill> GetBills()
        {
            var response = RestClient.Get<GetBillsResponse>(nameof(_endPointRepository.BillsSummary), _endPointRepository.BillsSummary, null, out _);
            return response.Bills;
        }

        public IEnumerable<Bill> GetBills(DateTime? start = null, DateTime? end = null, bool ignoreFuture = true, bool includeItems = true)
        {
            var billService = new BillService(this);
            var bills = billService.GetBills(start, end, ignoreFuture, includeItems);
            var events = GetEvents().ToList();
            billService.PopulateEvents(bills, events);

            return bills;
        }

        public Bill GetBill(string name, string link)
        {
            var response = RestClient.Get<GetBillResponse>(name, link, null, out _);
            return response.Bill;
        }

        public IEnumerable<Transaction> GetDebitTransactions(DateTime? start = null, DateTime? end = null)
        {
            var feeds = GetSavingsAccountFeed();
            start = (start ?? feeds.Min(f => f.PostDate)).Date.GetDateBeginningOfMonth();
            end = (end ?? feeds.Max(f => f.PostDate)).Date.GetDateBeginningOfMonth();

            var deniedTypes = new TransactionType[]
{
                TransactionType.Unknown,
                TransactionType.WelcomeEvent
            };

            var selecteds = feeds
                .Where(f => !deniedTypes.Contains(f.TypeName) && f.PostDate.GetDateBeginningOfMonth() >= start && f.PostDate.GetDateBeginningOfMonth() <= end)
                .OrderBy(f => f.PostDate)
                .ToList();

            var savings = new List<Transaction>();
            foreach (var s in selecteds)
            {
                var valueInDetails = new List<TransactionType>()
                {
                    TransactionType.BillPaymentEvent,
                    TransactionType.TransferOutReversalEvent,
                    TransactionType.GenericFeedEvent
                };

                if (valueInDetails.Contains(s.TypeName))
                {
                    // Modelo: "Cartão Nubank - R$ 1.987,06",
                    // 1) Divide na primeira ocorrencia de "-"
                    // 2) Remove os espaços; " R$ 1.987,06"
                    s.Amount ??= s.GetValueFromDetails();
                }

                if (s.Amount != null)
                    savings.Add(new Transaction(s));
                else
                    Console.WriteLine($"Registro não contém valores monetários {s.PostDate} - {s.Title} - {s.Detail}");
            }

            savings = savings.OrderBy(f => f.EventDate).ToList();
            return savings;
        }

        public IEnumerable<Transaction> GetCreditTransactions(DateTime? start, DateTime? end)
        {
            var billService = new BillService(this);
            var billTrans = billService.GetBillTransactions(start, end);

            var events = GetEvents().ToList();
            billService.PopulateEvents(billTrans, events);

            var transactions = billTrans
                .Select(f => new Transaction(f))
                .OrderBy(f => f.EventDate)
                .ToList();

            return transactions;
        }
    }
}