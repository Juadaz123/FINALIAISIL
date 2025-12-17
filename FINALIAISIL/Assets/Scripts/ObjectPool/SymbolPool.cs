using UnityEngine;

namespace ObjectPool
{
    public class SymbolPool : GenericPool<PlayerSymbol>
    {
        protected override void OnTakeFromPool(PlayerSymbol item)
        {
            base.OnTakeFromPool(item);
            item.SetPool(Pool);
        }

        public void SpawnPlayerSymbol(Transform exBitePosition)
        {
            PlayerSymbol playerSymbol = Pool.Get();
            playerSymbol.transform.position = exBitePosition.position;
        }
    }
}