using System;

namespace RSM
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class State : RSMAttribute
    {
    }
}