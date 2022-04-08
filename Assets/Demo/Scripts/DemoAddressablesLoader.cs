﻿using UnityScreenNavigator.Runtime.Foundation.AssetLoader;

namespace Demo.Scripts
{
    public class DemoAddressablesLoader : AssetLoaderObject
    {
        private readonly AddressableAssetLoader _loader = new AddressableAssetLoader();
        public override AssetLoadHandle<T> Load<T>(string key)
        {
            return _loader.Load<T>(GetResourceKey(key));
        }

        public override AssetLoadHandle<T> LoadAsync<T>(string key)
        {
            return _loader.LoadAsync<T>(GetResourceKey(key));
        }

        public override void Release(AssetLoadHandle handle)
        {
            _loader.Release(handle);
        }
        
        private string GetResourceKey(string key)
        {
            return $"Prefabs/prefab_demo_{key}";
        }
    }
}