using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    public class ADBEditorColliderEditor : Editor
    {
        [CustomEditor(typeof(ADBColliderReader))]
        public class ADBRuntimeEditor : Editor
        {
            ADBColliderReader controller;

            private enum CollideTypecZh
            {
                球体=0,
                胶囊体=1,
                立方体=2
            }
            private enum CollideFuncZh
            {
                碰撞体_向外排斥=1,
                碰撞体_约束在内=2,
                力场_向外排斥=3,
                力场_约束在内=4,
              //  冻结在表面 = 5,
            }
            private enum ColliderChoiceZh
            {
                头 = 1 << 0,
                上半身 = 1 << 1,
                下半身 = 1 << 2,
                大腿 = 1 << 3,
                小腿 = 1 << 4,
                大臂 = 1 << 5,
                小臂 = 1 << 6,
                手 = 1 << 7,
                脚 = 1 << 8,
                其他 = 1 << 9,
            }
            public void OnEnable()
            {
                controller = (target as ADBColliderReader);
            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                if (controller.unityCollider == null)
                {
                    controller.unityCollider = controller.gameObject.GetComponent<Collider>();
                }
                if (controller.collideFunc == 0)
                {
                    controller.collideFunc = CollideFunc.OutsideLimit;
                }
                if (Application.isPlaying&& controller.gameObject.activeSelf&& controller.enabled)
                {
                    controller.UpdatePriorities();
                }
                
                Titlebar("ADB碰撞体标记", Color.Lerp(Color.white, Color.blue, 0.5f));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("unityCollider"), new GUIContent("标记的碰撞体"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isReadOnly"), new GUIContent("┗━I 标记只读(提高性能)"), true);
                if (controller.isReadOnly)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isStatic"), new GUIContent("┗━I 标记静态(极致性能)"), true);
                }
                controller.collideFunc = (CollideFunc)EditorGUILayout.EnumPopup("┗━I 碰撞体功能", (CollideFuncZh)controller.collideFunc);
                controller.colliderMask = (ColliderChoice)EditorGUILayout.EnumFlagsField("┗━I 碰撞体属性", (ColliderChoiceZh)controller.colliderMask);
/*                EditorGUILayout.PropertyField(serializedObject.FindProperty("owners"), new GUIContent("┗━I 所有者"), true);*/

                serializedObject.ApplyModifiedProperties();
            }

            void Titlebar(string text, Color color)
            {
                GUILayout.Space(12);

                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = color;

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.Label(text);
                EditorGUILayout.EndHorizontal();

                GUI.backgroundColor = backgroundColor;

                GUILayout.Space(3);
            }
        }
    }
}

