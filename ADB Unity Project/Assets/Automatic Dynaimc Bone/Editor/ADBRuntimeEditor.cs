
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ADBRuntime
{
    using Mono;

    [CustomEditor(typeof(ADBRuntimeController))]
    public class ADBRuntimeEditor : Editor
    //OYM：它的编辑器，我觉得我有必要把一部分方法写到里面去
    {

        ADBRuntimeController controller;
        private bool isDeleteCollider;
        private int max;
        public void OnEnable()
        {
            controller = target as ADBRuntimeController;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //OYM：更新表现形式;

            if (!Application.isPlaying)
            {

                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("Global Setting"), true);
  
                Titlebar("=============== Point setting", new Color(0.7f, 1.0f, 0.7f));
                controller.generateTransform = (Transform)EditorGUILayout.ObjectField(new GUIContent("parent Transform"), controller.generateTransform, typeof(Transform), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordWhiteList"), new GUIContent("Name KeyWord"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blackListOfGenerateTransform"), new GUIContent("Transform BlackList"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordBlackList"), new GUIContent("Name Key Word BlackList"), true);

                if (GUILayout.Button("Generate Point", GUILayout.Height(22.0f)))
                {
                    controller.initializePoint();
                    controller.isDebug = true;
                }

                if (controller.allPointTrans != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTrans"), new GUIContent("All point list :" + controller.allPointTrans?.Count), true);
                    GUILayout.Space(5);
                }

                Titlebar("=============== Collider setting", new Color(0.7f, 1.0f, 0.7f));
                string key = controller.isGenerateColliderAutomaitc ? "Generate" : "Refresh";

                if (GUILayout.Button(key + "Collider", GUILayout.Height(22.0f)))
                {
                    controller.initializeCollider();
                    controller.isDebug = true;
                }
                controller.isGenerateColliderAutomaitc = EditorGUILayout.Toggle("┗━Is Generate Body Collider Automatic ", controller.isGenerateColliderAutomaitc);
                if (controller.isGenerateColliderAutomaitc)
                {
                    controller.isGenerateByFixedPoint = EditorGUILayout.Toggle("   ┗━Is Generate By Fixed Point ", controller.isGenerateByFixedPoint);
                }

                if (GUILayout.Button("Remove All Collider", GUILayout.Height(22.0f)))
                {
                    if (controller.editorColliderList == null) return;
                    if (isDeleteCollider)
                    {
                        for (int i = 0; i < controller.editorColliderList.Count; i++)
                        {
                            DestroyImmediate(controller.editorColliderList[i]);
                        }
                        isDeleteCollider = false;
                    }
                    controller.editorColliderList = null;
                }
                isDeleteCollider = EditorGUILayout.Toggle("is Delete Collider Script", isDeleteCollider);

                if (controller.editorColliderList != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("editorColliderList"), new GUIContent("All Collider List :" + controller.editorColliderList?.Count), true);
                }

                controller.delayTime = EditorGUILayout.FloatField("delayTime", controller.delayTime);
                Titlebar("=============== physical setting", new Color(0.7f, 1.0f, 0.7f));
            }
            else
            {
                Titlebar("RuntimeMode", new Color(0.5F, 1, 1));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("Global Setting"), true);
                EditorGUILayout.Space(10);

                Titlebar("=============== Point setting", new Color(0.5F,1,1));

                if (GUILayout.Button("Refresh Point Position", GUILayout.Height(22.0f)))
                {
                    controller.RestorePoint();
                }
                if (GUILayout.Button("Refresh All Point Data", GUILayout.Height(22.0f)))
                {
                    controller.Reset();
                }
                if (controller.allPointTrans != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTrans"), new GUIContent("All point list :" + controller.allPointTrans?.Count), true);
                }

                Titlebar("=============== Collider setting", new Color(0.5F, 1, 1));
                EditorGUILayout.Space(5);
                if (GUILayout.Button("RefreshColliderGizmo", GUILayout.Height(22.0f)))
                {
                    for (int i = 0; i < controller.editorColliderList.Count; i++)
                    {
                        controller.editorColliderList[i].Refresh();
                    }
                    controller.isDebug = true;
                }

                if (controller.editorColliderList != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("editorColliderList"), new GUIContent("All Collider List :" + controller.editorColliderList?.Count), true);
                }
                Titlebar("=============== physical setting", new Color(0.5F, 1, 1));
            }


            controller.isDebug = EditorGUILayout.Toggle("isDebug", controller.isDebug);
            controller.isOptimize = EditorGUILayout.Toggle("OptimizeMove", controller.isOptimize);
            max = max> controller.iteration?max : Mathf.CeilToInt(controller.iteration*1.1f);
            max = max > 2048 ? 2048 : max;
            controller.iteration = EditorGUILayout.IntSlider("Iterations number", controller.iteration, 1, max);
            controller.windForceScale = EditorGUILayout.Slider("windForcePower", controller.windForceScale, 0, 1);
            controller.colliderCollisionType = (ColliderCollisionType)EditorGUILayout.EnumPopup("Collision Quantity", controller.colliderCollisionType);
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