using Serilog;

namespace Utils
{
    public class ReferenceCollection
    {
        private Queue<IReference> references = new Queue<IReference>();

        private Type referenceType;

        public int CurUsingRefCount { get; private set; }

        public int ReferencesCount { get { return references.Count; } }

        private static object locker = new object();

        public ReferenceCollection(Type type)
        {
            referenceType = type;
            CurUsingRefCount = 0;
        }

        public T Acquire<T>() where T : IReference, new()
        {
            if (typeof(T) != referenceType)
            {
                throw new Exception("请求类型错误:" + typeof(T).Name);
            }
            CurUsingRefCount++;

            T? value = default(T);

            lock (locker)
            {
                if (references.Count > 0)
                {
                    value = (T)references.Dequeue();
                }
            }

            if (value == null)
            {
                value = new T();
            }

            value.OnAcquire();

            return value;
        }

        public void Release(IReference reference)
        {
            reference.OnRelease();
            lock (locker)
            {
                if (references.Contains(reference))
                {
                    Log.Error($"重复回收:{reference.ToString()}");
                    return;
                }
                references.Enqueue(reference);
            }
            CurUsingRefCount--;
        }

        public void Add<T>(int count) where T : IReference, new()
        {
            if (typeof(T) != referenceType)
            {
                Log.Error($"请求类型错误:{typeof(T).Name}");

                return;
            }
            lock (locker)
            {
                while (count-- > 0)
                {
                    references.Enqueue(new T());
                }
            }
        }

        public void Remove(int count)
        {
            lock (locker)
            {
                if (count > references.Count)
                {
                    count = references.Count;
                }
                while (count-- > 0)
                {
                    references.Dequeue();
                }
            }
        }

        public void RemoveAll()
        {
            lock (locker)
            {
                references.Clear();
            }
        }
    }
}
