using Core.ObjectPool;
using UnityEngine;

namespace UI
{
    public abstract class CanvasCommand 
    {
        protected CanvasReceiver Receiver { get; private set; }

        protected PooledGameObject PooledObjectReference;

        public bool IsDisposed { private set; get; } = false;
        protected CanvasCommand(CanvasReceiver receiver)
        {
            Receiver = receiver;

            Receiver.RegisterCanvasCommand(this);

            Initialize();
        }

        protected virtual void Initialize()
        {
            IsDisposed = false;
        }

        public abstract void Update();

        public virtual void Dispose()
        {
            Receiver.UnregisterCanvasCommand(this);
            IsDisposed = true;
        }
    }

}

