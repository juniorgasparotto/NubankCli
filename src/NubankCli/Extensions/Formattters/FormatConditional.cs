using System;

namespace NubankSharp.Cli.Extensions.Formatters
{
    public interface IFormatConditionalBase
    {
        Func<object, bool> CanFormat { get; set; }
        string FormatValue(object obj);
    }

    public class FormatConditional<T> : IFormatConditionalBase
    {
        private Func<object, bool> _canFormat;

        public Func<object, bool> CanFormat
        {
            get
            {
                if (_canFormat == null)
                    _canFormat = f => f is T;

                return _canFormat;
            }
            set
            {
                _canFormat = value;
            }
        }

        public Func<T, string> Formatter { get; set; }
        public Func<object, T> Cast { get; set; }

        public string FormatValue(object obj)
        {
            if (Cast != null)
                return Formatter(Cast(obj));

            return Formatter((T)obj);
        }
    }
}