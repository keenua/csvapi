using System;
using System.IO;
using System.Threading.Tasks;

namespace Ireckonu.Tests
{
    internal sealed class TestFileGenerator
    {
        const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMONPQRSTUVWXYZ0123456789/";

        static Random Random { get; } = new Random(Environment.TickCount);

        private static string RandomString(int length)
        {
            var s = new char[length];

            for (int i = 0; i < length; i++)
            {
                s[i] = Alphabet[Random.Next(0, Alphabet.Length)];
            }

            return new string(s);
        }

        private static string RandomString(int minLenght, int maxLenght)
        {
            var length = Random.Next(minLenght, maxLenght);
            return RandomString(length);
        }

        private string GenerateValidRecord()
        {
            var key = RandomString(8, 20);
            var articleCode = RandomString(1, 10);
            var colorCode = RandomString(5, 15);
            var description = RandomString(5, 15);
            var price = Random.Next(0, 10000);
            var discount = Random.Next(0, 10000);
            var deliveredIn = RandomString(5, 15);
            var q1 = RandomString(3, 10);
            var size = Random.Next(0, 200);
            var color = RandomString(3, 10);

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
            return RandomString(1, 100);
        }

        public async Task Generate(string path, int numberOfRecords)
        {
            using var writer = new StreamWriter(path);

            // write header
            await writer.WriteLineAsync("Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color");

            for (int i = 0; i < numberOfRecords; i++)
            {
                var record = Random.NextDouble() > 0.01 ? GenerateValidRecord() : GenerateInvalidRecord();
                await writer.WriteLineAsync(record);
            }
            writer.Flush();
        }
    }
}
