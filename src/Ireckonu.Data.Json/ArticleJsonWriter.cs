using Ireckonu.Data.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ireckonu.Data.Json
{
    internal class ArticleJsonWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly StreamWriter _streamWriter;
        private readonly JsonWriter _writer;
        private readonly JsonSerializer _serializer;

        private bool IsFirstRecord { get; set; } = true;

        public ArticleJsonWriter(string filePath)
        {
            _stream = File.Open(filePath, FileMode.Create);
            _streamWriter = new StreamWriter(_stream);
            _writer = new JsonTextWriter(_streamWriter);
            _serializer = new JsonSerializer();
        }

        private async Task WriteHeader()
        {
            await _writer.WriteStartObjectAsync().ConfigureAwait(false);
            await _writer.WritePropertyNameAsync("Articles").ConfigureAwait(false);
            await _writer.WriteStartArrayAsync().ConfigureAwait(false);
        }

        public async Task Write(Article article)
        {
            if (IsFirstRecord)
            {
                await WriteHeader().ConfigureAwait(false);
                IsFirstRecord = false;
            }

            _serializer.Serialize(_writer, article);
            IsFirstRecord = false;
        }

        public async Task WriteFooter()
        {
            await _writer.WriteEndArrayAsync().ConfigureAwait(false);
            await _writer.WriteEndObjectAsync().ConfigureAwait(false);
            await _writer.FlushAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            _writer?.Close();
            _streamWriter?.Dispose();
            _stream?.Dispose();
        }
    }
}
