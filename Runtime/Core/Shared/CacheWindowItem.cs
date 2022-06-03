using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace UnityScreenNavigator.Runtime.Core.Shared
{
    public readonly struct CacheWindowItem
    {
        public GameObject Window { get; }
        public string Key { get; }

        public CacheWindowItem(GameObject window, string key)
        {
            Window = window;
            Key = key;
        }
    }
}