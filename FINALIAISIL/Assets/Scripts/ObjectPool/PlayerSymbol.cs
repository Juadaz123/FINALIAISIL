using UnityEngine;
using UnityEngine.Pool;

public class PlayerSymbol : MonoBehaviour
{
    private IObjectPool<PlayerSymbol> _pool;
        
    public void SetPool(IObjectPool<PlayerSymbol> pool)
    {
        _pool = pool;
    }

    private void OnEnable()
    {
        Invoke(nameof(ReturnToPool), 1f);
    }

    public void ReturnToPool()
    {
        _pool.Release(this);
            
    }
}
