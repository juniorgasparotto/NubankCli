using NubankCli.Core.Configuration;
using NubankCli.Extensions;

namespace NubankCli.Extensions.Configurations
{
    public class ConfigManager
    {
        public const string FILE_NAME = "settings.json";

        private readonly JsonFileManager jsonFileManager;

        public ConfigManager(JsonFileManager jsonFileManager)
        {
            this.jsonFileManager = jsonFileManager;
        }

        public void Save(AppSettings appSettings)
        {
            jsonFileManager.Save(appSettings, FILE_NAME);
        }
    }
}
