using Ireckonu.BusinessLogic;
using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;
using Ireckonu.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ireckonu.Tests
{
    [TestFixture]
    public class CsvProcessorTests
    {
        const string CsvHeader = "Key,ArtikelCode,ColorCode,Description,Price,DiscountPrice,DeliveredIn,Q1,Size,Color";
        const string CsvHeaderReordered = "Color,Price,Key,ArtikelCode,ColorCode,Description,DiscountPrice,DeliveredIn,Q1,Size";

        private ICsvProcessor _csvProcessor;

        [SetUp]
        public void Setup()
        {
            var logger = new NullLogger<CsvProcessor>();
            _csvProcessor = new CsvProcessor(logger);
        }

        private async Task<List<RecordProcessingResult>> Test(string header, params string[] records)
        {
            var builder = new StringBuilder();

            var withHeader = !string.IsNullOrEmpty(header);
            
            if (withHeader)
            {
                builder.AppendLine(header);
            }

            foreach (var record in records)
            {
                builder.AppendLine(record);
            }

            var stream = builder.ToString().ToStream();
            var processed = _csvProcessor.Process(stream, withHeader).ConfigureAwait(false);

            var result = new List<RecordProcessingResult>();
            await foreach (var record in processed)
            {
                result.Add(record);
            }
            return result;
        }

        [Test]
        public async Task EmptyFile()
        {
            var actual = await Test("").ConfigureAwait(false);

            Assert.AreEqual(0, actual.Count);
        }

        [Test]
        public async Task OnlyHeader()
        {
            var actual = await Test(CsvHeader).ConfigureAwait(false);

            Assert.AreEqual(0, actual.Count);
        }

        [Test]
        public async Task OneValidRecord()
        {
            var actual = await Test(CsvHeader, "a,0,b,c,1,2,d,e,3,f").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(0, result.Issues.Count);

            var record = result.Record;
            Assert.IsNotNull(record);
            Assert.AreEqual(record.Key, "a");
            Assert.AreEqual(record.ArtikelCode, "0");
            Assert.AreEqual(record.ColorCode, "b");
            Assert.AreEqual(record.Description, "c");
            Assert.AreEqual(record.Price, 1);
            Assert.AreEqual(record.DiscountPrice, 2);
            Assert.AreEqual(record.DeliveredIn, "d");
            Assert.AreEqual(record.Q1, "e");
            Assert.AreEqual(record.Size, 3);
            Assert.AreEqual(record.Color, "f");
        }

        [Test]
        public async Task OneValidRecordWithSomeEmptyValues()
        {
            var actual = await Test(CsvHeader, "a,0,b,,1,2,,e,3,f").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(0, result.Issues.Count);

            var record = result.Record;
            Assert.IsNotNull(record);
            Assert.AreEqual(record.Key, "a");
            Assert.AreEqual(record.ArtikelCode, "0");
            Assert.AreEqual(record.ColorCode, "b");
            Assert.AreEqual(record.Description, "");
            Assert.AreEqual(record.Price, 1);
            Assert.AreEqual(record.DiscountPrice, 2);
            Assert.AreEqual(record.DeliveredIn, "");
            Assert.AreEqual(record.Q1, "e");
            Assert.AreEqual(record.Size, 3);
            Assert.AreEqual(record.Color, "f");
        }


        [Test]
        public async Task WrongColumnCount()
        {
            var actual = await Test(CsvHeader, "a,0,b,c,1,2").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(1, result.Issues.Count);

            var issue = result.Issues[0];
            Assert.AreEqual(Severity.Error, issue.Severity);
            Assert.IsNotEmpty(issue.Text);
        }

        [Test]
        public async Task InvalidValueForInt()
        {
            var actual = await Test(CsvHeader, "a,0,b,c,1,2,d,e,WRONG_VALUE,f").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(1, result.Issues.Count);

            var issue = result.Issues[0];
            Assert.AreEqual(Severity.Error, issue.Severity);
            Assert.IsNotEmpty(issue.Text);
        }

        [Test]
        public async Task UnmatchedQuote()
        {
            var actual = await Test(CsvHeader, "a,0,b,\"c\",1,2,\"d,e,3,f").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsFalse(result.Success);
            Assert.AreEqual(2, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(1, result.Issues.Count);

            var issue = result.Issues[0];
            Assert.AreEqual(Severity.Error, issue.Severity);
            Assert.IsNotEmpty(issue.Text);
        }

        [Test]
        public async Task OneValidRecordWithoutHeader()
        {
            var actual = await Test(header:"", "a,0,b,c,1,2,d,e,3,f").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(0, result.Issues.Count);

            var record = result.Record;
            Assert.IsNotNull(record);
            Assert.AreEqual(record.Key, "a");
            Assert.AreEqual(record.ArtikelCode, "0");
            Assert.AreEqual(record.ColorCode, "b");
            Assert.AreEqual(record.Description, "c");
            Assert.AreEqual(record.Price, 1);
            Assert.AreEqual(record.DiscountPrice, 2);
            Assert.AreEqual(record.DeliveredIn, "d");
            Assert.AreEqual(record.Q1, "e");
            Assert.AreEqual(record.Size, 3);
            Assert.AreEqual(record.Color, "f");
        }

        [Test]
        public async Task OneValidRecordWithReorderedHeader()
        {
            var actual = await Test(header:CsvHeaderReordered, "f,1,a,0,b,c,2,d,e,3").ConfigureAwait(false);

            Assert.AreEqual(1, actual.Count);

            var result = actual[0];
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Line);
            Assert.IsNotNull(result.Issues);
            Assert.AreEqual(0, result.Issues.Count);

            var record = result.Record;
            Assert.IsNotNull(record);
            Assert.AreEqual(record.Key, "a");
            Assert.AreEqual(record.ArtikelCode, "0");
            Assert.AreEqual(record.ColorCode, "b");
            Assert.AreEqual(record.Description, "c");
            Assert.AreEqual(record.Price, 1);
            Assert.AreEqual(record.DiscountPrice, 2);
            Assert.AreEqual(record.DeliveredIn, "d");
            Assert.AreEqual(record.Q1, "e");
            Assert.AreEqual(record.Size, 3);
            Assert.AreEqual(record.Color, "f");
        }

        [Test]
        public async Task MultipleValidRecords()
        {
            var input = new[]
            {
                "00000002groe56,2,broek,Gaastra,8,0,1-3 werkdagen,baby,56,groen",
                "00000002groe122,2,broek,Gaastra,8,0,1-3 werkdagen,baby,122,groen",
                "00000002wit/bcup80,2,broek,Gaastra,8,0,1-3 werkdagen,baby,80,wit",
                "00000002wit/bcup80,2,broek,Gaastra,8,0,1-3 werkdagen,baby,80,wit"
            };

            var actual = await Test(header: CsvHeader, input).ConfigureAwait(false);

            Assert.AreEqual(4, actual.Count);

            for (int i = 0; i < 4; i++)
            {
                var record = actual[i];
                Assert.IsNotNull(record);
                Assert.AreEqual(i + 2, record.Line);
                Assert.IsTrue(record.Success);
                Assert.IsNotNull(record.Issues);
                Assert.AreEqual(0, record.Issues.Count);
            }
        }

        [Test]
        public async Task MultipleInvalidRecords()
        {
            var input = new[]
            {
                "00000002groe56,2,\"broek\",Gaastra,8,0,1-3 werkdagen,baby,56,groen",
                "00000002groe122,2,broek,Gaastra,8,0,1-3 werkdagen,baby,122,groen",
                "INVALID",
                "00000002wit/bcup80,2,broek,Gaastra,8,0,\"1-3 werkdagen\",baby,80,wit",
                "INVALID,AS,WELL"
            };

            var actual = await Test(header: CsvHeader, input).ConfigureAwait(false);

            Assert.AreEqual(5, actual.Count);

            for (int i = 0; i < 5; i++)
            {
                var record = actual[i];

                Assert.IsNotNull(record);
                Assert.AreEqual(i + 2, record.Line);
                Assert.IsNotNull(record.Issues);

                // INVALID 
                if (i == 2 || i == 4)
                {
                    Assert.IsFalse(record.Success);
                    Assert.AreEqual(1, record.Issues.Count);

                    var issue = record.Issues[0];
                    Assert.AreEqual(Severity.Error, issue.Severity);
                    Assert.IsNotEmpty(issue.Text);
                }
                // valid records
                else
                {
                    Assert.IsTrue(record.Success);
                    Assert.AreEqual(0, record.Issues.Count);
                }
            }
        }
    }
}

