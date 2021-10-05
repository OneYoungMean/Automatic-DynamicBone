
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ADBRuntime
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
    [CustomEditor(typeof(ADBRuntimeController))]
    public class ADBRuntimeEditor : Editor
    //OYM：它的编辑器，我觉得我有必要把一部分方法写到里面去
    {

        ADBRuntimeController controller;
        private bool isDeleteCollider;
        private bool isGenerateColliderOpenTrigger;
        private const int max=64;
        public void OnEnable()
        {
            controller = target as ADBRuntimeController;

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
            serializedObject.Update();
            //OYM：更新表现形式;
            if (!Application.isPlaying)
            {
                Titlebar("ADB控制器", color);
                //报错
                if (controller.settings == null)
                {
                    Titlebar("错误:全局关联设置不能为空!", new Color(0.7f, 0.3f, 0.3f));
                }               
                if (controller.generateKeyWordWhiteList==null|| controller.generateKeyWordWhiteList.Count==0)
                {
                    Titlebar("警告:识别关键词缺失", Color.yellow);
                }
                else if(controller.settings!=null)
                {
                    for (int i = 0; i < controller.generateKeyWordWhiteList.Count; i++)
                    {
                        if (!controller.settings.isContain(controller.generateKeyWordWhiteList[i]))
                        {
                            Titlebar("警告:关键词: "+controller.generateKeyWordWhiteList[i]+"不在全局关联设置内!", Color.yellow);
                        }

                    }
                }
                if (controller.colliderControll!=null&& (controller.colliderControll.isGenerateSuccessful == -1))
                {
                    Titlebar("碰撞体似乎没有生成成功,尝试将脚本挂载在Animator脚本下方试试", Color.grey);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("全局关联设置"), true);

                GUILayout.Space(5);
                Titlebar("=============== 节点设置", color);
                if (controller.generateTransform==null)
                {
                    controller.generateTransform = controller.transform;
                }
                controller.generateTransform = (Transform)EditorGUILayout.ObjectField(new GUIContent("搜索起始点"), controller.generateTransform, typeof(Transform), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordWhiteList"), new GUIContent("识别关键词"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("blackListOfGenerateTransform"), new GUIContent("节点黑名单"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("generateKeyWordBlackList"), new GUIContent("关键词黑名单"), true);

                if (GUILayout.Button("生成节点数据", GUILayout.Height(22.0f)))
                {
                    controller.ListCheck();
                    controller.InitializePoint();
                    controller.isDebug = true;
                }

                if (controller.allPointTrans != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inspectorPointList"), new GUIContent("所有节点坐标 :" + controller.allPointTrans.Count), true);
                    GUILayout.Space(5);
                }
            }
            else
            {
                Titlebar("运行中", new Color(0.5F, 1, 1));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings"), new GUIContent("全局关联设置"), true);
                GUILayout.Space(10);

                Titlebar("=============== 节点设置", color);

                if (GUILayout.Button("重置所有节点位置", GUILayout.Height(22.0f)))
                {
                    controller.RestoreRuntimePoint();
                }
                if (GUILayout.Button("重置所有节点数据并重新运行", GUILayout.Height(22.0f)))
                {
                    controller.Reset();
                }
                if (controller.allPointTrans != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("inspectorPointList"), new GUIContent("所有节点坐标 :" + controller.allPointTrans?.Count), true);
                }
            }
            Titlebar("=============== 碰撞体设置", color);
            if (controller.overlapsColliderList != null)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("overlapsColliderList"), new GUIContent("碰撞体列表 :" + controller.overlapsColliderList.Count), true);

            }
            GUILayout.Space(5);

            string key = controller.isGenerateColliderAutomaitc ? "生成" : "刷新";

            if (GUILayout.Button(key + "碰撞体", GUILayout.Height(22.0f)))
            {
                controller.initializeCollider();
                controller.UpdateOverlapsCollider();
            }
            if (controller.generateColliderList == null|| controller.generateColliderList.Count==0)
            {
                controller.isGenerateColliderAutomaitc = EditorGUILayout.Toggle("自动生成全身碰撞体 ", controller.isGenerateColliderAutomaitc);
                if (controller.isGenerateColliderAutomaitc)
                {
                    controller. isGenerateColliderOpenTrigger = EditorGUILayout.Toggle("  ┗━生成的碰撞体为trigger ", controller.isGenerateColliderOpenTrigger);
                }
                if (controller.isGenerateColliderAutomaitc)
                {
                    controller.isGenerateByAllPoint = EditorGUILayout.Toggle("  ┗━以所有节点作为参照 ", controller.isGenerateByAllPoint);
                }
                if (controller.isGenerateColliderAutomaitc)
                {
                    controller.isGenerateFinger = EditorGUILayout.Toggle("  ┗━生成手指 ", controller.isGenerateFinger);
                }
            }
            if (GUILayout.Button("删除所有生成的碰撞体", GUILayout.Height(22.0f)))
            {
                if (EditorUtility.DisplayDialog("你确定需要删除吗?", "该操作不可撤销", "ok", "cancel"))
                {
                    for (int i = 0; i < controller.overlapsColliderList?.Count; i++)
                    {
                        if (controller.overlapsColliderList[i] != null)
                        {
                            if (controller.overlapsColliderList[i].gameObject.GetComponents<Component>().Length <= 3)
                            {
                                DestroyImmediate(controller.overlapsColliderList[i].gameObject);
                            }
                            else
                            {
                                DestroyImmediate(controller.overlapsColliderList[i]);
                            }

                        }
                    }
                    controller.generateColliderList = null;

                    if (isDeleteCollider)
                    {
                        for (int i = 0; i < controller.overlapsColliderList?.Count; i++)
                        {
                            if (controller.overlapsColliderList[i] != null)
                            {
                                if (controller.overlapsColliderList[i].gameObject.GetComponents<Component>().Length <= 3)
                                {
                                    DestroyImmediate(controller.overlapsColliderList[i].gameObject);
                                }
                                else
                                {
                                    DestroyImmediate(controller.overlapsColliderList[i]);
                                }

                            }
                        }
                        controller.overlapsColliderList.Clear();
                    }
                }
            }
            isDeleteCollider = EditorGUILayout.Toggle("  ┗━包括不是自动生成的碰撞体 ", isDeleteCollider);


            GUILayout.Space(10);

            Titlebar("=============== 物理设置", color);
            controller.iteration = EditorGUILayout.IntSlider("迭代次数", controller.iteration, 1, max * (controller.isParallel ? 8 : 8) * (controller.isDebug ? 2 : 1));
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

            GUILayout.Space(10);
            controller.windForceScale = EditorGUILayout.Slider("风力", controller.windForceScale, 0, 1);
            controller.isDebug = EditorGUILayout.Toggle("是否绘制所有辅助线", controller.isDebug);
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