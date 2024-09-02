using System.Text.Json;

namespace Dovid.TextFixedWidthGenerator
{
    public class Generator
    {
        private int CurrentRowPosition { get; set; }
        private JsonElement CurrentRowElement { get; set; }

        public DucumentSpecificationSet Specification { get; set; }

        public Generator(string schemeFileName)
        {
            var text = File.ReadAllText(schemeFileName);
            Specification = JsonSerializer.Deserialize<DucumentSpecificationSet>(text);
        }

        public Generator(DucumentSpecificationSet specification)
        {
            Specification = specification;
        }

        public IEnumerable<string> Export(string jsonAsString)
        {
            var rows = JsonToRows(jsonAsString);

            foreach (var element in rows)
            {
                CurrentRowPosition++;
                CurrentRowElement = element;
                yield return string.Concat(Specification.FieldSets.Select(fs => fs.ToString(this)));
            }
        }

        public class DucumentSpecificationSet
        {
            public List<DataFieldSet> FieldSets { get; set; }
        }

        public class DataFieldSet
        {
            public string FriednlyName { get; set; }
            public string SourceFieldName { get; set; }
            public TypeValue? Type { get; set; } = null;
            public SpecialValueProvider SpecialValue { get; set; }
            public string Format { get; set; } //passed to ToString(Format) function
            public string DefaultValue { get; set; }

            //Padding
            public int? Size { get; set; }
            public Side PaddingSide { get; set; }
            public char PaddingChar { get; set; } = ' ';


            private string GetJsonValue(JsonElement rowObj)
            {
                var exsist = rowObj.TryGetProperty(SourceFieldName, out var elVal);

                string output = "";

                if (exsist)
                    output = Type switch
                    {
                        TypeValue.String => elVal.GetString().ToString(),
                        TypeValue.Int => elVal.GetInt64().ToString(Format),
                        TypeValue.Double => elVal.GetDouble().ToString(Format),
                        TypeValue.DateTime => elVal.GetDateTime().ToString(Format),
                        TypeValue.Boolean => GetBool(elVal).ToString(),
                        _ => elVal.ToString()
                    };

                return output;
            }


            public string ToString(Generator exporter)
            {
                var output = SpecialValue?.GetValue(exporter, Format) ?? GetJsonValue(exporter.CurrentRowElement) ?? DefaultValue;

                if (Size.HasValue)
                {
                    if (output.Length > Size)
                        output = output.Substring(0, Size.Value);
                    else if (output.Length < Size)
                    {
                        output = PaddingSide switch
                        {
                            Side.Left => output.PadLeft(Size.Value, PaddingChar),
                            Side.Right => output.PadRight(Size.Value, PaddingChar),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }
                }


                return output;
            }

            bool GetBool(JsonElement el)
            {
                if (el.ValueKind == JsonValueKind.True)
                    return true;
                if (el.ValueKind == JsonValueKind.False)
                    return false;
                throw new FormatException();
            }

            public enum Side
            {
                Right,
                Left
            }

            public enum TypeValue
            {
                Int,
                String,
                DateTime,
                Double,
                Boolean
            }
        }



        public class SpecialValueProvider
        {
            private TypeValue TypeOfValue { get; set; }

            public SpecialValueProvider() { }
            public SpecialValueProvider(TypeValue typeOfValue) => TypeOfValue = typeOfValue;


            public enum TypeValue
            {
                RowNumber
            }

            public string GetValue(Generator exporter, string Format)
            {
                return TypeOfValue switch
                {
                    TypeValue.RowNumber => exporter.CurrentRowPosition.ToString(Format),
                    _ => throw new ArgumentException()
                };
            }
        }

        private IEnumerable<JsonElement> JsonToRows(string jsonAsString)
        {
            var doc = JsonDocument.Parse(jsonAsString);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                yield break;

            var enumArr = doc.RootElement.EnumerateArray();

            foreach (var element in enumArr)
                if (element.ValueKind == JsonValueKind.Object)
                    yield return element;
        }
    }
}
