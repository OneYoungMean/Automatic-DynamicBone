/*
 * MIT License
 *  Copyright (c) 2018 SPARKCREATIVE
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 *  The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *  
 *  @author Noriyuki Hiromoto <hrmtnryk@sparkfx.jp>
*/
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using System;

namespace ADBRuntime
{
    public class ADBRuntimePoint
    {

        public PointRead pointRead;
        public PointReadWrite pointReadWrite;
        public float pointDepthRateMaxPointDepth;

        public Transform trans { get; set; }
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
    }

    public unsafe struct PointRead
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
        /// 可塑性，默认为1
        /// </summary>);
        public float mass;
        /// <summary>
        /// 阻尼，默认为1
        /// </summary>);
        public float resistance;
        /// <summary>
        /// 摩擦力大小
        /// </summary>);
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

    }

    //OYM：写入系统
    public struct PointReadWrite
    {
        public Vector3 position { get { return Position; }
            set {
                if (float.IsNaN(value.x))
                {
                    Position = Vector3.zero;
                }
                else
                {
                    Position = value;
                }
            } }
        public Vector3 Position;

        public Vector3 velocity
        {
            get { return Velocity; }
            set
            {
                if (float.IsNaN(value.x))
                {
                    Velocity = Vector3.zero;
                }
                else
                {
                    Velocity = value;
                }
            }
        }
        public Vector3 Velocity;
    }

    /// <summary>
    /// 傻逼才这么写代码
    /// 我是傻逼吗?我是傻逼吗?
    /// </summary>
    public struct Child
    {
        int lastChild;
        int c0;
        int c1;
        int c2;
        int c3;
        int c4;
        int c5;
        int c6;
        int c7;
        int c8;
        int c9;
        int c10;
        int c11;
        int c12;
        int c13;
        int c14;
        int c15;
        int c16;
        int c17;
        int c18;
        int c19;
        int c20;
        int c21;
        int c22;
        int c23;
        int c24;
        int c25;
        int c26;
        int c27;
        int c28;
        int c29;
        int c30;
        int c31;
        int c32;
        int c33;
        int c34;
        int c35;
        int c36;
        int c37;
        int c38;
        int c39;
        int c40;
        int c41;
        int c42;
        int c43;
        int c44;
        int c45;
        int c46;
        int c47;
        int c48;
        int c49;
        int c50;
        int c51;
        int c52;
        int c53;
        int c54;
        int c55;
        int c56;
        int c57;
        int c58;
        int c59;
        int c60;
        int c61;
        int c62;
        int c63;
        int c64;
        int c65;
        int c66;
        int c67;
        int c68;
        int c69;
        int c70;
        int c71;
        int c72;
        int c73;
        int c74;
        int c75;
        int c76;
        int c77;
        int c78;
        int c79;
        int c80;
        int c81;
        int c82;
        int c83;
        int c84;
        int c85;
        int c86;
        int c87;
        int c88;
        int c89;
        int c90;
        int c91;
        int c92;
        int c93;
        int c94;
        int c95;
        int c96;
        int c97;
        int c98;
        int c99;
        int c100;
        int c101;
        int c102;
        int c103;
        int c104;
        int c105;
        int c106;
        int c107;
        int c108;
        int c109;
        int c110;
        int c111;
        int c112;
        int c113;
        int c114;
        int c115;
        int c116;
        int c117;
        int c118;
        int c119;
        int c120;
        int c121;
        int c122;
        int c123;
        int c124;
        int c125;
        int c126;
        int c127;
        int c128;

        public void SetChildIndex(int[] child)
        {
            lastChild = child.Length;
            if (lastChild > 128)
            {
                Debug.Log("128个变量已经满足不了你了,赶紧来debug");
            }
            int i = 0;

            int c0 = (i < lastChild) ? child[i++] : -1;
            int c1 = (i < lastChild) ? child[i++] : -1;
            int c2 = (i < lastChild) ? child[i++] : -1;
            int c3 = (i < lastChild) ? child[i++] : -1;
            int c4 = (i < lastChild) ? child[i++] : -1;
            int c5 = (i < lastChild) ? child[i++] : -1;
            int c6 = (i < lastChild) ? child[i++] : -1;
            int c7 = (i < lastChild) ? child[i++] : -1;
            int c8 = (i < lastChild) ? child[i++] : -1;
            int c9 = (i < lastChild) ? child[i++] : -1;
            int c10 = (i < lastChild) ? child[i++] : -1;
            int c11 = (i < lastChild) ? child[i++] : -1;
            int c12 = (i < lastChild) ? child[i++] : -1;
            int c13 = (i < lastChild) ? child[i++] : -1;
            int c14 = (i < lastChild) ? child[i++] : -1;
            int c15 = (i < lastChild) ? child[i++] : -1;
            int c16 = (i < lastChild) ? child[i++] : -1;
            int c17 = (i < lastChild) ? child[i++] : -1;
            int c18 = (i < lastChild) ? child[i++] : -1;
            int c19 = (i < lastChild) ? child[i++] : -1;
            int c20 = (i < lastChild) ? child[i++] : -1;
            int c21 = (i < lastChild) ? child[i++] : -1;
            int c22 = (i < lastChild) ? child[i++] : -1;
            int c23 = (i < lastChild) ? child[i++] : -1;
            int c24 = (i < lastChild) ? child[i++] : -1;
            int c25 = (i < lastChild) ? child[i++] : -1;
            int c26 = (i < lastChild) ? child[i++] : -1;
            int c27 = (i < lastChild) ? child[i++] : -1;
            int c28 = (i < lastChild) ? child[i++] : -1;
            int c29 = (i < lastChild) ? child[i++] : -1;
            int c30 = (i < lastChild) ? child[i++] : -1;
            int c31 = (i < lastChild) ? child[i++] : -1;
            int c32 = (i < lastChild) ? child[i++] : -1;
            int c33 = (i < lastChild) ? child[i++] : -1;
            int c34 = (i < lastChild) ? child[i++] : -1;
            int c35 = (i < lastChild) ? child[i++] : -1;
            int c36 = (i < lastChild) ? child[i++] : -1;
            int c37 = (i < lastChild) ? child[i++] : -1;
            int c38 = (i < lastChild) ? child[i++] : -1;
            int c39 = (i < lastChild) ? child[i++] : -1;
            int c40 = (i < lastChild) ? child[i++] : -1;
            int c41 = (i < lastChild) ? child[i++] : -1;
            int c42 = (i < lastChild) ? child[i++] : -1;
            int c43 = (i < lastChild) ? child[i++] : -1;
            int c44 = (i < lastChild) ? child[i++] : -1;
            int c45 = (i < lastChild) ? child[i++] : -1;
            int c46 = (i < lastChild) ? child[i++] : -1;
            int c47 = (i < lastChild) ? child[i++] : -1;
            int c48 = (i < lastChild) ? child[i++] : -1;
            int c49 = (i < lastChild) ? child[i++] : -1;
            int c50 = (i < lastChild) ? child[i++] : -1;
            int c51 = (i < lastChild) ? child[i++] : -1;
            int c52 = (i < lastChild) ? child[i++] : -1;
            int c53 = (i < lastChild) ? child[i++] : -1;
            int c54 = (i < lastChild) ? child[i++] : -1;
            int c55 = (i < lastChild) ? child[i++] : -1;
            int c56 = (i < lastChild) ? child[i++] : -1;
            int c57 = (i < lastChild) ? child[i++] : -1;
            int c58 = (i < lastChild) ? child[i++] : -1;
            int c59 = (i < lastChild) ? child[i++] : -1;
            int c60 = (i < lastChild) ? child[i++] : -1;
            int c61 = (i < lastChild) ? child[i++] : -1;
            int c62 = (i < lastChild) ? child[i++] : -1;
            int c63 = (i < lastChild) ? child[i++] : -1;
            int c64 = (i < lastChild) ? child[i++] : -1;
            int c65 = (i < lastChild) ? child[i++] : -1;
            int c66 = (i < lastChild) ? child[i++] : -1;
            int c67 = (i < lastChild) ? child[i++] : -1;
            int c68 = (i < lastChild) ? child[i++] : -1;
            int c69 = (i < lastChild) ? child[i++] : -1;
            int c70 = (i < lastChild) ? child[i++] : -1;
            int c71 = (i < lastChild) ? child[i++] : -1;
            int c72 = (i < lastChild) ? child[i++] : -1;
            int c73 = (i < lastChild) ? child[i++] : -1;
            int c74 = (i < lastChild) ? child[i++] : -1;
            int c75 = (i < lastChild) ? child[i++] : -1;
            int c76 = (i < lastChild) ? child[i++] : -1;
            int c77 = (i < lastChild) ? child[i++] : -1;
            int c78 = (i < lastChild) ? child[i++] : -1;
            int c79 = (i < lastChild) ? child[i++] : -1;
            int c80 = (i < lastChild) ? child[i++] : -1;
            int c81 = (i < lastChild) ? child[i++] : -1;
            int c82 = (i < lastChild) ? child[i++] : -1;
            int c83 = (i < lastChild) ? child[i++] : -1;
            int c84 = (i < lastChild) ? child[i++] : -1;
            int c85 = (i < lastChild) ? child[i++] : -1;
            int c86 = (i < lastChild) ? child[i++] : -1;
            int c87 = (i < lastChild) ? child[i++] : -1;
            int c88 = (i < lastChild) ? child[i++] : -1;
            int c89 = (i < lastChild) ? child[i++] : -1;
            int c90 = (i < lastChild) ? child[i++] : -1;
            int c91 = (i < lastChild) ? child[i++] : -1;
            int c92 = (i < lastChild) ? child[i++] : -1;
            int c93 = (i < lastChild) ? child[i++] : -1;
            int c94 = (i < lastChild) ? child[i++] : -1;
            int c95 = (i < lastChild) ? child[i++] : -1;
            int c96 = (i < lastChild) ? child[i++] : -1;
            int c97 = (i < lastChild) ? child[i++] : -1;
            int c98 = (i < lastChild) ? child[i++] : -1;
            int c99 = (i < lastChild) ? child[i++] : -1;
            int c100 = (i < lastChild) ? child[i++] : -1;
            int c101 = (i < lastChild) ? child[i++] : -1;
            int c102 = (i < lastChild) ? child[i++] : -1;
            int c103 = (i < lastChild) ? child[i++] : -1;
            int c104 = (i < lastChild) ? child[i++] : -1;
            int c105 = (i < lastChild) ? child[i++] : -1;
            int c106 = (i < lastChild) ? child[i++] : -1;
            int c107 = (i < lastChild) ? child[i++] : -1;
            int c108 = (i < lastChild) ? child[i++] : -1;
            int c109 = (i < lastChild) ? child[i++] : -1;
            int c110 = (i < lastChild) ? child[i++] : -1;
            int c111 = (i < lastChild) ? child[i++] : -1;
            int c112 = (i < lastChild) ? child[i++] : -1;
            int c113 = (i < lastChild) ? child[i++] : -1;
            int c114 = (i < lastChild) ? child[i++] : -1;
            int c115 = (i < lastChild) ? child[i++] : -1;
            int c116 = (i < lastChild) ? child[i++] : -1;
            int c117 = (i < lastChild) ? child[i++] : -1;
            int c118 = (i < lastChild) ? child[i++] : -1;
            int c119 = (i < lastChild) ? child[i++] : -1;
            int c120 = (i < lastChild) ? child[i++] : -1;
            int c121 = (i < lastChild) ? child[i++] : -1;
            int c122 = (i < lastChild) ? child[i++] : -1;
            int c123 = (i < lastChild) ? child[i++] : -1;
            int c124 = (i < lastChild) ? child[i++] : -1;
            int c125 = (i < lastChild) ? child[i++] : -1;
            int c126 = (i < lastChild) ? child[i++] : -1;
            int c127 = (i < lastChild) ? child[i++] : -1;
            int c128 = (i < lastChild) ? child[i++] : -1;
        }
    }
}
