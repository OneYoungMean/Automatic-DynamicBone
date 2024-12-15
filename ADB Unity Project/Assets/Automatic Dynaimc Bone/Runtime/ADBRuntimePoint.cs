using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;
namespace ADBRuntime.Mono
{
   // [DisallowMultipleComponent]
    public class ADBRuntimePoint: MonoBehaviour
    {
        public MonoBehaviour Target => this;
        public ADBRuntimePoint Parent { get { return parent; }set { parent = value; } }
        public List<ADBRuntimePoint> ChildPoints { get { return childPoints; } }//OYM：子节点 
        public bool isFixed { get { return depth == 0; } }//OYM：是否固定

        public bool isRoot { get { return depth == -1; } }//OYM：是否为根节点

        [NonSerialized]
        public PointReadWrite pointReadWrite;
        [SerializeField]
        public float3 initialScale;
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
        public static ADBRuntimePoint CreateRuntimePoint(Transform trans, int depth, string keyWord = null, bool isAllowComputeOtherConstraint = true)
        {
            ADBRuntimePoint point = trans.gameObject.AddComponent<ADBRuntimePoint>();
            point.keyWord = keyWord;
            point.depth = depth;
            point.pointRead = new PointRead();
            point.pointReadWrite = new PointReadWrite();
            point.allowCreateAllConstraint = isAllowComputeOtherConstraint;
            point.initialScale = trans.lossyScale;
            return point;
        }
        internal virtual void DrawGizmos()
        {
            Gizmos.color = allowCreateAllConstraint ?  Color.yellow: new Color(0.3f,1f,0.3f,1);
            if (pointRead.radius > 0.005f)
            {
                Matrix4x4 temp = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.FromToRotation(Vector3.up, pointRead.initialLocalPosition) * transform.rotation, transform.lossyScale/ initialScale);
                Gizmos.DrawWireSphere(Vector3.zero, pointRead.radius);
                Gizmos.matrix = temp;

            }
            else
            {
                Gizmos.DrawSphere(transform.position, 0.005f);//OYM：都说了画点用的
            }
        }
       public void SetDepth(int i )
        {
            depth = i;
        }

        public void AddChild(List<ADBRuntimePoint> childPoints)
        {
            for (int i = 0; i < childPoints?.Count; i++)
            {
                AddChild(childPoints[i]);
            }

        }
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
