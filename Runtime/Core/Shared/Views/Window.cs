using UnityEngine;
using UnityScreenNavigator.Runtime.Core.DynamicWindow;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    [DisallowMultipleComponent]
    public abstract class Window : WindowView, IWindow
    {
        [Range(0, 10)] private int windowPriority = 0;

        public virtual int WindowPriority
        {
            get { return this.windowPriority; }
            set
            {
                if (value < 0)
                    this.windowPriority = 0;
                else if (value > 10)
                    this.windowPriority = 10;
                else
                    this.windowPriority = value;
            }
        }

        private bool _created = false;

        public bool Created
        {
            get { return this._created; }
        }

        public bool Dismissed { get; }

        public bool Activated { get; }



        public virtual string Identifier { get; set; }

        //protected abstract void OnCreate(IBundle bundle);

        // public void Create(IBundle bundle = null)
        // {
        //     if (this._created)
        //         return;
        //
        //     this.Visibility = false;
        //     this.Interactable = this.Activated;
        //     this.OnCreate(bundle);
        //     this._created = true;
        //     WindowManager.Add(this);
        // }
    }
}