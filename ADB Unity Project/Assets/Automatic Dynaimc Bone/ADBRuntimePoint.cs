using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using System;
using Unity.Mathematics;

namespace ADBRuntime
{
    public class ADBRuntimePoint
    {
        public PointRead pointRead;
        public PointReadWrite pointReadWrite;
        public float pointDepthRateMaxPointDepth;

        public Transform trans { get;private set; }
        public ADBRuntimePoint parent { get;private set; }
        public List<ADBRuntimePoint> childNode { get; set; }//OYM：子节点 
        public bool isFixed { get; private set; }//OYM：是否固定
        public string keyWord { get; private set; }//OYM：匹配的关键词
        public int depth { get; private set; }//OYM：深度
        public int index { get; set; }//OYM：序号
        public bool isVirtual { get; private set; }

        public ADBRuntimePoint(Transform trans, int depth, string keyWord = null, bool isVirtual = false)
        {

            if (keyWord == null )//OYM：root点(只起一个逻辑点的作用)
            {
                this.trans = trans;
                this.depth = -1;
                isVirtual = true;
            }
            else
            {
                this.trans = trans;
                this.keyWord = keyWord;
                this.depth = depth;
                this.isVirtual = isVirtual;
                this.isFixed = depth == 0;
                pointRead = new PointRead();
                pointReadWrite = new PointReadWrite();
            }
            if (isVirtual)
            {
                pointRead.isVirtual = true;
            }
        }
        internal void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(trans.position, 0.005f);//OYM：都说了画点用的
        }
        public void SetParent(ADBRuntimePoint point)
        {
            pointRead.parent = point.index;
            this.parent = point;
    }
    }

    public struct PointRead
    {
        public bool isVirtual;
        public int fixedIndex;
        /// <summary>
        /// 父节点序号
        /// </summary>
        public int parent;
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
        /// 怠速，默认为1
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
        ///  惰性,防止头发末梢跟美杜莎一样到处乱动
        /// </summary>
        public float moveByPrePoint;
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
        public Vector3 gravity;
        public Vector3 boneAxis;
        public Vector3 initialPosition;
        public Quaternion localRotation;

        public float freeze;
        internal float windScale;
        internal float distanceCompensation;
    }

    //OYM：写入系统
    public struct PointReadWrite
    {
        public Vector3 position;
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

        public Vector3 velocity;
        
        public Quaternion rotation;
        public Quaternion oldRotation;
        
    }
}
