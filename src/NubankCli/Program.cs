using SysCommand.ConsoleApp;
using System;
using Microsoft.Extensions.DependencyInjection;
using NubankCli.Core.DI;
using NubankCli.Extensions;
using System.IO;
using System.Reflection;
using NubankCli.Extensions.DI;
using NubankCli.Extensions.Configurations;

namespace NubankCli
{
    public partial class Program : Command
    {
        public static int Main()
        {
            //CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("nl-NL");
            var CONFIG_FOLDER = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;

            return App.RunApplication(() =>
            {
                var app = new App();
                app.Console.Verbose = Verbose.All;
                app.Console.ColorSuccess = ConsoleColor.DarkGray;

                var services = new ServiceCollection()
                    .AddConfiguration(CONFIG_FOLDER, ConfigManager.FILE_NAME)
                    .AddRepositories()
                    .ConfigureTables()
                    .AddServices();

                services.AddScoped(s =>
                {
                    return new JsonFileManager
                    {
                        SaveInRootFolderWhenIsDebug = false,
                        DefaultFolder = CONFIG_FOLDER,
                        DefaultFileExtension = ""
                    };
                });

                services.AddScoped<ConfigManager>();

                app.Items.SetServiceProvider(services.BuildServiceProvider());

                return app;
            });
        }
    }
}
