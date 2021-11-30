namespace Utils.Tests
{
    using NUnit.Framework;
    using SampleData.ObjectByTypeCollection;
    using SberGames.Utils;

    [TestFixture]
    public class ObjectByTypeCollectionWithAutoCreationTest
    {
        [SetUp]
        public void Setup()
        {
            collection = new ObjectByTypeCollectionWithAutoCreation<ISomeObjectMarkerInterface>(5);
        }

        private ObjectByTypeCollectionWithAutoCreation<ISomeObjectMarkerInterface> collection = default!;

        [Test]
        public void AddGetTest()
        {
            collection.Add(new SomeObjectWithMarkerInterface());
            SomeObjectWithMarkerInterface someObject = collection.GetObjectAs<SomeObjectWithMarkerInterface>();

            Assert.NotNull(someObject);
        }

        [Test]
        public void GetWithoutAddSucceededTest()
        {
            SomeOtherObjectWithMarkerInterface someOtherObject = collection.GetObjectAs<SomeOtherObjectWithMarkerInterface>();

            Assert.NotNull(someOtherObject);
        }

        [Test]
        public void TryGetTest()
        {
            collection.Add(new SomeObjectWithMarkerInterface());

            Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out SomeObjectWithMarkerInterface someObject));
            Assert.NotNull(someObject);
            Assert.True(collection.TryGetObjectAs<SomeOtherObjectWithMarkerInterface>(out SomeOtherObjectWithMarkerInterface someOtherObject));
            Assert.NotNull(someOtherObject);
        }

        [Test]
        public void AddRemoveByTypeTest()
        {
            SomeObjectWithMarkerInterface someObject;

            collection.Add(new SomeObjectWithMarkerInterface());
            Assert.True(collection.ContainsObjectOfType<SomeObjectWithMarkerInterface>());
            Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out someObject));
            Assert.NotNull(someObject);

            collection.RemoveByType(typeof(SomeObjectWithMarkerInterface));

            Assert.False(collection.ContainsObjectOfType<SomeObjectWithMarkerInterface>());

            // Auto creation here
            Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out someObject));
            Assert.NotNull(someObject);
        }

        [Test]
        public void AddRemoveObjectTest()
        {
            SomeObjectWithMarkerInterface someObject;

            collection.Add(new SomeObjectWithMarkerInterface());
            Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out someObject));
            Assert.True(collection.ContainsObject(someObject));
            Assert.NotNull(someObject);

            collection.RemoveObject(someObject);

            Assert.False(collection.ContainsObject(someObject));

            // Auto creation here
            Assert.True(collection.TryGetObjectAs<SomeObjectWithMarkerInterface>(out someObject));
            Assert.True(collection.ContainsObject(someObject));
            Assert.NotNull(someObject);
        }
    }
}