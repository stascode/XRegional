using System;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;

namespace XRegional.Tests
{
    class TestConfig
    {
        static TestConfig()
        {
            GatewayStorageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["Gateway.StorageAccount"]
                );
            PrimaryStorageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["Table.Primary.StorageAccount"]
                );
            SecondaryStorageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings["Table.Secondary.StorageAccount"]
                );

            DocDbPrimaryUri = new Uri(ConfigurationManager.AppSettings["DocDb.Primary.Uri"]);
            DocDbPrimaryAuthKey = ConfigurationManager.AppSettings["DocDb.Primary.AuthKey"];
            DocDbPrimaryDatabaseId = ConfigurationManager.AppSettings["DocDb.Primary.DatabaseId"];

            DocDbSecondaryUri = new Uri(ConfigurationManager.AppSettings["DocDb.Secondary.Uri"]);
            DocDbSecondaryAuthKey = ConfigurationManager.AppSettings["DocDb.Secondary.AuthKey"];
            DocDbSecondaryDatabaseId = ConfigurationManager.AppSettings["DocDb.Secondary.DatabaseId"];
        }

        public static CloudStorageAccount GatewayStorageAccount { get; private set; }
        public static CloudStorageAccount PrimaryStorageAccount { get; private set; }
        public static CloudStorageAccount SecondaryStorageAccount { get; private set; }

        public const string TableName = "Stars";


        public static Uri DocDbPrimaryUri { get; private set; }
        public static string DocDbPrimaryAuthKey { get; private set; }
        public static string DocDbPrimaryDatabaseId { get; private set; }
        public static Uri DocDbSecondaryUri { get; private set; }
        public static string DocDbSecondaryAuthKey { get; private set; }
        public static string DocDbSecondaryDatabaseId { get; private set; }
    }
}
