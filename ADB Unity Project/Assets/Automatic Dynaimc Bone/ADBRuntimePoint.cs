using UnityEngine;
using System.Collections.Generic;

namespace ADBRuntime
{
    public class ADBRuntimePoint
    {
        public PointRead pointRead;
        public PointReadWrite pointReadWrite;
        public float pointDepthRateMaxPointDepth;

        public Transform trans { get;private set; }
        public ADBRuntimePoint parent { get;set; }
        public List<ADBRuntimePoint> childNode { get; set; }//OYM：子节点 
        public bool isFixed { get; private set; }//OYM：是否固定
        public string keyWord { get; private set; }//OYM：匹配的关键词
        public int depth { get; private set; }//OYM：深度
        public int index { get; set; }//OYM：序号
        public bool isAllowComputeOtherConstraint
        { get; private set; }

        public ADBRuntimePoint(Transform trans, int depth, string keyWord = null, bool isAllowComputeOtherConstraint = false)
        {

            if (keyWord == null )//OYM：root点(只起一个逻辑点的作用)
            {
                this.trans = trans;
                this.depth = depth;
            }
            else
            {
                this.trans = trans;
                this.keyWord = keyWord;
                this.depth = depth;
                this.isFixed = depth == 0;
                pointRead = new PointRead();
                pointReadWrite = new PointReadWrite();
            }
        }
        internal void OnDrawGizmos(Mono.ColliderCollisionType colliderCollisionType)
        {
            Gizmos.color = isAllowComputeOtherConstraint ? Color.grey: Color.black;
            if (pointRead.radius > 0.005f&&( colliderCollisionType==Mono.ColliderCollisionType.Point|| colliderCollisionType == Mono.ColliderCollisionType.Both))
            {
                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(trans.position, Quaternion.FromToRotation(Vector3.up, pointRead.initialLocalPosition) * trans.rotation, trans.lossyScale);
                Gizmos.DrawWireSphere(Vector3.zero, pointRead.radius);
                Gizmos.matrix = temp;

            }
            else
            {
                Gizmos.DrawSphere(trans.position, 0.005f);//OYM：都说了画点用的
            }
        }
    }

    public struct PointRead
    {
        public bool isFixGravityAxis;
        public int fixedIndex;
        /// <summary>
        /// 父节点序号
        /// </summary>
        public int parentIndex;
        /// <summary>
        /// 子节点开头编号
        /// </summary>
        public int childFirstIndex;
        /// <summary>
        /// 子节点结尾编号
        /// </summary>
        public int childLastIndex;
        /// <summary>
        ///1/ 重量（似乎并没有是没用）
        /// </summary>);
        public float weight;
        /// <summary>
        /// 怠速,防止头发末梢跟美杜莎一样到处乱动，默认为1
        /// </summary>);
        public float mass;
        /// <summary>
        /// 风阻，默认为1
        /// </summary>);
        public float moveByFixedPoint;
        /// <summary>
        /// 摩擦力大小(对于collider来说)
        /// </summary>);
        public float friction;
        /// <summary>
        ///  暂时不用
        /// </summary>
        public float moveByPrePoint;
        /// <summary>
        /// 冻结,使骨骼回到原来的位置上的力度
        /// </summary>
        public float freeze;
        /// <summary>
        /// Collider选择性对撞
        /// </summary>
        public ColliderChoice colliderChoice;
        public float structuralShrinkVertical;
        public float structuralStretchVertical;
        public float structuralShrinkHorizontal;
        public float structuralStretchHorizontal;
        public float shearShrink;
        public float shearStretch;
        public float bendingShrinkVertical;
        public float bendingStretchVertical;
        public float bendingShrinkHorizontal;
        public float bendingStretchHorizontal;
        public float circumferenceShrink;
        public float circumferenceStretch;
        public float radius;
        public Vector3 gravity;
        public Vector3 initialLocalPosition;
        public Vector3 initialPosition;//OYM：并不是直接的position ,二是相对于fixed点的position;


        internal float addForceScale;
        internal float distanceCompensation;
        internal Quaternion initialLocalRotation;
        internal Quaternion initialRotation;
        internal float velocityLimit;
    }

    //OYM：写入系统
    public struct PointReadWrite
    {
        public Vector3 position;
        public Quaternion rotation;
        public Quaternion rotationY;
        /* { get { return Position; }
            set {
                if (float.IsNaN(value.x))
                {
                    Position = Vector3.zero;
                }
                else
                {
                    Position = value;
                }
            } }*/

        public Vector3 deltaPosition;
        public Quaternion deltaRotation;
        public Quaternion deltaRotationY;
    }
}
