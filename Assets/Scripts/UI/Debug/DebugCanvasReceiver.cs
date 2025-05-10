using UI.Canvas;
using UnityEngine;

namespace UI.Debug
{
    public sealed class DebugCanvasReceiver : CanvasReceiver
    {
        public static DebugCanvasReceiver Instance => _instance ?? (_instance = new DebugCanvasReceiver());

        public override GameObject Canvas => CanvasManager.Instance?.DebugCanvas?.gameObject;

        private static DebugCanvasReceiver _instance;

        private DebugCanvasReceiver() { }

        public override void Dispose()
        {
            base.Dispose();
            _instance = null;
        }

    }

}
