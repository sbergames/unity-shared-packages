namespace Utils.Tests
{
    using System;
    using NUnit.Framework;
    using SampleData.ObjectByTypeCollection;
    using SberGames.Utils;

    [TestFixture]
    public class ObjectByTypeCollectionTest
    {
        [SetUp]
        public void Setup()
        {
            collection = new ObjectByTypeCollection<ISomeObjectMarkerInterface>(5);
        }

        private ObjectByTypeCollection<ISomeObjectMarkerInterface> collection = default!;

        [Test]
        public void AddGetTest()
        {
            collection.Add(new SomeObjectWithMarkerInterface());
            SomeObjectWithMarkerInterface someObject = collection.GetObjectAs<SomeObjectWithMarkerInterface>();

            Assert.NotNull(someObject);
        }

        [Test]
        public void GetWithoutAddFailureTest()
        {
            collection.Add(new SomeObjectWithMarkerInterface());
            Assert.Throws<ArgumentException>(() => { collection.GetObjectAs<SomeOtherObjectWithMarkerInterface>(); });
        }

        [Test]
        public void TryGetTest()
        {
            collection.Add(new SomeObjectWithMarkerInterface());

            Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out var someObject));
            Assert.NotNull(someObject);
            Assert.False(collection.TryGetObjectAs<SomeOtherObjectWithMarkerInterface>(out var someOtherObject));
            Assert.IsNull(someOtherObject);
        }

        [Test]
        public void AddRemoveByTypeTest()
        {
            {
                collection.Add(new SomeObjectWithMarkerInterface());
                Assert.True(collection.ContainsObjectOfType<SomeObjectWithMarkerInterface>());
                Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out SomeObjectWithMarkerInterface? someObject));
                Assert.NotNull(someObject);
            }

            collection.RemoveByType(typeof(SomeObjectWithMarkerInterface));

            {
                Assert.False(collection.ContainsObjectOfType<SomeObjectWithMarkerInterface>());
                Assert.False(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out SomeObjectWithMarkerInterface? someObject));
                Assert.IsNull(someObject);
            }
        }

        [Test]
        public void AddRemoveObjectTest()
        {
            {
                collection.Add(new SomeObjectWithMarkerInterface());
                Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out SomeObjectWithMarkerInterface? someObject));

                if (someObject != null)
                {
                    Assert.True(collection.ContainsObject(someObject));
                    Assert.NotNull(someObject);

                    collection.RemoveObject(someObject);
                }
            }

            {
                Assert.False(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out SomeObjectWithMarkerInterface? someObject));
                Assert.False(collection.ContainsObjectOfType<SomeObjectWithMarkerInterface>());
                Assert.IsNull(someObject);
            }
        }
    }
}
