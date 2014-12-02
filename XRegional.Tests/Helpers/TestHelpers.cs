using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using XRegional.Tests.TestSuites.DocDb;
using XRegional.Tests.TestSuites.Table;

namespace XRegional.Tests
{
    public static class TestHelpers
    {
        public const string TableKey = "The-Stars";

        /// <summary>
        /// Generates unique name (for blob containers, files, etc)
        /// </summary>
        public static string GenUnique(string prefix)
        {
            return prefix + DateTime.Now.ToString("yyMMddhhmmssfffff");
        }

        public static StarEntity CreateStarEntity(string rowKey =null)
        {
            return new StarEntity
            {
                PartitionKey = "1",
                RowKey = rowKey ?? "1",
                Name = StarNames[Random.Next(StarNames.Length)],
                Age = Random.Next(int.MaxValue),
                Distance = 1024L * Random.Next(int.MaxValue),
                DtDiscovered = DateTime.SpecifyKind(new DateTime(1982, 11, 4), DateTimeKind.Utc),
                DtUpdated = null,
                Id = Guid.NewGuid(),
                Image = Encoding.UTF8.GetBytes("Image "),
                InMilkyWay = false,
                SurfaceTemperature = Random.NextDouble() * 1000,
            };
        }

        public static List<StarEntity> CreateStarEntities(int count)
        {
            List<StarEntity> entities = new List<StarEntity>();
            for (int i = 0; i < count; ++i)
                entities.Add(CreateStarEntity(i.ToString()));
            return entities;
        }

        public static StarDocument CreateStarDocument(string id = "1")
        {
            return new StarDocument
            {
                Id = id,
                Name = StarNames[Random.Next(StarNames.Length)],
                Age = Random.Next(int.MaxValue),
                Distance = 1024L * Random.Next(int.MaxValue),
                DtDiscovered = DateTime.SpecifyKind(new DateTime(1982, 11, 4), DateTimeKind.Utc),
                DtUpdated = null,
                Image = Encoding.UTF8.GetBytes("Image "),
                InMilkyWay = false,
                SurfaceTemperature = Random.NextDouble() * 1000,
            };
        }

        public static List<StarDocument> CreateStarDocuments(int count)
        {
            List<StarDocument> docs = new List<StarDocument>();
            for (int i = 0; i < count; ++i)
                docs.Add(CreateStarDocument(i.ToString()));
            return docs;
        }

        public static void AssertEqualStars(StarEntity expected, StarEntity actual)
        {
            Assert.AreEqual(expected.PartitionKey, actual.PartitionKey);
            Assert.AreEqual(expected.RowKey, actual.RowKey);
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Age, actual.Age);
            Assert.AreEqual(expected.Distance, actual.Distance);
            Assert.AreEqual(expected.DtDiscovered, actual.DtDiscovered);
            Assert.AreEqual(expected.DtUpdated, actual.DtUpdated);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Image, actual.Image);
            Assert.AreEqual(expected.InMilkyWay, actual.InMilkyWay);
            Assert.AreEqual(expected.SurfaceTemperature, actual.SurfaceTemperature);
        }

        public static void AssertEqualStars(StarDocument expected, StarDocument actual)
        {
            Assert.AreEqual(expected.Version, actual.Version);
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.Age, actual.Age);
            Assert.AreEqual(expected.Distance, actual.Distance);
            Assert.AreEqual(expected.DtDiscovered, actual.DtDiscovered);
            Assert.AreEqual(expected.DtUpdated, actual.DtUpdated);
            Assert.AreEqual(expected.Id, actual.Id);
            Assert.AreEqual(expected.Image, actual.Image);
            Assert.AreEqual(expected.InMilkyWay, actual.InMilkyWay);
            Assert.AreEqual(expected.SurfaceTemperature, actual.SurfaceTemperature);
        }

       

        private static string[] StarNames =
        {
            "Acamar", "Sun", "Alpha Corvi", "Nu Draconis", "Mira", "Rigel", "Vega"
        };

        private static readonly Random Random = new Random();
    }
}
