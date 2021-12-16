using SysCommand.ConsoleApp;
using System;
using NubankSharp.Repositories.Api;
using NubankSharp.Extensions;
using Microsoft.Extensions.Options;
using SysCommand.Mapping;
using QRCoder;
using NubankSharp.Entities;
using NubankSharp.Repositories.Files;
using NubankSharp.Models;

namespace NubankSharp.Cli
{
    public class AuthCommand : Command
    {
        private NuAppSettings _appSettings;
        private NuAppSettings AppSettings
        {
            get
            {
                if (_appSettings == null)
                    _appSettings = this.GetService<IOptions<NuAppSettings>>().Value;

                return _appSettings;
            }
        }

        public void Login(string userName, string password = null)
        {
            try
            {
                //userName = "35412702848";
                //password = "Junior1986#$&";

                if (string.IsNullOrWhiteSpace(userName))
                    userName = App.Console.Read("UserName: ");

                ValidateInfo(userName, nameof(userName));

                var currentUser = this.GetUser(userName);
                if (currentUser == null || !currentUser.IsValid())
                {
                    currentUser = new NuUser(userName, password);

                    var httpClient = this.CreateNuHttpClient(currentUser, nameof(Login));
                    var endPointRepository = new EndPointApi(httpClient);
                    var authRepository = new NuAuthApi(httpClient, endPointRepository);

                    // PARTE_1: Cria a aplicação no nubank e requisita o código por e-mail
                    App.Console.Write($"Criando contexto de login...");
                    var connectAppRes = authRepository.ConnectApp(userName, password);
                    App.Console.Success($"Contexto criado com sucesso! Um e-mail será enviado para {connectAppRes.SentTo} contendo o código de acesso");

                    // PARTE_2: Valida o código e gera o certificado .p12
                    var code = App.Console.Read($"[>] Digite o código: ");
                    App.Console.Write($"Validando código...'");
                    authRepository.GenerateAppCertificate(connectAppRes, code);
                    App.Console.Success($"Código validado com sucesso!");

                    // PARTE_3: Faz o login e obtem as infos do usuário com o token e links
                    App.Console.Write($"Iniciando login do usuário...'");
                    authRepository.Login();

                    currentUser.CleanPassword();
                    this.SaveUser(currentUser);
                }

                this.SetCurrentUser(userName);

                App.Console.Success($"Sucesso! Login efetuado com '{currentUser.GetLoginType()}'");
                App.Console.Write($" ");
                App.Console.Write($"LOCALIZAÇÃO USUÁRIO:");
                App.Console.Write($"  {this.GetUserFileName(currentUser)}");
                App.Console.Write($" ");
                App.Console.Warning($"Seu token expira em: {currentUser.GetExpiredDate()}");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }

        public void Login(
            [Argument(ShortName = 'q', LongName = "qrcode")] bool qrCode,
            string userName,
            string password = null
        )
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                    userName = App.Console.Read("UserName: ");

                ValidateInfo(userName, nameof(userName));

                var currentUser = this.GetUser(userName);
                if (currentUser == null || !currentUser.IsValid())
                {
                    currentUser = new NuUser(userName, password);
                    var httpClient = this.CreateNuHttpClient(currentUser, nameof(Login));
                    var endPointRepository = new EndPointApi(httpClient);
                    var authRepository = new NuAuthApi(httpClient, endPointRepository);

                    if (string.IsNullOrWhiteSpace(password))
                        password = App.Console.Read("Password: ");
                    ValidateInfo(password, nameof(password));

                    // Inicia o login
                    StartLogin(authRepository);

                    currentUser.CleanPassword();
                    this.SaveUser(currentUser);
                }

                this.SetCurrentUser(userName);

                App.Console.Success($"Sucesso! Login efetuado com '{currentUser.GetLoginType()}'");
                App.Console.Write($" ");
                App.Console.Write($"LOCALIZAÇÃO USUÁRIO:");
                App.Console.Write($"  {this.GetUserFileName(currentUser)}");
                App.Console.Write($" ");
                App.Console.Warning($"Seu token expira em: {currentUser.GetExpiredDate()}");
            }
            catch (Exception ex)
            {
                this.ShowApiException(ex);
            }
        }

        private void StartLogin(NuAuthApi authRepository)
        {
            authRepository.Login(out var needLoginValidation);

            if (needLoginValidation)
            {
                var code = Guid.NewGuid().ToString();

                if (!this.AppSettings.EnableMockServer)
                {
                    var qrCodeData = new QRCodeGenerator().CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new AsciiQRCode(qrCodeData).GetGraphic(1);

                    App.Console.Warning("Você deve se autenticar com seu telefone para poder acessar seus dados.");
                    App.Console.Warning("Digitalize o QRCode abaixo com seu aplicativo Nubank no seguinte menu:");
                    App.Console.Warning("Ícone de configurações > Perfil > Acesso pelo site");

                    App.Console.Write(" ");
                    App.Console.Write(qrCode);

                    App.Console.Write(" ");
                    App.Console.Warning($"Use seu telefone para escanear e depois disso pressione a tecla 'enter' para continuar ...");

                    App.Console.Read();
                }

                authRepository.ValidateQRCode(code);
            }
        }

        private void ValidateInfo(string info, string infoName)
        {
            if (string.IsNullOrWhiteSpace(info))
                throw new Exception($"Nenhum '{infoName}' informado");
        }
    }
}
