using UnityEngine;
using System.Collections.Generic;

namespace ADBRuntime
{
    [DisallowMultipleComponent]

    public class ADBEditorCollider : MonoBehaviour
    {
        [SerializeField]
        public ADBRuntimeCollider editor;
        [SerializeField]
        public bool isDraw;
        [SerializeField]
        public bool isGlobal;

        private ADBRuntimeCollider aDBRuntimeCollider;
        public static List<ADBRuntimeCollider> globalColliderList;


        private void Awake()
        {
            if (globalColliderList == null)
            {
                globalColliderList = new List<ADBRuntimeCollider>();
            }

            if (aDBRuntimeCollider == null)
            {
                initialize();
            }

            if (aDBRuntimeCollider.appendTransform == null )
            {
                aDBRuntimeCollider.appendTransform = transform;
            }

            if (isGlobal&&Application.isPlaying)
            {
                globalColliderList.Add(aDBRuntimeCollider);
            }
            isDraw = false;
        }
        public static ADBEditorCollider RuntimeCollider2Editor(ADBRuntimeCollider runtime)
        {
           
            var editor = runtime.appendTransform.gameObject.AddComponent<ADBEditorCollider>();
            if(editor==null)
            {
                editor = runtime.appendTransform.gameObject.GetComponent<ADBEditorCollider>();
                return editor;
            }
            editor.editor = runtime;
            
            return editor;
        }
       
        public ADBRuntimeCollider GetCollider()
        {
            initialize();
            return aDBRuntimeCollider;
        }
        public void initialize()
        {
            if (aDBRuntimeCollider?.colliderRead == null || aDBRuntimeCollider.colliderRead.Equals(editor.colliderRead)) ;
            {
                editor.colliderRead.CheckValue();
                editor.colliderRead.isOpen = true;
                switch (editor.colliderRead.colliderType)
                {
                    case ColliderType.Sphere:
                        aDBRuntimeCollider = new SphereCollider(editor.colliderRead, editor.appendTransform);
                        break;
                    case ColliderType.Capsule:
                        aDBRuntimeCollider = new CapsuleCollider(editor.colliderRead, editor.appendTransform);
                        break;
                    case ColliderType.OBB:
                        aDBRuntimeCollider = new OBBBox(editor.colliderRead, editor.appendTransform);
                        break;
                }
            }

        }
        private void OnDrawGizmos()
        {

            initialize();
            if (isDraw && aDBRuntimeCollider != null)
            {
                aDBRuntimeCollider.OnDrawGizmos();
            }
        }
    }
}