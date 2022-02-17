using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace ADBRuntime.UntiyEditor
{
    using Mono;
    [CustomEditor(typeof(ADBPhysicsSettingSwitcher))]
    public class ADBPhysicsSettingSwitcherEditor : Editor
    {
        ADBPhysicsSettingSwitcher controller;
        public void OnEnable()
        {
            controller = (target as ADBPhysicsSettingSwitcher);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("runtimeController"), new GUIContent("切换目标"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentLinker"), new GUIContent("当前物理设定"), true);
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetLinkers"), new GUIContent("物理设定列表"), true);
            if (GUILayout.Button("切换效果"))
            {
                controller.Switch();
            }
            serializedObject.ApplyModifiedProperties();
        }

    }

}
