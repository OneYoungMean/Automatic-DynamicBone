using UnityEngine;
using System;

namespace ADBRuntime
{
    public enum ColliderType
    {
        Sphere,//OYM：现有的
        Capsule,//OYM：现有的
        OBB
    }

    //OYM：暂时没用
    public enum CollideFunc
    {
        /// <summary>
        /// 体积会往外排斥,面会往法线方向排斥
        /// </summary>
        Outside=1,
        /// <summary>
        /// 体积会向内约束,面会往法线相反的方向排斥
        /// </summary>
        Inside=2,
        /// <summary>
        /// 所有的点都会被限制在面上或者体的外壳上
        /// </summary>
        Freeze=4,
        /// <summary>
        /// 嘿,它会给物体的两面都会碰撞,但是再碰撞之前它都是可以任意在里面或者外面
        /// </summary>
    }
    [Serializable]
    public  class ADBRuntimeCollider
    {
        public ColliderRead colliderRead;
        public ColliderReadWrite colliderReadWrite;
        public Transform appendTransform;

        public ColliderRead GetColliderRead()
        {
            return colliderRead;
        }
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
        public SphereCollider(float radius, Vector3 positionOffset,ColliderChoice colliderChoice, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.Outside)
        {
            colliderRead.radius = radius;

            colliderRead.colliderType = ColliderType.Sphere;
            colliderRead.collideFunc = CollideFunc.Outside;
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
            if (appendTransform)
            {
                Gizmos.DrawWireSphere(appendTransform.rotation * colliderRead.positionOffset + appendTransform.position, colliderRead.radius);
            }
            else
            {
                Gizmos.DrawWireSphere(colliderRead.positionOffset, colliderRead.radius);
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
        public CapsuleCollider(float radius, Vector3 pointHead, Vector3 pointTail, ColliderChoice colliderChoice,Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.Outside)
        {

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
        public CapsuleCollider(float radius,float length, Vector3 positionOffset,Vector3 direction, ColliderChoice colliderChoice,Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.Outside)
        {

            colliderRead.colliderType = ColliderType.Capsule;
            colliderRead.collideFunc = collideFunc;
            colliderRead.colliderChoice = colliderChoice;
            colliderRead.radius = radius;
            colliderRead.length = length;
            if (appendTransform != null)
            {
                this.appendTransform = appendTransform;
                colliderRead.staticDirection = Quaternion.Inverse(appendTransform.rotation) * (direction == Vector3.zero ? Vector3.up : direction);
                colliderRead.positionOffset = Quaternion.Inverse(appendTransform.rotation)*positionOffset;
            }
            else
            {
                colliderRead.positionOffset = positionOffset;
                colliderRead.staticDirection = direction==Vector3.zero?Vector3.up:direction;
            }
        }
        public override void OnDrawGizmos()
        {
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
                pos = appendTransform.position + appendTransform.rotation * colliderRead.positionOffset;
            }

            var mOld = Gizmos.matrix;//OYM：把旧的拿出来
            Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);//OYM：创造一个坐标矩阵
            Vector3 up = Vector3.up * colliderRead.length;
            Vector3 forward = Vector3.forward * colliderRead.radius;
            Vector3 right = Vector3.right * colliderRead.radius;

            Gizmos.DrawLine(forward, forward + up);
            Gizmos.DrawLine(-forward, -forward + up);
            Gizmos.DrawLine(right, right + up);
            Gizmos.DrawLine(-right, -right + up);
            var upPos = pos + rot * up;

            DrawWireArc(colliderRead.radius, 360);
            Gizmos.matrix = Matrix4x4.TRS(upPos, rot, Vector3.one);
            DrawWireArc(colliderRead.radius, 360);

            Gizmos.matrix = Matrix4x4.TRS(upPos, rot * Quaternion.AngleAxis(90, Vector3.forward), Vector3.one);//OYM： 翻转,然后画圆,就是头尾周围那几条插插
            DrawWireArc(colliderRead.radius, 180);//OYM：这里不用看了
            Gizmos.matrix = Matrix4x4.TRS(upPos, rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(90, Vector3.forward), Vector3.one);
            DrawWireArc(colliderRead.radius, 180);
            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, Vector3.forward), Vector3.one);
            DrawWireArc(colliderRead.radius, 180);
            Gizmos.matrix = Matrix4x4.TRS(pos, rot * Quaternion.AngleAxis(-90, Vector3.forward), Vector3.one);
            DrawWireArc(colliderRead.radius, 180);

            Gizmos.matrix = mOld;//OYM：记得给它还回去

        }
    }

    public class OBBBox : ADBRuntimeCollider
    {
        Vector3 OBBposition;
        Quaternion OBBRotation;
        public OBBBox(ColliderRead colliderRead, Transform appendtTransform)
        {
            this.colliderRead = colliderRead;
            this.appendTransform = appendtTransform;
            OBBposition = colliderRead.positionOffset;
            OBBRotation = colliderRead.staticNormal;
        }
        public OBBBox(Vector3 center, Vector3 range, Vector3 direction, ColliderChoice colliderChoice, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.Outside)
        {
            OBBposition = appendTransform ? appendTransform.InverseTransformPoint(center) : center;
            OBBRotation = appendTransform ? appendTransform.rotation * Quaternion.FromToRotation(Vector3.up, direction) : Quaternion.FromToRotation(Vector3.up, direction);
            this.appendTransform = appendTransform;
            colliderRead.staticNormal = OBBRotation;
            colliderRead.positionOffset = OBBposition;
            colliderRead.boxSize = range * 0.5f;
            colliderRead.colliderType = ColliderType.OBB;
            colliderRead.collideFunc = CollideFunc.Outside;
            colliderRead.colliderChoice = colliderChoice;
        }

        public override void OnDrawGizmos()
        {
            Matrix4x4 before = Gizmos.matrix;
            if (appendTransform)
            {
                Gizmos.matrix = Matrix4x4.TRS(appendTransform.position + OBBposition, appendTransform.rotation * OBBRotation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, colliderRead.boxSize * 2);
            }
            else
            {
                Gizmos.matrix = Matrix4x4.TRS(OBBposition, OBBRotation, Vector3.one);
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
        public Vector3 positionOffset;
        public Quaternion staticNormal;
        public Vector3 staticDirection;

        public Vector3 boxSize;
        //public Vector3 pointB;
        //public Vector3 pointC;

        public float radius;
        public float length;
        //public float lengthC;

        //public float friction;
        //OYM：too more exprience
        public bool Equals(ColliderRead other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        public void CheckValue()
        {
            radius = radius < 0 ? 0 : radius;
            length = length < 0 ? 0 : length;
            boxSize = boxSize.normalized * boxSize.magnitude;
            staticDirection = staticDirection == Vector3.zero ? Vector3.up : staticDirection;
            staticNormal = Quaternion.FromToRotation(Vector3.up, staticDirection);
            if (((int)colliderChoice) == 0)
            {
                colliderChoice = ColliderChoice.Other;
            }
        }
    }
    public struct ColliderReadWrite
    {
        public Vector3 position;
        public Vector3 direction;
        public Quaternion normal;
        public Vector3 positionForward;
        public Vector3 directionForward;
        public Quaternion normalForward;
    }
}//OYM：写死我了....历时四个月有余
/*
class SphereComBine : ADBRuntimeCllider
{
    float radius;
    public SphereComBine(float radiu, float thickness, float curvature, Vector3 center, Vector3 direction, Transform appendTransform = null, CollideFunc collideFunc = CollideFunc.Outside)
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
