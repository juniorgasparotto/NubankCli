using SysCommand.ConsoleApp;
using System;
using NubankCli.Core.Entities;
using NubankCli.Core.Repositories.Api;
using NubankCli.Extensions;

namespace NubankCli.Cli
{
    public class AuthCommand : Command
    {
        public void Login(string userName, string password = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    userName = App.Console.Read("UserName: ");

                ValidateInfo(userName, nameof(userName));

                var user = new User(userName);
                var userInfo = user.GetUserInfo();

                var repository = this.GetService<NubankRepository>();
                var jsonFileManager = this.GetService<JsonFileManager>();

                if (userInfo == null || !userInfo.IsValid())
                {
                    // Inicia o login
                    StartLogin(repository, userName, password);

                    if (string.IsNullOrWhiteSpace(password))
                        password = App.Console.Read("Password: ");

                    ValidateInfo(password, nameof(password));

                    userInfo = new User.UserInfo
                    {
                        AutenticatedUrls = repository.Endpoints.AutenticatedUrls,
                        Token = repository.AuthToken
                    };

                    jsonFileManager.Save(userInfo, user.UserInfoPath);
                }

                this.SetCurrentUser(userName);

                App.Console.Success("Login efetuado com sucesso!");
                App.Console.Write($" ");
                App.Console.Write($"LOCALIZAÇÃO USUÁRIO:");
                App.Console.Write($"  {user.GetPath()}");
                App.Console.Write($" ");
                App.Console.Warning($"Seu token expira em: {user.GetUserInfo().GetExpiredDate()}");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }

        private void StartLogin(NubankRepository repository, string login, string password)
        {
            var result = repository.Login(login, password);

            if (result.NeedsDeviceAuthorization)
            {
                if (!repository.RestClient.EnableMockServer)
                {
                    App.Console.Warning("You must authenticate with your phone to be able to access your data.");
                    App.Console.Warning("Scan the QRCode below with you Nubank application on the following menu:");
                    App.Console.Warning("Nu(Seu Nome) > Perfil > Acesso pelo site");
                    
                    App.Console.Write(" ");
                    App.Console.Write(result.GetQrCodeAsAscii());
                    
                    App.Console.Write(" ");
                    App.Console.Warning($"Use your phone to scan and after this press any key to continue...");

                    App.Console.Read();
                }

                repository.AutenticateWithQrCode(result.Code);
            }
        }

        private void ValidateInfo(string info, string infoName)
        {
            if (string.IsNullOrWhiteSpace(info))
                throw new Exception($"Nenhum '{infoName}' informado");
        }
    }
}
