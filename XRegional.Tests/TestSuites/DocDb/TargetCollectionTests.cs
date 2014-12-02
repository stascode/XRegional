using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NUnit.Framework;
using XRegional.Docdb;
using XRegional.Serializers;

namespace XRegional.Tests.TestSuites.DocDb
{
    [TestFixture]
    class TargetDocDbTests
    {
        private DocumentClient _client;
        private DocumentCollection _collection;

        [SetUp]
        public void SetUp()
        {
            DocumentClient client = new DocumentClient(TestConfig.DocDbPrimaryUri, TestConfig.DocDbPrimaryAuthKey);
            Database database = client.CreateDatabaseQuery()
                .Where(db => db.Id == TestConfig.DocDbPrimaryDatabaseId)
                .AsEnumerable()
                .FirstOrDefault();
            Assert.NotNull(database);

            // async SetUps are not supported yet
            var task = client.CreateDocumentCollectionAsync(
                    database.SelfLink,
                    new DocumentCollection { Id = TestHelpers.GenUnique("Stars") }
                    );
            task.Wait();
            DocumentCollection collection = task.Result.Resource;
            Assert.NotNull(collection);

            _client = client;
            _collection = collection;
        }

        [TearDown]
        public void TearDown()
        {
            if (_collection != null)
                _client.DeleteDocumentCollectionAsync(_collection.SelfLink).Wait();
            if (_client != null)
                _client.Dispose();

            _collection = null;
            _client = null;
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            var doc = TestHelpers.CreateStarDocument();

            byte[] packed = ZipCompressor.Compress(doc);

            // tricky casting through dynamic
            StarDocument unpacked = (StarDocument)(dynamic)ZipCompressor.Uncompress<Document>(packed);

            TestHelpers.AssertEqualStars(doc, unpacked);
        }

        [Test]
        public void TestDiscarding()
        {
            var doc1 = TestHelpers.CreateStarDocument();
            var doc2 = TestHelpers.CreateStarDocument();
            doc2.Version++;
            doc2.Name = "Document Version 2 name";
            var doc3 = TestHelpers.CreateStarDocument();
            doc3.Name = "Document Version 3 name";

            StarDocument pp;
            XCollectionResult result;
            TargetCollection tcol = new TargetCollection(_client, _collection);

            result = tcol.Write(doc1);
            Assert.IsFalse(result.Discarded);
            pp = tcol.ReadDocument<StarDocument>(doc1.Id);
            TestHelpers.AssertEqualStars(doc1, pp);

            result = tcol.Write(doc2);
            Assert.IsFalse(result.Discarded);
            pp = tcol.ReadDocument<StarDocument>(doc2.Id);
            TestHelpers.AssertEqualStars(doc2, pp);

            result = tcol.Write(doc3);
            Assert.IsTrue(result.Discarded);
            pp = tcol.ReadDocument<StarDocument>(doc3.Id);
            TestHelpers.AssertEqualStars(doc2, pp);

            int retryAttempt = 0;
            result = tcol.Write(doc1,
                () =>
                {
                    if (retryAttempt++ == 0)
                    {
                        result = tcol.Write(doc2);
                        Assert.IsFalse(result.Discarded);
                    }
                }
                );
            Assert.IsTrue(result.Discarded);
        }

        [Test]
        public void TestETagViolation409()
        {
            var doc1 = TestHelpers.CreateStarDocument();
            var doc2 = TestHelpers.CreateStarDocument();
            doc2.Version++;
            doc2.Name = "Document Version 2 name";

            XCollectionResult result;
            TargetCollection tcol = new TargetCollection(_client, _collection);
            int retryAttempt = 0;
            result = tcol.Write(doc1,
                () =>
                {
                    if (retryAttempt++ == 0)
                    {
                        result = tcol.Write(doc2);
                        Assert.IsFalse(result.Discarded);
                    }
                }
                );
            Assert.IsTrue(result.Discarded);

            var rdoc = tcol.ReadDocument<StarDocument>(doc1.Id);
            TestHelpers.AssertEqualStars(doc2, rdoc);
        }


        [Test]
        public void TestETagViolation412()
        {
            var doc1 = TestHelpers.CreateStarDocument();
            var doc2 = TestHelpers.CreateStarDocument();
            doc2.Version = 2;
            doc2.Name = "Doc Version 2 name";
            var doc3 = TestHelpers.CreateStarDocument();
            doc3.Version = 3;
            doc3.Name = "Doc Version 3 name";

            XCollectionResult result;
            TargetCollection tcol = new TargetCollection(_client, _collection);

            result = tcol.Write(doc1);
            Assert.IsFalse(result.Discarded);

            // Test ETag violation as HTTP code 412
            int retryAttempt = 0;
            result = tcol.Write(doc2,
                () =>
                {
                    if (retryAttempt++ == 0) {
                        result = tcol.Write(doc3);
                        Assert.IsFalse(result.Discarded);
                    }
                }
                );
            Assert.IsTrue(result.Discarded);

            var rdoc = tcol.ReadDocument<StarDocument>(doc1.Id);
            TestHelpers.AssertEqualStars(doc3, rdoc);
        }

        [Test]
        public void TestETagViolation412WithSuccessfulRetry()
        {
            var doc1 = TestHelpers.CreateStarDocument();
            var doc2 = TestHelpers.CreateStarDocument();
            doc2.Version = 2;
            doc2.Name = "Document Version 2 name";
            var doc3 = TestHelpers.CreateStarDocument();
            doc3.Version = 3;
            doc3.Name = "Document Version 3 name";

            TargetCollection tcol = new TargetCollection(_client, _collection);
            XCollectionResult result;
            StarDocument rdoc;

            result = tcol.Write(doc1);
            Assert.IsFalse(result.Discarded);

            // Test ETag violation as HTTP code 412
            int retryAttempt = 0;
            result = tcol.Write(doc3,
                () =>
                {
                    if (retryAttempt++ == 0) {
                        result = tcol.Write(doc2);
                        Assert.IsFalse(result.Discarded);

                        rdoc = tcol.ReadDocument<StarDocument>(doc2.Id);
                        TestHelpers.AssertEqualStars(doc2, rdoc);
                    }
                }
                );
            Assert.IsFalse(result.Discarded);

            rdoc = tcol.ReadDocument<StarDocument>(doc3.Id);
            TestHelpers.AssertEqualStars(doc3, rdoc);
        }
    }
}
