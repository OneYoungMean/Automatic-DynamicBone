using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    [CustomEditor(typeof(ADBChainProcessor))]
    public class ADBChainProcessorEditor : Editor
    {
        ADBChainProcessor controller;

        public void OnEnable()
        {
            controller = target as ADBChainProcessor;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.ObjectField("根节点",controller.transform, typeof(Transform), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("aDBSetting"), new GUIContent("┗━物理设置"), true);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyWord"), new GUIContent("┗━关键词"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTransforms"), new GUIContent("┗━节点列表"), true);
        }
    }

}

