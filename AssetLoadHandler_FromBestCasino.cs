using System;
using System.Collections;
using UniRx;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Features.AssetManagement.Handlers
{
    public class AssetLoadHandler
    {
        private readonly PoolHandler _pool;
        private readonly ErrorHandler _errorHandler;
        
        public AssetLoadHandler(PoolHandler pool, ErrorHandler errorHandler)
        {
            _pool = pool;
            _errorHandler = errorHandler;
        }
        
        public Object LoadAsset(object key, bool autoReleaseHandle = false) 
        {
            if (_pool.ContainsKey(key) && _pool[key].IsValid() && _pool[key].Result)
            {
                return _pool[key].Result;
            }
            var handle = Addressables.LoadAssetAsync<Object>(key);
            var obj = handle.WaitForCompletion();
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                if (autoReleaseHandle) Addressables.Release(handle);
                else _pool[key] = handle;
            }
            else
            {
                return null;
            }

            return obj;
        }

        public void LoadAssetAsync(object key, Action<float> onProgress, Action<Object> onLoaded, Action<Exception> onFail, bool autoRelease = false)
        {
            if (_pool.GetAssetAsync(key, onLoaded))
            {
                return;
            }
            _pool.Add(key, default);
            Observable.FromCoroutine(_ => Process()).Subscribe();

            IEnumerator Process()
            {
                var handle = Addressables.LoadAssetAsync<Object>(key);
                
                float progress = 0;
                while (handle.Status == AsyncOperationStatus.None)
                {
                    float percentageComplete = handle.GetDownloadStatus().Percent;
                    if (percentageComplete > progress * 1.1) // Report at most every 10% or so
                    {
                        progress = percentageComplete; // More accurate %
                        onProgress?.Invoke(progress);
                    }

                    yield return null;
                }
                
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    if (!autoRelease) _pool[key] = handle;
                    onLoaded?.Invoke(handle.Result);
                    if (autoRelease) Addressables.Release(handle);
                }
                else
                {
                    _errorHandler.IsDownloadError(handle);
                    onFail?.Invoke(handle.OperationException);
                    Addressables.Release(handle);
                }
            }
        }

        public void ReleaseAsset(object key)
        {
            if (_pool.ContainsKey(key))
            {
                Addressables.Release(_pool[key]);
                _pool.Remove(key);
            }
        }
    }
}