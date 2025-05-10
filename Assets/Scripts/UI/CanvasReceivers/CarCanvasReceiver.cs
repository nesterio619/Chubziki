using UI.Canvas;
using UnityEngine;

namespace UI
{
    public class CarCanvasReceiver : CanvasReceiver
    {
        public static CarCanvasReceiver Instance => _instance ?? (_instance = new CarCanvasReceiver());

        public override GameObject Canvas => CanvasManager.Instance?.CarCanvas?.gameObject;

        private static CarCanvasReceiver _instance;

        public override void Dispose()
        {
            base.Dispose();
            _instance = null;
        }
    }
}