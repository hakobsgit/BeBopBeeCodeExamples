using DI.Interfaces;
using UnityEngine;
using Zenject;

namespace DI.Factories {
    public class PrefabFactory : IPrefabFactory {
        private readonly DiContainer _container;

        [Inject]
        public PrefabFactory(DiContainer container) {
            _container = container;
        }
        
        public GameObject Create(GameObject prefab) {
            return _container.InstantiatePrefab(prefab);
        }

        public GameObject Create(GameObject prefab, Transform container) {
            return _container.InstantiatePrefab(prefab, container);
        }
        
        public GameObject Create(string path) {
            return _container.InstantiatePrefabResource(path);
        }

        public GameObject Create(string path, Transform parent) {
            return _container.InstantiatePrefabResource(path, parent);
        }

        public T Create<T>(string path) {
            return _container.InstantiatePrefabResource(path).GetComponent<T>();
        }

        public T Create<T>(string path, Transform parent) {
            return _container.InstantiatePrefabResource(path, parent).GetComponent<T>();
        }

        public T Create<T>(T original) where T : Object {
            return _container.InstantiatePrefab(original).GetComponent<T>();
        }

        public T Create<T>(T original, Transform parent) where T : Object {
            return _container.InstantiatePrefab(original, parent).GetComponent<T>();
        }
    }
}