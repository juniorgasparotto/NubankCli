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

        public static List<Statement> ExcludeBillPayment(this List<Statement> statements)
        {
            foreach (var s in statements)
                s.Transactions = s.Transactions.ExcludeBillPayment().ToList();

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

        public static IEnumerable<Transaction> ExcludeBillPayment(this IEnumerable<Transaction> transactions)
        {
            return transactions.Where(f => !f.IsBillPayment);
        }

        public static void CorrelateTransactions(this IEnumerable<Transaction> transactions)
        {
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
