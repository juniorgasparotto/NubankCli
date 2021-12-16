using SysCommand.ConsoleApp;
using System;
using NubankSharp.Extensions;
using NubankSharp.Repositories.Files;
using NubankSharp.Entities;

namespace NubankSharp.Cli
{
    public class LogoutCommand : Command
    {
        public void Logout()
        {
            try
            {
                var user = this.GetCurrentUser();
                user.Token = null;
                user.AutenticatedUrls = null;

                this.SaveUser(user);
                this.SetCurrentUser(null);

                App.Console.Success("Logout efetuado com sucesso!");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }
    }
}
