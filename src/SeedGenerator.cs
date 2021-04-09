using System;

namespace Tavenem.Randomize
{
    /// <summary>
    /// Facilitates the generation of a pseudo-random seed value.
    /// </summary>
    public static class SeedGenerator
    {
        /// <summary>
        /// Generates a new seed value based on the system time, the guid creation algorithm, and
        /// arbitrary process information.
        /// </summary>
        /// <returns>A pseudorandom seed value.</returns>
        public static uint GetNewSeed()
        {
            unchecked
            {
                const uint Factor = 19U;
                var seed = (Factor * 1777771U) + (uint)Environment.TickCount;
                var guid = Guid.NewGuid().ToByteArray();
                seed = (Factor * seed) + BitConverter.ToUInt32(guid, 0);
                seed = (Factor * seed) + BitConverter.ToUInt32(guid, 8);
                seed = (Factor * seed) + (uint)System.Threading.Thread.CurrentThread.ManagedThreadId;
                return (Factor * seed) + (uint)Environment.ProcessId;
            }
        }
    }
}
