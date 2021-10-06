using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ADBRuntime
{
    public class ADBRuntimePoint
    {
        public PointRead pointRead;
        public PointReadWrite pointReadWrite;
        public float pointDepthRateMaxPointDepth;
        public float3 initialScale;
        public Transform trans { get;private set; }
        public ADBRuntimePoint parent { get;set; }
        public List<ADBRuntimePoint> childNode { get; set; }//OYM：子节点 
        public bool isFixed { get { return depth == 0; } }//OYM：是否固定
        public string keyWord { get; private set; }//OYM：匹配的关键词
        public int depth { get; private set; }//OYM：深度
        public int index { get; set; }//OYM：序号
        public bool allowCreateAllConstraint
        { get; private set; }

        public ADBRuntimePoint(Transform trans, int depth, string keyWord = null, bool isAllowComputeOtherConstraint = true)
        {
            this.trans = trans;
            this.keyWord = keyWord;
            this.depth = depth;
            pointRead = new PointRead();
            pointReadWrite = new PointReadWrite();
            this.allowCreateAllConstraint = isAllowComputeOtherConstraint;
            initialScale = trans.lossyScale;
        }
        internal void OnDrawGizmos()
        {
            Gizmos.color = allowCreateAllConstraint ?  Color.black: new Color(0.3f,1f,0.3f,1);
            if (pointRead.radius > 0.005f)
            {
                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(trans.position, Quaternion.FromToRotation(Vector3.up, pointRead.initialLocalPosition) * trans.rotation, trans.lossyScale/ initialScale);
                Gizmos.DrawWireSphere(Vector3.zero, pointRead.radius);
                Gizmos.matrix = temp;

            }
            else
            {
                Gizmos.DrawSphere(trans.position, 0.005f);//OYM：都说了画点用的
            }
        }
       public void SetDepth(int i )
        {
            depth = i;
        }
    }


    public struct PointRead
    {
        //public bool isFixGravityAxis;
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
        /// 子节点结尾编号的后一个编号
        /// </summary>
        public int childLastIndex;
        /// <summary>
        ///质量,杆件拉伸时候计算相互作用力使用
        /// </summary>);
        public float weight;

        /// <summary>
        /// Collider选择性对撞
        /// </summary>
        public ColliderChoice colliderChoice;
        /// <summary>
        /// 固定节点是否发生旋转?
        /// </summary>
        internal bool isFixedPointFreezeRotation;
        /// <summary>
        /// 怠速,防止头发末梢跟美杜莎一样到处乱动，默认为1
        /// </summary>);
        public float mass;
        /// <summary>
        /// 风阻，默认为1
        /// </summary>);
        public float moveByFixedPoint;
        /// <summary>
        /// 摩擦力大小,计算collider时候会对速度进行减缓时候用到,
        /// </summary>);
        public float friction;
        /// <summary>
        ///  暂时不用
        /// </summary>
        public float moveByPrePoint;
        /// <summary>
        /// 冻结,使骨骼回到初始的worldPosition的力度的大小
        /// </summary>
        public float freezeScale;
        /// 冻结限制,使骨骼回到初始的worldPosition的力度的大小限制
        /// </summary>
        public float freezeLimit;
        /// 刚性,强制节点回到初始的localPosition的力的大小,
        /// </summary>
        public float rigidScale;
        /// <summary>
        /// 外来力的大小,比如风力,爆炸力,等
        /// </summary>
        internal float addForceScale;
        /// <summary>
        /// 距离模拟,fixed节点发生位移,会对子节点进行位移补偿的比率,可以减少位移过长导致拉伸
        /// </summary>
        internal float distanceCompensation;
        /// <summary>
        /// 重力
        /// </summary>
        public float3 gravity;



        //下面一串都是与杆件力相关
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




        public float3 initialLocalPosition;//OYM:相对于父粒子的节点
        public float3 initialPosition;//OYM：并不是直接的position ,二是相对于fixed点的position;
        /// <summary>
        ///
        /// </summary>
        internal quaternion initialLocalRotation;
        internal quaternion initialRotation;
        internal float massPerIteration;
    }

    //OYM：写入系统
    public struct PointReadWrite
    {
        public float3 position;
        /// <summary>
        /// 当为fixed节点的时候，它代表rotation，当为fixed节点的时候，它代表父节点的父节点的rotation
        /// </summary>
        public quaternion rotationTemp;
        /// <summary>
        /// 速度
        /// </summary>
        public float3 deltaPosition;
        /// <summary>
        /// 角速度,目前还没完全用上
        /// </summary>
        public quaternion deltaRotation;
        public float physicProcess;//OYM:物理进程值，0-1范围，fixed节点为0
        //public quaternion deltaRotationY;
    }
}
