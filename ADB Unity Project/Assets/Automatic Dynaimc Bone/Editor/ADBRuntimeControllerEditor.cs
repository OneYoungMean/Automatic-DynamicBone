
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    public enum ColliderCollisionTypeZh
    {
        CollideAll = 1,
        CollideStick = 2,
        CollidePoint = 3,
        NoCollide = 4
    }

    public enum UpdateModeZh
    {
        Update = 1,
        FixedUpdate = 2,
        LateUpdate = 3,
    }
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ADBRuntimeController))]
    public class ADBRuntimeControllerEditor : Editor
    {
        private bool isFoldout;
        private bool[] isFoldouts;
        ADBRuntimeController controller;
        Dictionary<Object, Editor> containerEditors;
        private const int max=16;
        public void OnEnable()
        {
            controller = target as ADBRuntimeController;
            controller.InitializeChain();
            isFoldouts = new bool[controller.allChain == null ? 0 : controller.allChain.Length];
        }
        public override void OnInspectorGUI()
        {
            Color color;
            if (Application.isPlaying)
            {
                color = new Color(0.5F, 1, 1);
            }
            else
            {
                color = new Color(0.7f, 1.0f, 0.7f);
            }
            Titlebar("ADB Kernel Controller", color,0);
            serializedObject.Update();
            if (!Application.isPlaying)
            {
                controller.ListCheck();
            }

            if (controller.allChain != null)
            {
                isFoldout = EditorGUILayout.Foldout(isFoldout, " All Transform List:" + controller.GetPointCount());
                if (isFoldout&& controller.allChain!=null)
                {
                    if (isFoldouts == null || isFoldouts.Length != controller.allChain.Length)
                    {
                        isFoldouts = new bool[controller.allChain.Length];
                    }
                    for (int i = 0; i < controller.allChain.Length; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        EditorGUILayout.BeginVertical();
                        isFoldouts[i] = EditorGUILayout.Foldout(isFoldouts[i], "Element " + i + ": Count:" + (controller.allChain[i].allPointTransforms == null ? 0 : controller.allChain[i].allPointTransforms.Length));
                        if (isFoldouts[i])
                        {
                            ShowContainerGUI(controller.allChain[i]);
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                if (controller.overlapsColliderList != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("overlapsColliderList"), new GUIContent(" All Collider List :" + controller.overlapsColliderList.Count), true);
                }
            }

            if (!Application.isPlaying)
            {
                // There need something...
            }
            else
            {
                Titlebar("=============== Transform Setting", color);

                if (GUILayout.Button("Reset Transform Data", GUILayout.Height(22.0f)))
                {
                    controller.RestoreRuntimePoint();
                }
                if (GUILayout.Button("Reset All Data", GUILayout.Height(22.0f)))
                {
                    controller.ResetData();
                }
                Titlebar("=============== Collider Setting", color);
                if (controller.overlapsColliderList != null)
                {
                    controller.colliderSize = EditorGUILayout.Slider("ColliderScale", controller.colliderSize, 0, 2);
                    controller.RefreshSize();
                }
            }

            GUILayout.Space(10);

            Titlebar("=============== Physics Setting", color,0);
            controller.iteration = EditorGUILayout.IntSlider("Iteration Mode", controller.iteration, 1, max * (controller.isRunAsync ? 8 : 1) * (controller.isParallel ? 4 : 1));
            controller.isRunAsync = EditorGUILayout.Toggle("Run Async", controller.isRunAsync);
            if (controller.isRunAsync)
            {
                controller.isParallel = EditorGUILayout.Toggle("  ┗━Run Parallel", controller.isParallel);
            }
            controller.updateMode = (UpdateMode)EditorGUILayout.EnumPopup("Update Mode", (UpdateModeZh)controller.updateMode);
            controller.colliderCollisionType = (ColliderCollisionType)EditorGUILayout.EnumPopup("ColliderMode", (ColliderCollisionTypeZh)controller.colliderCollisionType);


            GUILayout.Space(10);
            controller.bufferTime = EditorGUILayout.Slider("Smooth Time", controller.bufferTime, 0.001f, 10f);
            controller.isOptimize = EditorGUILayout.Toggle("OptimizeTrack(experiment)", controller.isOptimize);
            controller.timeScale = EditorGUILayout.Slider("TimeScale(experiment)", controller.timeScale,0,2);
            GUILayout.Space(10);
            controller.windForceScale = EditorGUILayout.Slider("Wind Force", controller.windForceScale, 0, 1);
            controller.isDrawGizmo = EditorGUILayout.Toggle("IsDrawAllGizmo", controller.isDrawGizmo);
            GUILayout.Space(20);
            serializedObject.ApplyModifiedProperties();
        }

        void Titlebar(string text, Color color, int space = 12)
        {
            GUILayout.Space(space);

            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(text);
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = backgroundColor;

            GUILayout.Space(3);
        }

        void ShowContainerGUI<T>(T container) where T : MonoBehaviour
        {
            if (container == null) return;
            if (containerEditors == null) containerEditors = new Dictionary<Object, Editor>();
            if (containerEditors.TryGetValue(container, out Editor editor))
            {
                editor.OnInspectorGUI();
            }
            else
            {
                containerEditors.Add(container, Editor.CreateEditor(container));
            }
        }
    }
}