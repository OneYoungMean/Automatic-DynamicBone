using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime
{
    using Mono;
    public class ADBEditorColliderEditor : Editor
    {
        [CustomEditor(typeof(ADBEditorCollider))]
        public class ADBRuntimeEditor : Editor
        {
            ADBEditorCollider controller;
            private bool isDeleteCollider;

            public void OnEnable()
            {
                controller = (target as ADBEditorCollider);

            }
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.isDraw"), new GUIContent("Is Draw"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isGlobal"), new GUIContent("Is Global"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.isOpen"), new GUIContent("Is Open"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.colliderType"), new GUIContent("Collider Type"), true);

                switch (controller.GetColliderType())
                {
                    case ColliderType.Sphere:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.radius"), new GUIContent("┗━I Radius"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.positionOffset"), new GUIContent("┗━I Position Offset"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.collideFunc"), new GUIContent("┗━I Collider Func"), true);
                        break;
                    case ColliderType.Capsule:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.radius"), new GUIContent("┗━I Radius"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.length"), new GUIContent("┗━I Length"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.positionOffset"), new GUIContent("┗━I Position Offset"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.staticDirection"), new GUIContent("┗━I Direction"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.collideFunc"), new GUIContent("┗━I Collider Func"), true);
                        break;
                    case ColliderType.OBB:
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.boxSize"), new GUIContent("┗━I BoxSize"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.positionOffset"), new GUIContent("┗━I Position Offset"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.staticDirection"), new GUIContent("┗━I Direction"), true);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.collideFunc"), new GUIContent("┗━I Collider Func"), true);
                        break;
                    default:
                        break;
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.colliderChoice"), new GUIContent("┗━I Collider Choice"), true);

                // EditorGUILayout.PropertyField(serializedObject.FindProperty("editor.colliderRead.isConnectWithBody"), new GUIContent("Is Connect With Body"), true);
                controller.Refresh();
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

