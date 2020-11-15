using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ireckonu.Data
{
    public class AggregateDbContext : IDbContext
    {
        private readonly ILogger<AggregateDbContext> _logger;
        private readonly IDbContext[] _contexts;

        public AggregateDbContext(IEnumerable<IDbContext> contexts, ILogger<AggregateDbContext> logger)
        {
            _contexts = contexts?.ToArray();
            if (_contexts == null || _contexts.Length == 0)
            {
                throw new ArgumentNullException(nameof(contexts), $"{nameof(contexts)} cannot be null or empty");
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task BulkUpsert(IEnumerable<Article> records)
        {
            var materialized = records.ToList();

            foreach (var context in _contexts)
            {
                try
                {
                    await context.BulkUpsert(materialized);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to bulk upsert into {context.GetType().DeclaringType} storage");
                    throw;
                }
            }
        }

        public void Dispose()
        {
            foreach (var context in _contexts)
            {
                context.Dispose();
            }
        }
    }
}
