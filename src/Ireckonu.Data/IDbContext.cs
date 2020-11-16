using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ireckonu.Data.Models;

namespace Ireckonu.Data
{
    public interface IDbContext : IDisposable
    {
        Task BulkUpsert(IEnumerable<Article> records);
    }
}
