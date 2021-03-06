using NubankCli.Core.DTOs;
using NubankCli.Core.Entities;
using NubankCli.Core.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace NubankCli.Core.Repositories
{
    public class StatementRepository
    {
        public IEnumerable<Statement> GetStatements(User user, CardType? cardType = null, bool excludeBillPayment = false, bool excludeCorrelations = false, string where = null, string[] args = null, string orderby = null)
        {
            var list = new List<Statement>();
            var directories = Directory.GetDirectories(user.GetPath());

            foreach (string d in directories)
            {
                var dirName = Path.GetFileName(d);

                // 1) Se for solicitado apenas cartão de credito e a pasta corrente NÃO for cartão de credito então continua
                // 2) Se for solicitado apenas NuConta e a pasta corrente NÃO for NuConta então continua
                if (cardType == CardType.CreditCard && !Card.CREDIT_CARD_NAME.Contains(dirName))
                    continue;
                else if (cardType == CardType.NuConta && !Card.NUCONTA_NAME.Contains(dirName))
                    continue;

                // Se a pasta atual for "credit-card-by-month" e existir a pasta "credit-card", então
                // dá prioridade para a pasta "credit-card" que é separada por boleto. Isso deixa a leitura do usuário
                // mais facil pois é igual ao que ele ver no APP
                if (dirName == Card.CREDIT_CARD_BY_MONTH_NAME && directories.Any(f => Path.GetFileName(f) == Card.CREDIT_CARD_NAME))
                    continue;

                var files = Directory.GetFiles(d);
                foreach (var f in files)
                {
                    var statement = Newtonsoft.Json.JsonConvert.DeserializeObject<Statement>(File.ReadAllText(f));

                    foreach (var t in statement.Transactions)
                    {
                        t.CardName = statement.Card.Name;
                        t.Statement = statement;
                    }

                    list.Add(statement);
                }
            }

            // Cria a correlação entre as transações
            // NuConta: manda 10 reais para o cartão de crédito
            // CreditCard: Recebe 10 reais
            // 1) A transação da NuConta terá na propriedade "Target" o Id da transação que foi para o cartão de crédito
            // 2) A transação do cartão de crédito terá na propriedade "Origin" o Id da transação que veio da NuConta
            list.GetTransactions().CorrelateTransactions();

            if (excludeBillPayment)
                list = list.ExcludeBillPaymentLastBill();

            if (excludeCorrelations)
                list = list.ExcludeCorrelations();

            var queryable = list.AsQueryable();

            if (!string.IsNullOrWhiteSpace(where))
                queryable = queryable.Where(where, args);

            if (!string.IsNullOrWhiteSpace(orderby))
                queryable = queryable.OrderBy(orderby);

            return queryable;
        }

        public IEnumerable<Transaction> GetTransactions(User user, string idOrName, string where = null, string[] args = null, string orderby = null)
        {
            var statements = GetStatements(user);
            var list = statements.GetTransactions();

            var queryable = list.AsQueryable();

            if (!string.IsNullOrWhiteSpace(idOrName))
                queryable = queryable.Where($"Id.ToString().StartsWith(@0) || Name.StartsWith(@0)", idOrName);

            if (!string.IsNullOrWhiteSpace(where))
                queryable = queryable.Where(where, args);

            if (!string.IsNullOrWhiteSpace(orderby))
                queryable = queryable.OrderBy(orderby);

            return queryable;
        }
    }
}
