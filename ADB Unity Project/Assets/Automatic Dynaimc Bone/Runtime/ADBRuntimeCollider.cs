using UnityEngine;
using System;
using Unity.Mathematics;

namespace ADBRuntime
{
    public enum ColliderType
    {
        Sphere=0,//OYM：现有的
        Capsule=1,//OYM：现有的
        OBB=2
    }

    //OYM：暂时没用
    public enum CollideFunc
    {
        /// <summary>
        /// 往外排斥,并且有边界
        /// </summary>
        OutsideLimit=1,
        /// <summary>
        /// 向内约束,并且有边界
        /// </summary>
        InsideLimit=2,
        /// <summary>
        /// 往外排斥,并且没有边界
        /// </summary>
        OutsideNoLimit = 3,
        /// <summary>
        /// 向内约束,并且没有边界
        /// </summary>
        InsideNoLimit = 4,
        /// <summary>
        /// 冻死在边界上
        /// </summary>
        Freeze = 5
    }

    [Serializable]
    public abstract class ADBRuntimeCollider
    {
        public ColliderRead colliderRead;

        public Transform appendTransform;
        public float3 initialScale;//OYM:用来解决Gizmo绘制问题
         
        public void UpdateColliderData()
        {
            colliderRead.scale = appendTransform.lossyScale;

            colliderRead.fromRotation = colliderRead.toRotation;
            colliderRead.toRotation = appendTransform.rotation;

            colliderRead.fromDirection = colliderRead.toDirection;
            colliderRead.toDirection = appendTransform.rotation * colliderRead.staticDirection;

            colliderRead.fromPosition = colliderRead.toPosition;
            colliderRead.toPosition = (float3)appendTransform.position + math.mul(colliderRead.toRotation, colliderRead.scale* colliderRead.positionOffset);
        }

        public void InitialColliderData()
        {
            colliderRead.scale = appendTransform.lossyScale;
            colliderRead.fromRotation = colliderRead.toRotation = appendTransform.rotation;
            colliderRead.fromDirection = colliderRead.toDirection = appendTransform.rotation * colliderRead.staticDirection;
            colliderRead.fromPosition = colliderRead.toPosition = (float3)appendTransform.position + math.mul(colliderRead.toRotation, colliderRead.positionOffset);
        }

        public abstract void OnDrawGizmos();
        

        public static void DrawWireArc(float radius, float angle)
        {
            Vector3 from = Vector3.forward * radius;
            var step = Mathf.RoundToInt(angle / 120.0f);
            for (int i = 0; i <= angle; i += step)
            {
                var rad = (float)i * Mathf.Deg2Rad;
                var to = new Vector3(radius * Mathf.Sin(rad), 0, radius * Mathf.Cos(rad));
                Gizmos.DrawLine(from, to);
                from = to;
            }
        }

    }

    public class ADBSphereCollider : ADBRuntimeCollider
    {
        public ADBSphereCollider(ColliderRead colliderRead, Transform appendtTransform)
        {
            this.colliderRead = colliderRead;
            this.appendTransform = appendtTransform;
        }
        public ADBSphereCollider(float radius, Vector3 positionOffset,ColliderChoice colliderChoice, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;
            colliderRead.originRadius = radius;

            colliderRead.colliderType = ColliderType.Sphere;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = (int)colliderChoice;
            if (appendTransform != null)
            {
                this.appendTransform = appendTransform;
                colliderRead.positionOffset =Quaternion.Inverse(appendTransform.rotation)* positionOffset;
                initialScale = appendTransform.lossyScale;
            }
            else
            {
                colliderRead.positionOffset = positionOffset;
                initialScale = 1;
            }

            colliderRead.CheckValue();
        }

        public override void OnDrawGizmos()
        {
            if (appendTransform)
            {
                Gizmos.DrawWireSphere(appendTransform.rotation * colliderRead.positionOffset *(appendTransform.lossyScale.x/ initialScale) +(float3) appendTransform.position, colliderRead.originRadius *(appendTransform.lossyScale.x / initialScale.x));
            }
            else
            {
                Gizmos.DrawWireSphere(colliderRead.positionOffset , colliderRead.originRadius );
            }
        }
    }

    public class ADBCapsuleCollider : ADBRuntimeCollider
    {
        public ADBCapsuleCollider(ColliderRead colliderRead, Transform appendtTransform)
        {
            this.colliderRead = colliderRead;
            this.appendTransform = appendtTransform;
        }
        public ADBCapsuleCollider(float radius, Vector3 pointHead, Vector3 pointTail, ColliderChoice colliderChoice,Transform appendTransform ,CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;
            colliderRead.colliderType = ColliderType.Capsule;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = (int)colliderChoice;
            colliderRead.originRadius = radius;
            colliderRead.originHeight = (pointHead - pointTail).magnitude;

            this.appendTransform = appendTransform;
            colliderRead.staticDirection = Quaternion.Inverse(appendTransform.rotation) * (pointTail - pointHead)/ colliderRead.originHeight;
            colliderRead.positionOffset = Quaternion.Inverse(appendTransform.rotation) * (pointHead - appendTransform.position);
            initialScale = appendTransform.lossyScale;

            colliderRead.CheckValue();

        }
        public ADBCapsuleCollider(float radius,float length, Vector3 positionOffset,Quaternion localRotation, ColliderChoice colliderChoice,Transform appendTransform , CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;
            colliderRead.colliderType = ColliderType.Capsule;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice =(int) colliderChoice;
            colliderRead.originRadius = radius;
            colliderRead.originHeight = length;
            this.appendTransform = appendTransform;
            colliderRead.staticDirection = localRotation * Vector3.up ;
            colliderRead.positionOffset = Quaternion.Inverse(appendTransform.rotation) * positionOffset;
            initialScale = appendTransform.lossyScale;

            colliderRead.CheckValue();
        }
        public override void OnDrawGizmos()
        {

            quaternion rot;
            float3 pos;
            float3 scale;
            if (appendTransform == null)
            {
                rot = Quaternion.LookRotation(colliderRead.staticDirection);
                pos = colliderRead.positionOffset;
                scale=1;
            }
            else
            {
                rot = appendTransform.rotation * Quaternion.FromToRotation(Vector3.up, colliderRead.staticDirection);
                scale = initialScale / appendTransform.lossyScale;
                pos = (float3)appendTransform.position + appendTransform.rotation * colliderRead.positionOffset* scale;
            }

            var mOld = Gizmos.matrix;//OYM：把旧的拿出来
            Gizmos.matrix = Matrix4x4.TRS(pos, rot,Vector3.one);//OYM：创造一个坐标矩阵
            float3 up = Vector3.up * colliderRead.originHeight*scale;
            float3 forward = Vector3.forward * colliderRead.originRadius* scale;
            float3 right = Vector3.right * colliderRead.originRadius* scale;

            Gizmos.DrawLine(forward, forward + up);
            Gizmos.DrawLine(-forward, -forward + up);
            Gizmos.DrawLine(right, right + up);
            Gizmos.DrawLine(-right, -right + up);
            float3 upPos = pos + math.mul(rot , up);

            Gizmos.matrix = Matrix4x4.TRS(pos, rot, scale);//OYM：创造一个坐标矩阵
            DrawWireArc(colliderRead.originRadius, 360);
            Gizmos.matrix = Matrix4x4.TRS(upPos, rot, scale);
            DrawWireArc(colliderRead.originRadius, 360);

            Gizmos.matrix = Matrix4x4.TRS(upPos, rot * Quaternion.AngleAxis(90, Vector3.forward), scale);//OYM： 翻转,然后画圆,就是头尾周围那几条插插
            DrawWireArc(colliderRead.originRadius, 180);//OYM：这里不用看了
            Gizmos.matrix = Matrix4x4.TRS(upPos, rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward), scale);
            DrawWireArc(colliderRead.originRadius, 180);
            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward), scale);
            DrawWireArc(colliderRead.originRadius, 180);
            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(-90, Vector3.forward), scale);
            DrawWireArc(colliderRead.originRadius, 180);

            Gizmos.matrix = mOld;//OYM：记得给它还回去

        }
    }

    public class OBBBoxCollider : ADBRuntimeCollider
    {

        public OBBBoxCollider(ColliderRead colliderRead, Transform appendtTransform)
        {
            this.colliderRead = colliderRead;
            this.appendTransform = appendtTransform;
        }
        public OBBBoxCollider(Vector3 center, Vector3 size, Vector3 direction, ColliderChoice colliderChoice, Transform appendTransform, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;

            this.appendTransform = appendTransform;
            colliderRead.staticRotation =appendTransform.rotation * Quaternion.FromToRotation(Vector3.up, direction);
            colliderRead.positionOffset =Quaternion.Inverse(appendTransform.rotation) * (center- appendTransform.position) ;
            colliderRead.originBoxSize = new Vector3(Mathf.Abs(size.x ), Mathf.Abs(size.y), Mathf.Abs(size.z ));
            colliderRead.colliderType = ColliderType.OBB;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = (int)colliderChoice;
            initialScale = appendTransform? appendTransform.lossyScale: Vector3.one;

            colliderRead.CheckValue();
        }
        public OBBBoxCollider(Vector3 center, Vector3 size, Quaternion rotation, ColliderChoice colliderChoice, Transform appendTransform , CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;

            this.appendTransform = appendTransform;
            colliderRead.staticRotation =  appendTransform.rotation * rotation;
            colliderRead.positionOffset = Quaternion.Inverse(appendTransform.rotation) * center;
            colliderRead.originBoxSize = math.abs(size);
            colliderRead.colliderType = ColliderType.OBB;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = (int)colliderChoice;
            initialScale = appendTransform ? appendTransform.lossyScale : Vector3.one;

            colliderRead.CheckValue();
        }
        public override void OnDrawGizmos()
        {
            Matrix4x4 before = Gizmos.matrix;
            if (appendTransform)
            {
                Gizmos.matrix = Matrix4x4.TRS(appendTransform.position + appendTransform.rotation *( colliderRead.positionOffset * (appendTransform.lossyScale/ initialScale)), appendTransform.rotation * colliderRead.staticRotation, appendTransform.lossyScale);
                Gizmos.DrawWireCube(Vector3.zero, colliderRead.originBoxSize * 2);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(colliderRead.positionOffset, colliderRead.staticRotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, colliderRead.originBoxSize * 2);
            }
            Gizmos.DrawLine(Vector3.zero, Vector3.up);

            Gizmos.matrix = before;
        }
    }
    [System.Serializable]
    public struct ColliderRead 
    {
        public bool isOpen;

        public ColliderType colliderType;
        public CollideFunc collideFunc;
        public int colliderChoice;
        public float3 positionOffset;
        public quaternion staticRotation;
        public float3 staticDirection;

        /// <summary>
        /// 半尺寸
        /// </summary>


        //OYM:从ColliderRead里面迁移过来数据，目前是Readonly的
        public float3 fromPosition;
        public float3 toPosition;
        public float3 deltaPosition;

        public float3 fromDirection;
        public float3 toDirection;
        public float3 deltaDirection;

        public quaternion fromRotation;
        public quaternion toRotation;
        public quaternion deltaRotation;

        public float3 originBoxSize;
        public float originRadius;
        public float originHeight;

        public float3 scale;

        public MinMaxAABB AABB;

        public void CheckValue()
        {
            originRadius = originRadius < 0 ? 0 : originRadius;
            originHeight = originHeight < 0 ? 0 : originHeight;
            if (Application.isPlaying)
            {
                if (originHeight == 0 && colliderType == ColliderType.Capsule)
                {
                    colliderType = ColliderType.Sphere;
                }
            }
        }
    }
    public struct ColliderReadWrite
    {
        public ColliderType colliderType;
        public CollideFunc collideFunc;

        public float3 position;
        public float3 direction; //OYM:胶囊体的单位长度朝向
        public quaternion rotation;

        //OYM:sphere radius :x 
        //OYM:capsule radius:x,height:y
        //OYM:box size:xyz
        public float3 size;
    }


}//OYM：写死我了....历时四个月有余
/*
class SphereComBine : ADBRuntimeCllider
{
    float radius;
    public SphereComBine(float radiu, float thickness, float curvature, Vector3 center, Vector3 direction, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
    {
        //OYM：A*B=radius^2,A/B=curvature
        this.radius = radiu;
        this.appendTransform = appendTransform;
        //colliderRead.colliderType = ColliderType.SphereCombine;
        colliderRead.collideFunc = collideFunc;
        float A1 = Mathf.Sqrt(radiu * radiu * curvature);
        float A2 = A1 < thickness ? 0.001f : A1 - thickness;
        float B1 = radiu * radiu / A1;
        float B2 = radiu * radiu / A2;
        colliderRead.lengthA = (A1 + B1) * 0.5f;
        colliderRead.lengthB = (A2 + B2) * 0.5f;

        colliderRead.pointB = appendTransform ? (center - (B1 - colliderRead.lengthA) * direction) : appendTransform.InverseTransformPoint((center - (B1 - colliderRead.lengthA) * direction));
        colliderRead.pointC = appendTransform ? (center - (B2 - colliderRead.lengthB) * direction) : appendTransform.InverseTransformPoint((center - (B2 - colliderRead.lengthB) * direction));

    }

    public override void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        var mOld = Gizmos.matrix;//OYM：把旧的拿出来

        Gizmos.matrix = Matrix4x4.TRS(appendTransform.position + appendTransform.rotation * colliderRead.positionOffset, appendTransform.rotation, Vector3.one);//OYM：创造一个坐标矩阵
        DrawWireArc(radius, 360);

        Gizmos.matrix = mOld;

        Gizmos.DrawWireSphere(colliderRead.pointA, colliderRead.lengthA);
        Gizmos.DrawWireSphere(colliderRead.pointB, colliderRead.lengthB);
    }
}
*/
