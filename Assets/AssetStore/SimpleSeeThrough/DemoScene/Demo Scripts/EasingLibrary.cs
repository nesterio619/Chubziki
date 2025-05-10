using UnityEngine;

namespace SimpleSeeThroughDemo
{
    public static class EasingLibrary
    {
        public static float EaseInSine(float t)
        {
            return 1 * Mathf.Cos(t * (Mathf.PI / 2));
        }

        public static float EaseOutSine(float t)
        {
            return Mathf.Sin(t * (Mathf.PI / 2));
        }

        public static float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
        }

        public static float EaseInQuad(float t)
        {
            return t * t;
        }

        public static float EaseOutQuad(float t)
        {
            return 1 - (1 - t) * (1 - t);
        }

        public static float EaseInOutQuad(float t)
        {
            return t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;
        }

        public static float EaseInCubic(float t)
        {
            return t * t * t;
        }

        public static float EaseOutCubic(float t)
        {
            return 1 - Mathf.Pow(1 - t, 3);
        }

        public static float EaseInOutCubic(float t)
        {
            return t < 0.5 ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
        }

        public static float EaseInQuart(float t)
        {
            return t * t * t * t;
        }

        public static float EaseOutQuart(float t)
        {
            return 1 - Mathf.Pow(1 - t, 4);
        }

        public static float EaseInOutQuart(float t)
        {
            return t < 0.5 ? 8 * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 4) / 2;
        }

        public static float EaseInQuint(float t)
        {
            return t * t * t * t * t;
        }

        public static float EaseOutQuint(float t)
        {
            return 1 - Mathf.Pow(1 - t, 5);
        }

        public static float EaseInOutQuint(float t)
        {
            return t < 0.5 ? 16 * t * t * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 5) / 2;
        }

        public static float EaseInExpo(float t)
        {
            return t == 0 ? 0 : Mathf.Pow(2, 10 * t - 10);
        }

        public static float EaseOutExpo(float t)
        {
            return t == 1 ? 1 : 1 - Mathf.Pow(2, -10 * t);
        }

        public static float EaseInOutExpo(float t)
        {
            return t == 0 ? 0 : t == 1 ? 1 : t < 0.5 ? Mathf.Pow(2, 20 * t - 10) / 2 : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;
        }

        public static float EaseInCirc(float t)
        {
            return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2));
        }

        public static float EaseOutCirc(float t)
        {
            return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2));
        }

        public static float EaseInOutCirc(float t)
        {
            return t < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2;
        }

        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return c3 * t * t * t - c1 * t * t;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2);
        }

        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            return t < 0.5 ? (1 + c2 * t * t * t) / 2 : (1 + c2 * (t - 1) * (t - 1) * (t - 1) + 1) / 2;
        }

        public static float EaseInElastic(float t)
        {
            return Mathf.Sin(13 * Mathf.PI / 2 * t) * Mathf.Pow(2, 10 * (t - 1));
        }

        public static float EaseOutElastic(float t)
        {
            return Mathf.Sin(-13 * Mathf.PI / 2 * t) * Mathf.Pow(2, -10 * t) + 1;
        }

        public static float EaseInOutElastic(float t)
        {
            return t < 0.5 ? (Mathf.Sin(13 * Mathf.PI / 2 * t * 2) * Mathf.Pow(2, 10 * (t * 2 - 1)) / 2) : (1 + Mathf.Sin(-13 * Mathf.PI / 2 * (t * 2 - 1)) * Mathf.Pow(2, -10 * (t * 2 - 1)) / 2);
        }
    }
}