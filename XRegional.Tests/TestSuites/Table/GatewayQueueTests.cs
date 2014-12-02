using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XRegional.Table;
using XRegional.Tests.Common;
using XRegional.Tests.Helpers;
using XRegional.Wrappers;

namespace XRegional.Tests.TestSuites.Table
{
    [TestFixture]
    class GatewayQueueTests
    {
        [Test]
        public void PackUnpack()
        {
            var entity = TestHelpers.CreateStarEntity();
            var original = TableGatewayMessage.Create("Person", entity);

            var storage = new InMemoryGatewayBlobStore();
            byte[] packed = GatewayPacket.Pack(original, storage);
            TableGatewayMessage unpacked = GatewayPacket.Unpack<TableGatewayMessage>(packed, storage);

            Assert.AreEqual(original.Key, unpacked.Key);

            TestHelpers.AssertEqualStars(
                original.EntitiesAs<StarEntity>().First(),
                unpacked.EntitiesAs<StarEntity>().First()
                );
        }

        [Test]
        public void TestQueue()
        {
            using (var dl = new DisposableList())
            {
                var entities = TestHelpers.CreateStarEntities(3);
                var original = TableGatewayMessage.Create("Star", entities);

                var queue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
                dl.Add(queue.Delete);

                var blobStorage = new InMemoryGatewayBlobStore();
                GatewayQueueWriter writer = new GatewayQueueWriter(queue, blobStorage);
                writer.Write(original);

                GatewayQueueReader reader = new GatewayQueueReader(queue, blobStorage);
                reader.ReadNextMessage<TableGatewayMessage>(
                    gm =>
                    {
                        var rentities = gm.EntitiesAs<StarEntity>().ToList();
                        Assert.AreEqual(entities.Count, rentities.Count);

                        for (int i = 0; i < rentities.Count; ++i)
                            TestHelpers.AssertEqualStars(entities[i], rentities[i]);
                    },
                    (e, gm, cqm) => Assert.Fail());
            }
        }


        [Test]
        public void TestMultiQueue()
        {
            using (var dl = new DisposableList())
            {
                var entities = TestHelpers.CreateStarEntities(3);
                var original = TableGatewayMessage.Create("Star", entities);

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
                    reader.ReadNextMessage<TableGatewayMessage>(
                        gm =>
                        {
                            var rentities = gm.EntitiesAs<StarEntity>().ToList();
                            Assert.AreEqual(entities.Count, rentities.Count);

                            for (int i = 0; i < rentities.Count; ++i)
                                TestHelpers.AssertEqualStars(entities[i], rentities[i]);

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
                var entities = TestHelpers.CreateStarEntities(10000);
                var original = TableGatewayMessage.Create("Star", entities);

                var queue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
                var bcont = new BlobContainerWrapper(TestHelpers.GenUnique("gatecont"), TestConfig.GatewayStorageAccount);
                dl.Add(queue.Delete);
                dl.Add(bcont.Delete);

                var blobStorage = new GatewayBlobStore(bcont);
                GatewayQueueWriter writer = new GatewayQueueWriter(queue, blobStorage);
                writer.Write(original);

                GatewayQueueReader reader = new GatewayQueueReader(queue, blobStorage);
                reader.ReadNextMessage<TableGatewayMessage>(
                    gm =>
                    {
                        var rentities = gm.EntitiesAs<StarEntity>().ToList();
                        Assert.AreEqual(entities.Count, rentities.Count);

                        for (int i = 0; i < rentities.Count; ++i)
                            TestHelpers.AssertEqualStars(entities[i], rentities[i]);
                    },
                    (e, gm, cqm) => Assert.Fail());
            }
        }

        [Test]
        public void TestMultiQueueBig()
        {
            using (var dl = new DisposableList())
            {
                var entities = TestHelpers.CreateStarEntities(10000);
                var original = TableGatewayMessage.Create("Star", entities);

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
                    reader.ReadNextMessage<TableGatewayMessage>(
                        gm =>
                        {
                            var rentities = gm.EntitiesAs<StarEntity>().ToList();
                            Assert.AreEqual(entities.Count, rentities.Count);

                            for (int i = 0; i < rentities.Count; ++i)
                                TestHelpers.AssertEqualStars(entities[i], rentities[i]);

                            processed = true;
                        },
                        (e, gm, cqm) => Assert.Fail());

                    Assert.IsTrue(processed);
                }
            }
            
        }
    }
}
