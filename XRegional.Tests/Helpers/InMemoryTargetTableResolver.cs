using System;
using System.Collections.Generic;
using XRegional.Table;
using XRegional.Wrappers;

namespace XRegional.Tests.Helpers
{
    class InMemoryTargetTableResolver : ITargetTableResolver
    {
        private readonly Dictionary<string, TargetTable> _map =
            new Dictionary<string, TargetTable>();

        public InMemoryTargetTableResolver()
        {
        }

        public void Add(string key, TableWrapper table)
        {
            _map.Add(key, new TargetTable(table));
        }

        public TargetTable Resolve(string key)
        {
            if (_map.ContainsKey(key))
                return _map[key];

            return null;
        }
    }
}
