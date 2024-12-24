
using Serilog;

namespace Utils
{
    public class ReferencePool : Singleton<ReferencePool>
    {
        private Dictionary<Type, ReferenceCollection> referenceCollections = new Dictionary<Type, ReferenceCollection>();
        public int Count { get { return referenceCollections.Count; } }

        private static object locker = new object();
        protected override void OnDispose()
        {
            ClearAll();
        }

        protected override void OnInit()
        {
            //throw new NotImplementedException();
        }

        public void ClearAll()
        {
            lock (locker)
            {
                foreach (var item in referenceCollections.Values)
                {
                    item.RemoveAll();
                }

                referenceCollections.Clear();
            }
        }

        public T Acquire<T>() where T : class, IReference, new()
        {
            return GetReferenceCollection(typeof(T)).Acquire<T>();
        }
        public void Release(IReference reference)
        {
            GetReferenceCollection(reference.GetType()).Release(reference);
        }
        public void Add<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Add<T>(count);
        }
        public void Remove<T>(int count) where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).Remove(count);
        }
        public void RemoveAll<T>() where T : class, IReference, new()
        {
            GetReferenceCollection(typeof(T)).RemoveAll();
        }

        public ReferenceCollection GetReferenceCollection(Type type)
        {
            if (type == null)
            {
                throw new NullReferenceException("传入参数 type is Null");
            }
            ReferenceCollection? referenceCollection = null;
            lock (locker)
            {
                if (!referenceCollections.TryGetValue(type, out referenceCollection))
                {
                    referenceCollection = new ReferenceCollection(type);
                    referenceCollections.Add(type, referenceCollection);
                }
            }
            return referenceCollection;
        }
    }
}
