
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ADBRuntime
{
    using Mono;
    public enum ColliderCollisionTypeZh
    {
        全体碰撞=1,
        仅杆件碰撞=2,
        仅节点碰撞=3,
        不计算碰撞=4
    }
    [CustomEditor(typeof(ADBRuntimeController))]
    public class ADBRuntimeEditor : Editor
    //OYM：它的编辑器，我觉得我有必要把一部分方法写到里面去
    {

        ADBRuntimeController controller;
        private bool isDeleteCollider;
        private int max;
        private ColliderCollisionTypeZh colliderCollisionTypeZh;
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

                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("全局关联设置"), true);
  
                Titlebar("=============== 节点设置", new Color(0.7f, 1.0f, 0.7f));
                controller.generateTransform = (Transform)EditorGUILayout.ObjectField(new GUIContent("搜索起始点"), controller.generateTransform, typeof(Transform), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordWhiteList"), new GUIContent("识别关键词"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blackListOfGenerateTransform"), new GUIContent("节点黑名单"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordBlackList"), new GUIContent("关键词黑名单"), true);

                if (GUILayout.Button("生成节点数据", GUILayout.Height(22.0f)))
                {
                    controller.initializePoint();
                    controller.isDebug = true;
                }

                if (controller.allPointTrans != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTrans"), new GUIContent("所有节点坐标 :" + controller.allPointTrans?.Count), true);
                    GUILayout.Space(5);
                }

                Titlebar("=============== 碰撞体设置", new Color(0.7f, 1.0f, 0.7f));
                string key = controller.isGenerateColliderAutomaitc ? "生成" : "刷新";

                if (GUILayout.Button(key + "碰撞体", GUILayout.Height(22.0f)))
                {
                    controller.initializeCollider();
                    controller.isDebug = true;
                }
                controller.isGenerateColliderAutomaitc = EditorGUILayout.Toggle("自动生成全身碰撞体 ", controller.isGenerateColliderAutomaitc);
                if (controller.isGenerateColliderAutomaitc)
                {
                    controller.isGenerateByFixedPoint = EditorGUILayout.Toggle( controller.isGenerateByFixedPoint? "  ┗━以固定节点作为参照 ": "  ┗━以所有节点作为参照 ", controller.isGenerateByFixedPoint);
                }

                if (GUILayout.Button("移除所有碰撞体", GUILayout.Height(22.0f)))
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
                isDeleteCollider = EditorGUILayout.Toggle("同时删去碰撞体脚本", isDeleteCollider);

                if (controller.editorColliderList != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("editorColliderList"), new GUIContent("碰撞体列表 :" + controller.editorColliderList?.Count), true);
                }

                controller.delayTime = EditorGUILayout.FloatField("延迟时间", controller.delayTime);
                Titlebar("=============== physical setting", new Color(0.7f, 1.0f, 0.7f));
            }
            else
            {
                Titlebar("运行中", new Color(0.5F, 1, 1));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("全局关联设置"), true);
                EditorGUILayout.Space(10);

                Titlebar("=============== 节点设置", new Color(0.5F,1,1));

                if (GUILayout.Button("重置所有节点位置", GUILayout.Height(22.0f)))
                {
                    controller.RestorePoint();
                }
                if (GUILayout.Button("重置所有节点数据并重新运行", GUILayout.Height(22.0f)))
                {
                    controller.Reset();
                }
                if (controller.allPointTrans != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allPointTrans"), new GUIContent("所有节点坐标 :" + controller.allPointTrans?.Count), true);
                }

                Titlebar("===============碰撞体设定", new Color(0.5F, 1, 1));
                EditorGUILayout.Space(5);
                if (GUILayout.Button("重新绘制碰撞体", GUILayout.Height(22.0f)))
                {
                    for (int i = 0; i < controller.editorColliderList.Count; i++)
                    {
                        controller.editorColliderList[i].Refresh();
                    }
                    controller.isDebug = true;
                }

                if (controller.editorColliderList != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("editorColliderList"), new GUIContent("碰撞体列表 :" + controller.editorColliderList?.Count), true);
                }
                Titlebar("=============== physical setting", new Color(0.5F, 1, 1));
            }


            controller.isDebug = EditorGUILayout.Toggle("是否绘制所有辅助线", controller.isDebug);
            controller.isOptimize = EditorGUILayout.Toggle("轨迹优化", controller.isOptimize);

            max = max> controller.iteration?max : Mathf.CeilToInt(controller.iteration*1.1f);
            max = max < 16 ? 16 : max;
            max = max > 2048 ? 2048 : max;
            controller.iteration = EditorGUILayout.IntSlider("迭代次数", controller.iteration, 1, max);
            controller.windForceScale = EditorGUILayout.Slider("风力", controller.windForceScale, 0, 1);
            controller.colliderCollisionType = (ColliderCollisionType)EditorGUILayout.EnumPopup("碰撞模式", (ColliderCollisionTypeZh)controller.colliderCollisionType);
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