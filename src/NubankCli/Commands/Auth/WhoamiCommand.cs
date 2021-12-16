using NubankSharp.Entities;
using NubankSharp.Extensions;
using NubankSharp.Repositories.Files;
using SysCommand.ConsoleApp;
using System;

namespace NubankSharp.Cli
{
    public class WhoamiCommand : Command
    {
        public void Whoami()
        {
            try
            {
                var user = this.GetCurrentUser();
                App.Console.Write($"USERNAME           : {user.UserName}");
                App.Console.Write($"LOCALIZAÇÃO USUÁRIO: {this.GetUserFileName(user)}");
                App.Console.Write($" ");
                App.Console.Warning($"Seu token gerado por '{user.GetLoginType()}' expira em: {user.GetExpiredDate()}");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
