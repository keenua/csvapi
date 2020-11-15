using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ireckonu.Tests
{
    [TestFixture]
    public class Tests
    {
        const string TestDataDir = "TestData";

        [SetUp]
        public async Task Setup()
        {
            var largeFile = Path.Combine(TestDataDir, "large.csv");

            if (!File.Exists(largeFile))
            {
                var fileGenerator = new TestFileGenerator();
                await fileGenerator.Generate(largeFile, 1000000).ConfigureAwait(false);
            }
        }

        [Test]
        [TestCase("small")]
        [TestCase("large")]
        public async Task TestFile(string file)
        {
            Console.WriteLine(file);
            Assert.Pass();
        }
    }
}