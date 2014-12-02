using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XRegional.Docdb;
using XRegional.Tests.Common;
using XRegional.Tests.Helpers;
using XRegional.Wrappers;

namespace XRegional.Tests.TestSuites.DocDb
{
    [TestFixture]
    class GatewayQueueTests
    {
        [Test]
        public void PackUnpack()
        {
            var doc = TestHelpers.CreateStarDocument();
            var original = DocdbGatewayMessage.Create("Star", doc);

            var storage = new InMemoryGatewayBlobStore();
            byte[] packed = GatewayPacket.Pack(original, storage);
            var unpacked = GatewayPacket.Unpack<DocdbGatewayMessage>(packed, storage);

            Assert.AreEqual(original.Key, unpacked.Key);

            TestHelpers.AssertEqualStars(
                original.DocumentsAs<StarDocument>().First(),
                unpacked.DocumentsAs<StarDocument>().First()
                );
        }

        [Test]
        public void TestQueue()
        {
            using (var dl = new DisposableList())
            {
                var docs = TestHelpers.CreateStarDocuments(3);
                var original = DocdbGatewayMessage.Create("Star", docs);

                var queue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
                dl.Add(queue.Delete);

                var blobStorage = new InMemoryGatewayBlobStore();
                GatewayQueueWriter writer = new GatewayQueueWriter(queue, blobStorage);
                writer.Write(original);

                GatewayQueueReader reader = new GatewayQueueReader(queue, blobStorage);
                reader.ReadNextMessage<DocdbGatewayMessage>(
                    gm =>
                    {
                        var rdocs = gm.DocumentsAs<StarDocument>().ToList();
                        Assert.AreEqual(docs.Count, rdocs.Count);

                        for (int i = 0; i < rdocs.Count; ++i)
                            TestHelpers.AssertEqualStars(docs[i], rdocs[i]);
                    },
                    (e, gm, cqm) => Assert.Fail());
            }
        }


        [Test]
        public void TestMultiQueue()
        {
            using (var dl = new DisposableList())
            {
                var docs = TestHelpers.CreateStarDocuments(3);
                var original = DocdbGatewayMessage.Create("Star", docs);

                List<QueueWrapper> queues = new List<QueueWrapper>();
                for (int i = 0; i < 3; ++i)
                {
                    var queue = new QueueWrapper(TestHelpers.GenUnique("gateq" + i), TestConfig.GatewayStorageAccount);
                    dl.Add(queue.Delete);
                    queues.Add(queue);
                }

                var blobStorage = new InMemoryGatewayBlobStore();
                GatewayMultiQueueWriter writer = new GatewayMultiQueueWriter(queues, blobStorage);
                writer.Write(original);

                foreach (var queue in queues)
                {
                    GatewayQueueReader reader = new GatewayQueueReader(queue, blobStorage);
                    bool processed = false;
                    reader.ReadNextMessage<DocdbGatewayMessage>(
                        gm =>
                        {
                            var rdocs = gm.DocumentsAs<StarDocument>().ToList();
                            Assert.AreEqual(docs.Count, rdocs.Count);
                            for (int i = 0; i < rdocs.Count; ++i)
                                TestHelpers.AssertEqualStars(docs[i], rdocs[i]);

                            processed = true;
                        },
                        (e, gm, cqm) => Assert.Fail());

                    Assert.IsTrue(processed);
                }
            }
        }

        [Test]
        public void TestQueueBig()
        {
            using (var dl = new DisposableList())
            {
                var docs = TestHelpers.CreateStarDocuments(10000);
                var original = DocdbGatewayMessage.Create("Star", docs);

                var queue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
                var bcont = new BlobContainerWrapper(TestHelpers.GenUnique("gatecont"), TestConfig.GatewayStorageAccount);
                dl.Add(queue.Delete);
                dl.Add(bcont.Delete);

                var blobStorage = new GatewayBlobStore(bcont);
                GatewayQueueWriter writer = new GatewayQueueWriter(queue, blobStorage);
                writer.Write(original);

                GatewayQueueReader reader = new GatewayQueueReader(queue, blobStorage);
                reader.ReadNextMessage<DocdbGatewayMessage>(
                    gm =>
                    {
                        var rdocs = gm.DocumentsAs<StarDocument>().ToList();
                        Assert.AreEqual(docs.Count, rdocs.Count);

                        for (int i = 0; i < rdocs.Count; ++i)
                            TestHelpers.AssertEqualStars(docs[i], rdocs[i]);
                    },
                    (e, gm, cqm) => Assert.Fail());
            }
        }

        [Test]
        public void TestMultiQueueBig()
        {
            using (var dl = new DisposableList())
            {
                var docs = TestHelpers.CreateStarDocuments(10000);
                var original = DocdbGatewayMessage.Create("Star", docs);

                List<QueueWrapper> queues = new List<QueueWrapper>();
                for (int i = 0; i < 3; ++i)
                {
                    var queue = new QueueWrapper(TestHelpers.GenUnique("gateq" + i), TestConfig.GatewayStorageAccount);
                    dl.Add(queue.Delete);
                    queues.Add(queue);
                }

                var blobStorage = new InMemoryGatewayBlobStore();
                GatewayMultiQueueWriter writer = new GatewayMultiQueueWriter(queues, blobStorage);
                writer.Write(original);
                Assert.AreEqual(1, blobStorage.Count);

                foreach (var queue in queues)
                {
                    GatewayQueueReader reader = new GatewayQueueReader(queue, blobStorage);
                    bool processed = false;
                    reader.ReadNextMessage<DocdbGatewayMessage>(
                        gm =>
                        {
                            var rdocs = gm.DocumentsAs<StarDocument>().ToList();
                            Assert.AreEqual(docs.Count, rdocs.Count);
                            for (int i = 0; i < rdocs.Count; ++i)
                                TestHelpers.AssertEqualStars(docs[i], rdocs[i]);

                            processed = true;
                        },
                        (e, gm, cqm) => Assert.Fail());

                    Assert.IsTrue(processed);
                }
            }
        }
    }
}
