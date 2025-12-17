using UnityEngine;
using UnityEngine.Pool;

namespace ObjectPool
{
    public abstract class GenericPool<T> : MonoBehaviour where T : Component
    {
        [Header("Configuración del Pool")] [SerializeField]
        private T prefab;

        [SerializeField] private int defaultCapacity = 10;
        [SerializeField] private int maxSize = 100;

        private IObjectPool<T> _pool;

        public IObjectPool<T> Pool
        {
            get
            {
                if (_pool == null) InitializePool();
                return _pool;
            }
        }

        private void InitializePool()
        {
            _pool = new ObjectPool<T>(
                createFunc: CreatePooledItem,
                actionOnGet: OnTakeFromPool,
                actionOnRelease: OnReturnedToPool,
                actionOnDestroy: OnDestroyPoolObject,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private T CreatePooledItem()
        {
            T instance = Instantiate(prefab, transform);
            return instance;
        }


        protected virtual void OnTakeFromPool(T item)
        {
            item.gameObject.SetActive(true);
        }

        protected virtual void OnReturnedToPool(T item)
        {
            item.gameObject.SetActive(false);
        }

        protected virtual void OnDestroyPoolObject(T item)
        {
            Destroy(item.gameObject);
        }
    }
}