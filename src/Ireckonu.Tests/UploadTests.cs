using Ireckonu.BusinessLogic;
using Ireckonu.BusinessLogic.Converters;
using Ireckonu.BusinessLogic.Exceptions;
using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;
using Ireckonu.BusinessLogic.Services;
using Ireckonu.Data;
using Ireckonu.Data.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ireckonu.Tests
{
    [TestFixture]
    public class UploadTests
    {
        private Mock<ICsvProcessor> _csvProcessor;
        private Mock<IValidationService> _validationService;
        private Mock<IDbContext> _db;
        private IUploadService _service;

        [SetUp]
        public void Setup()
        {
            var logger = new NullLogger<UploadService>();
            var converter = new ModelConverter();
            var settings = new UploadServiceSettings { BufferSize = 1000 };

            _csvProcessor = new Mock<ICsvProcessor>();
            
            _validationService = new Mock<IValidationService>();
            _validationService.Setup(x => x.Validate(It.IsAny<CsvRecord>())).ReturnsAsync(new Issue[0]);

            _db = new Mock<IDbContext>();

            _service = new UploadService(_csvProcessor.Object, _validationService.Object, _db.Object, converter, settings, logger);
        }


        void MockCsvProcessorReturn(params RecordProcessingResult[] records)
        {
            async IAsyncEnumerable<RecordProcessingResult> Generate(params RecordProcessingResult[] records)
            {
                foreach (var r in records)
                {
                    yield return r;
                }
            }

            _csvProcessor
                .Setup(x => x.Process(It.IsAny<Stream>(), It.IsAny<bool>()))
                .Returns(Generate(records));
        }

        RecordProcessingResult MockRecordProcessingResult(bool success = true, int line = 1)
        {
            var result = new RecordProcessingResult
            {
                Record = new CsvRecord(),
                Line = line
            };

            if (!success)
            {
                var issue = new Error("Test error");
                result.Issues.Add(issue);
            }

            return result;
        }

        [Test]
        public async Task EmptyFile()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = true,
                MaxRecordsInResponse = 1000
            };

            MockCsvProcessorReturn(new RecordProcessingResult[0]);

            var actual = await _service.Upload(null, config).ConfigureAwait(false);

            Assert.IsTrue(actual.Success);
            Assert.IsNull(actual.Issue);
            Assert.AreEqual(0, actual.LastProcessedLine);
            Assert.IsNotNull(actual.Records);
            Assert.AreEqual(0, actual.Records.Count);
        }

        [Test]
        public async Task ValidRecord()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = true,
                MaxRecordsInResponse = 1000
            };

            var processedRecord = MockRecordProcessingResult();
            MockCsvProcessorReturn(new[] { processedRecord });

            var actual = await _service.Upload(null, config).ConfigureAwait(false);

            Assert.IsTrue(actual.Success);
            Assert.IsNull(actual.Issue);
            Assert.AreEqual(1, actual.LastProcessedLine);
            Assert.IsNotNull(actual.Records);
            Assert.AreEqual(1, actual.Records.Count);
            Assert.IsTrue(actual.Records[0].Success);
        }

        [Test]
        public async Task InvalidRecord()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = true,
                MaxRecordsInResponse = 1000
            };

            var failedRecord = MockRecordProcessingResult(success:false);
            MockCsvProcessorReturn(new[] { failedRecord });

            var actual = await _service.Upload(null, config).ConfigureAwait(false);

            Assert.IsTrue(actual.Success);
            Assert.IsNull(actual.Issue);
            Assert.AreEqual(1, actual.LastProcessedLine);
            Assert.IsNotNull(actual.Records);
            Assert.AreEqual(1, actual.Records.Count);
            Assert.IsFalse(actual.Records[0].Success);
        }

        [Test]
        public async Task RespectMaxRecordsInResponse()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = true,
                MaxRecordsInResponse = 5
            };

            var records = Enumerable
                .Range(0, 100)
                .Select(i => MockRecordProcessingResult(success: true, line: i + 1))
                .ToArray();
            MockCsvProcessorReturn(records);

            var actual = await _service.Upload(null, config).ConfigureAwait(false);

            Assert.IsTrue(actual.Success);
            Assert.IsNull(actual.Issue);
            Assert.IsNotNull(actual.Records);
            Assert.AreEqual(5, actual.Records.Count);
        }

        [Test]
        public async Task RespectReportSuccessForRecords()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = false,
                MaxRecordsInResponse = 5
            };

            var records = Enumerable
                .Range(0, 100)
                .Select(i => MockRecordProcessingResult(success: i > 0, line: i + 1))
                .ToArray();
            MockCsvProcessorReturn(records);

            var actual = await _service.Upload(null, config).ConfigureAwait(false);

            Assert.IsTrue(actual.Success);
            Assert.IsNull(actual.Issue);
            Assert.AreEqual(100, actual.LastProcessedLine);
            Assert.IsNotNull(actual.Records);
            Assert.AreEqual(1, actual.Records.Count);
        }

        [Test]
        public async Task GeneralFailure()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = false,
                MaxRecordsInResponse = 5
            };

            MockCsvProcessorReturn(new RecordProcessingResult[0]);

            var exception = new Exception("Exception message with credentials");
            _db.Setup(x => x.BulkUpsert(It.IsAny<IEnumerable<Article>>())).ThrowsAsync(exception);

            var actual = await _service.Upload(null, config).ConfigureAwait(false);
            Assert.IsFalse(actual.Success);
            Assert.IsNotNull(actual.Issue);
            Assert.IsNotEmpty(actual.Issue.Text);

            Assert.IsFalse(actual.Issue.Text.Contains("credentials"));
        }

        [Test]
        public async Task BusinessFailure()
        {
            var config = new UploadConfiguration
            {
                ReportSuccessForRecords = false,
                MaxRecordsInResponse = 5
            };

            MockCsvProcessorReturn(new RecordProcessingResult[0]);

            var exception = new BusinessException("Exception message with business reason");
            _db.Setup(x => x.BulkUpsert(It.IsAny<IEnumerable<Article>>())).ThrowsAsync(exception);

            var actual = await _service.Upload(null, config).ConfigureAwait(false);
            Assert.IsFalse(actual.Success);
            Assert.IsNotNull(actual.Issue);
            Assert.IsNotEmpty(actual.Issue.Text);

            Assert.IsTrue(actual.Issue.Text.Contains("business reason"));
        }
    }
}
