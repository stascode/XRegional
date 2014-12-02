using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;
using XRegional.Common;
using XRegional.Wrappers;

namespace XRegional.Table
{
    public class TableGatewayMessage : IGatewayMessage
    {
        public string Key { get; set; }

        public DynamicTableEntity[] Entities { get; set; }

        public TableGatewayMessage()
        {
        }

        public TableGatewayMessage(string key, DynamicTableEntity entity)
            : this(key, new [] { entity })
        {
            Guard.NotNull(entity, "entity");
        }

        public TableGatewayMessage(string key, IEnumerable<DynamicTableEntity> entities)
        {
            Guard.NotNullOrEmpty(key, "key");
            Guard.NotNull(entities, "entities");

            Key = key;
            Entities = entities.ToArray();
        }

        public static TableGatewayMessage Create(string key, TableEntity entity) 
        {
            Guard.NotNullOrEmpty(key, "key");
            Guard.NotNull(entity, "entity");

            DynamicTableEntity dentity = TableConvert.ToDynamicTableEntity(entity);

            return new TableGatewayMessage(key, dentity);
        }

        public static TableGatewayMessage Create(string key, IEnumerable<TableEntity> entities) 
        {
            Guard.NotNullOrEmpty(key, "key");
            Guard.NotNull(entities, "entities");

            IEnumerable<DynamicTableEntity> dentities = entities.Select(TableConvert.ToDynamicTableEntity);

            return new TableGatewayMessage(key, dentities);
        }

        public IEnumerable<T> EntitiesAs<T>() where T : TableEntity, new()
        {
            return Entities.Select(TableConvert.FromDynamicTableEntity<T>);
        }
    }
}