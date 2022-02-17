using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;
namespace ADBRuntime
{
    [Serializable]
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
        public float mass;

        /// <summary>
        /// Collider选择性对撞
        /// </summary>
        public int colliderMask;
        /// <summary>
        /// 固定节点是否发生旋转?
        /// </summary>
        public bool isFixedPointFreezeRotation;
        /// <summary>
        /// 阻尼：How much the bones slowed down.
        /// </summary>);
        public float damping;

        /// <summary>
        /// 摩擦力大小,计算collider时候会对速度进行减缓时候用到,
        /// </summary>);
        public float friction;
        /// <summary>
        /// 世界刚性,使骨骼回到初始的worldPosition的力度的大小
        /// </summary>
        public float stiffnessWorld;
        /// 冻结限制,使骨骼回到初始的worldPosition的力度的大小限制
        /// </summary>
        //public float stiffnessWorldLimit;
        ///局部刚性,强制节点回到初始的localPosition的力的大小,
        /// </summary>
        public float stiffnessLocal;
        /// <summary>
        /// 弹性：How much the force applied to return each bone to original orientation.
        /// </summary>
        public float elasticity;
        /// <summary>
        /// 惰性：How much character's position change is ignored in physics simulation.
        /// </summary>);
        public float moveInert;
        /// <summary>
        /// 距离模拟,fixed节点发生位移,会对子节点进行位移补偿的比率,可以减少位移过长导致拉伸
        /// </summary>
        public float velocityIncrease;
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
        /// <summary>
        /// 半径
        /// </summary>
        public float radius;

        public float3 initialLocalPosition;//OYM:相对于父粒子的节点
        internal float initialLocalPositionLength;

        public float3 initialPosition;//OYM：并不是直接的position ,二是相对于fixed点的position;

        internal quaternion initialLocalRotation;
        internal quaternion initialRotation;

        internal float dampDivIteration;
        internal float addForceScale;
        public float lengthLimitForceScale;
        public float elasticityVelocity;


        /*        public float value2;
                public float value3;
                public float value4;
                public float value5;
                public float value6;
                public float value7;
                public float value8;
                public float value9;*/
    }

    //OYM：写入系统
    public struct PointReadWrite
    {
        public float3 position;
        /// <summary>
        /// 当为fixed节点的时候，它代表rotation，当为fixed节点的时候，它代表父节点的父节点的rotation
        /// </summary>
        public quaternion rotationNoSelfRotateChange;
        /// <summary>
        /// 速度
        /// </summary>
        public float3 deltaPosition;

        /// <summary>
        /// 角速度,目前还没完全用上
        /// </summary>
        public quaternion deltaRotation;

        public bool isCollide;
        //public quaternion deltaRotationY;
    }
}
