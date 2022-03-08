using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    public enum ColliderChoiceZh
    {
        Head = 1 << 0,
        UpperBody = 1 << 1,
        LowerBody = 1 << 2,
        UpperLeg = 1 << 3,
        LowerLeg = 1 << 4,
        UpperArm = 1 << 5,
        LowerArm = 1 << 6,
        Hand = 1 << 7,
        Foot = 1 << 8,
        Other = 1 << 9,
    }
    public class ADBEditorColliderEditor : Editor
    {
        [CustomEditor(typeof(ADBColliderReader))]
        public class ADBRuntimeEditor : Editor
        {
            ADBColliderReader controller;

            private enum CollideTypecZh
            {
                Sphere=0,
                Capsule=1,
                Box=2
            }
            private enum CollideFuncZh
            {
                OutsideStrict=1,
                InsideStrict=2,
                OutsideSoft=3,
                InsideSoft=4,
              //  冻结在表面 = 5,
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("unityCollider"), new GUIContent("ColliderTarget"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isReadOnly"), new GUIContent("┗━I is Collider ReadOnly (highly performance)"), true);
                if (controller.isReadOnly)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("isStatic"), new GUIContent("┗━I Is Collider Fixed(highly performance)"), true);
                }
                controller.collideFunc = (CollideFunc)EditorGUILayout.EnumPopup("┗━I ColliderMode", (CollideFuncZh)controller.collideFunc);
                controller.colliderMask = (ColliderChoice)EditorGUILayout.EnumFlagsField("┗━I CollideMask", (ColliderChoiceZh)controller.colliderMask);
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

