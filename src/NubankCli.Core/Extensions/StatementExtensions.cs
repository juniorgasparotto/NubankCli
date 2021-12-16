﻿using NubankSharp.Entities;
using NubankSharp.Extensions;
using NubankSharp.Repositories.Api;
using System.Collections.Generic;
using System.Linq;

namespace NubankSharp.Extensions
{
    public static class StatementExtensions
    {
        public static List<Statement> ToStatementByBill(this IEnumerable<Bill> bills, Card card)
        {
            var list = new List<Statement>();
            foreach (var b in bills)
            {
                var statement = new Statement
                {
                    Version = Statement.Version1_0,
                    StatementType = StatementType.ByBill,
                    Start = b.Summary.OpenDate,
                    End = b.Summary.CloseDate,
                    Card = card,
                    Transactions = new List<Transaction>()
                };

                foreach (var l in b.LineItems)
                    statement.Transactions.Add(new Transaction(l));

                list.Add(statement);
            }

            return list;
        }

        // VALIDAR
        //public static List<Statement> ToStatementByMonth(this IEnumerable<BillTransaction> bills, Card card)
        //{
        //    var months = bills.GroupBy(f => f.EventDate.GetDateBeginningOfMonth()).Select(f =>
        //    {
        //        return new
        //        {
        //            DateBegginingOfMonth = f.Key,
        //            Transactions = f.ToList()
        //        };
        //    }).ToList();

        //    var list = new List<Statement>();
        //    foreach (var m in months)
        //    {
        //        var statement = new Statement
        //        {
        //            Version = Statement.Version1_0,
        //            StatementType = StatementType.ByMonth,
        //            Start = m.DateBegginingOfMonth,
        //            End = m.DateBegginingOfMonth.GetDateEndOfMonth(),
        //            Card = card,
        //            Transactions = new List<Transaction>()
        //        };

        //        foreach (var l in m.Transactions)
        //            statement.Transactions.Add(new Transaction(l));

        //        statement.Transactions = statement.Transactions.OrderBy(f => f.EventDate).ToList();
        //        list.Add(statement);
        //    }

        //    return list;
        //}

        public static List<Statement> ToStatementByMonth(this IEnumerable<Transaction> transactions, bool mergeCards = false)
        {
            var months = transactions.GroupBy(f => new { CardName = mergeCards ? "" : f.Statement.Card.Name, Date = f.EventDate.GetDateBeginningOfMonth() }).Select(f =>
            {
                return new
                {
                    Card = mergeCards ? null : f.First().Statement.Card,
                    DateBegginingOfMonth = f.Key.Date,
                    Transactions = f.ToList()
                };
            }).ToList();

            var list = new List<Statement>();
            foreach (var m in months)
            {
                var statement = new Statement
                {
                    Version = Statement.Version1_0,
                    StatementType = StatementType.ByMonth,
                    Start = m.DateBegginingOfMonth,
                    End = m.DateBegginingOfMonth.GetDateEndOfMonth(),
                    Card = m.Card,
                    Transactions = new List<Transaction>()
                };

                foreach (var l in m.Transactions)
                    statement.Transactions.Add(new Transaction
                    {
                        Id = l.Id,
                        CardName = l.CardName,
                        Category = l.Category,
                        Count = l.Count,
                        EventDate = l.EventDate,
                        EventDateUtc = l.EventDateUtc,
                        Href = l.Href,
                        Latitude = l.Latitude,
                        Longitude = l.Longitude,
                        Name = l.Name,
                        Number = l.Number,
                        PostDate = l.PostDate,
                        Statement = statement,
                        Value = l.Value
                    });

                statement.Transactions = statement.Transactions.OrderBy(f => f.EventDate).ToList();
                list.Add(statement);
            }

            return list;
        }

        public static List<Statement> ToStatementByMonth(this IEnumerable<Transaction> transactions, Card card = null)
        {
            var months = transactions.GroupBy(f => f.PostDate.GetDateBeginningOfMonth()).Select(f =>
            {
                return new
                {
                    DateBegginingOfMonth = f.Key,
                    Transactions = f.ToList()
                };
            }).ToList();

            var list = new List<Statement>();
            foreach (var m in months)
            {
                var statement = new Statement
                {
                    Version = Statement.Version1_0,
                    StatementType = StatementType.ByMonth,
                    Start = m.DateBegginingOfMonth,
                    End = m.DateBegginingOfMonth.GetDateEndOfMonth(),
                    Card = card,
                    Transactions = m.Transactions
                };

                statement.Transactions = statement.Transactions.OrderBy(f => f.EventDate).ToList();
                list.Add(statement);
            }

            return list;
        }
    }
}
