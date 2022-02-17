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
    public class ADBColliderReader :MonoBehaviour, IADBPhysicMonoComponent
    {
        public bool isReadOnly;
        public bool isStatic;
        private Vector3 initialSize;
        public MonoBehaviour Target => this;

        public CollideFunc collideFunc = CollideFunc.OutsideLimit;
        public ColliderChoice colliderMask=ColliderChoice.Other;

        internal void Resize(float colliderSize)
        {
            transform.localScale = colliderSize*initialSize;
        }

        private ColliderChecker colliderChecker;//OYM:用来检查collider有没有被改变,这该死的untiy连个委托都没有留给我....

        public ADBRuntimeCollider runtimeCollider;
        public Collider unityCollider;
        public string colliderType;

        private UnityEngine.CapsuleCollider unityCapsuleCollider;
        private UnityEngine.SphereCollider unitySphereCollider;
        private BoxCollider unityBoxCollider;


        private int id;

        public void Awake()
        {
            isReadOnly |= gameObject.isStatic;
            isStatic |= gameObject.isStatic;
            initialSize = transform.localScale;
        }
        public void UpdateCollider()
        {
            if (!isReadOnly)
            {
                CheckAndBuildADBRuntimeCollider();
            }
            if (!isStatic)
            {
                runtimeCollider.UpdateColliderData();
            }


        }

    private void OnEnable()
        {
            CheckAndBuildADBRuntimeCollider();
        }

        private void OnDisable()
        {

            if (ColliderTokenDic.TryGetValue(id, out _))
            {
                ColliderTokenDic.Remove(id);
                id = 0;
            }
           
        }

        public void UpdatePriorities()
        {
            if (unityCollider!=null)
            {
                runtimeCollider.colliderRead.colliderChoice = (int)colliderMask;
                runtimeCollider.colliderRead.collideFunc = collideFunc;
            }
        }
        public bool CheckAndBuildADBRuntimeCollider()
        {
            if (unityCollider == null && (!TryGetComponent<Collider>(out unityCollider) || !unityCollider.enabled))//OYM:获取不到或者没打开
            {
                if (ColliderTokenDic.TryGetValue(id,out _))
                {
                    ColliderTokenDic.Remove(id);
                    id = 0;
                }
                return false;
            }
            else
            {
                if (id==0)
                {
                    id = unityCollider.GetInstanceID();
                }
                if (!ColliderTokenDic.TryGetValue(id, out _))
                {
                    colliderType = unityCollider.GetType().Name;
                    ColliderTokenDic.Add(id,this);
                }


                switch (colliderType)
                {
                    case "SphereCollider":
                        return CheckOrBuildSphereCollider();

                    case "CapsuleCollider":
                        return CheckOrBuildCapsuleCollider();

                    case "BoxCollider":
                        return CheckOrBuildOBBCollider();

                    default:
                        Debug.Log(transform.name + " Cannot build Collider from " + colliderType);
                        return false;
                }
            }

        }

        bool CheckOrBuildSphereCollider()
        {
            if (unitySphereCollider == null)
            {
                unitySphereCollider = unityCollider as UnityEngine.SphereCollider;
            }
            if (colliderChecker.Equals(unitySphereCollider))//OYM:检查是否跟之前构建的一样
            { return false; }

            colliderChecker = new ColliderChecker(unitySphereCollider);
            runtimeCollider = new ADBSphereCollider(unitySphereCollider.radius, unitySphereCollider.center, colliderMask, unitySphereCollider.transform, collideFunc);//OYM:懒得写更改函数了,这部分麻烦的要死,直接new 吧,又不是每帧运行
            runtimeCollider.InitialColliderData();
            return true;
        }


        bool CheckOrBuildCapsuleCollider()
        {
            if (unityCapsuleCollider == null)
            {
                unityCapsuleCollider = unityCollider as UnityEngine.CapsuleCollider;
            }
            if (colliderChecker.Equals(unityCapsuleCollider))//OYM:检查是否跟之前构建的一样
            { return false; }

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
            runtimeCollider = new ADBCapsuleCollider(unityCapsuleCollider.radius, trueHeight, offset, direnction, colliderMask, unityCapsuleCollider.transform, collideFunc);
            runtimeCollider.InitialColliderData();

            return true;
        }
        

        bool CheckOrBuildOBBCollider()
        {
            if (unityBoxCollider == null)
            {
                unityBoxCollider = unityCollider as UnityEngine.BoxCollider;
            }
            if (colliderChecker.Equals(unityBoxCollider))//OYM:检查是否跟之前构建的一样
            { return false; }
            colliderChecker = new ColliderChecker(unityBoxCollider);

            unityBoxCollider = unityCollider as UnityEngine.BoxCollider;
            Vector3 offset = unityBoxCollider.transform.rotation * unityBoxCollider.center;
            runtimeCollider = new OBBBoxCollider(offset, unityBoxCollider.size*0.5f, Quaternion.identity, colliderMask, unityBoxCollider.transform, collideFunc);
            runtimeCollider.InitialColliderData();

            return true;
        }
    }


}