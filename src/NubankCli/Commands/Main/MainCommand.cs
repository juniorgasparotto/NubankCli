using System.Collections.Generic;
using SysCommand.ConsoleApp;
using SysCommand.ConsoleApp.Results;

namespace NubankCli.Cli
{
    public class MainCommand : Command
    {
        public RedirectResult Main(string[] args = null)
        {
            return new RedirectResult("help");
        }
    }
}
