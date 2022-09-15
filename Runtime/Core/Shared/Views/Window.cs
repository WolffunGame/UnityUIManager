using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    [DisallowMultipleComponent]
    public abstract class Window : WindowView
    {
        public virtual string Identifier { get; set; }
        
        public virtual int Priority { get; set; }
        
    }
}