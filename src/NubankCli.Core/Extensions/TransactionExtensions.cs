using NubankCli.Core.DTOs;
using NubankCli.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NubankCli.Core.Extensions
{
    public static class TransactionExtensions
    {
        #region Statement

        public static IEnumerable<Transaction> GetTransactions(this IEnumerable<Statement> statements)
        {
            return statements.SelectMany(f => f.Transactions);
        }

        public static List<Statement> ExcludeBillPaymentLastBill(this List<Statement> statements)
        {
            foreach (var s in statements)
                s.Transactions = s.Transactions.ExcludeBillPaymentLastBill().ToList();

            return statements;
        }


        public static List<Statement> ExcludeCorrelations(this List<Statement> statements)
        {
            foreach (var s in statements)
                s.Transactions = s.Transactions.Where(f => !f.IsCorrelated).ToList();

            return statements;
        }

        #endregion

        #region Summary

        public static SummaryDTO Summary(this IEnumerable<Transaction> transactions)
        {
            var tTn = transactions.GetIn();
            var tOut = transactions.GetOut();

            return new SummaryDTO
            {
                CountIn = tTn.Count(),
                ValueIn = tTn.Total(),

                CountOut = tOut.Count(),
                ValueOut = tOut.Total(),

                CountTotal = transactions.Count(),
                ValueTotal = transactions.Total(),
            };
        }

        #endregion

        public static IEnumerable<Transaction> GetIn(this IEnumerable<Transaction> transactions)
        {
            return transactions.Where(f => f.Value > 0);
        }

        public static IEnumerable<Transaction> GetOut(this IEnumerable<Transaction> transactions)
        {
            return transactions.Where(f => f.Value < 0);
        }

        public static IEnumerable<Transaction> ExcludeBillPaymentLastBill(this IEnumerable<Transaction> transactions)
        {
            return transactions.Where(f => !f.IsBillPaymentLastBill);
        }

        public static void CorrelateTransactions(this IEnumerable<Transaction> transactions)
        {
            // Esse código precisa ser melhorado
            // 1) Primeiro, considere os débitos como sendo sempre a origin da relação (conta de crédit0)
            // 2) Considere os créditos como sendo sempre o destino origin da relação (conta do nuconta)
            // 3) Origin e destino devem ter o mesmo valor, mas com o sinal invertido
            // 4) Origin e destino não podem ser do mesmo cartão (um nuconta e outro crédito)
            // 5) As origins precisam ter sempre a data menor ou igual ao destino
            // 6) A diferença de data deve ser de no máximo 5 dias. Considere que o pagamento seja feita numa sexta e existe feriado prolongado
            //    no qual a efetivação do pagamento da fatura seja só na terça feira (talvez não exista esse cenário no nubank)
            var groupCorrelation = transactions.GroupBy(f => new { f.PostDate, ValueAbs = Math.Abs(f.Value) }).Where(f => f.Count() == 2);
            foreach (var g in groupCorrelation)
            {
                var nuconta = g.FirstOrDefault(f => f.CardName == Card.NUCONTA_NAME);
                var creditCard = g.FirstOrDefault(f => f.CardName.Contains(Card.CREDIT_CARD_NAME));

                if (nuconta != null && creditCard != null)
                {
                    nuconta.Target = creditCard.Id;
                    creditCard.Origin = nuconta.Id;
                }
            }
        }

        public static decimal Total(this IEnumerable<Transaction> transactions)
        {
            return transactions.Sum(f => f.Value);
        }

    }
}
