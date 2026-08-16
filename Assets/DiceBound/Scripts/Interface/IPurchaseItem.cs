using UnityEngine;

namespace DiceBound.Interface
{
    public interface IPurchaseItem
    {
        public Sprite GetSprite();
        public string GetId();
    }
}