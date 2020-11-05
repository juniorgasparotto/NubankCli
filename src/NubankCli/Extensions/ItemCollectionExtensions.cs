using System;
using SysCommand.ConsoleApp;

namespace NubankCli.Extensions
{
    public static class ItemCollectionExtensions
    {
        public static void SetServiceProvider(this ItemCollection itemCollection, IServiceProvider provider)
        {
            itemCollection[nameof(IServiceProvider)] = provider;
        }

        public static IServiceProvider GetServiceProvider(this ItemCollection itemCollection)
        {
            return (IServiceProvider)itemCollection[nameof(IServiceProvider)];
        }
    }
}