namespace SberGames.SystemLibraries.Tests.Editor
{
    using System;
    using NUnit.Framework;

    /// <summary>
    /// This class contains sanity tests for System.Memory assembly.
    /// </summary>
    public static class SystemMemorySanityTest
    {
        /// <summary>
        /// This test ensures that Span is available. 
        /// </summary>
        [Test]
        public static void SpanTest()
        {
            var byteBuffer = new byte[10];
            Span<byte> bufferSpan = byteBuffer.AsSpan();

            for (int i = 0; i < bufferSpan.Length; ++i)
            {
                bufferSpan[i] = (byte)i;
            }
        }
        
        /// <summary>
        /// This test ensures that Memory is available. 
        /// </summary>
        [Test]
        public static void MemoryTest()
        {
            var byteBuffer = new byte[10];
            Memory<byte> bufferMemory = byteBuffer.AsMemory();

            for (int i = 0; i < bufferMemory.Length; ++i)
            {
                bufferMemory.Span[i] = (byte)i;
            }
        }
        
        /// <summary>
        /// This test ensures that String extensions for Span is available. 
        /// </summary>
        [Test]
        public static void StringSpanTest()
        {
            const string testString = "Some random characters";
            ReadOnlySpan<char> charactersSpan = testString.AsSpan();

            for (int i = 0; i < charactersSpan.Length; ++i)
            {
                Assert.True(charactersSpan[i] == testString[i]);
            }
        }
        
        /// <summary>
        /// This test ensures that stackalloc can be used with Span without unsafe block. 
        /// </summary>
        [Test]
        public static void StackallocSpanTest()
        {
            Span<byte> span = stackalloc byte[10];

            for (int i = 0; i < span.Length; ++i)
            {
                span[i] = (byte)i;
            }
        }
    }
}
