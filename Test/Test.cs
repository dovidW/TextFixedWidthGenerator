using Dovid.TextFixedWidthGenerator;

namespace Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Test1()
        {
            var specification = new DucumentSpecificationSet
            {
                FieldSets = new List<DataFieldSet>
                {
                    new DataFieldSet
                    {
                        FriednlyName = "Name",
                        SourceFieldName = "name",
                        Size = 10,
                        PaddingSide = DataFieldSet.Side.Left,
                        PaddingChar = '0'
                    },
                    new DataFieldSet
                    {
                        FriednlyName = "Age",
                        SourceFieldName = "age",
                        Size = 3,
                        PaddingSide = DataFieldSet.Side.Right,
                        PaddingChar = ' '
                    }
                }
            };


            var json = "[{\"name\":\"John\",\"age\":25}]";
            var result = Generator.ExportToText(new[] { new { name = "John", age = 25 } }, specification).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("000000John25 ", result[0]);

        }
    }
}