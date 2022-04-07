using UnityEngine;

namespace UnityScreenNavigator.Runtime.Core.Shared.Views
{
    /// <summary>
    /// Do not need <see cref="IWindowManager"/> for <see cref="ContainerBase"/>
    /// </summary>
    public class ContainerBase: WindowView
    {
        [Range(0, 10)]
        private int windowPriority = 0;
        
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
        public virtual string Identifier { get; set; }

    }
}