namespace SberGames.SystemLibraries.Tests.Editor
{
    using System.Buffers;
    using NUnit.Framework;

    /// <summary>
    /// This class contains sanity tests for System.Buffers assembly.
    /// </summary>
    public static class SystemBuffersSanityTest
    {
        /// <summary>
        /// This test ensures that ArrayPool is available. 
        /// </summary>
        [Test]
        public static void ArrayPoolTest()
        {
            ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;
            byte[] buffer = arrayPool.Rent(10);
            arrayPool.Return(buffer);
        }
    }
}
