using System;

namespace XRegional.Internal
{
    internal static class VersionIncrementer
    {
        public static long Increment(long currentVersion)
        {
            return Add(currentVersion, 1);
        }

        public static long Add(long currentVersion, long increment)
        {
            long result;

            try
            {
                result = checked(currentVersion + increment);
            }
            catch (OverflowException e)
            {
                throw new VersionOverflowException(e);
            }

            return result;
        }
    }
}
