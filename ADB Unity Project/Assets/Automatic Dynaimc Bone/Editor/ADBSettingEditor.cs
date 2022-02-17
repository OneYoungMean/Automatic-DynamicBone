using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime.UntiyEditor
{
    [CustomEditor(typeof(ADBPhysicsSetting))]
    public class ADBSettingFileEditor : Editor
    {
        ADBPhysicsSetting controller;
        bool showConstraintValue=false;
        bool showConstrainForce=false;
        private enum ColliderChoiceZh
        {
            头 = 1 << 0,
            上半身 = 1 << 1,
            下半身 = 1 << 2,
            大腿 = 1 << 3,
            小腿 = 1 << 4,
            大臂 = 1 << 5,
            小臂 = 1 << 6,
            手 = 1 << 7,
            脚 = 1 << 8,
            其他 = 1 << 9,
        }
        public void OnEnable()
        {
            controller = target as ADBPhysicsSetting;
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("创建副本"))
            {
                var copy = Object.Instantiate(controller);
                
                copy.name = controller.name+" copy";
                string path =System.IO.Path.GetDirectoryName( AssetDatabase.GetAssetPath(controller));
                string newPath = EditorUtility.SaveFilePanel("PhysicsSetting Save Path", path, copy.name, "asset");
                if (!string.IsNullOrEmpty(newPath))
                {
                    newPath= System.IO.Path.GetFullPath(newPath);
                    string rootPath = System.IO.Path.GetDirectoryName(Application.dataPath);
                    newPath = newPath.Replace(rootPath+"\\", "");
                    AssetDatabase.CreateAsset(Object.Instantiate(controller), newPath);
                }
            }

/*            if (GUILayout.Button("转换为曲线"))
            {

            }*/

            serializedObject.Update();
            Titlebar("物理控制器", Color.white);

            Titlebar("曲线|节点设置", Color.green);
                DrawValueOrCurve(serializedObject,"gravityScale","重力系数");
                DrawValueOrCurve(serializedObject,"stiffnessWorld","位移刚性系数");
                DrawValueOrCurve(serializedObject,"stiffnessLocal","夹角刚性系数");
                DrawValueOrCurve(serializedObject,"elasticity","弹性");
                DrawValueOrCurve(serializedObject,"elasticityVelocity","弹性速度");
                DrawValueOrCurve(serializedObject,"lengthLimitForceScale","长度限制力系数");
                DrawValueOrCurve(serializedObject,"damping","怠速");
                DrawValueOrCurve(serializedObject,"moveInert","位移减少");
                DrawValueOrCurve(serializedObject,"velocityIncrease","速度增强");
                DrawValueOrCurve(serializedObject,"friction","摩擦力");
                DrawValueOrCurve(serializedObject,"addForceScale","附加力系数");

            GUILayout.Space(5);
            DrawValueOrCurve(serializedObject,"pointRadiu","节点碰撞体积半径");
            Titlebar("杆件设置", Color.green);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeStructuralVertical"), new GUIContent("开启垂直相邻杆件"), true);
            if (controller.isComputeStructuralVertical)
            {       
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isCollideStructuralVertical"), new GUIContent("┗━允许垂直相邻杆件碰撞"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeStructuralHorizontal"), new GUIContent("开启水平相邻杆件"), true);
            if (controller.isComputeStructuralHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isCollideStructuralHorizontal"), new GUIContent("┗━允许水平相邻杆件碰撞"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isLoopRootPoints"), new GUIContent("┗━允许首尾衔接"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeShear"), new GUIContent("开启网状分布杆件"), true);
            if (controller.isComputeShear)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isCollideShear"), new GUIContent("┗━允许网状分布杆件碰撞"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isLoopRootPoints"), new GUIContent("┗━允许首尾衔接"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeBendingVertical"), new GUIContent("开启垂直相间杆件"), true);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeBendingHorizontal"), new GUIContent("开启水平相间杆件"), true);
            if (controller.isComputeBendingHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isLoopRootPoints"), new GUIContent("┗━允许首尾衔接"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeCircumference"), new GUIContent("开启放射分布杆件"), true);

            GUILayout.Space(10);
            showConstraintValue = EditorGUILayout.Foldout(showConstraintValue, "杆件系数曲线");
            if (showConstraintValue)
            {
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "structuralShrinkVertical", "structuralStretchVertical", "垂直相邻范围");
                DrawValueOrCurve(serializedObject, "structuralShrinkVerticalScale", "垂直相邻收缩系数");
                DrawValueOrCurve(serializedObject, "structuralStretchVerticalScale", "垂直相邻拉伸系数");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "structuralShrinkHorizontal", "structuralStretchHorizontal", "水平相邻范围");
                DrawValueOrCurve(serializedObject, "structuralShrinkHorizontalScale", "水平相邻收缩系数");
                DrawValueOrCurve(serializedObject, "structuralStretchHorizontalScale", "水平相邻拉伸系数");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "shearShrink", "shearStretch", "网状分布范围");
                DrawValueOrCurve(serializedObject, "shearShrinkScale", "网状分布收缩力系数");
                DrawValueOrCurve(serializedObject, "shearStretchScale", "网状分布拉伸力系数");
                GUILayout.Space(5);

                DrawMinMaxSlider(serializedObject, "bendingShrinkVertical", "bendingStretchVertical", "垂直相间范围");
                DrawValueOrCurve(serializedObject, "bendingShrinkVerticalScale", "垂直相间收缩系数");
                DrawValueOrCurve(serializedObject, "bendingStretchVerticalScale", "垂直相间拉伸系数");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "bendingShrinkHorizontal", "bendingStretchHorizontal", "水平相间范围");
                DrawValueOrCurve(serializedObject, "bendingShrinkHorizontalScale", "水平相间收缩系数");
                DrawValueOrCurve(serializedObject, "bendingStretchHorizontalScale", "水平相间拉伸系数");
                GUILayout.Space(5);
                DrawMinMaxSlider(serializedObject, "circumferenceShrink", "circumferenceStretch", "分布放射范围");
                DrawValueOrCurve(serializedObject, "circumferenceShrinkScale", "放射分布收缩系数");
                DrawValueOrCurve(serializedObject, "circumferenceStretchScale", "放射分布拉伸系数");
                GUILayout.Label("杆件最终力=杆件力系数值*超出范围的长度部分");
            }
            GUILayout.Space(10);

            Titlebar("其他设置", Color.green);

/*            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isDebugDraw"), new GUIContent("绘制杆件"), true);*/
            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isComputeVirtual"), new GUIContent("生成虚拟节点"), true);
            
            if (controller.isComputeVirtual)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "isAllowComputeOtherConstraint"), new GUIContent("┗━允许虚拟节点生成其他杆件"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "virtualPointAxisLength"), new GUIContent("┗━虚拟杆件长度"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "ForceLookDown"), new GUIContent("┗━强制末端朝下"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isAutoComputeWeight"), new GUIContent("自动计算节点质量"), true);
            if (!controller.isAutoComputeWeight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty( "weightCurve"), new GUIContent("┗━质量曲线"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty( "gravity"), new GUIContent("重力"), true);
            //EditorGUILayout.PropertyField(serializedObject.FindProperty( "isFixGravityAxis"), new GUIContent("重力轴随角色旋转而旋转"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty( "isFixedPointFreezeRotation"), new GUIContent("固定节点是否冻结旋转"), true);
            controller.colliderChoice =(ColliderChoice) EditorGUILayout.EnumFlagsField("接收以下种类的碰撞体的信息",(ColliderChoiceZh)controller.colliderChoice);
            EditorUtility.SetDirty(controller);
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

        static void DrawValueOrCurve(SerializedObject serializedObject, string baseName,string baseNameCN)
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                string boolName = "is" + baseName + "Curve";
                var boolField = serializedObject.FindProperty(boolName);
                EditorGUILayout.PropertyField(boolField, new GUIContent(""), false, GUILayout.MaxWidth(30));
                if (boolField.boolValue)
                {
                    var cruveField = serializedObject.FindProperty(baseName + "Curve");
                    EditorGUILayout.PropertyField(cruveField, new GUIContent(baseNameCN + "曲线"), true, GUILayout.MinWidth(250));
                }
                else
                {
                    var valueField = serializedObject.FindProperty(baseName + "Value");
                    var valueMin = serializedObject.FindProperty(baseName + "Min");
                    var valueMax = serializedObject.FindProperty(baseName + "Max");
                    valueField.floatValue = EditorGUILayout.Slider(baseNameCN + "值", valueField.floatValue, valueMin.floatValue, valueMax.floatValue);
                    //EditorGUILayout.PropertyField(valueField, new GUIContent(baseNameCN + "值"), false);
                }

                EditorGUILayout.EndHorizontal();
            }
            catch (System.Exception)
            {

                throw;
            }

        }

        static void DrawMinMaxSlider(SerializedObject serializedObject, string baseNameMin, string baseNameMax, string baseNameCN)
        {
            EditorGUILayout.BeginHorizontal();
            var fieldMin = serializedObject.FindProperty(baseNameMin);
            var fieldMax = serializedObject.FindProperty(baseNameMax);
            var valueMin = fieldMin.floatValue;
            var valueMax = fieldMax.floatValue;
            valueMin= EditorGUILayout.FloatField(valueMin, GUILayout.MaxWidth(30));
            valueMax = EditorGUILayout.FloatField(valueMax, GUILayout.MaxWidth(30));
            EditorGUILayout.MinMaxSlider(baseNameCN+" "+valueMin.ToString("f2")+"-"+ valueMax.ToString("f2"), ref valueMin, ref valueMax, 0, 2);

            fieldMin.floatValue = valueMin;
            fieldMax.floatValue = valueMax;
            EditorGUILayout.EndHorizontal();
        }
    }
}


