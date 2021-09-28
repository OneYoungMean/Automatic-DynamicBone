using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace ADBRuntime.Mono
{
    public struct ColliderChecker:IEquatable<UnityEngine.SphereCollider>,IEquatable<UnityEngine.CapsuleCollider>, IEquatable<UnityEngine.BoxCollider>
    {
        public float radius;
        public Vector3 center;
        public Vector3 size;
        public ColliderType colliderType;
        public ColliderChecker(UnityEngine.SphereCollider sphereCollider)
        {
            colliderType = ColliderType.Sphere;
            radius = sphereCollider.radius;
            center = sphereCollider.center;
            size = Vector3.zero;
        }

        public ColliderChecker(UnityEngine.CapsuleCollider capsuleCollider)
        {
            colliderType = ColliderType.Capsule;
            radius = capsuleCollider.radius;
            center = capsuleCollider.center;
            size =Vector3.one* capsuleCollider.height;
        }


        public ColliderChecker(UnityEngine.BoxCollider boxCollider)
        {
            colliderType = ColliderType.OBB;
            radius = 0;
            center = boxCollider.center;
            size = boxCollider.size;
        }

        public bool Equals(UnityEngine.SphereCollider sphereCollider)
        {
            
            return colliderType == ColliderType.Sphere&&radius == sphereCollider.radius&& center == sphereCollider.center;
        }

        public bool Equals(UnityEngine.CapsuleCollider capsuleCollider)
        {

            return colliderType == ColliderType.Capsule&&radius == capsuleCollider.radius &&
            center == capsuleCollider.center &&
            size == Vector3.one * capsuleCollider.height;
        }

        public bool Equals(BoxCollider boxCollider)
        {
            return colliderType == ColliderType.OBB&&
                radius == 0&&
            center == boxCollider.center&&
            size == boxCollider.size;
        }
    }
    public class ADBColliderReader : MonoBehaviour
    {
        public static Dictionary<int, ADBColliderReader> ColliderTokenDic
        {
            get
            {
                if (colliderTokenDic == null)
                {
                    colliderTokenDic = new Dictionary<int, ADBColliderReader>();
                    Application.quitting += () => colliderTokenDic = null;//OYM:只有退出的时候删除
                }
                return colliderTokenDic;
            }
        }
        private static Dictionary<int, ADBColliderReader> colliderTokenDic;
        public CollideFunc collideFunc = CollideFunc.OutsideLimit;
        public ColliderChoice colliderChoice=ColliderChoice.Other;

        private ColliderChecker colliderChecker;//OYM:用来检查collider有没有被改变,这该死的untiy连个委托都没有留给我....

        public ADBRuntimeCollider runtimeCollider;
        public List<ADBRuntimeController> owners = new List<ADBRuntimeController>();
        public Collider unityCollider;
        public bool isinitial { get; private set; }
        private UnityEngine.CapsuleCollider unityCapsuleCollider;
        private UnityEngine.SphereCollider unitySphereCollider;
        private BoxCollider unityBoxCollider;
        private string colliderType;

        private int id;

        public void Start()
        {
            colliderChecker=new ColliderChecker();
            unityCollider = GetComponent<Collider>();
            if (unityCollider == null) return;

            id = unityCollider.GetInstanceID();
            colliderType = unityCollider.GetType().Name;
            CheckAndBuildADBRuntimeCollider();

            if (runtimeCollider!=null)
            {
                ColliderTokenDic.Add(id, this);
            }

            isinitial = true;
        }
        private void FixedUpdate()
        {
            if (!isinitial) return;
            CheckAndBuildADBRuntimeCollider();
            runtimeCollider.UpdateColliderData();

        }

    private void OnEnable()
        {
            if (ColliderTokenDic!=null&&runtimeCollider != null&&! ColliderTokenDic.TryGetValue(id, out _))
            {
                ColliderTokenDic.Add(unityCollider.GetInstanceID(), this);
            }
        }

        private void OnDisable()
        {
            if (ColliderTokenDic != null && ColliderTokenDic.TryGetValue(id, out _))
            {
                ColliderTokenDic.Remove(unityCollider.GetInstanceID());
            }
            
        }
        public bool AddOwner(ADBRuntimeController target)
        {
            if (owners!=null&&target != null)
            {
                owners.Add(target);
                return true;
            }
            return false;
        }
        public bool RemoveOwner(ADBRuntimeController target)
        {
            if (owners != null && target != null)
            {
                return owners.Remove(target);
            }
            return false;
        }
        public bool IsOwner(ADBRuntimeController target)
        {
            if (owners != null&& owners.Count!=0)
            {
                return owners.Contains(target);
            }
            return true;
        }
        void CheckParentOwner()
        {
            var parentOwner = gameObject.GetComponentInParent<ADBRuntimeController>(true);
            if (parentOwner != null)
            {
                owners.Add(parentOwner);
            }
        }

        public void UpdatePriorities()
        {
            if (isinitial)
            {
                runtimeCollider.colliderRead.colliderChoice = colliderChoice;
                runtimeCollider.colliderRead.collideFunc = collideFunc;
            }
        }
        public void CheckAndBuildADBRuntimeCollider()
        {
            switch (colliderType)
            {
                case "SphereCollider":
                    BuildSphereCollider();
                    break;
                case "CapsuleCollider":
                    BuildCapsuleCollider();
                    break;
                case "BoxCollider":
                    BuildOBBCollider();
                    break;
                default:
                    Debug.Log(transform.name+" Cannot build Collider from " + colliderType);
                    break;
            }
        }


        void BuildSphereCollider()
        {
            if (unitySphereCollider == null)
            {
                unitySphereCollider = unityCollider as UnityEngine.SphereCollider;
            }
            if (colliderChecker.Equals(unitySphereCollider))//OYM:检查是否跟之前构建的一样
            { return; }

            colliderChecker = new ColliderChecker(unitySphereCollider);
            runtimeCollider = new ADBSphereCollider(unitySphereCollider.radius, unitySphereCollider.center, colliderChoice, unitySphereCollider.transform, collideFunc);//OYM:懒得写更改函数了,这部分麻烦的要死,直接new 吧,又不是每帧运行
            runtimeCollider.InitialColliderData();
        }


        void BuildCapsuleCollider()
        {
            if (unityCapsuleCollider == null)
            {
                unityCapsuleCollider = unityCollider as UnityEngine.CapsuleCollider;
            }
            if (colliderChecker.Equals(unityCapsuleCollider))//OYM:检查是否跟之前构建的一样
            { return; }

            colliderChecker = new ColliderChecker(unityCapsuleCollider);

            Quaternion direnction = Quaternion.identity;
            switch (unityCapsuleCollider.direction)
            {
                case 0:
                    direnction = Quaternion.Euler(0, 0, 90);
                    break;
                case 1:
                    direnction = Quaternion.identity;
                    break;
                case 2:
                    direnction = Quaternion.Euler(90, 0, 0);
                    break;
            }
            float trueHeight = unityCapsuleCollider.height - unityCapsuleCollider.radius * 2;
            trueHeight = trueHeight > 0 ? trueHeight : 0;
            Vector3 offset =   unityCapsuleCollider.transform.rotation *(unityCapsuleCollider.center- direnction * Vector3.up * trueHeight * 0.5f);
            runtimeCollider = new ADBCapsuleCollider(unityCapsuleCollider.radius, trueHeight, offset, direnction, colliderChoice, unityCapsuleCollider.transform, collideFunc);
            runtimeCollider.InitialColliderData();
        }
        

        void BuildOBBCollider()
        {
            if (unityBoxCollider == null)
            {
                unityBoxCollider = unityCollider as UnityEngine.BoxCollider;
            }
            if (colliderChecker.Equals(unityBoxCollider))//OYM:检查是否跟之前构建的一样
            { return; }
            colliderChecker = new ColliderChecker(unityBoxCollider);

            unityBoxCollider = unityCollider as UnityEngine.BoxCollider;
            Vector3 offset = unityBoxCollider.transform.rotation * unityBoxCollider.center;
            runtimeCollider = new OBBBoxCollider(offset, unityBoxCollider.size*0.5f, Quaternion.identity, colliderChoice, unityBoxCollider.transform, collideFunc);
            runtimeCollider.InitialColliderData();
        }
    }


}