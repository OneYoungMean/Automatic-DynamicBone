
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

        public void OnEnable()
        {
            controller = target as ADBRuntimeController;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //OYM：更新表现形式;
            GUILayout.Space(4);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("Global Setting"), true);
            GUILayout.Space(4);
            if (!Application.isPlaying)
            {
                Titlebar("EditorMode", new Color(0.7f, 1.0f, 0.7f));
            }
            else
            {
                Titlebar("RuntimeMode", Color.red);
            }
            EditorGUILayout.LabelField("=============== Point");
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTrans"), new GUIContent("All point list :"+controller.allPointTrans?.Count), true);
                GUILayout.Space(5);
            }

            EditorGUILayout.LabelField("=============== Collider");
            string key = controller.isGenerateColliderAutomaitc?"Generate":"Refresh";

                if (GUILayout.Button(key+"Collider", GUILayout.Height(22.0f)))
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

            if ( Application.isPlaying)
            {
                EditorGUILayout.LabelField("=============== Runtime Debug");

                if (GUILayout.Button("Refresh point", GUILayout.Height(22.0f)))
                {
                    controller.RestorePoint();
                }
                if (GUILayout.Button("Force refresh point", GUILayout.Height(22.0f)))
                {
                    controller.Reset();
                }
            }


            if (controller.editorColliderList != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("editorColliderList"), new GUIContent("All Collider List :" + controller.editorColliderList?.Count), true);
            }

            Titlebar("physical setting", new Color(0.7f, 1.0f, 0.7f));

            controller. isDebug = EditorGUILayout.Toggle("isDebug", controller.isDebug);
            controller.isOptimize = EditorGUILayout.Toggle("OptimizeMove", controller.isOptimize);
            if (controller.isDebug)
            {
                controller.iteration = EditorGUILayout.IntSlider("Iterations number", controller.iteration, 1, 1024);
            }
            else
            {
                controller.iteration = EditorGUILayout.IntSlider("Iterations number", controller.iteration, 1, 256);
            }
            controller.delayTime = EditorGUILayout.FloatField("delayTime", controller.delayTime);
            controller.windForceScale = EditorGUILayout.Slider("windForcePower",controller.windForceScale, 0, 1); 
            controller.colliderCollisionType= (ColliderCollisionType)EditorGUILayout.EnumPopup("Collision Quantity",controller.colliderCollisionType);
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