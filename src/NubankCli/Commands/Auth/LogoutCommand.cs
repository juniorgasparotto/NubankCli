using SysCommand.ConsoleApp;
using System;
using NubankCli.Extensions;

namespace NubankCli.Cli
{
    public class LogoutCommand : Command
    {
        public void Logout()
        {
            try
            {
                var jsonFileManager = this.GetService<JsonFileManager>();

                var user = this.GetCurrentUser();
                var userInfo = user.GetUserInfo();
                userInfo.Token = null;
                userInfo.AutenticatedUrls = null;
                
                jsonFileManager.Save(userInfo, user.UserInfoPath);
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
