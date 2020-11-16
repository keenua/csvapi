using Ireckonu.Data.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ireckonu.Data.Json
{
    internal class ArticleJsonReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly StreamReader _streamReader;
        private readonly JsonReader _reader;
        private readonly JsonSerializer _serializer;

        public ArticleJsonReader(string filePath)
        {
            _stream = File.Open(filePath, FileMode.Open);
            _streamReader = new StreamReader(_stream);
            _reader = new JsonTextReader(_streamReader);
            _serializer = new JsonSerializer();
        }

        public async Task SkipHeader()
        {
            while (await _reader.ReadAsync().ConfigureAwait(false))
            {
                if (_reader.TokenType == JsonToken.StartArray)
                {
                    break;
                }
            }
        }

        public async IAsyncEnumerable<Article> Read()
        {
            while (await _reader.ReadAsync().ConfigureAwait(false))
            {
                if (_reader.TokenType == JsonToken.StartObject)
                {
                    var record = _serializer.Deserialize<Article>(_reader);
                    if (record != null)
                    {
                        yield return record;
                    }
                }
            }
        }

        public void Dispose()
        {
            try
            {
                _reader?.Close();
            }
            catch { }

            try
            {
                _streamReader?.Dispose();
            }
            catch { }

            try
            {
                _stream?.Dispose();
            }
            catch { }
        }
    }
}
