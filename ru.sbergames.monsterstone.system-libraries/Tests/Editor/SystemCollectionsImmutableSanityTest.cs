namespace SberGames.SystemLibraries.Tests.Editor
{
    using System.Collections.Immutable;
    using NUnit.Framework;

    /// <summary>
    /// This class contains sanity tests for System.Collections.Immutable assembly.
    /// </summary>
    public static class SystemCollectionsImmutableSanityTest
    {
        /// <summary>
        /// This test ensures that ImmutableHashSet is available. 
        /// </summary>
        [Test]
        public static void ImmutableHashSetTest()
        {
            var hashSetItems = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            ImmutableHashSet<int> immutableHashSet = ImmutableHashSet.Create(hashSetItems);
            
            Assert.True(immutableHashSet.Contains(7));
            Assert.False(immutableHashSet.Contains(11));
        }
    }
}
