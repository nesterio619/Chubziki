using UnityEngine;

namespace Core.Utilities
{
    public static class ScreenUtilities
    {
        private static readonly Vector2Int[] _commonRatios =
        {
            new(16, 9), new(4, 3), new(21, 9), new(32, 9),
            new(5, 4), new(3, 2), new(1, 1), new(16, 10),
            new(32, 10), new(18, 5)
        };

        public static Vector2 GetScreenSize()
        {
            return new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        }

        //Aspect ratio refers to the ratio of the width and height of screen
        //We find largest divisible number for width and height and with his help find aspect ratio 
        //From 1920:1080 -> 16:9 ; 640:480 -> 4:3  
        public static Vector2Int GetAspectRatio()
        {
            Vector2 size = GetScreenSize();
            float aspect = size.x / size.y;

            return GetClosestAspectRatio(aspect);
        }

        public static Vector2Int GetClosestAspectRatio(float aspect)
        {
            float minDifference = float.MaxValue;
            Vector2Int closestRatio = _commonRatios[0];

            foreach (var ratio in _commonRatios)
            {
                float ratioValue = (float)ratio.x / ratio.y;
                float diff = Mathf.Abs(aspect - ratioValue);

                if (diff < minDifference)
                {
                    minDifference = diff;
                    closestRatio = ratio;
                }
            }

            return closestRatio;
        }
    }
}