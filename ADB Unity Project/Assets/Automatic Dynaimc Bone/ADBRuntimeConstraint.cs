using UnityEngine;
using System;

namespace ADBRuntime
{
    public enum ConstraintType
    {
        Structural_Vertical,
        Structural_Horizontal,
        Shear,
        Bending_Vertical,
        Bending_Horizontal,
        Circumference,
        Virtual,
    }
    public class ADBRuntimeConstraint
    {
        public ConstraintRead constraintRead;
        public ADBRuntimePoint pointA { get; private set; }//OYM：父节点
        public ADBRuntimePoint pointB { get; private set; }//OYM：子节点
        public Vector3 direction { get; private set; }
        
        public ADBRuntimeConstraint(ConstraintType type, ADBRuntimePoint pointA, ADBRuntimePoint pointB,float shrink,float stretch,bool isCollide, float freeAngle=0,Vector3? normal=null)
            //OYM：说实话这个v3我一点都不想携程这样,但是不这么写直接赋值vector3.zero又报错
        {
            constraintRead.type = type;
            this.pointA = pointA;
            this.pointB = pointB;

            constraintRead.indexA = pointA.index;
            constraintRead.indexB = pointB.index;
            constraintRead.shrink = shrink;
            constraintRead.stretch = stretch;
            CheckLength();
            constraintRead.rotationFreeAngle =freeAngle;
            constraintRead.rotationConstraintNormal = freeAngle == 0?Vector3.zero:(Vector3)normal ;
            constraintRead.isCollider = isCollide;
        }
        public void CheckLength()
        {
            if (pointB.isVirtual)
            {
                this.direction = Vector3.down * 0.1f;
                constraintRead.length = 0.1f;
            }
            else
            {
                this.direction = pointA.trans.position - pointB.trans.position;
                constraintRead.length = (this.direction).magnitude;
            }


        }
        public ConstraintRead GetConstraintRead()
        {
            //OYM：是结构体所以没必要老是引用了...
            return constraintRead;
        }
        public void OnDrawGizmos()
        {
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

            Gizmos.DrawLine(pointA.trans.position, pointB.trans.position);
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
        /// 旋转约束的法线
        /// </summary>
        public Vector3 rotationConstraintNormal;
        /// <summary>
        /// 旋转约束的角度
        /// </summary>
        public float rotationFreeAngle;

        public bool Equals(ConstraintRead other)
        {
            return(other.indexA == indexA ||other.indexB == indexB|| other.indexA == indexB || other.indexB == indexA);
        }
    }
}
