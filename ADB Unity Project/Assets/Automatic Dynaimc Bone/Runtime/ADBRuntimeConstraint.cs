﻿using UnityEngine;
using System;

namespace ADBRuntime
{
    using Mono;
    public enum ConstraintType
    {
        Structural_Vertical,
        Structural_Horizontal,
        Shear,
        Bending_Vertical,
        Bending_Horizontal,
        Circumference,
    }
    public class ADBRuntimeConstraint
    {
        public ConstraintRead constraintRead;
        public ADBRuntimePoint pointA { get; private set; }//OYM：父节点
        public ADBRuntimePoint pointB { get; private set; }//OYM：子节点
        public Vector3 direction { get; private set; }

        public ADBRuntimeConstraint(ConstraintType type, ADBRuntimePoint pointA, ADBRuntimePoint pointB, float shrink, float stretch, bool isCollide)
        {
            constraintRead.type = type;
            this.pointA = pointA;
            this.pointB = pointB;
            constraintRead.radius = 0.5f * (pointA.pointRead.radius + pointB.pointRead.radius);//要用这个属性自己改,反正用起来太奇怪了=w=

            constraintRead.indexA = pointA.index;
            constraintRead.indexB = pointB.index;
            constraintRead.shrink = shrink;
            constraintRead.stretch = stretch;
            constraintRead.isCollider = isCollide;
            this.direction = pointA.transform.position - pointB.transform.position;
            constraintRead.length = (this.direction).magnitude;

        }
        public override string ToString()//OYM:Debug用
        {
            return pointA.transform.name + " " +pointB.transform.name;
        }
        public void OnDrawGizmos(bool IsDrawOutLine)
        {
            if (pointA==null||pointB==null)
            {
                return;
            }
            switch (constraintRead.type)
            {
                case ConstraintType.Structural_Vertical:
                    Gizmos.color = Color.red;
                    break;
                
                case ConstraintType.Structural_Horizontal:
                    Gizmos.color = new Color(0.4f, 0.8f, 0.4f);
                    break;
                case ConstraintType.Shear:
                    Gizmos.color = new Color(0.4f, 0.4f, 0.8f);
                    break;
                case ConstraintType.Circumference:
                    Gizmos.color = Color.white;
                    break;
                case ConstraintType.Bending_Horizontal:
                    Gizmos.color = new Color(0.2f, 0.1f, 0.6f);
                    break;
                case ConstraintType.Bending_Vertical:
                    Gizmos.color = Color.cyan;
                    break;
                default:
                    return;
            }
            Gizmos.DrawLine(pointA.transform.position, pointB.transform.position);
        }
    }

    public struct ConstraintRead:IEquatable<ConstraintRead>
    {
        public bool isCollider;
        /// <summary>
        /// 种类
        /// </summary>
        public ConstraintType type;
        /// <summary>
        /// 节点A
        /// </summary>
        public int indexA;
        /// <summary>
        /// 节点B
        /// </summary>
        public int indexB;
        /// <summary>
        /// 长度
        /// </summary>
        public float length;
        /// <summary>
        /// 收缩值
        /// </summary>
        public float shrink;
        /// <summary>
        /// 拉伸值
        /// </summary>
        public float stretch;
        /// <summary>
        /// 半径
        /// </summary>
        public float radius;
         
        public bool Equals(ConstraintRead other)
        {
            return(other.indexA == indexA ||other.indexB == indexB|| other.indexA == indexB || other.indexB == indexA);
            //OYM：理论上而言是存在完全相等的杆件的,但是我应该尽量避免了这种情况.
        }
    }
}
