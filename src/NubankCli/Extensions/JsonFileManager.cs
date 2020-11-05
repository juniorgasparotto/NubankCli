namespace NubankCli.Extensions
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using global::SysCommand.ConsoleApp.Files;
    using global::SysCommand.Helpers;
    using global::SysCommand.ConsoleApp.Helpers;
    using NubankCli.Core.Extensions;

    /// <summary>
    /// Is responsible for the management of standard control objects in the form of files. 
    /// It contains some features that will help you save time if you need to persist objects. 
    /// The format will always be Json .
    /// </summary>
    public class JsonFileManager
    {
        // private static TypeNameSerializationBinder binder = new TypeNameSerializationBinder();
        private Dictionary<string, object> objectsFiles = new Dictionary<string, object>();
        private string defaultFolder;

        /// <summary>
        /// Adds a prefix on all files. The default is null.
        /// </summary>
        public string DefaultFilePrefix { get; set; }

        /// <summary>
        /// Specifies the extension of the files. The default is .json
        /// </summary>
        public string DefaultFileExtension { get; set; }

        /// <summary>
        /// Determines if the formatting of the types T will include the full name of the type. The default is false
        /// </summary>
        public bool UseTypeFullName { get; set; }

        /// <summary>
        /// Determines if the default folder is created at the root of the project when this in debug mode inside Visual Studio.
        /// This helps to show the files generated using Show all files the option Solution Explorer .
        /// </summary>
        public bool SaveInRootFolderWhenIsDebug { get; set; }

        /// <summary>
        /// Name of the default folder. The default is .app
        /// </summary>
        public string DefaultFolder
        {
            get
            {
                return this.defaultFolder;
            }
            set
            {
                this.defaultFolder = value;
                if (Development.IsAttached && this.SaveInRootFolderWhenIsDebug)
                    this.defaultFolder = Path.Combine(GetCurrentDebugDirectory(), this.defaultFolder);
            }
        }

        private string GetCurrentDebugDirectory()
        {
            return EnvironmentExtensions.GetProjectDirectory();
        }

        /// <summary>
        /// Initialize
        /// </summary>
        public JsonFileManager()
        {
            this.SaveInRootFolderWhenIsDebug = true;
            this.DefaultFolder = ".app";
            this.DefaultFilePrefix = "";
            this.DefaultFileExtension = ".json";
        }

        /// <summary>
        /// Saves an object in the default folder where file name is the T type name formatted, 
        /// with exception to classes that have the attribute ObjectFile .
        /// </summary>
        /// <typeparam name="T">Type of obj</typeparam>
        /// <param name="obj">Object to save</param>
        public void Save<T>(T obj)
        {
            var fileName = this.GetObjectFileName(typeof(T));
            this.SaveInternal(obj, this.GetFilePath(fileName));
        }

        /// <summary>
        /// Saves an object in the default folder with a specific name.
        /// </summary>
        /// <param name="obj">Object to save</param>
        /// <param name="fileName">Full file name to save</param>
        public void Save(object obj, string fileName)
        {
            this.SaveInternal(obj, this.GetFilePath(fileName));
        }

        private void SaveInternal(object obj, string fileName)
        {
            if (obj != null)
            {
                SaveToFileJson(obj, fileName);
                this.objectsFiles[fileName] = obj;
            }
        }

        /// <summary>
        /// Removes an object in the default folder where file name is the T type name formatted, 
        /// with exception to classes that have the attribute ObjectFile .
        /// </summary>
        /// <typeparam name="T">Type of object to remove</typeparam>
        public void Remove<T>()
        {
            var fileName = this.GetObjectFileName(typeof(T));
            this.RemoveInternal(this.GetFilePath(fileName));
        }

        /// <summary>
        /// Removes an object in the default folder with a specific name.
        /// </summary>
        /// <param name="fileName">Full file name to remove</param>
        public void Remove(string fileName)
        {
            this.RemoveInternal(this.GetFilePath(fileName));
        }

        private void RemoveInternal(string fileName)
        {
            FileHelper.RemoveFile(fileName);
            if (this.objectsFiles.ContainsKey(fileName))
                this.objectsFiles.Remove(fileName);
        }

        /// <summary>
        /// Returns a default folder object.
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="fileName">Indicates the file name, if null the name of the type T is used in the search, with exception to classes that have the attribute ObjectFile</param>
        /// <param name="refresh">If false , will seek in the internal cache if you have already been loaded previously. Otherwise be forced file loading.</param>
        /// <returns>Returns a default folder object.</returns>
        public T Get<T>(string fileName = null, bool refresh = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = this.GetObjectFileName(typeof(T));

            return GetOrCreateInternal<T>(this.GetFilePath(fileName), true, refresh);
        }

        /// <summary>
        /// Same behavior of the method Get<T>, but creates a new instance when you can't find the 
        /// file in the default folder. It is important to say that the file will not be created, 
        /// only the instance of the type T . To save physically it is necessary to use the method Save .
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="fileName">Indicates the file name, if null the name of the type T is used in the search, with exception to classes that have the attribute ObjectFile .</param>
        /// <param name="refresh">If false , will seek in the internal cache if you have already been loaded previously. Otherwise be forced file loading.</param>
        /// <returns>Returns a default folder object or a new instance if not exists</returns>
        public T GetOrCreate<T>(string fileName = null, bool refresh = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = this.GetObjectFileName(typeof(T));

            return GetOrCreateInternal<T>(this.GetFilePath(fileName), false, refresh);
        }

        private T GetOrCreateInternal<T>(string fileName = null, bool onlyGet = false, bool refresh = false)
        {
            if (this.objectsFiles.ContainsKey(fileName) && !refresh)
                return this.objectsFiles[fileName] == null ? default(T) : (T)this.objectsFiles[fileName];

            var objFile = GetFromFileJson<T>(fileName);

            if (objFile == null && !onlyGet)
                objFile = Activator.CreateInstance<T>();

            this.objectsFiles[fileName] = objFile;

            return objFile;
        }

        /// <summary>
        /// Returns the formatted type name or if you are using the ObjectFile attribute, 
        /// returns the value of the FileName property.
        /// </summary>
        /// <param name="type">Type reference to get name</param>
        /// <returns>Return a file name</returns>
        public string GetObjectFileName(Type type)
        {
            string fileName;

            var attr = type.GetCustomAttribute<ObjectFileAttribute>(true);
            if (attr != null && !string.IsNullOrWhiteSpace(attr.FileName))
            {
                fileName = attr.FileName;
            }
            else
            {
                fileName = ReflectionHelper.CSharpName(type, this.UseTypeFullName).Replace("<", "[").Replace(">", "]").Replace(@"\", "");
                fileName = this.DefaultFilePrefix + ToLowerSeparate(fileName, '.') + this.DefaultFileExtension;
            }

            return fileName;
        }

        /// <summary>
        /// Returns the file path into the default folder.
        /// </summary>
        /// <param name="fileName">Filename reference</param>
        /// <returns>Full path</returns>
        public string GetFilePath(string fileName)
        {
            return Path.Combine(this.DefaultFolder, fileName);
        }

        #region Json

        /// <summary>
        /// Get JSON by object
        /// </summary>
        /// <param name="obj">Object base</param>
        /// <param name="config">Json settings, if null the default config will be used</param>
        /// <returns>Objet to JSON</returns>
        public static string GetContentJsonFromObject(object obj, JsonSerializerSettings config = null)
        {
            if (config == null)
            {
                config = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented
                };

                //config.SerializationBinder = binder;
            }

            return JsonConvert.SerializeObject(obj, config.Formatting, config);
        }

        /// <summary>
        /// Get object from JSON
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="contentJson">JSON to parse to object</param>
        /// <param name="config">Json settings, if null the default config will be used</param>
        /// <returns>Expected object</returns>
        public static T GetFromContentJson<T>(string contentJson, JsonSerializerSettings config = null)
        {
            if (config == null)
            {
                config = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };

                //config.SerializationBinder = binder;
            }

            return JsonConvert.DeserializeObject<T>(contentJson, config);
        }

        /// <summary>
        /// Get object from file JSON
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="fileName">File location</param>
        /// <param name="config">Json settings, if null the default config will be used</param>
        /// <returns>Expected object</returns>
        public static T GetFromFileJson<T>(string fileName, JsonSerializerSettings config = null)
        {
            var objFile = default(T);

            if (File.Exists(fileName))
                objFile = GetFromContentJson<T>(FileHelper.GetContentFromFile(fileName), config);

            return objFile;
        }

        /// <summary>
        /// Save object to JSON in a specific local
        /// </summary>
        /// <param name="obj">Object to save</param>
        /// <param name="fileName">File location</param>
        /// <param name="config">Json settings, if null the default config will be used</param>
        public static void SaveToFileJson(object obj, string fileName, JsonSerializerSettings config = null)
        {
            FileHelper.SaveContentToFile(GetContentJsonFromObject(obj, config), fileName);
        }

        #endregion

        private string ToLowerSeparate(string str, char separate)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                var newStr = "";
                for (var i = 0; i < str.Length; i++)
                {
                    var c = str[i];
                    if (i > 0 && separate != str[i - 1] && char.IsLetterOrDigit(str[i - 1]) && char.IsUpper(c) && !char.IsUpper(str[i - 1]))
                        newStr += separate + c.ToString().ToLower();
                    else
                        newStr += c.ToString().ToLower();
                }

                return newStr;
            }

            return str;
        }
    }
}