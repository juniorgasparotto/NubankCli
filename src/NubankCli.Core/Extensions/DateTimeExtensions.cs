using System;

namespace NubankCli.Core.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Retorna a data com o mês no primeiro dia
        /// In : 2020-01-20
        /// Out: 2020-01-01
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime GetDateBeginningOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetDateEndOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
        }
    }
}
