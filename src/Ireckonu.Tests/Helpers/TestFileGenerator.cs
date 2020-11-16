using Ireckonu.Tests.Helpers;
using System.IO;
using System.Threading.Tasks;

namespace Ireckonu.Tests
{
    /// <summary>
    /// A class used to generate large test files for manual / integration testing
    /// </summary>
    internal sealed class TestFileGenerator
    {
        private string GenerateValidRecord()
        {
            var key = RandomHelper.RandomString(8, 20);
            var articleCode = RandomHelper.RandomString(1, 10);
            var colorCode = RandomHelper.RandomString(5, 15);
            var description = RandomHelper.RandomString(5, 15);
            var price = RandomHelper.Random.Next(0, 10000);
            var discount = RandomHelper.Random.Next(0, 10000);
            var deliveredIn = RandomHelper.RandomString(5, 15);
            var q1 = RandomHelper.RandomString(3, 10);
            var size = RandomHelper.Random.Next(0, 200);
            var color = RandomHelper.RandomString(3, 10);

            var values = new string[]
            {
                key,
                articleCode,
                colorCode,
                description,
                price + "",
                discount + "",
                deliveredIn,
                q1,
                size + "",
                color
            };

            return string.Join(",", values);
        }

        private string GenerateInvalidRecord()
        {
            return RandomHelper.RandomString(1, 100);
        }

        public async Task Generate(string path, int numberOfRecords)
        {
            using var writer = new StreamWriter(path);

            // write header
            await writer.WriteLineAsync("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color");

            for (int i = 0; i < numberOfRecords; i++)
            {
                var record = RandomHelper.Random.NextDouble() > 0.001 ? GenerateValidRecord() : GenerateInvalidRecord();
                await writer.WriteLineAsync(record);
            }
            writer.Flush();
        }
    }
}
