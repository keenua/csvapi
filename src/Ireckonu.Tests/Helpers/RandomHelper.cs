using Ireckonu.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ireckonu.Tests.Helpers
{
    static class RandomHelper
    {
        const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMONPQRSTUVWXYZ0123456789/";

        public static Random Random { get; } = new Random(Environment.TickCount);

        public static string RandomString(int length)
        {
            var s = new char[length];

            for (int i = 0; i < length; i++)
            {
                s[i] = Alphabet[Random.Next(0, Alphabet.Length)];
            }

            return new string(s);
        }

        public static string RandomString(int minLenght, int maxLenght)
        {
            var length = Random.Next(minLenght, maxLenght);
            return RandomString(length);
        }

        public static Article RandomArticle()
        {
            var article = new Article
            {
                Key = RandomString(10, 20),
                ArticleCode = RandomString(0, 20),
                ColorCode = RandomString(0, 20),
                Description = RandomString(0, 20),
                Price = Random.Next(0, 1000),
                DiscountPrice = Random.Next(0, 1000),
                DeliveredIn = RandomString(0, 20),
                Q1 = RandomString(0, 20),
                Size = Random.Next(0, 1000),
                Color = RandomString(0, 20)
            };

            return article;
        }

        public static IEnumerable<Article> RandomArticles(int count)
        {
            return Enumerable.Range(0, count).Select(_ => RandomArticle());
        }
    }
}
