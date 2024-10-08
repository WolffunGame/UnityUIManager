using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    public enum LifeCycleEnum
    {
        DEFAULT = 0,
        INIT = 1,
        WILL_PUSH_ENTER = 2,
        DID_PUSH_ENTER = 3,
        WILL_PUSH_EXIT = 4,
        DID_PUSH_EXIT = 5,
        WILL_POP_ENTER = 6,
        DID_POP_ENTER = 7,
        WILL_POP_EXIT = 8,
        DID_POP_EXIT = 9,
        CLEANUP = 10,
    }
}