using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using XRegional.Table;
using XRegional.Tests.Common;
using XRegional.Wrappers;

namespace XRegional.Tests.TestSuites.Table
{
    [TestFixture]
    class TargetTableTests
    {
        [Test]
        public void TestWriteBulk()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);
                
                List<DynamicTableEntity> entities = new List<DynamicTableEntity>();
                for (int i = 0; i < 350; ++i)
                {
                    entities.Add(
                        TableConvert.ToDynamicTableEntity(
                            TestHelpers.CreateStarEntity(i.ToString())
                        ));
                }

                TargetTable ttable = new TargetTable(table);
                var results = ttable.Write(entities);
                Assert.AreEqual(entities.Count, results.Count);
                Assert.IsTrue(results.All(x => !x.Discarded));
            }
        }

        [Test]
        public void TestWriteBulkDifferentPartitionKeys()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);

                List<DynamicTableEntity> entities = new List<DynamicTableEntity>();
                for (int i = 0; i < 350; ++i)
                {
                    var entity = TestHelpers.CreateStarEntity(i.ToString());
                    entity.PartitionKey = i.ToString();
                    entities.Add(TableConvert.ToDynamicTableEntity(entity));
                }

                TargetTable ttable = new TargetTable(table);
                Assert.Throws<ArgumentException>(() => ttable.Write(entities));
            }
        }

        [Test]
        public void TestWriteBulkSameRowKeys()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);

                List<DynamicTableEntity> entities = new List<DynamicTableEntity>();
                for (int i = 0; i < 350; ++i)
                {
                    var entity = TestHelpers.CreateStarEntity("same");
                    entities.Add(TableConvert.ToDynamicTableEntity(entity));
                }

                TargetTable ttable = new TargetTable(table);
                Assert.Throws<ArgumentException>(() => ttable.Write(entities));
            }
        }

        [Test]
        public void TestDiscarding()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);

                var entity1 = TestHelpers.CreateStarEntity();
                var entity2 = TestHelpers.CreateStarEntity();
                entity2.Version++;
                entity2.Name = "Entity Version 2 name";
                var entity3 = TestHelpers.CreateStarEntity();
                entity3.Name = "Entity Version 3 name";

                TargetTable ttable = new TargetTable(table);
                XTableResult result;
                StarEntity rentity;

                result = ttable.Write(TableConvert.ToDynamicTableEntity(entity1));
                Assert.IsFalse(result.Discarded);
                rentity = table.ReadEntity<StarEntity>(entity3.PartitionKey, entity3.RowKey);
                TestHelpers.AssertEqualStars(entity1, rentity);

                result = ttable.Write(TableConvert.ToDynamicTableEntity(entity2));
                Assert.IsFalse(result.Discarded);
                rentity = table.ReadEntity<StarEntity>(entity3.PartitionKey, entity3.RowKey);
                TestHelpers.AssertEqualStars(entity2, rentity);

                result = ttable.Write(TableConvert.ToDynamicTableEntity(entity3));
                Assert.IsTrue(result.Discarded);
                rentity = table.ReadEntity<StarEntity>(entity3.PartitionKey, entity3.RowKey);
                TestHelpers.AssertEqualStars(entity2, rentity);
            }
        }

        [Test]
        public void TestETagViolation409()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);

                var entity1 = TestHelpers.CreateStarEntity();
                entity1.Version = 1;
                var entity2 = TestHelpers.CreateStarEntity();
                entity2.Version = 2;
                entity2.Name = "Entity Version 2 name";

                TargetTable ttable = new TargetTable(table);
                XTableResult result;

                int retryAttempt = 0;
                result = ttable.Write(
                    TableConvert.ToDynamicTableEntity(entity1),
                    () => {
                        if (retryAttempt++ == 0) {
                            result = ttable.Write(TableConvert.ToDynamicTableEntity(entity2));
                            Assert.AreEqual(false, result.Discarded);
                        }
                    }
                    );
                Assert.AreEqual(true, result.Discarded);

                var rentity = table.ReadEntity<StarEntity>(entity1.PartitionKey, entity1.RowKey);
                TestHelpers.AssertEqualStars(entity2, rentity);
            }
        }

        [Test]
        public void TestETagViolation412()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);

                var entity1 = TestHelpers.CreateStarEntity();
                entity1.Version = 1;
                var entity2 = TestHelpers.CreateStarEntity();
                entity2.Version = 2;
                entity2.Name = "Entity Version 2 name";
                var entity3 = TestHelpers.CreateStarEntity();
                entity3.Version = 3;
                entity3.Name = "Entity Version 3 name";

                TargetTable ttable = new TargetTable(table);
                XTableResult result;

                result = ttable.Write(TableConvert.ToDynamicTableEntity(entity1));
                Assert.IsFalse(result.Discarded);

                // Test ETag violation as HTTP code 412
                int retryAttempt = 0;
                result = ttable.Write(
                    TableConvert.ToDynamicTableEntity(entity2),
                    () => {
                        if (retryAttempt++ == 0) {
                            result = ttable.Write(TableConvert.ToDynamicTableEntity(entity3));
                            Assert.IsFalse(result.Discarded);
                        }
                    }
                    );
                Assert.IsTrue(result.Discarded);

                var rentity = table.ReadEntity<StarEntity>(entity3.PartitionKey, entity3.RowKey);
                TestHelpers.AssertEqualStars(entity3, rentity);
            }
        }

        [Test]
        public void TestETagViolation412WithSuccessfulRetry()
        {
            using (var dl = new DisposableList())
            {
                var table = new TableWrapper(TestHelpers.GenUnique(TestConfig.TableName), TestConfig.PrimaryStorageAccount, true);
                dl.Add(table.Delete);

                var entity1 = TestHelpers.CreateStarEntity();
                entity1.Version = 1;
                var entity2 = TestHelpers.CreateStarEntity();
                entity2.Version = 2;
                entity2.Name = "Entity Version 2 name";
                var entity3 = TestHelpers.CreateStarEntity();
                entity3.Version = 3;
                entity3.Name = "Entity Version 3 name";

                TargetTable ttable = new TargetTable(table);
                XTableResult result;
                StarEntity rentity;

                result = ttable.Write(TableConvert.ToDynamicTableEntity(entity1));
                Assert.AreEqual(false, result.Discarded);

                // Test ETag violation as HTTP code 412
                int retryAttempt = 0;
                result = ttable.Write(
                    TableConvert.ToDynamicTableEntity(entity3),
                    () => {
                        if (retryAttempt++ == 0) {
                            result = ttable.Write(TableConvert.ToDynamicTableEntity(entity2));
                            Assert.IsFalse(result.Discarded);

                            rentity = table.ReadEntity<StarEntity>(entity2.PartitionKey, entity2.RowKey);
                            TestHelpers.AssertEqualStars(entity2, rentity);
                        }
                    }
                    );
                Assert.IsFalse(result.Discarded);

                rentity = table.ReadEntity<StarEntity>(entity3.PartitionKey, entity3.RowKey);
                TestHelpers.AssertEqualStars(entity3, rentity);
            }
        }
    }
}
