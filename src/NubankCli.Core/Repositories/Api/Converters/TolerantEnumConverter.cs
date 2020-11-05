using Newtonsoft.Json;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NubankCli.Core.Repositories.Api
{
    /// <summary>
    /// Code extracted from https://stackoverflow.com/questions/22752075/how-can-i-ignore-unknown-enum-values-during-json-deserialization
    /// </summary>
    class TolerantEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var type = IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
            return type.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var isNullable = IsNullableType(objectType);
            var enumType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            var names = Enum.GetNames(enumType);
            var options = names.Select(n => (Enum)Enum.Parse(enumType, n));

            if (reader.TokenType == JsonToken.String)
            {
                var enumText = reader.Value.ToString();

                if (!string.IsNullOrEmpty(enumText))
                {
                    var match = options
                        .FirstOrDefault(n => {
                            return string.Equals(n.GetJsonValue(), enumText, StringComparison.OrdinalIgnoreCase);
                        });

                    if (match != null)
                    {
                        return match;
                    }
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                var enumVal = Convert.ToInt32(reader.Value);
                var values = (int[])Enum.GetValues(enumType);
                if (values.Contains(enumVal))
                {
                    return Enum.Parse(enumType, enumVal.ToString());
                }
            }

            if (!isNullable)
            {
                var defaultName = names
                    .FirstOrDefault(n => string.Equals(n, "Unknown", StringComparison.OrdinalIgnoreCase));

                if (defaultName == null)
                {
                    defaultName = names.First();
                }

                return Enum.Parse(enumType, defaultName);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        private static bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }

    static class EventCategoryExtensions
    {
        public static string GetJsonValue(this Enum @enum)
        {
            var fieldInfo = @enum.GetType().GetField(@enum.ToString());

            return !(Attribute.GetCustomAttribute(fieldInfo, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
                ? @enum.ToString()
                : attribute.Value;
        }
    }
}
