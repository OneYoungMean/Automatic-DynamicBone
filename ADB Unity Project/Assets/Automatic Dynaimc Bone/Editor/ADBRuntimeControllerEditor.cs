
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ADBRuntime.UntiyEditor
{
    using Mono;
    public enum ColliderCollisionTypeZh
    {
        全体碰撞I杆件迭代 = 1,
        仅杆件碰撞I杆件迭代 = 2,
        仅节点碰撞I节点迭代 = 3,
        不计算碰撞I节点迭代 = 4
    }

    public enum UpdateModeZh
    {
        Update更新= 1,
        FixedUpdate更新 = 2,
        LateUpdate更新 = 3,
    }
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ADBRuntimeController))]
    public class ADBRuntimeControllerEditor : Editor
    //OYM：它的编辑器，我觉得我有必要把一部分方法写到里面去
    {
        private bool isFoldout;
        private bool[] isFoldouts;
        ADBRuntimeController controller;
        Dictionary<Object, Editor> containerEditors;
        private const int max=16;
        public void OnEnable()
        {
            //OYM:在这里,serializedObject 是所有被选中的对象,target是最后一个被选中的对象
            //OYM:如果需要批量修改的话,建议使用serializedObject而不是target

            controller = target as ADBRuntimeController;
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
            Titlebar("ADB物理控制器", color,0);
            serializedObject.Update();
            if (!Application.isPlaying)
            {
                controller.ListCheck();
            }

            if (controller.allChain != null)
            {
                isFoldout = EditorGUILayout.Foldout(isFoldout, "  所有节点坐标 :" + controller.GetPointCount());
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
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("overlapsColliderList"), new GUIContent(" 所有碰撞体坐标 :" + controller.overlapsColliderList.Count), true);
                }
            }
            //OYM：更新表现形式;
            if (!Application.isPlaying)
            {

            }
            else
            {
                Titlebar("=============== 节点设置", color);

                if (GUILayout.Button("重置所有节点位置", GUILayout.Height(22.0f)))
                {
                    controller.RestoreRuntimePoint();
                }
                if (GUILayout.Button("重置所有节点数据并重新运行", GUILayout.Height(22.0f)))
                {
                    controller.ResetData();
                }
                Titlebar("=============== 碰撞体设置", color);
                if (controller.overlapsColliderList != null)
                {
                    controller.colliderSize = EditorGUILayout.Slider("碰撞体大小", controller.colliderSize, 0, 2);
                    controller.RefreshSize();
                }
            }

            GUILayout.Space(10);

            Titlebar("=============== 物理设置", color,0);
            controller.iteration = EditorGUILayout.IntSlider("迭代次数", controller.iteration, 1, max * (controller.isRunAsync ? 8 : 1) * (controller.isParallel ? 4 : 1));
            controller.isRunAsync = EditorGUILayout.Toggle("是否在多线程运行", controller.isRunAsync);
            if (controller.isRunAsync)
            {
                controller.isParallel = EditorGUILayout.Toggle("  ┗━并行模式", controller.isParallel);
            }
            controller.updateMode = (UpdateMode)EditorGUILayout.EnumPopup("更新模式", (UpdateModeZh)controller.updateMode);
            controller.colliderCollisionType = (ColliderCollisionType)EditorGUILayout.EnumPopup("碰撞模式", (ColliderCollisionTypeZh)controller.colliderCollisionType);


            GUILayout.Space(10);
            controller.bufferTime = EditorGUILayout.FloatField("平滑时间长度", controller.bufferTime);
            controller.isOptimize = EditorGUILayout.Toggle("优化移动轨迹(实验)", controller.isOptimize);
            controller.timeScale = EditorGUILayout.Slider("时间比率(实验)", controller.timeScale,0,2);
            GUILayout.Space(10);
            controller.windForceScale = EditorGUILayout.Slider("风力", controller.windForceScale, 0, 1);
            controller.isDrawGizmo = EditorGUILayout.Toggle("是否绘制所有辅助线", controller.isDrawGizmo);
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