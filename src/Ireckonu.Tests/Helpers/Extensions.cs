using Ireckonu.Data.Models;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ireckonu.Tests.Helpers
{
    public static class Extensions
    {
        public static Stream ToStream(this string csv)
        {
            var bytes = Encoding.UTF8.GetBytes(csv);
            var stream = new MemoryStream(bytes);
            return stream;
        }

        public static Article Clone(this Article article)
        {
            if (article == null)
            {
                return null;
            }

            var clone = new Article
            {
                Key = article.Key,
                ArticleCode = article.ArticleCode,
                ColorCode = article.ColorCode,
                Description = article.Description,
                Price = article.Price,
                DiscountPrice = article.DiscountPrice,
                DeliveredIn = article.DeliveredIn,
                Q1 = article.Q1,
                Size = article.Size,
                Color = article.Color
            };

            return clone;
        }

        public static void IsEqualTo(this Article actual, Article expected)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
            }

            Assert.AreEqual(expected.Key, actual.Key);
            Assert.AreEqual(expected.ArticleCode, actual.ArticleCode);
            Assert.AreEqual(expected.ColorCode, actual.ColorCode);
            Assert.AreEqual(expected.Description, actual.Description);
            Assert.AreEqual(expected.Price, actual.Price);
            Assert.AreEqual(expected.DiscountPrice, actual.DiscountPrice);
            Assert.AreEqual(expected.DeliveredIn, actual.DeliveredIn);
            Assert.AreEqual(expected.Q1, actual.Q1);
            Assert.AreEqual(expected.Size, actual.Size);
            Assert.AreEqual(expected.Color, actual.Color);
        }

        public static void IsEqualTo(this IEnumerable<Article> actual, IEnumerable<Article> expected)
        {
            if (expected == null)
            {
                Assert.IsNull(actual);
            }
            else
            {
                Assert.IsNotNull(actual);
            }

            var materializedActual = actual.ToList();
            var materializedExpected = expected.ToList();

            Assert.AreEqual(materializedExpected.Count, materializedActual.Count);

            var actualByKey = materializedActual.ToDictionary(x => x.Key, x => x);

            foreach (var e in expected)
            {
                Assert.IsTrue(actualByKey.ContainsKey(e.Key));
                actualByKey[e.Key].IsEqualTo(e);
            }
        }

    }
}
