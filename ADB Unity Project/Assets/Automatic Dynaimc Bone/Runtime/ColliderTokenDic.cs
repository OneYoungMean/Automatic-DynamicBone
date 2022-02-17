using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace ADBRuntime.Mono
{
    public class ColliderTokenDic : MonoBehaviour
    {
        public UpdateMode updateMode=UpdateMode.FixedUpdate;
        public static ColliderTokenDic Instance {
            get
            {
                if (instance==null)
                {
                    GameObject go = new GameObject("ADBColliderUpdator");
                    GameObject.DontDestroyOnLoad(go);
                    instance= go.AddComponent<ColliderTokenDic>();
                    instance.colliderTokenDic = new Dictionary<int, ADBColliderReader>();
                    instance.removeList = new List<int>();
                }
                return instance;
            }
        }
        private static ColliderTokenDic instance;
        private Dictionary<int, ADBColliderReader> colliderTokenDic;
        private List<int> removeList;
        // Update is called once per frame

        void Update()
        {
            if (updateMode==UpdateMode.Update)
            {
                UpdateDic();
            }
        }


        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
            {
                UpdateDic();
            }
        }

        private void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate)
            {
                UpdateDic();
            }
        }

        private void UpdateDic()
        {
            foreach (var item in colliderTokenDic)
            {
                item.Value.UpdateCollider();
            }
            for (int i = 0; i < removeList.Count; i++)
            {
                colliderTokenDic.Remove(removeList[i]);
            }
            removeList.Clear();
        }

        private void OnDestroy()
        {
            colliderTokenDic = null;
        }

        internal static bool TryGetValue(int id, out ADBColliderReader aDBColliderReader)
        {
            return Instance.colliderTokenDic.TryGetValue(id, out aDBColliderReader);

        }

        internal static void Remove(int id)
        {
            Instance. removeList.Add(id);
        }

        internal static void Add(int id, ADBColliderReader aDBColliderReader)
        {
            Instance.colliderTokenDic.Add(id, aDBColliderReader);
        }
    }
}