using System;
using XRegional.Table;

namespace XRegional.Tests.TestSuites.Table
{
    public class StarEntity : VersionedTableEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public long Distance { get; set; }
        public DateTime DtDiscovered { get; set; }
        public DateTime? DtUpdated { get; set; }
        public Guid Id { get; set; }
        public byte[] Image { get; set; }
        public bool InMilkyWay { get; set; }
        public double SurfaceTemperature { get; set; }
    }
}
