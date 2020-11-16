using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;
using Ireckonu.BusinessLogic.Services;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Ireckonu.Tests
{
    [TestFixture]
    public class ValidationTests
    {
        private IValidationService _service;

        [SetUp]
        public void Setup()
        {
            _service = new ValidationService();
        }

        [Test]
        public async Task EmptyKey()
        {
            var record = new CsvRecord
            {
                Key = ""
            };

            var issues = await _service.Validate(record).ConfigureAwait(false);
            var actual = issues?.ToList();

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);

            var issue = actual[0];
            Assert.AreEqual(Severity.Error, issue.Severity);
            Assert.AreEqual("Key cannot be null or empty", issue.Text);
        }

        [Test]
        public async Task NullKey()
        {
            var record = new CsvRecord
            {
                Key = null
            };

            var issues = await _service.Validate(record).ConfigureAwait(false);
            var actual = issues?.ToList();

            Assert.IsNotNull(actual);
            Assert.AreEqual(1, actual.Count);

            var issue = actual[0];
            Assert.AreEqual(Severity.Error, issue.Severity);
            Assert.AreEqual("Key cannot be null or empty", issue.Text);
        }

        [Test]
        public async Task ValidKey()
        {
            var record = new CsvRecord
            {
                Key = "abc"
            };

            var issues = await _service.Validate(record).ConfigureAwait(false);

            Assert.IsNotNull(issues);
            Assert.IsFalse(issues.Any());
        }
    }
}
