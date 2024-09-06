using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Dovid.TextFixedWidthGenerator
{


    public class Generator
    {
        public DocumentSpecificationSet Specification { get; private set; }

        private int CurrentRowPosition { get; set; }
        private dynamic CurrentRowElement { get; set; }
        private Dictionary<string, PropertyInfo> props;
        private Func<string, dynamic?> PropertyExtractor = null;

        private Generator(DocumentSpecificationSet scheme)
        {
            Specification = scheme;
        }

        public static IEnumerable<string> ExportToText<T>(IEnumerable<T> inputRows, DocumentSpecificationSet scheme)
        {
            var instance = new Generator(scheme);

            if (typeof(IDictionary<string, dynamic>).IsAssignableFrom(typeof(T)))
                instance.PropertyExtractor = new Func<string, dynamic>((key) => ((IDictionary<string, dynamic>)instance.CurrentRowElement)[key]);
            else
            {
                instance.props = typeof(T).GetProperties().ToDictionary(p => p.Name, p => p);
                instance.PropertyExtractor = (key) => instance.props.TryGetValue(key, out var prop) ? prop.GetValue(instance.CurrentRowElement) : null;
            }

            if (instance.Specification.SeperatorString == null)
                foreach (var element in inputRows)
                {
                    instance.CurrentRowPosition++;
                    instance.CurrentRowElement = element;
                    yield return string.Concat(instance.Specification.FieldSets.Select(fs => instance.GetField(fs)));
                }
            else
                foreach (var element in inputRows)
                {
                    instance.CurrentRowPosition++;
                    instance.CurrentRowElement = element;
                    yield return string.Join(instance.Specification.SeperatorString, instance.Specification.FieldSets.Select(fs => instance.GetField(fs)));
                }
        }

        private string GetSpecialValue(SpecialValueOptions TypeOfValue, string Format)
        {
            return TypeOfValue switch
            {
                SpecialValueOptions.RowNumber => CurrentRowPosition.ToString(Format),
                _ => throw new ArgumentException()
            };
        }

        private static string FormatObj(dynamic obj, string format)
        {
            if (format.StartsWith(".F"))
                return obj.ToString(format.Substring(1)).Replace(".", "");
            return obj.ToString(format);
        }

        public string GetField(DataFieldSet fs)
        {
            string output = null;
            if (fs.SpecialValue.HasValue)
                output = GetSpecialValue(fs.SpecialValue.Value, fs.Format);
            else
            {
                var obj = PropertyExtractor(fs.SourceFieldName);
                if (obj is not null)
                    output = fs.Format == null ? obj.ToString() : FormatObj(obj, fs.Format);
            }

            output ??= fs.DefaultValue ?? "";
            if (fs.ReverseOrder)
                output = new string(output.Reverse().ToArray());

            if (fs.Size.HasValue)
            {
                if (output.Length > fs.Size)
                    output = output.Substring(0, fs.Size.Value);
                else if (output.Length < fs.Size)
                {
                    output = fs.PaddingSide switch
                    {
                        Side.Left => output.PadLeft(fs.Size.Value, fs.PaddingChar),
                        Side.Right => output.PadRight(fs.Size.Value, fs.PaddingChar),
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }

            return output;
        }
    }

    public class DocumentSpecificationSet
    {
        public List<DataFieldSet> FieldSets { get; set; } = new();
        public string SeperatorString { get; set; }
    }

    public class DataFieldSet
    {
        public SpecialValueOptions? SpecialValue { get; set; }
        public string? FriednlyName { get; set; }
        public string SourceFieldName { get; set; }

        public string? Format { get; set; } //passed to ToString(Format) function
        public string? DefaultValue { get; set; }

        //Padding
        public int? Size { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Side PaddingSide { get; set; } = Side.Left;
        public char PaddingChar { get; set; } = ' ';

        public bool ReverseOrder { get; set; }
    }
    public enum Side
    {
        Right,
        Left
    }
    public enum SpecialValueOptions
    {
        RowNumber
    }
}
