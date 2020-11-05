namespace NubankCli.Core.Extensions.Formatters
{
    public static class FormatBooleanExtensions
    {
        public static string HumanizeDefault(this bool? value)
        {
            if (value == null)
                return "-";

            return HumanizeDefault(value.Value);
        }

        public static string HumanizeDefault(this bool value)
        {
            if (value)
                return "Sim";

            return "Não";
        }

    }
}