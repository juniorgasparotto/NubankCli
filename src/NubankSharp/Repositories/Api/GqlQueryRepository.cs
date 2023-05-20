using Newtonsoft.Json;
using NubankSharp.Extensions;
using System.Collections.Generic;
using System.IO;

namespace NubankSharp.Repositories.Api
{
    /// <summary>
    /// Essa classe pode trabalhar de 2 formas,
    /// 1) Pode ser uma representação 1x1 para um arquivo quando passado o path no construtor e não passado como args nos métodos
    /// 2) Pode ser uma representação 1xN, ou seja, como se fosse um repositório para um determinado tipo quando não passado o path no construtor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GqlQueryRepository : IGqlQueryRepository
    {
        private readonly string _folderPath;
        private Dictionary<string, string> _queries = new();

        public GqlQueryRepository(string folderPath = null)
        {
            this._folderPath = folderPath;
            var assembly = typeof(GqlQueryRepository).Assembly;
            var resourceFolder = "NubankSharp.Repositories.Api.Queries.";

            foreach (var name in assembly.GetManifestResourceNames())
            {
                if (name.StartsWith(resourceFolder))
                {
                    var nameWithouFolder = Path.GetFileNameWithoutExtension(name.Replace(resourceFolder, ""));
                    using (var reader = new StreamReader(assembly.GetManifestResourceStream(name)))
                    {
                        _queries.Add(nameWithouFolder, reader.ReadToEnd());
                    }
                }
            }
        }

        public string GetGql(string queryName)
        {
            // Da prioridade para o arquivo caso exista, do contrário, tenta encontrar nos resources
            if (!string.IsNullOrWhiteSpace(this._folderPath))
            {
                var filePath = Path.Combine(this._folderPath, queryName + ".gql");
                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);
            }

            return _queries[queryName];
        }
    }
}
