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
            EditorGUILayout.ObjectField("Root Transform", controller.transform, typeof(Transform), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("aDBSetting"), new GUIContent("©»©¥Physics Setting"), true);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("keyWord"), new GUIContent("©»©¥Keyword"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTransforms"), new GUIContent("©»©¥Transform List"), true);
        }
    }

}

