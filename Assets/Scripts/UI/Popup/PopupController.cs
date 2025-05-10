using System;
using Core.ObjectPool;
using UnityEngine;

namespace UI.Popup
{
    public abstract class PopupController : PooledGameObject
    {
        [SerializeField] private protected Vector2Int DefaultPopupSize;
        private protected Action OnCloseAction = null;
    }
}
