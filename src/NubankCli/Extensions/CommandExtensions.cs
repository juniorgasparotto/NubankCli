namespace NubankSharp.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NubankSharp.Extensions.Tables;
    using NubankSharp.Extensions.Langs;
    using NubankSharp.Entities;
    using NubankSharp.Exceptions;
    using NubankSharp.Repositories.Api;
    using SysCommand.ConsoleApp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Runtime.InteropServices;
    using System.Security;
    using Microsoft.Extensions.Options;
    using NubankSharp.Cli.Extensions;
    using NubankSharp.Repositories.Files;
    using NubankSharp.Models;

    public static class CommandExtensions
    {
        public static Guid _userId;

        private const string LINE_SEPARATOR = "\r\n\r\n";
        public static readonly string QUERY_FOLDER = Path.Combine(EnvironmentExtensions.ProjectRootOrExecutionDirectory, "Queries");
        public static readonly string USERSDATA_FOLDER = Path.Combine(EnvironmentExtensions.ProjectRootOrExecutionDirectory, "UsersData");
        public static readonly string SESSION_FILE_PATH = Path.Combine(USERSDATA_FOLDER, "session.json");
        public static readonly string USER_FILE_NAME = "nu-user.json";
        public static NuSession SESSION = null;
        public const int FIRST_PAGE = 1;

        #region Session

        public static NuSession GetSession(this Command command)
        {
            if (SESSION == null)
            {
                var repo = command.GetService<JsonFileRepository<NuSession>>();
                var nuSession = repo.GetFile(SESSION_FILE_PATH);
                SESSION = nuSession ?? new NuSession();
            }
            return SESSION;
        }

        public static void SaveSession(this Command command)
        {
            var nuSession = GetSession(command);
            var repo = command.GetService<JsonFileRepository<NuSession>>();
            repo.Save(nuSession, SESSION_FILE_PATH);
        }

        public static NuUser GetCurrentUser(this Command command)
        {
            var session = GetSession(command);
            if (string.IsNullOrWhiteSpace(session?.CurrentUser))
                throw new UnauthorizedException();

            var repo = command.GetService<JsonFileRepository<NuUser>>();
            return repo.GetFile(command.GetUserFileName(session.CurrentUser));
        }

        public static void SetCurrentUser(this Command command, string userName)
        {
            var session = GetSession(command);
            session.CurrentUser = userName;
            SaveSession(command);
        }

        #endregion

        #region UsersData

        public static string GetUserPath(this Command command, NuUser user)
        {
            return command.GetUserPath(user.UserName);
        }

        public static string GetUserPath(this Command command, string userName)
        {
            return Path.Combine(EnvironmentExtensions.ProjectRootOrExecutionDirectory, "UsersData", userName);
        }

        public static string GetUserFileName(this Command command, NuUser user)
        {
            return command.GetUserFileName(user.UserName);
        }

        public static string GetUserFileName(this Command command, string userName)
        {
            return Path.Combine(command.GetUserPath(userName), USER_FILE_NAME);
        }

        public static string GetCardPath(this Command command, Card card)
        {
            return Path.Combine(command.GetUserPath(card.UserName), card.Name);
        }

        public static string GetStatementFileName(this Command command, Statement statement)
        {
            var dtStart = statement.Start.ToString("yyyy-MM");
            var dtEnd = statement.End.ToString("yyyy-MM");
            var fileName = dtStart;

            if (dtStart != dtEnd)
                fileName += $"_{dtEnd}";

            return Path.Combine(command.GetCardPath(statement.Card), $"{fileName}.json");
        }

        public static NuUser GetUser(this Command command, string userName)
        {
            var repo = command.GetService<JsonFileRepository<NuUser>>();
            return repo.GetFile(command.GetUserFileName(userName));
        }

        public static void SaveUser(this Command command, NuUser user)
        {
            var repo = command.GetService<JsonFileRepository<NuUser>>();
            repo.Save(user, command.GetUserFileName(user.UserName));
        }

        #endregion

        public static NuHttpClient CreateNuHttpClient(this Command command, NuUser user, string scope = null)
        {
            var appSettings = command.GetService<IOptions<NuAppSettings>>().Value;
            return new NuHttpClient(
                        user,
                        appSettings.NubankUrl,
                        appSettings.EnableMockServer ? appSettings.MockUrl : null,
                        appSettings.EnableDebugFile ? new NuHttpClientLogging(user.UserName, scope, Path.Combine(EnvironmentExtensions.ProjectRootOrExecutionDirectory, "Logs")) : null
                    );
        }

        public static NuApi CreateNuApiByUser(this Command command, NuUser user, string scope = null)
        {
            if (user.Token == null)
                throw new UnauthorizedException();

            var httpClient = command.CreateNuHttpClient(user, scope);
            var endPointRepository = new EndPointApi(httpClient);
            var repository = new NuApi(httpClient, endPointRepository, new GqlQueryRepository(QUERY_FOLDER));
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
                command.App.Console.Error("Você não está logado");
            }
            else
            {
                command.App.Console.Error(exception.Message);
                command.App.Console.Error(exception.StackTrace);
            }

            static bool check<TVerify>(Exception eIn, out TVerify eOut) where TVerify : Exception
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