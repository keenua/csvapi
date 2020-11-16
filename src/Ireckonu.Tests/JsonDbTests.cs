using Ireckonu.Data;
using Ireckonu.Data.Json;
using Ireckonu.Data.Models;
using Ireckonu.Tests.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ireckonu.Tests
{
    [TestFixture]
    public class JsonDbTests
    {
        private IDbContext _db;

        const string TestFile = "test_db.json";
        const string BackupFile = "test_backup.json";
        const string SecondaryFile = "test_secondary.json";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(TestFile)) File.Delete(TestFile);
            if (File.Exists(BackupFile)) File.Delete(BackupFile);
            if (File.Exists(SecondaryFile)) File.Delete(SecondaryFile);

            var settings = new JsonDbSettings 
            {
                FilePath = TestFile,
                SecondaryFilePath = SecondaryFile,
                BackupFilePath = BackupFile
            };

            _db = new JsonDbContext(settings);
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(TestFile)) File.Delete(TestFile);
            if (File.Exists(BackupFile)) File.Delete(BackupFile);
            if (File.Exists(SecondaryFile)) File.Delete(SecondaryFile);
        }

        class ArticleCollection
        {
            public List<Article> Articles { get; set; }
        }

        [Test]
        public async Task SingleRecord()
        {
            var article = RandomHelper.RandomArticle();

            await _db.BulkUpsert(new[] { article.Clone() }).ConfigureAwait(false);

            Assert.IsFalse(File.Exists(BackupFile));
            Assert.IsFalse(File.Exists(SecondaryFile));
            Assert.IsTrue(File.Exists(TestFile));

            var json = File.ReadAllText(TestFile);
            var articles = JsonConvert.DeserializeObject<ArticleCollection>(json);

            Assert.IsNotNull(articles);
            Assert.IsNotNull(articles.Articles);
            Assert.AreEqual(1, articles.Articles.Count);

            articles.Articles[0].IsEqualTo(article);
        }

        [Test]
        public async Task MultipleRecords()
        {
            var expected = RandomHelper.RandomArticles(50).ToList();

            var input = expected.Select(a => a.Clone());
            await _db.BulkUpsert(input).ConfigureAwait(false);

            Assert.IsFalse(File.Exists(BackupFile));
            Assert.IsFalse(File.Exists(SecondaryFile));
            Assert.IsTrue(File.Exists(TestFile));

            var json = File.ReadAllText(TestFile);
            var articles = JsonConvert.DeserializeObject<ArticleCollection>(json);

            Assert.IsNotNull(articles);
            Assert.IsNotNull(articles.Articles);
            articles.Articles.IsEqualTo(expected);
        }

        [Test]
        public async Task UpdateSameAndAddMore()
        {
            var expected = RandomHelper.RandomArticles(50).ToList();

            var input = expected.Select(a => a.Clone());
            await _db.BulkUpsert(input).ConfigureAwait(false);

            // Modify existing
            foreach (var e in expected)
            {
                e.Color = RandomHelper.RandomString(0, 20);
                e.Size = RandomHelper.Random.Next(0, 1000);
            }
            // Add 50 new
            expected = expected.Concat(RandomHelper.RandomArticles(50)).ToList();
            
            var input2 = expected.Select(a => a.Clone());
            await _db.BulkUpsert(input2).ConfigureAwait(false);

            Assert.IsFalse(File.Exists(BackupFile));
            Assert.IsFalse(File.Exists(SecondaryFile));
            Assert.IsTrue(File.Exists(TestFile));

            var json = File.ReadAllText(TestFile);
            var articles = JsonConvert.DeserializeObject<ArticleCollection>(json);

            Assert.IsNotNull(articles);
            Assert.IsNotNull(articles.Articles);
            articles.Articles.IsEqualTo(expected);
        }
    }
}
