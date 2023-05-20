using Newtonsoft.Json;
using NubankSharp.Extensions;
using System.IO;

namespace NubankSharp.Repositories.Files
{
    /// <summary>
    /// Essa classe pode trabalhar de 2 formas,
    /// 1) Pode ser uma representação 1x1 para um arquivo quando passado o path no construtor e não passado como args nos métodos
    /// 2) Pode ser uma representação 1xN, ou seja, como se fosse um repositório para um determinado tipo quando não passado o path no construtor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonFileRepository<T>
    {
        private readonly string _filePath;

        public JsonFileRepository(string filePath = null)
        {
            this._filePath = filePath;
        }

        public T GetFile(string filePath = null)
        {
            filePath ??= _filePath;
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(json);
            }

            return default;
        }

        public void Save(T entity, string filePath = null)
        {
            filePath ??= _filePath;
            FileExtensions.CreateFolderIfNeeded(filePath);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(entity, Formatting.Indented));
        }
    }
}
