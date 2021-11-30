namespace SberGames.Utils
{
    using System;

    public class ObjectByTypeCollectionWithAutoCreation<TBaseType>
        where TBaseType : class
    {
        private readonly ObjectByTypeCollection<TBaseType> innerCollection;

        public ObjectByTypeCollectionWithAutoCreation(int initialMapSize = 4)
        {
            innerCollection = new ObjectByTypeCollection<TBaseType>(initialMapSize);
        }

        public void Add(TBaseType newObject)
        {
            innerCollection.Add(newObject);
        }

        public void RemoveByType(Type typeToRemove)
        {
            innerCollection.RemoveByType(typeToRemove);
        }

        public void RemoveObject(TBaseType objectToRemove)
        {
            innerCollection.RemoveObject(objectToRemove);
        }

        public bool ContainsObjectOfType<TSpecificType>()
            where TSpecificType : class, TBaseType
        {
            return innerCollection.ContainsObjectOfType<TSpecificType>();
        }

        public bool ContainsObject(TBaseType objectToCheck)
        {
            return innerCollection.ContainsObject(objectToCheck);
        }

        public TSpecificType GetObjectAs<TSpecificType>()
            where TSpecificType : class, TBaseType, new()
        {
            if (!innerCollection.TryGetObjectAs(out TSpecificType? desiredObject))
            {
                desiredObject = new TSpecificType();
                innerCollection.Add(desiredObject);
            }

            return desiredObject;
        }

        public bool TryGetObjectAs<TSpecificType>(out TSpecificType specificDesiredObject)
            where TSpecificType : class, TBaseType, new()
        {
            if (!innerCollection.TryGetObjectAs(out TSpecificType? desiredObject))
            {
                desiredObject = new TSpecificType();
                innerCollection.Add(desiredObject);
            }

            specificDesiredObject = desiredObject;

            return true;
        }
        
        public void Clear()
        {
            innerCollection.Clear();
        }
    }
}