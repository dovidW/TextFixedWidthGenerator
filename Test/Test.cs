using Dovid.TextFixedWidthGenerator;

namespace Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Test1()
        {
            var specification = new DocumentSpecificationSet
            {
                FieldSets = new List<DataFieldSet>
                {
                    new DataFieldSet
                    {
                        SourceFieldName = "name",
                        Size = 10,
                        PaddingSide = Side.Left,
                        PaddingChar = '0'
                    },
                    new DataFieldSet
                    {
                        SourceFieldName = "lastName",
                        Size = 20,
                        PaddingSide = Side.Right,
                        PaddingChar = ' ',
                        ReverseOrder = true
                    },
                    new DataFieldSet
                    {
                        SourceFieldName = "age",
                        Size = 5,
                        PaddingSide = Side.Left,
                        PaddingChar = '_',
                        Format = ".F2"
                    }
                }
            };


            var result = Generator.ExportToText(new[] { new { name = "John", age = 25, lastName = "בנימין ברבישבסקי" } }, specification).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("000000Johnיקסבשיברב ןימינב    _2500", result[0]);

        }
    }
}