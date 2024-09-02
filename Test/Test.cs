using Dovid.TextFixedWidthGenerator;

namespace Test
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Test1()
        {
            var specification = new Generator.DucumentSpecificationSet
            {
                FieldSets = new List<Generator.DataFieldSet>
                {
                    new Generator.DataFieldSet
                    {
                        FriednlyName = "Name",
                        SourceFieldName = "name",
                        Type = Generator.DataFieldSet.TypeValue.String,
                        Size = 10,
                        PaddingSide = Generator.DataFieldSet.Side.Left,
                        PaddingChar = '0'
                    },
                    new Generator.DataFieldSet
                    {
                        FriednlyName = "Age",
                        SourceFieldName = "age",
                        Type = Generator.DataFieldSet.TypeValue.Int,
                        Size = 3,
                        PaddingSide = Generator.DataFieldSet.Side.Right,
                        PaddingChar = ' '
                    }
                }
            };

            var generator = new Generator(specification);

            var json = "[{\"name\":\"John\",\"age\":25}]";
            var result = generator.Export(json).ToList();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("000000John25 ", result[0]);

        }
    }
}