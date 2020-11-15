using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ireckonu.Data.Json
{
    public class JsonDbContext : IDbContext
    {
        private readonly JsonDbSettings _settings;
        
        static SemaphoreSlim dbSemaphore = new SemaphoreSlim(1, 1);

        public JsonDbContext(JsonDbSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrEmpty(settings.FilePath))
            {
                throw new ArgumentNullException(nameof(settings.FilePath));
            }

            if (string.IsNullOrEmpty(settings.SecondaryFilePath))
            {
                throw new ArgumentNullException(nameof(settings.SecondaryFilePath));
            }

            if (string.IsNullOrEmpty(settings.BackupFilePath))
            {
                throw new ArgumentNullException(nameof(settings.BackupFilePath));
            }

            var allFiles = new[]
            {
                settings.FilePath,
                settings.SecondaryFilePath,
                settings.BackupFilePath
            }
            .Select(x => x.ToLower())
            .Distinct();

            if (allFiles.Count() != 3)
            {
                throw new ArgumentException("All file paths should be different", nameof(settings));
            }
        }

        public async Task BulkUpsert(IEnumerable<Article> articles)
        {
            await dbSemaphore.WaitAsync();

            try
            {
                await BulkInsertIntoTempFile(articles).ConfigureAwait(false);
                ExchangeFiles();
            }
            finally
            {
                dbSemaphore.Release();
            }
        }
        
        private async Task BulkInsertIntoTempFile(IEnumerable<Article> articles)
        {
            var byKey = articles.ToDictionary(x => x.Key, x => x);

            using var writer = new ArticleJsonWriter(_settings.SecondaryFilePath);

            if (File.Exists(_settings.FilePath))
            {
                using var reader = new ArticleJsonReader(_settings.FilePath);

                await reader.SkipHeader().ConfigureAwait(false);

                await foreach (var record in reader.Read())
                {
                    if (!byKey.ContainsKey(record.Key))
                    {
                        await writer.Write(record).ConfigureAwait(false);
                    }
                }
            }

            foreach (var article in articles)
            {
                await writer.Write(article).ConfigureAwait(false);
            }

            await writer.WriteFooter().ConfigureAwait(false);
        }

        private void ExchangeFiles()
        {
            if (File.Exists(_settings.FilePath))
            {
                File.Move(_settings.FilePath, _settings.BackupFilePath, true);
            }

            try
            {
                File.Move(_settings.SecondaryFilePath, _settings.FilePath, true);
            }
            catch
            {
                if (File.Exists(_settings.BackupFilePath))
                {
                    File.Move(_settings.BackupFilePath, _settings.FilePath, true);
                }
                throw;
            }

            if (File.Exists(_settings.BackupFilePath))
            {
                File.Delete(_settings.BackupFilePath);
            }
            File.Delete(_settings.SecondaryFilePath);
        }

        public void Dispose()
        {
        }
    }
}
