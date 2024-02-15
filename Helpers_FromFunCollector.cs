using System;
using UniRx;
using UnityEngine;

namespace Utils {
    public static class Helpers {
        public static void ChangeWholeObjectLayer(this GameObject obj, int layer) {
            obj.layer = layer;
            foreach (Transform child in obj.transform) {
                child.gameObject.layer = layer;
                child.gameObject.ChangeWholeObjectLayer(layer);
            }
        }

        public static void ClearChildrenFromTransform(Transform tr) {
            foreach (Transform child in tr) {
                GameObject.Destroy(child.gameObject);
            }
        }
        public static IDisposable CountAnimation(float startValue, float endValue, Action<float> onStepComplete = null, float duration = 0.5f) {
            float timer = 0;
            float value = 0;
            return Observable.EveryUpdate().TakeWhile(x => timer < duration).Subscribe(_ => {
                float progress = timer / duration;
                value = Mathf.Lerp(startValue, endValue, progress);
                timer += Time.deltaTime;
                onStepComplete?.Invoke(value);
            }, () => {
                onStepComplete?.Invoke(endValue);
            });
        }
    }
}