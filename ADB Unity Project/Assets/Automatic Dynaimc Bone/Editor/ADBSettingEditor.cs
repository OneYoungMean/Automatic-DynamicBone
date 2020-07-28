using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ADBRuntime
{
    [CustomEditor(typeof(ADBSetting))]
    public class ADBSettingEditor : Editor
    {
        ADBSetting controller;
        bool showConstraintGlobal=false;
        bool showConstrainForce=false;
        public void OnEnable()
        {
            controller = target as ADBSetting;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Titlebar("节点设置", Color.green);
            controller.useGlobal=! EditorGUILayout.Toggle("高级曲线模式", !controller.useGlobal);
            if (!controller.useGlobal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScaleCurve"), new GUIContent("重力系数曲线"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("freezeCurve"), new GUIContent("刚性系数曲线"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("massCurve"), new GUIContent("怠速曲线"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByFixedPointCurve"), new GUIContent("速度补偿曲线"), true);
               // EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByPrePointCurve"), new GUIContent("Move By PrePoint 曲线"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceCompensationCurve"), new GUIContent("距离补偿曲线"), true); 
                EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionCurve"), new GUIContent("摩擦力曲线"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addForceScaleCurve"), new GUIContent("附加力系数曲线"), true);
                GUILayout.Space(10);
                showConstraintGlobal = EditorGUILayout.Foldout(showConstraintGlobal,"杆件系数曲线");
                if (showConstraintGlobal)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkVerticalScaleCurve"), new GUIContent("垂直相邻-杆件-收缩力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchVerticalScaleCurve"), new GUIContent("垂直相邻-杆件-拉伸力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkHorizontalScaleCurve"), new GUIContent("水平相邻-杆件-收缩力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchHorizontalScaleCurve"), new GUIContent("水平相邻-杆件-拉伸力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearShrinkScaleCurve"), new GUIContent("网状分布-杆件-收缩力系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearStretchScaleCurve"), new GUIContent("网状分布-杆件-拉伸力系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkVerticalScaleCurve"), new GUIContent("垂直相间-杆件-收缩力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchVerticalScaleCurve"), new GUIContent("垂直相间-杆件-拉伸力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkHorizontalScaleCurve"), new GUIContent("水平相间-杆件-收缩力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchHorizontalScaleCurve"), new GUIContent("水平相间-杆件-拉伸力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceShrinkScaleCurve"), new GUIContent("放射分布-杆件-收缩力-系数曲线"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceStretchScaleCurve"), new GUIContent("放射分布-杆件-拉伸力-系数曲线"), true);
                }
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScaleGlobal"), new GUIContent("重力系数值"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("freezeGlobal"), new GUIContent("刚性系数值"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("massGlobal"), new GUIContent("怠速值"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByFixedPointGlobal"), new GUIContent("速度补偿值"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceCompensationGlobal"), new GUIContent("距离补偿值"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionGlobal"), new GUIContent("摩擦力值"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addForceScaleGlobal"), new GUIContent("附加力系数值"), true);
              //  EditorGUILayout.PropertyField(serializedObject.FindProperty("moveByPrePointGlobal"), new GUIContent("Move By PrePoint 值"), true);


                GUILayout.Space(10);
                showConstraintGlobal = EditorGUILayout.Foldout(showConstraintGlobal, "杆件力系数值");
                if (showConstraintGlobal)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkVerticalScaleGlobal"), new GUIContent("垂直相邻-杆件-收缩力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchVerticalScaleGlobal"), new GUIContent("垂直相邻-杆件-拉伸力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkHorizontalScaleGlobal"), new GUIContent("水平相邻-杆件-收缩力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchHorizontalScaleGlobal"), new GUIContent("水平相邻-杆件-拉伸力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearShrinkScaleGlobal"), new GUIContent("网状分布-杆件-收缩力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("shearStretchScaleGlobal"), new GUIContent("网状分布-杆件-拉伸力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkVerticalScaleGlobal"), new GUIContent("垂直相间-杆件-收缩力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchVerticalScaleGlobal"), new GUIContent("垂直相间-杆件-拉伸力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkHorizontalScaleGlobal"), new GUIContent("水平相间-杆件-收缩力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchHorizontalScaleGlobal"), new GUIContent("水平相间-杆件-拉伸力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceShrinkScaleGlobal"), new GUIContent("放射分布-杆件-收缩力-系数值"), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceStretchScaleGlobal"), new GUIContent("放射分布-杆件-拉伸力-系数值"), true);
                }
            }
            Titlebar("杆件设置", Color.green);
            showConstrainForce = EditorGUILayout.Foldout(showConstrainForce, "杆件基础力");
            if (showConstrainForce)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkVertical"), new GUIContent("垂直相邻-杆件-基础收缩力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchVertical"), new GUIContent("相邻-杆件-垂直基础拉伸力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralShrinkHorizontal"), new GUIContent("相邻-杆件-水平基础收缩力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("structuralStretchHorizontal"), new GUIContent("相邻-杆件-水平基础拉伸力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shearShrink"), new GUIContent("网状分布-杆件-基础收缩力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shearStretch"), new GUIContent("网状分布-杆件-基础拉伸力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkVertical"), new GUIContent("垂直相间-杆件-基础收缩力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchVertical"), new GUIContent("垂直相间-杆件-基础拉伸力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingShrinkHorizontal"), new GUIContent("水平相间-杆件-基础收缩力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("bendingStretchHorizontal"), new GUIContent("水平相间-杆件-基础拉伸力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceShrink"), new GUIContent("分布放射-杆件-基础收缩力"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("circumferenceStretch"), new GUIContent("分布放射-杆件-基础拉伸力"), true);
                EditorGUILayout.TextArea("杆件最终力=杆件力系数值*杆件基础力");
            }

            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeStructuralVertical"), new GUIContent("开启垂直相邻杆件"), true);
            if (controller.isComputeStructuralVertical)
            {       
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isCollideStructuralVertical"), new GUIContent("┗━允许垂直相邻杆件碰撞"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeStructuralHorizontal"), new GUIContent("开启水平相邻杆件"), true);
            if (controller.isComputeStructuralHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isCollideStructuralHorizontal"), new GUIContent("┗━允许水平相邻杆件碰撞"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoopRootPoints"), new GUIContent("┗━允许首尾衔接"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeShear"), new GUIContent("开启网状分布杆件"), true);
            if (controller.isComputeShear)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isCollideShear"), new GUIContent("┗━允许网状分布杆件碰撞"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoopRootPoints"), new GUIContent("┗━允许首尾衔接"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeBendingVertical"), new GUIContent("开启垂直相间杆件"), true);
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeBendingHorizontal"), new GUIContent("开启水平相间杆件"), true);
            if (controller.isComputeBendingHorizontal)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoopRootPoints"), new GUIContent("┗━允许首尾衔接"), true);
            }
            GUILayout.Space(5);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeCircumference"), new GUIContent("开启放射分布杆件"), true);
            if (controller.isCollideShear|| controller.isCollideStructuralHorizontal||controller.isCollideStructuralVertical)
            {
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("pointRadiuCurve"), new GUIContent("节点碰撞体积半径曲线"), true);
            }
            GUILayout.Space(10);

            Titlebar("其他设置", Color.green);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isDebugDraw"), new GUIContent("绘制杆件"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isComputeVirtual"), new GUIContent("生成虚拟节点"), true);
            
            if (controller.isComputeVirtual)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isAllowComputeOtherConstraint"), new GUIContent("┗━允许虚拟节点生成其他杆件"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualPointAxisLength"), new GUIContent("┗━虚拟杆件长度"), true);
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("isAutoComputeWeight"), new GUIContent("自动计算节点质量"), true);
            if (!controller.isAutoComputeWeight)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("weightCurve"), new GUIContent("┗━质量曲线"), true);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"), new GUIContent("重力"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isFixGravityAxis"), new GUIContent("重力轴随角色旋转而旋转"), true);
            controller.colliderChoice =(ColliderChoice) EditorGUILayout.EnumFlagsField("接收以下种类的碰撞体的信息",controller.colliderChoice);
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


