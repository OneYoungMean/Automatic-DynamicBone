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
    public class ADBRuntimeCollider
    {
        public ColliderRead colliderRead;
        public ColliderReadWrite colliderReadWrite;
        public Transform appendTransform;
        public bool isDraw;
        internal ADBRuntimeCollider()
        { }
        public ColliderRead GetColliderRead()
        {
            ColliderRead mirror = colliderRead;

            if (appendTransform != null)
            {
                mirror.radius *= appendTransform.lossyScale.x;
                mirror.staticDirection *= appendTransform.lossyScale.x;
                mirror.positionOffset *= appendTransform.lossyScale.x;
            }
            return mirror;
        }
        /*
         全部木大,交给jobs处理
        public ColliderReadWrite GetColliderReadWrite()
        {
            if (appendTransform)
            {
                colliderReadWrite.position = appendTransform.position;
                colliderReadWrite.direction = appendTransform.rotation * colliderRead.staticDirection;
                colliderReadWrite.normal = Quaternion.Inverse(appendTransform.rotation) * colliderRead.staticNormal;//OYM：这里注意一下,这个变量是专门给obb盒用的,所以乘以一个inverse
                //OYM：不过说起来AB-1不应该是B-1A-1嘛....但是实际上A-1*B-1也可以
                //OYM：难道是因为对角矩阵的原因?

            }
            else
            {
                colliderReadWrite = default(ColliderReadWrite);
            }

            return colliderReadWrite;
        }
        */
        public virtual void OnDrawGizmos() { }
        

        public void DrawWireArc(float radius, float angle)
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

    public class SphereCollider : ADBRuntimeCollider
    {
        public SphereCollider(ColliderRead colliderRead, Transform appendtTransform)
        {
            this.colliderRead = colliderRead;
            this.appendTransform = appendtTransform;
        }
        public SphereCollider(float radius, Vector3 positionOffset,ColliderChoice colliderChoice, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;
            colliderRead.radius = radius;

            colliderRead.colliderType = ColliderType.Sphere;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = colliderChoice;
            if (appendTransform != null)
            {
                this.appendTransform = appendTransform;
                colliderRead.positionOffset =positionOffset;
            }
            else
            {
                colliderRead.positionOffset = positionOffset;
            }
        }

        public override void OnDrawGizmos()
        {
            if (!isDraw) return;

            if (appendTransform)
            {
                Gizmos.DrawWireSphere(appendTransform.rotation * colliderRead.positionOffset * appendTransform.lossyScale.x + appendTransform.position, colliderRead.radius* appendTransform.lossyScale.x);
            }
            else
            {
                Gizmos.DrawWireSphere(colliderRead.positionOffset * appendTransform.lossyScale.x, colliderRead.radius * appendTransform.lossyScale.x);
            }
   
        }

    }

    public class CapsuleCollider : ADBRuntimeCollider
    {
        public CapsuleCollider(ColliderRead colliderRead, Transform appendtTransform)
        {
            this.colliderRead = colliderRead;
            this.appendTransform = appendtTransform;
        }
        public CapsuleCollider(float radius, Vector3 pointHead, Vector3 pointTail, ColliderChoice colliderChoice,Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;
            colliderRead.colliderType = ColliderType.Capsule;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = colliderChoice;
            colliderRead.radius = radius;
            colliderRead.length = (pointHead - pointTail).magnitude;
            if (appendTransform != null)
            {
                this.appendTransform = appendTransform;
                colliderRead.staticDirection = Quaternion.Inverse(appendTransform.rotation) * (pointTail - pointHead);
                colliderRead.positionOffset = appendTransform.InverseTransformPoint(pointHead);
            }
            else
            {
                colliderRead.positionOffset = pointHead;
                colliderRead.staticDirection = pointTail - pointHead;
            }
        }
        public CapsuleCollider(float radius,float length, Vector3 positionOffset,Quaternion localRotation, ColliderChoice colliderChoice,Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;
            colliderRead.colliderType = ColliderType.Capsule;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = colliderChoice;
            colliderRead.radius = radius;
            colliderRead.length = length;
            if (appendTransform != null)
            {
                this.appendTransform = appendTransform;
                colliderRead.staticDirection = localRotation* Vector3.up* length;
                colliderRead.positionOffset = Quaternion.Inverse(appendTransform.rotation) *positionOffset;
            }
            else
            {
                colliderRead.positionOffset = positionOffset;
                colliderRead.staticDirection = Quaternion.Inverse(localRotation) * Vector3.up;
            }
        }
        public override void OnDrawGizmos()
        {
            if (!isDraw) return;

            Quaternion rot;
            Vector3 pos;
            if (appendTransform == null)
            {
                rot = Quaternion.LookRotation(colliderRead.staticDirection);
                pos = colliderRead.positionOffset;
            }
            else
            {
                rot = appendTransform.rotation * Quaternion.FromToRotation(Vector3.up, colliderRead.staticDirection);
                pos = appendTransform.position + appendTransform.rotation * colliderRead.positionOffset* appendTransform.lossyScale.x;
            }

            var mOld = Gizmos.matrix;//OYM：把旧的拿出来
            Gizmos.matrix = Matrix4x4.TRS(pos, rot,Vector3.one);//OYM：创造一个坐标矩阵
            float scale = appendTransform.lossyScale.x;
            Vector3 up = Vector3.up * colliderRead.length* scale;
            Vector3 forward = Vector3.forward * colliderRead.radius* scale;
            Vector3 right = Vector3.right * colliderRead.radius* scale;

            Gizmos.DrawLine(forward, forward + up);
            Gizmos.DrawLine(-forward, -forward + up);
            Gizmos.DrawLine(right, right + up);
            Gizmos.DrawLine(-right, -right + up);
            var upPos = pos + rot * up;

            Gizmos.matrix = Matrix4x4.TRS(pos, rot, appendTransform.lossyScale);//OYM：创造一个坐标矩阵
            DrawWireArc(colliderRead.radius, 360);
            Gizmos.matrix = Matrix4x4.TRS(upPos, rot, appendTransform.lossyScale);
            DrawWireArc(colliderRead.radius, 360);

            Gizmos.matrix = Matrix4x4.TRS(upPos, rot * Quaternion.AngleAxis(90, Vector3.forward), appendTransform.lossyScale);//OYM： 翻转,然后画圆,就是头尾周围那几条插插
            DrawWireArc(colliderRead.radius, 180);//OYM：这里不用看了
            Gizmos.matrix = Matrix4x4.TRS(upPos, rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward), appendTransform.lossyScale);
            DrawWireArc(colliderRead.radius, 180);
            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward), appendTransform.lossyScale);
            DrawWireArc(colliderRead.radius, 180);
            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(-90, Vector3.forward), appendTransform.lossyScale);
            DrawWireArc(colliderRead.radius, 180);

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
        public OBBBoxCollider(Vector3 center, Vector3 size, Vector3 direction, ColliderChoice colliderChoice, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;

            this.appendTransform = appendTransform;
            colliderRead.staticRotation = appendTransform ? appendTransform.rotation * Quaternion.FromToRotation(Vector3.up, direction) : Quaternion.FromToRotation(Vector3.up, direction);
            colliderRead.positionOffset = appendTransform ? appendTransform.InverseTransformPoint(center) : center;
            colliderRead.boxSize = new Vector3(Mathf.Abs(size.x ), Mathf.Abs(size.y), Mathf.Abs(size.z ));
            colliderRead.colliderType = ColliderType.OBB;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = colliderChoice;
        }
        public OBBBoxCollider(Vector3 center, Vector3 size, Quaternion rotation, ColliderChoice colliderChoice, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.OutsideLimit)
        {
            colliderRead.isOpen = true;

            this.appendTransform = appendTransform;
            colliderRead.staticRotation = appendTransform ? appendTransform.rotation * rotation:rotation;
            colliderRead.positionOffset = appendTransform ? appendTransform.InverseTransformPoint(center) : center;
            colliderRead.boxSize = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            colliderRead.colliderType = ColliderType.OBB;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = colliderChoice;
        }
        public override void OnDrawGizmos()
        {
            if (!isDraw) return;

            Matrix4x4 before = Gizmos.matrix;
            if (appendTransform)
            {
                Gizmos.matrix = Matrix4x4.TRS(appendTransform.position + appendTransform.rotation * colliderRead.positionOffset * appendTransform.lossyScale.x, appendTransform.rotation * colliderRead.staticRotation, appendTransform.lossyScale);
                Gizmos.DrawWireCube(Vector3.zero, colliderRead.boxSize * 2);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(colliderRead.positionOffset, colliderRead.staticRotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, colliderRead.boxSize * 2);
            }
            Gizmos.DrawLine(Vector3.zero, Vector3.up);

            Gizmos.matrix = before;
        }
    }
    [System.Serializable]
    public struct ColliderRead : System.IEquatable<ColliderRead>
    {
        public bool isOpen;

        public ColliderType colliderType;
        public CollideFunc collideFunc;
        public ColliderChoice colliderChoice;
        public float3 positionOffset;
        public quaternion staticRotation;
        public float3 staticDirection;
        /// <summary>
        /// 半尺寸
        /// </summary>
        public float3 boxSize; 
        //public Vector3 pointB;
        //public Vector3 pointC;

        public float radius;
        public float length;
        public bool isConnectWithBody;

        public bool Equals(ColliderRead other)
        {
            return other.isOpen == isOpen &&
                other.colliderType == colliderType &&
                other.collideFunc == collideFunc &&
                other.colliderChoice == colliderChoice &&
                other.positionOffset .Equals( positionOffset) &&
                other.staticRotation .Equals( staticRotation) &&
                other.staticDirection .Equals(staticDirection) &&
                other.boxSize.Equals( boxSize) &&
                other.radius == radius &&
                other.length == length &&
                other.isConnectWithBody == isConnectWithBody;
        }

        public void CheckValue()
        {
            radius = radius < 0 ? 0 : radius;
            length = length < 0 ? 0 : length;
            if (Application.isPlaying)
            {
                if (length == 0 && colliderType == ColliderType.Capsule)
                {
                    colliderType = ColliderType.Sphere;
                }
            }
            staticDirection = math.all(staticDirection == float3.zero) ? new float3(0, 1, 0) : math.normalize(staticDirection); //OYM:float3.up
            staticRotation = Quaternion.FromToRotation(Vector3.up, staticDirection);
            if (((int)colliderChoice) == 0)
            {
                colliderChoice = ColliderChoice.Other;
            }
        }
    }
    public struct ColliderReadWrite
    {
        public float3 position;
        public float3 direction; 
        public quaternion rotation;
        public float3 deltaPosition;
        public float3 deltaDirection;
        public quaternion deltaRotation;
        public MinMaxAABB AABB;
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
