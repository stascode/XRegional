using System;
using System.Reflection;
using Microsoft.WindowsAzure.Storage.Table;

namespace XRegional.Wrappers
{
    public static class TableConvert
    {
        public static T FromDynamicTableEntity<T>(DynamicTableEntity dynentity)
            where T : TableEntity, new()
        {
            if (dynentity == null)
                return null;

            T entity = new T
            {
                PartitionKey = dynentity.PartitionKey,
                RowKey = dynentity.RowKey,
                ETag = dynentity.ETag,
                Timestamp = dynentity.Timestamp
            };

            foreach (PropertyInfo info in entity.GetType().GetProperties())
            {
                if (IsEntityProperty(info))
                {
                    if (dynentity.Properties.ContainsKey(info.Name))
                    {
                        info.SetValue(entity, FromEntityProperty(dynentity[info.Name], info.PropertyType), null);
                    }
                }
            }

            return entity;
        }

        public static DynamicTableEntity ToDynamicTableEntity(TableEntity entity)
        {
            DynamicTableEntity dynentity
                = new DynamicTableEntity
                {
                    PartitionKey = entity.PartitionKey,
                    RowKey = entity.RowKey,
                    ETag = entity.ETag,
                    Timestamp = entity.Timestamp
                };

            foreach (PropertyInfo info in entity.GetType().GetProperties())
            {
                if (IsEntityProperty(info))
                {
                    var val = info.GetValue(entity, null);
                    EntityProperty property = CreateEntityPropertyFromObject(val, false);
                    if (property != null)
                        dynentity.Properties.Add(info.Name, property);
                }
            }

            return dynentity;
        }

        public static object FromEntityProperty(EntityProperty entityProperty, Type toType = null)
        {
            switch (entityProperty.PropertyType)
            {
                case EdmType.Binary:
                    return entityProperty.BinaryValue;
                case EdmType.Boolean:
                    return entityProperty.BooleanValue;
                case EdmType.DateTime:
                    return typeof(DateTime) == toType || typeof(DateTime?) == toType
                        ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime
                        : (object)entityProperty.DateTimeOffsetValue;
                case EdmType.Double:
                    return entityProperty.DoubleValue;
                case EdmType.Guid:
                    return entityProperty.GuidValue;
                case EdmType.Int32:
                    return entityProperty.Int32Value;
                case EdmType.Int64:
                    return entityProperty.Int64Value;
                case EdmType.String:
                    return entityProperty.StringValue;
                default:
                    return null;
            }
        }

        private static bool IsEntityProperty(PropertyInfo info)
        {
            return ("PartitionKey" != info.Name && "RowKey" != info.Name
                && "Timestamp" != info.Name && "ETag" != info.Name
                && null != info.GetSetMethod() && info.GetSetMethod().IsPublic
                && null != info.GetGetMethod() && info.GetGetMethod().IsPublic);
        }

        private static EntityProperty CreateEntityPropertyFromObject(object value, bool allowUnknownTypes)
        {
            if (value is string)
                return new EntityProperty((string)value);
            if (value is byte[])
                return new EntityProperty((byte[])value);
            if (value is bool)
                return new EntityProperty((bool)value);
            if (value is DateTime)
                return new EntityProperty((DateTime)value);
            if (value is DateTimeOffset)
                return new EntityProperty((DateTimeOffset)value);
            if (value is double)
                return new EntityProperty((double)value);
            if (value is Guid)
                return new EntityProperty((Guid)value);
            if (value is int)
                return new EntityProperty((int)value);
            if (value is long)
                return new EntityProperty((long)value);
            if (value == null)
                return new EntityProperty((string)null);
            if (allowUnknownTypes)
                return new EntityProperty(value.ToString());
            return null;
        }
    }
}
