﻿#if HE_SYSCORE

using System;
using UnityEngine.Events;

namespace HeathenEngineering.UX
{
    [Serializable]
    public class CommandRaisedEvent : UnityEvent<CommandData>
    { }
}

#endif
