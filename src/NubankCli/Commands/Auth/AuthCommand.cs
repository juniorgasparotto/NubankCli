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
                //if (!repository.RestClient.EnableMockServer)
                {
                    App.Console.Warning("Você deve se autenticar com seu telefone para poder acessar seus dados.");
                    App.Console.Warning("Digitalize o QRCode abaixo com seu aplicativo Nubank no seguinte menu:");
                    App.Console.Warning("Ícone de configurações > Perfil > Acesso pelo site");
                    
                    App.Console.Write(" ");
                    App.Console.Write(result.GetQrCodeAsAscii());
                    
                    App.Console.Write(" ");
                    App.Console.Warning($"Use seu telefone para escanear e depois disso pressione a tecla 'enter' para continuar ...");

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
