using UnityEngine;
using System.Collections.Generic;
using System;
using System.ComponentModel;

namespace ADBRuntime
{
    public class ADBEditorCollider : MonoBehaviour
    {
        [SerializeField]
        public ADBRuntimeCollider editor;
        [SerializeField]
        public bool isDraw;
        [SerializeField]
        public bool isGlobal;

        private ADBRuntimeCollider runner;        //OYM：这里怎么整都不好好访问,只能这么弄了

        public static List<ADBRuntimeCollider> globalColliderList;


        private void Awake()
        {
            if (globalColliderList == null)
            {
                globalColliderList = new List<ADBRuntimeCollider>();
            }
            if (runner == null)
            {
                initialize();
            }



            if (isGlobal && Application.isPlaying&& !globalColliderList.Contains(runner))
            {
                globalColliderList.Add(runner);
            }
            isDraw = false;
        }
        public static void RuntimeCollider2Editor(ADBRuntimeCollider runtime)
        {
            var editor = runtime.appendTransform.gameObject.AddComponent<ADBEditorCollider>();
            editor.editor = runtime;
            editor.editor.colliderRead.isOpen = true;
        }

        public ADBRuntimeCollider GetCollider()
        {
            if (isGlobal) return null;

            initialize();
            return runner;
        }
        public void initialize()
        {
            if (editor.appendTransform == null)//OYM：不允許為空
            {
                editor.appendTransform = transform;
            }
            if (runner?.colliderRead == null || !runner.colliderRead.Equals(editor.colliderRead))
            {
                editor.colliderRead.CheckValue();

                switch (editor.colliderRead.colliderType)
                {
                    case ColliderType.Sphere:
                        runner = new SphereCollider(editor.colliderRead, editor.appendTransform);
                        break;
                    case ColliderType.Capsule:
                        runner = new CapsuleCollider(editor.colliderRead, editor.appendTransform);
                        break;
                    case ColliderType.OBB:
                        runner = new OBBBox(editor.colliderRead, editor.appendTransform);
                        break;
                }
            }

        }
        
        private void OnDrawGizmosSelected()
        {

            initialize();

            if (!Application.isPlaying&& isDraw && runner != null)
            {
            runner.OnDrawGizmos();
            }

        }
        public override string ToString()
        {
            return runner.colliderRead.colliderType.ToString();
        }
    }
}