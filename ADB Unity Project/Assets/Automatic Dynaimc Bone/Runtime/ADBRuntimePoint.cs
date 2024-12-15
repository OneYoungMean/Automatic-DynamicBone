using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
namespace ADBRuntime.Mono
{
    /// <summary>
    /// ADB phyiscs bone
    /// </summary>
   // [DisallowMultipleComponent]
    public class ADBRuntimePoint: MonoBehaviour
    {
        public MonoBehaviour Target => this;
        public ADBRuntimePoint Parent { get { return parent; }set { parent = value; } }
        public List<ADBRuntimePoint> ChildPoints { get { Refresh(); return childPoints; } }
        public bool isFixed { get { return depth == 0; } }

        public bool isRoot { get { return depth == -1; } }

        [NonSerialized]
        public PointReadWrite pointReadWrite;
        [SerializeField]
        public PointRead pointRead;
        [SerializeField]
        private ADBRuntimePoint parent;
        [SerializeField]
        private List<ADBRuntimePoint> childPoints;
        public string keyWord;
        [SerializeField]
        public int depth;
        [SerializeField]
        public int index;
        [SerializeField]
        public bool allowCreateAllConstraint;
        [SerializeField]
        public float pointDepthRateMaxPointDepth;
        internal float centerPointDirectionRotates;


        public void Refresh()
        {
            for (int i = 0; i < childPoints?.Count; i++)
            {
                if (childPoints[i]==null)
                {
                    childPoints.RemoveAt(i);
                    i--;
                }
            }
        }
        /// <summary>
        /// Create a phyiscs bone by Transform
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="depth"></param>
        /// <param name="keyWord"></param>
        /// <param name="isAllowComputeOtherConstraint"></param>
        /// <returns></returns>
        public static ADBRuntimePoint CreateRuntimePoint(Transform trans, int depth, string keyWord = null, bool isAllowComputeOtherConstraint = true)
        {
            ADBRuntimePoint point = trans.gameObject.AddComponent<ADBRuntimePoint>();
            point.keyWord = keyWord;
            point.depth = depth;
            point.pointRead = new PointRead();
            point.pointReadWrite = new PointReadWrite();
            point.allowCreateAllConstraint = isAllowComputeOtherConstraint;
            return point;
        }

        /// <summary>
        /// Draw gizmo
        /// </summary>
        internal virtual void DrawGizmos()
        {
            Gizmos.color = allowCreateAllConstraint ?  Color.yellow: new Color(0.3f,1f,0.3f,1);
            if (pointRead.radius > 0.005f)
            {
                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.FromToRotation(Vector3.up, pointRead.initialLocalPosition) * transform.rotation, transform.lossyScale);
                Gizmos.DrawWireSphere(Vector3.zero, pointRead.radius);
                Gizmos.matrix = temp;

            }
            else
            {
                Gizmos.DrawSphere(transform.position, 0.005f);
            }
        }
        /// <summary>
        /// Set bone depth
        /// </summary>
        /// <param name="i"></param>
       public void SetDepth(int i )
        {
            depth = i;
        }
        /// <summary>
        /// Add a child bone
        /// </summary>
        /// <param name="childPoints"></param>
        public void AddChild(List<ADBRuntimePoint> childPoints)
        {
            for (int i = 0; i < childPoints?.Count; i++)
            {
                AddChild(childPoints[i]);
            }

        }

        /// <summary>
        /// Add child bone
        /// </summary>
        /// <param name="childPoint"></param>
        public void AddChild(ADBRuntimePoint childPoint)
        {
            if (childPoint != null)
            {
                if (childPoints == null)
                {
                    childPoints = new List<ADBRuntimePoint>();
                }
                childPoints.Add(childPoint);
                childPoint.Parent = this;
            }


        }
    }


}
