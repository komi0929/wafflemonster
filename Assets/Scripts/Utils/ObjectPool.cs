using UnityEngine;
using System.Collections.Generic;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// 汎用オブジェクトプール — GC Alloc最小化
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _initialSize = 10;
        [SerializeField] private Transform _poolParent;

        private readonly Queue<GameObject> _pool = new Queue<GameObject>();

        private void Awake()
        {
            if (_poolParent == null)
            {
                var parent = new GameObject($"Pool_{_prefab.name}");
                _poolParent = parent.transform;
            }

            for (int i = 0; i < _initialSize; i++)
            {
                CreateNewObject();
            }
        }

        private void CreateNewObject()
        {
            var obj = Instantiate(_prefab, _poolParent);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            if (_pool.Count == 0)
            {
                CreateNewObject();
            }

            var obj = _pool.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// オブジェクトをプールに返却
        /// </summary>
        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(_poolParent);
            _pool.Enqueue(obj);
        }
    }
}
