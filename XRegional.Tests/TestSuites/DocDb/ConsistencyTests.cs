using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NUnit.Framework;
using XRegional.Docdb;
using XRegional.Tests.Helpers;
using XRegional.Wrappers;

namespace XRegional.Tests.TestSuites.DocDb
{
    [TestFixture]
    class ConsistencyTests
    {
        private DocumentClient _primaryClient;
        private DocumentCollection _primaryCollection;
        private DocumentClient _secondaryClient;
        private DocumentCollection _secondaryCollection;

        [SetUp]
        public void SetUp()
        {
            {
                var client = new DocumentClient(TestConfig.DocDbPrimaryUri, TestConfig.DocDbPrimaryAuthKey);
                var database = client.CreateDatabaseQuery()
                    .Where(db => db.Id == TestConfig.DocDbPrimaryDatabaseId)
                    .AsEnumerable()
                    .FirstOrDefault();
                Assert.NotNull(database);
                // async SetUps are not supported yet
                var task = client.CreateDocumentCollectionAsync(
                    database.SelfLink,
                    new DocumentCollection {Id = TestHelpers.GenUnique("Stars")}
                    );
                task.Wait();
                DocumentCollection collection = task.Result.Resource;
                Assert.NotNull(collection);

                _primaryClient = client;
                _primaryCollection = collection;
            }

            {
                var client = new DocumentClient(TestConfig.DocDbSecondaryUri, TestConfig.DocDbSecondaryAuthKey);
                var database = client.CreateDatabaseQuery()
                    .Where(db => db.Id == TestConfig.DocDbSecondaryDatabaseId)
                    .AsEnumerable()
                    .FirstOrDefault();
                Assert.NotNull(database);
                // async SetUps are not supported yet
                var task = client.CreateDocumentCollectionAsync(
                    database.SelfLink,
                    new DocumentCollection {Id = TestHelpers.GenUnique("Stars")}
                    );
                task.Wait();
                DocumentCollection collection = task.Result.Resource;
                Assert.NotNull(collection);

                _secondaryClient = client;
                _secondaryCollection = collection;
            }

        }

        [TearDown]
        public void TearDown()
        {
            if (_primaryCollection != null)
                _primaryClient.DeleteDocumentCollectionAsync(_primaryCollection.SelfLink).Wait();
            if (_primaryClient != null)
                _primaryClient.Dispose();

            if (_secondaryCollection != null)
                _secondaryClient.DeleteDocumentCollectionAsync(_secondaryCollection.SelfLink).Wait();
            if (_secondaryClient != null)
                _secondaryClient.Dispose();

            _primaryCollection = null;
            _primaryClient = null;
            _secondaryCollection = null;
            _secondaryClient = null;
        }

        [Test]
        public void DocumentConsistent()
        {
            const string gatewayKey = "Stars";

            // create gateway blob storage
            var gateBlob = new InMemoryGatewayBlobStore();

            // create a gateway queue
            var gateQueue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
            var gateQueueWriter = new GatewayQueueWriter(gateQueue, gateBlob);

            var doc = TestHelpers.CreateStarDocument();

            SourceCollection scol = new SourceCollection(_primaryClient, _primaryCollection, gateQueueWriter, gatewayKey);
            DocdbGatewayQueueProcessor gateQueueProcessor = new DocdbGatewayQueueProcessor(
                new GatewayQueueReader(gateQueue, gateBlob),
                new FixedTargetCollectionResolver(_secondaryClient, _secondaryCollection)
                );
            var tcol = new TargetCollection(_secondaryClient, _secondaryCollection);

            scol.Write(doc);
            Assert.IsTrue(gateQueueProcessor.ProcessNext());
                
            var tdoc = tcol.ReadDocument<StarDocument>(doc.Id);
            TestHelpers.AssertEqualStars(doc, tdoc);
            Assert.AreEqual(1, tdoc.Version);

            // same Id but different data in the object
            doc = TestHelpers.CreateStarDocument();
            scol.Write(doc);
            Assert.AreEqual(2, doc.Version); // verify the side effect of setting Version
            Assert.IsTrue(gateQueueProcessor.ProcessNext());
            tdoc = tcol.ReadDocument<StarDocument>(doc.Id);
            TestHelpers.AssertEqualStars(doc, tdoc);
            Assert.AreEqual(2, tdoc.Version);
        }
    }
}
