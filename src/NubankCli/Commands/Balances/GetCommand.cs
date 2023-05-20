namespace NubankSharp.Commands.Balances
{
    using SysCommand.ConsoleApp;
    using SysCommand.Mapping;
    using System;
    using NubankSharp.Extensions;

    public partial class BalanceCommand : Command
    {

        [Action(Name = "get")]
        public void GetBalance(
           EntityNames type,
           [Argument(ShortName = 'o', LongName = "output")] string outputFormat = null
        )
        {
            try
            {
                var user = this.GetCurrentUser();
                var nuApi = this.CreateNuApiByUser(user, nameof(GetBalance));
                var total = nuApi.GetBalance();
                this.ViewSingleFormatted(new { Total = total }, outputFormat);
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
