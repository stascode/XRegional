using System;
using Microsoft.Azure.Documents;

namespace XRegional.Tests.TestSuites.DocDb
{
    public class StarDocument : Document
    {
        public string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }
        }
        public int Age
        {
            get { return GetValue<int>("Age"); }
            set { SetValue("Age", value); }
        }
        public long Distance
        {
            get { return GetValue<long>("Distance"); }
            set { SetValue("Distance", value); }
        }
        public DateTime DtDiscovered
        {
            get { return GetValue<DateTime>("DtDiscovered"); }
            set { SetValue("DtDiscovered", value); }
        }
        public DateTime? DtUpdated
        {
            get { return GetValue<DateTime?>("DtUpdated"); }
            set { SetValue("DtUpdated", value); }
        }
        public byte[] Image
        {
            get { return GetValue<byte[]>("Image"); }
            set { SetValue("Image", value); }
        }
        public bool InMilkyWay
        {
            get { return GetValue<bool>("InMilkyWay"); }
            set { SetValue("InMilkyWay", value); }
        }
        public double SurfaceTemperature
        {
            get { return GetValue<double>("SurfaceTemperature"); }
            set { SetValue("SurfaceTemperature", value); }
        }

        public long Version
        {
            get { return GetValue<long>("Version"); }
            set { SetValue("Version", value); }
        }

        public StarDocument()
        {
            Version = 1;
        }
    }
}
