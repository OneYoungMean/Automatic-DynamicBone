using UnityEngine;
using System.Collections.Generic;

namespace ADBRuntime.Mono
{
    public class ADBEditorCollider : MonoBehaviour
    {
        [SerializeField]
        public ADBRuntimeCollider editor;
        [SerializeField]
        public bool isGlobal;


        public static List<ADBRuntimeCollider> globalColliderList;


        private void Awake()
        {
            if (globalColliderList == null)
            {
                globalColliderList = new List<ADBRuntimeCollider>();
            }

            if (isGlobal && Application.isPlaying&& !globalColliderList.Contains(editor))
            {
                globalColliderList.Add(editor);
            }
        }
        public static void RuntimeCollider2Editor(ADBRuntimeCollider runtime)
        {
            var editor = runtime.appendTransform.gameObject.AddComponent<ADBEditorCollider>();
            editor.editor = runtime;
            editor.editor.colliderRead.isOpen = true;
        }

        public ColliderType GetColliderType()
        {
            return editor.colliderRead.colliderType;
        }
        public void Refresh()
        {
            editor.isDraw = true;
            editor.colliderRead.CheckValue();
            switch (editor.colliderRead.colliderType)
            {
                case ColliderType.Sphere:
                    if(editor.GetType()!=typeof(SphereCollider))
                    editor = new SphereCollider(editor.colliderRead, transform);
                    break;
                case ColliderType.Capsule:
                    if (editor.GetType() != typeof(CapsuleCollider))
                        editor = new CapsuleCollider(editor.colliderRead, transform);
                    break;
                case ColliderType.OBB:
                    if (editor.GetType() != typeof(OBBBoxCollider))
                        editor = new OBBBoxCollider(editor.colliderRead, transform);
                    break;
                default:
                    break;
            }

        }
        
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying&& editor != null)
            {
            editor.OnDrawGizmos();
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying && editor != null&& isGlobal)
            {
                editor.OnDrawGizmos();
            }
        }
        public override string ToString()
        {
            return editor.colliderRead.colliderType.ToString();
        }
    }
}