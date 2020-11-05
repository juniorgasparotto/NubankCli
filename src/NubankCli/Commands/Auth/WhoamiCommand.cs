using NubankCli.Extensions;
using SysCommand.ConsoleApp;
using System;

namespace NubankCli.Cli
{
    public class WhoamiCommand : Command
    {
        public void Whoami()
        {

            try
            {
                var user = this.GetCurrentUser();

                App.Console.Write($"USERNAME           : {user.UserName}");
                App.Console.Write($"LOCALIZAÇÃO USUÁRIO: {user.GetPath()}");
                App.Console.Write($" ");
                App.Console.Warning($"Seu token expira em: {user.GetUserInfo().GetExpiredDate()}");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
