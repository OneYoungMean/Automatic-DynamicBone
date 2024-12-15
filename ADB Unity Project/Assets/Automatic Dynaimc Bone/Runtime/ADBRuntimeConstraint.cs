using UnityEngine;
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
    /// <summary>
    /// ADB Constraint
    /// </summary>
    public class ADBRuntimeConstraint
    {
        public ConstraintRead constraintRead;
        public ADBRuntimePoint pointA { get; private set; }
        public ADBRuntimePoint pointB { get; private set; }
        public Vector3 direction { get; private set; }

        public ADBRuntimeConstraint(ConstraintType type, ADBRuntimePoint pointA, ADBRuntimePoint pointB, float shrink, float stretch, bool isCollide)
        {
            constraintRead.type = type;
            this.pointA = pointA;
            this.pointB = pointB;
            constraintRead.radius = 0.5f * (pointA.pointRead.radius + pointB.pointRead.radius);

            constraintRead.indexA = pointA.index;
            constraintRead.indexB = pointB.index;
            constraintRead.shrink = shrink;
            constraintRead.stretch = stretch;
            constraintRead.isCollider = isCollide;
            this.direction = pointA.transform.position - pointB.transform.position;
            constraintRead.length = (this.direction).magnitude;

        }
        public override string ToString()
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
    /// <summary>
    /// ADB Constraint native
    /// </summary>
    public struct ConstraintRead:IEquatable<ConstraintRead>
    {
        public bool isCollider;

        public ConstraintType type;

        public int indexA;

        public int indexB;

        public float length;

        public float shrink;

        public float stretch;

        public float radius;
         
        public bool Equals(ConstraintRead other)
        {
            return(other.indexA == indexA ||other.indexB == indexB|| other.indexA == indexB || other.indexB == indexA);
        }
    }
}
