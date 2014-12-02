using System;

namespace XRegional
{
    public class VersionOverflowException : OverflowException
    {
        public VersionOverflowException(Exception innerException)
            : this ("Version overflow", innerException)
        {
        }

        public VersionOverflowException(string message, Exception innerException)
            : base (message, innerException)
        {
        }
    }
}
