namespace NubankCli.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NubankCli.Extensions.Tables;
    using NubankCli.Extensions.Langs;
    using NubankCli.Core.Entities;
    using NubankCli.Core.Exceptions;
    using NubankCli.Core.Repositories.Api;
    using SysCommand.ConsoleApp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Runtime.InteropServices;
    using System.Security;
    using NubankCli.Core.Configuration;
    using Microsoft.Extensions.Options;
    using NubankCli.Extensions.Configurations;

    public static class CommandExtensions
    {
        public static Guid _userId;

        private const string LINE_SEPARATOR = "\r\n\r\n";

        public static void SetCurrentUser(this Command command, string userName)
        {
            var configManager = command.GetService<ConfigManager>();
            var appSettings = command.GetService<IOptions<AppSettings>>()?.Value;
            appSettings.CurrentUser = userName;
            configManager.Save(appSettings);
        }

        public static User GetCurrentUser(this Command command)
        {
            var appSettings = command.GetService<IOptions<AppSettings>>()?.Value;

            if (string.IsNullOrWhiteSpace(appSettings?.CurrentUser))
                throw new UnauthorizedException();

            return new User(appSettings.CurrentUser);
        }

        public static NubankRepository GetNubankRepositoryByUser(this Command command, User user)
        {
            var repository = command.GetService<NubankRepository>();
            var userInfo = user.GetUserInfo();

            if (userInfo == default)
                throw new UnauthorizedException();

            repository.Endpoints.AutenticatedUrls = userInfo.AutenticatedUrls;
            repository.AuthToken = userInfo.Token;
            return repository;
        }

        public static T GetService<T>(this Command command)
        {
            return command.App.Items.GetServiceProvider().GetService<T>();
        }

        public static object GetService(this Command command, Type type)
        {
            return command.App.Items.GetServiceProvider().GetService(type);
        }

        public static void ShowApiException(this Command command, Exception exception)
        {
            while (exception is AggregateException)
                exception = exception.InnerException;

            if (check<UnauthorizedException>(exception, out var unauthorizedException))
            {
                command.App.Console.Error("Voc� n�o est� logado");
            }
            else
            {
                command.App.Console.Error(exception.Message);
                command.App.Console.Error(exception.StackTrace);
            }

            bool check<TVerify>(Exception eIn, out TVerify eOut) where TVerify : Exception
            {
                eOut = default(TVerify);
                if (eIn is TVerify verify)
                {
                    eOut = verify;
                    return true;
                }
                else if (eIn.InnerException is TVerify verify1)
                {
                    eOut = verify1;
                    return true;
                }

                return false;
            }
        }

        //public static void ShowApiException(this Command command, MoreThanOneIdException exception)
        //{
        //    command.App.Console.Warning(string.Format(MessagesPtBr.MORE_THEN_ONE_ID_OR_NAME, exception.SearchText));
        //    // foreach (var i in exception.Ids)
        //    //     command.App.Console.Warning(i);
        //}

        //public static void ShowApiException(this Command command, Exception exception)
        //{
        //    while (exception is AggregateException)
        //        exception = exception.InnerException;

        //    if (check<MoreThanOneIdException>(exception, out var moreThanOneIdException))
        //    {
        //        ShowApiException(command, moreThanOneIdException);
        //    }
        //    else if (check<ValidationApiException>(exception, out var validationApiException))
        //    {
        //        command.App.Console.Error(validationApiException.Content?.Detail);
        //    }
        //    else if (check<ApiException>(exception, out var apiException))
        //    {
        //        try
        //        {
        //            var error = apiException.GetContentAsAsync<ProblemDetailsResponse>().Result;

        //            if (apiException.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                command.App.Console.Warning("N�o autorizado, fa�a o login usando o comando abaixo:");
        //                command.App.Console.WriteLn();
        //                command.App.Console.Success("  login -u [username] -p [password]");
        //                command.App.Console.WriteLn();
        //                command.App.Console.Warning("Ou fa�a um novo cadastro:");
        //                command.App.Console.WriteLn();
        //                command.App.Console.Success("  add user");
        //                return;
        //            }

        //            command.App.Console.Error(apiException.ReasonPhrase);

        //            if (error?.Errors != null)
        //            {
        //                foreach (var e in error?.Errors)
        //                {
        //                    foreach (var fieldErro in e.Value)
        //                    {
        //                        var space = string.IsNullOrWhiteSpace(fieldErro.Message) ? null : " / ";
        //                        command.App.Console.Error($"{e.Key}: {fieldErro.Type}{space}{fieldErro.Message}");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (
        //                    apiException.StatusCode != HttpStatusCode.Forbidden &&
        //                    apiException.StatusCode != HttpStatusCode.Unauthorized
        //                )
        //                {
        //                    command.App.Console.Error(error.Detail);
        //                }
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            command.App.Console.Error(exception.Message);
        //            command.App.Console.Error(exception.StackTrace);
        //        }
        //    }
        //    else
        //    {
        //        command.App.Console.Error(exception.Message);
        //        command.App.Console.Error(exception.StackTrace);
        //    }

        //    bool check<TVerify>(Exception eIn, out TVerify eOut) where TVerify : Exception
        //    {
        //        eOut = default(TVerify);
        //        if (eIn is TVerify)
        //        {
        //            eOut = (TVerify)eIn;
        //            return true;
        //        }
        //        else if (eIn.InnerException is TVerify)
        //        {
        //            eOut = (TVerify)eIn.InnerException;
        //            return true;
        //        }

        //        return false;
        //    }
        //}

        public static void ViewPagination<T>(this Command command, int page, Func<int, PagedResult<T>> callback, bool autoPage, string output = null, bool showCountResume = true)
        {
            var next = true;
            while (next)
            {
                var paged = callback(page);
                string text;

                if (paged.PageCount == 0)
                    text = MessagesPtBr.SEARCH_EMPTY_RESULT;
                else if (output == "json")
                    text = JsonConvert.SerializeObject(paged.Queryable, Formatting.Indented);
                else
                    text = ToTable(command, paged.Queryable, output);

                command.App.Console.Write(text);

                if (paged.PageCount == 1 && showCountResume)
                    command.App.Console.Success(AddLineSeparator(string.Format(MessagesPtBr.SEARCH_RESULT, paged.RowCount)));
                else if (paged.PageCount > 1)
                    command.App.Console.Success(AddLineSeparator(string.Format(MessagesPtBr.SEARCH_PAGINATION_RESULT, paged.CurrentPage, paged.PageCount, paged.RowCount)));

                if (!autoPage)
                    break;

                next = page < paged.PageCount;
                page++;

                if (next && command.App.Console.Read("") != null)
                    break;
            }
        }

        public static void ViewFormatted<T>(this Command command, IEnumerable<T> value, string output = null, bool showCountResume = true)
        {
            string text;
            bool hasItems = value.Any();

            if (!hasItems)
                text = MessagesPtBr.SEARCH_EMPTY_RESULT;
            else if (output == "json")
                text = Newtonsoft.Json.JsonConvert.SerializeObject(value, Formatting.Indented);
            else
                text = ToTable(command, value, output);

            command.App.Console.Write(text);

            if (hasItems && showCountResume)
                command.App.Console.Success(AddLineSeparator(string.Format(MessagesPtBr.SEARCH_RESULT, value.Count())));
        }

        public static void ViewSingleFormatted<T>(this Command command, T value, string output)
        {
            string text;
            if (output == "json")
                text = JsonConvert.SerializeObject(value, Formatting.Indented);
            else
                text = ToTable(command, new List<T>() { value }, output);

            command.App.Console.Write(text);
        }

        public static SecureString ReadPassword(this Command command, string msg)
        {
            var pass = new SecureString();
            // var colorOriginal = Console.ForegroundColor;
            // Console.ForegroundColor = ConsoleColor.Blue;

            if (command.App.Console.BreakLineInNextWrite)
                Console.WriteLine();

            Console.Write(msg);
            // Console.ForegroundColor = colorOriginal;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    pass.AppendChar(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass.RemoveAt(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops receiving keys once enter is pressed
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            command.App.Console.BreakLineInNextWrite = false;
            return pass;
        }

        public static string SecureStringToString(this Command command, SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        public static T Get<T>(this IEnumerable<Command> collection) where T : Command
        {
            return collection.Where(f => f is T).Cast<T>().FirstOrDefault();
        }

        public static bool Continue(this Command command, string message, params string[] yesOptions)
        {
            return Continue(command, message, out _, yesOptions);
        }

        public static bool Continue(this Command command, string message, out string result, params string[] yesOptions)
        {
            result = command.App.Console.Read(message + " [Yes/No]: ")?.ToLower();

            if (yesOptions.Length == 0)
                yesOptions = new string[] { "y", "yes", "s", "sim" };

            return !string.IsNullOrWhiteSpace(result) && yesOptions.Contains(result);
        }

        //public static T ExternalEdit<T>(this Command command, T obj = default, string prefix = null, string extension = null)
        //    where T : class
        //{
        //    var appSettings = GetService<IOptions<AppSettings>>(command).Value;
        //    var configManager = GetService<IConfigManager>(command);
        //    string str;
        //    string tempFile;
        //    var isString = obj.GetDeclaredType() == typeof(string);

        //    if (isString)
        //    {
        //        str = obj as string;
        //        tempFile = GetTempFile<T>(prefix, extension ?? "txt");
        //    }
        //    else
        //    {
        //        if (obj == null)
        //            obj = Activator.CreateInstance<T>();

        //        str = obj.Serialize(true);
        //        tempFile = GetTempFile<T>(prefix, extension ?? "json");
        //    }

        //    string editor;

        //    File.WriteAllText(tempFile, str);

        //    if (!string.IsNullOrWhiteSpace(appSettings.Editor))
        //    {
        //        editor = appSettings.Editor;
        //    }
        //    else
        //    {
        //        var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
        //        var isFreeBSD = RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
        //        var isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        //        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        //        if (isLinux || isFreeBSD || isOSX)
        //            editor = "vi";
        //        else if (isWindows)
        //            editor = "notepad";
        //        else
        //            throw new Exception("Nenhum editor foi encontrado");
        //    }

        //    Process process;

        //    try
        //    {
        //        var args = ParseCommandAndArguments(editor, tempFile);
        //        var processStartInfo = new ProcessStartInfo()
        //        {
        //            FileName = args.FileName,
        //            Arguments = args.Arguments,
        //            UseShellExecute = true
        //        };

        //        process = Process.Start(processStartInfo);
        //        process.WaitForExit();
        //    }
        //    catch (Exception ex)
        //    {
        //        command.App.Console.Error(string.Format(MessagesPtBr.EDITOR_NOT_FOUND, editor));
        //        throw ex;
        //    }

        //    if (editor != appSettings.Editor)
        //    {
        //        appSettings.Editor = editor;
        //        configManager.Save(appSettings);
        //    }

        //    string fileEdited = File.ReadAllText(tempFile);

        //    try
        //    {
        //        File.Delete(tempFile);
        //    }
        //    catch { }


        //    if (isString)
        //        return fileEdited as T;

        //    return fileEdited.Deserialize<T>();
        //}

        //private static (string FileName, string Arguments) ParseCommandAndArguments(string arguments, string file)
        //{
        //    var split = arguments.Split(' ').ToList();
        //    var fileName = "";

        //    // 1) IF for um caminho completo, ou seja, com barras 
        //    // c:\program files\code.exe --wait
        //    // devolve "c:\program files\code.exe" como fileName e "--wait" como argumentos
        //    // 2) ELSE for um alias de algum comando: "code --wait"
        //    // devolve "code" como fileName e "--wait" como argumentos
        //    if (arguments.Contains("/") || arguments.Contains("\\"))
        //    {
        //        foreach (var i in split.ToArray())
        //        {
        //            split.RemoveAt(0);

        //            fileName += $"{i} ";
        //            if (File.Exists(fileName))
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        fileName = split[0];
        //        split.RemoveAt(0);
        //    }

        //    split.Insert(0, $"\"{file}\"");
        //    return (fileName.Trim(), string.Join(' ', split));
        //}

        private static string ToTable<T>(Command command, IEnumerable<T> value, string output)
        {
            TableView tableView;
            var tableConfig = command.App.Items.GetServiceProvider().GetService<ITableConfig<T>>();
            if (tableConfig == null)
                tableView = TableView.ToTableView(value, wide: output == "wide");
            else
                tableView = TableView.ToTableView(value, tableConfig, output == "wide");

            tableView.AddLineSeparator = false;
            tableView.ColumnSeparator = null;

            return tableView.Build().ToString();
        }

        public static string GetTempFile<T>(string prefix, string extension)
        {
            if (!extension.StartsWith("."))
                extension = "." + extension;

            if (string.IsNullOrWhiteSpace(prefix))
                prefix = typeof(T).Name;

            string fileName;
            int count = 0;

            while (true)
            {
                fileName = $"{prefix}{count}{extension}";
                fileName = Path.Combine(Path.GetTempPath(), fileName);

                if (!File.Exists(fileName))
                {
                    using var file = new FileStream(fileName, FileMode.CreateNew);
                    break;
                }

                count++;
            }

            return fileName;
        }

        #region Privates

        private static string AddLineSeparator(string msg)
        {
            return string.Format("{0}{1}", LINE_SEPARATOR, msg);
        }

        #endregion

    }
}