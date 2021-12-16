using System;
using System.IO;
using System.Reflection;

namespace NubankSharp.Cli.Extensions
{
    /// <summary>
    /// Helper class to working in debug mode
    /// </summary>
    public static class EnvironmentExtensions
    {
        private static string projectDirectory;
        private static object thisLock = new object();
        private static string executionPath;

        /// <summary>
        /// Check if is in debug mode
        /// </summary>
        public static bool IsAttached
        {
            get
            {
                return System.Diagnostics.Debugger.IsAttached;
            }
        }

        public static string DebugOrExecutionDirectory
        {
            get
            {
                if (IsAttached)
                    return GetProjectDirectory();

                return ExecutionDirectory;
            }
        }

        public static string ProjectRootOrExecutionDirectory
        {
            get
            {
#if RELEASE
                return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
#else

                var projectRoot = GetProjectDirectory();
                return projectRoot;
#endif
            }
        }

        public static string ExecutionDirectory
        {
            get
            {
                if (executionPath == null)
                    executionPath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;

                return executionPath;
            }
        }

        /// <summary>
        /// Get the project base path
        /// </summary>
        /// <param name="baseDir">Current directory, if null get from de system</param>
        /// <returns>Project base path</returns>
        public static string GetProjectDirectory(string baseDir = null)
        {
            lock (thisLock)
            {
                if (projectDirectory == null)
                {
                    var pathFull = baseDir ?? new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
                    projectDirectory = pathFull;
                    
                    // if TRUE is because in VisualStudio
                    var i = 1;
                    do
                    {
                        if (Directory.GetFiles(projectDirectory, "project.json").Length != 0)
                            return projectDirectory;
                        else if (Directory.GetFiles(projectDirectory, "*.csproj").Length != 0)
                            return projectDirectory;
                        else if (Directory.GetFiles(projectDirectory, "*.xproj").Length != 0)
                            return projectDirectory;
                        projectDirectory = GetHigherDirectoryPath(pathFull, i);
                        i++;
                    }
                    while (projectDirectory != null);
                    
                    throw new System.Exception("No project files were found");
                }
            }

            return projectDirectory;
        }

        public static string GetResourceContent(string resourcePath)
        {
            // Tenta encontrar nas pastas fisicas
            // 1) Tenta na pasta raiz do projeto (onde está o csproj)
            // 2) Se não encontrar (modo release), então tenta encontrar na pasta de execução (bin) 
            //    2.1) Nesse caso o arquivo razor deve estar configurado para sempre ser copiado para a output
            // 3) Se não encontrar em nenhum dos locais, então tenta no embededResources
            //    3.1) Nesse caso o arquivo razor deve estar configurado para sempre ser compilado junto ao assembly
            var fullPath = Path.Combine(GetProjectDirectory(), resourcePath);

            if (!File.Exists(fullPath))
                fullPath = Path.Combine(ExecutionDirectory, resourcePath);

            if (File.Exists(fullPath))
                return File.ReadAllText(fullPath);

            resourcePath = resourcePath.Replace("/", ".").ToLower();
            var assembly = Assembly.GetEntryAssembly();
            foreach (var f in assembly.GetManifestResourceNames())
            {
                if (f.ToLower().Contains(resourcePath))
                {
                    using Stream stream = assembly.GetManifestResourceStream(f);
                    using StreamReader reader = new StreamReader(stream);
                    return reader.ReadToEnd();
                }
            }

            return null;
        }

        private static string GetHigherDirectoryPath(string srcPath, int upLevel)
        {
            string[] directoryElements = srcPath.Split(Path.DirectorySeparatorChar);
            if (upLevel >= directoryElements.Length)
            {
                return null;
            }
            else
            {
                string[] resultDirectoryElements = new string[directoryElements.Length - upLevel];
                for (int elementIndex = 0; elementIndex < resultDirectoryElements.Length; elementIndex++)
                {
                    resultDirectoryElements[elementIndex] = directoryElements[elementIndex];
                }
                return string.Join(Path.DirectorySeparatorChar.ToString(), resultDirectoryElements);
            }
        }
    }
}