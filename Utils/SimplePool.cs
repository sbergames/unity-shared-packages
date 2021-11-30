namespace SberGames.Utils
{
    using System.Collections.Generic;

    public class SimplePool<T>
        where T : new()
    {
        private readonly Stack<T> pool;

        public SimplePool(int initialCapacity)
        {
            pool = new Stack<T>(initialCapacity);

            while (initialCapacity-- > 0)
            {
                pool.Push(new T());
            }
        }

        public T Get()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return new T();
        }

        public void Return(T returnedObject)
        {
            pool.Push(returnedObject);
        }
    }
}