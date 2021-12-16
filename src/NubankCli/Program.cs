using SysCommand.ConsoleApp;
using System;
using Microsoft.Extensions.DependencyInjection;
using NubankSharp.Extensions;
using NubankSharp.Cli.Extensions;
using NubankCli.Extensions.Configurations;
using NubankSharp.Extensions.Tables;
using NubankSharp.Entities;
using NubankSharp.Repositories.Files;

namespace NubankSharp
{
    public partial class Program : Command
    {
        public static int Main()
        {
            //CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("nl-NL");

            // Para os arquivos de configuração e dados do usuário:
            // 1) Usa a pasta do código como pasta root da execução quando estiver EM MODO DE DEBUG ou usando dotnet run
            // 2) Usa a pasta atual do código quando estiver em modo RELEASE, ou seja, quando é gerado o EXE (não gere o EXE em modo DEBUG para evitar confusões)
            var CONFIG_FOLDER = EnvironmentExtensions.ProjectRootOrExecutionDirectory;

            return App.RunApplication(() =>
            {
                var app = new App();
                app.Console.Verbose = Verbose.All;
                app.Console.ColorSuccess = ConsoleColor.DarkGray;

                var services = new ServiceCollection()
                    .AddConfigurations(CONFIG_FOLDER, "settings.json")
                    .ConfigureTables();

                services.AddSingleton(typeof(JsonFileRepository<>), typeof(JsonFileRepository<>));

                app.Items.SetServiceProvider(services.BuildServiceProvider());

                return app;
            });
        }
    }
}
