using System.Linq;
using NUnit.Framework;
using XRegional.Table;
using XRegional.Tests.Common;
using XRegional.Tests.Helpers;
using XRegional.Wrappers;

namespace XRegional.Tests.TestSuites.Table
{
    [TestFixture]
    class ConsistencyTests
    {
        [Test]
        public void EntityConsistent()
        {
            using (var dl = new DisposableList())
            {
                // create the source table
                var sourceTableWrapper = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(sourceTableWrapper.Delete);

                // initialize the target table and attach it to the disposable container
                var targetTable = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.SecondaryStorageAccount, true);
                dl.Add(targetTable.Delete);
                var tableParamsResolver = new InMemoryTargetTableResolver();
                tableParamsResolver.Add(TestHelpers.TableKey, targetTable);

                // create gateway blob storage
                var gateBlob = new InMemoryGatewayBlobStore();

                // create a gateway queue
                var gateQueue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
                var gateQueueWriter = new GatewayQueueWriter(gateQueue, gateBlob);

                var sourceTable = new SourceTable<StarEntity>(sourceTableWrapper, gateQueueWriter, TestHelpers.TableKey);

                var entity = TestHelpers.CreateStarEntity();
                
                // write the entity
                sourceTable.Write(entity);

                // now verify that the entity was synced to the secondary table storage
                TableGatewayQueueProcessor gateQueueProcessor = new TableGatewayQueueProcessor(
                    new GatewayQueueReader(gateQueue, gateBlob),
                    tableParamsResolver
                    );

                bool result = gateQueueProcessor.ProcessNext();

                Assert.IsTrue(result);

                var targetEntity = targetTable.ReadEntity<StarEntity>(entity.PartitionKey, entity.RowKey);
                TestHelpers.AssertEqualStars(entity, targetEntity);
            }
        }


        [Test]
        public void BulkOfEntitiesConsistent()
        {
            using (var dl = new DisposableList())
            {
                // create the source table
                var sourceTableWrapper = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(sourceTableWrapper.Delete);

                // initialize the target table and attach it to the disposable container
                var targetTable = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.SecondaryStorageAccount, true);
                dl.Add(targetTable.Delete);
                var tableParamsResolver = new InMemoryTargetTableResolver();
                tableParamsResolver.Add(TestHelpers.TableKey, targetTable);

                // create gateway blob storage
                var gateBlob = new InMemoryGatewayBlobStore();

                // create a gateway queue
                var gateQueue = new QueueWrapper(TestHelpers.GenUnique("gateq"), TestConfig.GatewayStorageAccount);
                var gateQueueWriter = new GatewayQueueWriter(gateQueue, gateBlob);

                var sourceTable = new SourceTable<StarEntity>(sourceTableWrapper, gateQueueWriter, TestHelpers.TableKey);

                // 100 entities to satisfy TableStorage's batch requirements
                var entities = TestHelpers.CreateStarEntities(100);

                sourceTable.Write(entities);

                // Now verify that the entities were synced to the secondary table storage
                TableGatewayQueueProcessor gateQueueProcessor = new TableGatewayQueueProcessor(
                    new GatewayQueueReader(gateQueue, gateBlob),
                    tableParamsResolver
                    );

                bool result = gateQueueProcessor.ProcessNext();

                Assert.IsTrue(result);

                var targetEntities = targetTable.ReadEntities<StarEntity>()
                    .ToList();
                Assert.AreEqual(entities.Count, targetEntities.Count);
                foreach (var entity in entities)
                {
                    TestHelpers.AssertEqualStars(
                        entity,
                        targetEntities.First(x => x.RowKey == entity.RowKey)
                        );
                }
            }
        }
    }
}
