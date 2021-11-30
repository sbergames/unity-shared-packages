namespace SberGames.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class ObjectByTypeCollection<TBaseType>
        where TBaseType : class
    {
        private readonly Dictionary<Type, TBaseType> map;

        public ObjectByTypeCollection(int initialMapSize = 4)
        {
            map = new Dictionary<Type, TBaseType>(initialMapSize);
        }

        public void Add(TBaseType newObject)
        {
            map.Add(newObject.GetType(), newObject);
        }

        public void RemoveByType(Type typeToRemove)
        {
            map.Remove(typeToRemove);
        }

        public void RemoveObject(TBaseType objectToRemove)
        {
            map.Remove(objectToRemove.GetType());
        }

        public TSpecificType GetObjectAs<TSpecificType>()
            where TSpecificType : class, TBaseType
        {
            if (!map.TryGetValue(typeof(TSpecificType), out TBaseType desiredObject))
            {
                throw new ArgumentException($"There is no object of type {typeof(TSpecificType)} in this collection.");
            }

            return (TSpecificType)desiredObject;
        }

        public bool TryGetObjectAs<TSpecificType>([NotNullWhen(true)] out TSpecificType? specificDesiredObject)
            where TSpecificType : class, TBaseType
        {
            if (map.TryGetValue(typeof(TSpecificType), out TBaseType desiredObject))
            {
                specificDesiredObject = (TSpecificType)desiredObject;

                return true;
            }

            specificDesiredObject = default;

            return false;
        }

        public bool ContainsObjectOfType<TSpecificType>()
            where TSpecificType : class, TBaseType
        {
            return map.ContainsKey(typeof(TSpecificType));
        }

        public bool ContainsObject(TBaseType objectToCheck)
        {
            return map.ContainsValue(objectToCheck);
        }
        
        public void Clear()
        {
            map.Clear();
        }
    }
}