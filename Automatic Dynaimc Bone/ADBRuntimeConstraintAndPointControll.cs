using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;

namespace ADBRuntime
{
    //OYM：写在开头,这里有很多种类的点,大致可以分成下面几种
    //OYM：rootpoint 最原始的节点,比如说head,只有一个逻辑上的作用
    //OYM：fixedpoint 固定的节点,通常是root的子节点,用于牵引普通节点
    //OYM：各种普通的point,可以自由活动
    //OYM：virtualpoint,不存在的点,只迭代一次,用于计算那种指起到旋转作用的节点
    public class ADBConstraintReadAndPointControll
    {

        public ADBSetting ADBSetting;

        //OYM：pointList
        public ADBRuntimePoint rootNode;
        public List<ADBRuntimePoint> fixedNodeList;
        public List<ADBRuntimePoint> allNodeList;

        private List<ADBRuntimePoint> addVirtualPointList;//OYM：add in this list if you want to add a virtual point to this
        //OYM：constraintList
        private List<ADBConstraintRead> constraintsStructuralVertical;//OYM：所有的垂直拉约束
        private List<ADBConstraintRead> constraintsStructuralHorizontal;//OYM：所有的水平拉约束
        private List<ADBConstraintRead> constraintsShear;//OYM：剪切力约束
        private List<ADBConstraintRead> constraintsBendingVertical;//OYM：垂直弯曲约束
        private List<ADBConstraintRead> constraintsBendingHorizontal;//OYM：水平弯曲约束
        private List<ADBConstraintRead> constraintsCircumference;//OYM：圆心约束
        private List<ADBConstraintRead> constraintsVirtual;//OYM：virtual点的约束
        private ADBConstraintRead[][] allconstraints;
        //OYM：struct list
        private ConstraintRead[][] constraintList;
        private PointRead[] pointReadList;
        private PointReadWrite[] pointReadWriteList;
        private Transform[] pointTransformsList;

        private bool isInitialize;
        private string keyWord;
        private int maxNodeDepth;


        private ADBConstraintReadAndPointControll(Transform rootTransform, string keyWord, ADBSetting setting)
        {
            rootNode = new ADBRuntimePoint(rootTransform, -1);//OYM：rootpoint指所有fix骨骼的同一个父节点,他会独立出来,而不是参与到计算当中
            rootNode.index = -1;
            this.keyWord = keyWord;
            fixedNodeList = new List<ADBRuntimePoint>();
            allNodeList = new List<ADBRuntimePoint>();
            addVirtualPointList = new List<ADBRuntimePoint>();
            maxNodeDepth = 1;
            ADBSetting = setting;
        }

        public void Initialize()
        {
            if (isInitialize) return;
            SerializeAndSearchAllPoints(rootNode, ref allNodeList, out maxNodeDepth);//OYM：递归搜索子节点,获取所有节点的List,计算所有节点的deepRate
            UpdateJointConnection(fixedNodeList);//OYM：这里是给所有的节点进行设置，同时获取所有可以搜索到的两点中间的约束并对其进行分类

            SerializeVirtuaPoint(ref addVirtualPointList);

            CreationConstraintList();
            CreatePointStructList(allNodeList);

            isInitialize = true;
        }

        private void SerializeVirtuaPoint(ref List<ADBRuntimePoint> PointList)//OYM：generate virtual point and a ConstraintStructuralVertical between then.
        {
            for (int i = 0; i < PointList.Count; i++)//OYM：在这里生成一个不存在的点,用于处理只有一个点控制骨骼的情况(比如前额头发),这里给它了一个oldposition加上一个Vector3.down
            {
                if (PointList[i].childNode == null && !PointList[i].isVirtual)
                {
                    var virtualPoint = new ADBRuntimePoint(PointList[i].trans, PointList[i].depth+1, PointList[i].keyWord, true);
                    PointList[i].childNode = new List<ADBRuntimePoint>() { virtualPoint };
                    PointList[i].pointRead.childFirstIndex = allNodeList.Count;
                    PointList[i].pointRead.childLastIndex = allNodeList.Count + 1;
                    virtualPoint.index = allNodeList.Count;
                    virtualPoint.pointRead.fixedIndex = PointList[i].pointRead.fixedIndex;
                    virtualPoint.pointRead.childFirstIndex = virtualPoint.pointRead.childLastIndex = -1;
                    virtualPoint.pointRead.parent = PointList[i].index;
                    virtualPoint.pointDepthRateMaxPointDepth = 1;
                    virtualPoint.pointRead.boneAxis = Vector3.down * 0.1f;
                    virtualPoint.pointRead.localRotation = Quaternion.identity;
                    virtualPoint.pointRead.initialPosition = Vector3.zero;
                   allNodeList.Add(virtualPoint);
                }
            }
            constraintsVirtual = new List<ADBConstraintRead>();
            for (int i = 0; i < PointList.Count; ++i)
            {
                CreateConstraintStructuralVertical(PointList[i], ref constraintsVirtual , ADBSetting.structuralShrinkVertical, ADBSetting.structuralStretchVertical);
            }
            for (int i = 0; i < constraintsVirtual.Count; i++)
            {
                constraintsVirtual[i].constraintRead.isCollider = false;
                constraintsVirtual[i].constraintRead.type = ConstraintType.Virtual ;
            }
        }

        private void CreatePointStructList(List<ADBRuntimePoint> allPointList)
        {
            ComputeQuality(constraintsStructuralHorizontal,constraintsStructuralVertical);

            pointReadList = new PointRead[allPointList.Count];
            pointReadWriteList = new PointReadWrite[allPointList.Count];
            pointTransformsList = new Transform[allPointList.Count];
            for (int i = 0; i < allPointList.Count; ++i)
            {
                //OYM：有一部分设置被我搬到生成constrain里面生成去了,在这里生成太啰嗦了,不想写.
                var point = allPointList[i];//OYM：翻译出来是源文件  
                point.pointRead.initialPosition = point.trans.position- allPointList[point.pointRead.fixedIndex].trans.position;//OYM：相对于固定点的位置

                if (ADBSetting.isAdvantage)
                {
                    float rate = point.pointDepthRateMaxPointDepth;
                    rate = Mathf.Clamp01(rate);

                    point.pointRead.mass =  ADBSetting.massCurve.Evaluate(rate);//OYM：重力补间
                    point.pointRead.gravity = ADBSetting.gravity * ADBSetting.gravityScaleCurve.Evaluate(rate);//OYM：重力补间
                    point.pointRead.circumferenceShrink = 0.5f * ADBSetting.structuralCircumferenceShrinkScaleCurve.Evaluate(rate);
                    point.pointRead.circumferenceStretch = 0.5f * ADBSetting.structuralCircumferenceStretchScaleCurve.Evaluate(rate);
                    point.pointRead.structuralShrinkVertical = 0.5f * ADBSetting.structuralShrinkVerticalScaleCurve.Evaluate(rate);
                    point.pointRead.structuralStretchVertical = 0.5f * ADBSetting.structuralStretchVerticalScaleCurve.Evaluate(rate);
                    point.pointRead.structuralShrinkHorizontal = 0.5f * ADBSetting.structuralShrinkHorizontalScaleCurve.Evaluate(rate);
                    point.pointRead.structuralStretchHorizontal = 0.5f * ADBSetting.structuralStretchHorizontalScaleCurve.Evaluate(rate);
                    point.pointRead.shearShrink = 0.5f * ADBSetting.shearShrinkScaleCurve.Evaluate(rate);
                    point.pointRead.shearStretch = 0.5f * ADBSetting.shearStretchScaleCurve.Evaluate(rate);
                    point.pointRead.bendingShrinkVertical = 0.5f * ADBSetting.bendingShrinkVerticalScaleCurve.Evaluate(rate);
                    point.pointRead.bendingStretchVertical = 0.5f * ADBSetting.bendingStretchVerticalScaleCurve.Evaluate(rate);
                    point.pointRead.bendingShrinkHorizontal = 0.5f * ADBSetting.bendingShrinkHorizontalScaleCurve.Evaluate(rate);
                    point.pointRead.bendingStretchHorizontal = 0.5f * ADBSetting.bendingStretchHorizontalScaleCurve.Evaluate(rate);
                }
                else
                {
                    point.pointRead.resistance = 1f;
                    point.pointRead.mass = ADBSetting.massGlobal;//OYM：重力补间
                    point.pointRead.gravity = ADBSetting.gravity ;//OYM：重力补间
                    point.pointRead.circumferenceShrink = 0.5f * ADBSetting.structuralCircumferenceShrinkScaleGlobal;
                    point.pointRead.structuralShrinkVertical = 0.5f * ADBSetting.structuralShrinkVerticalScaleGlobal;
                    point.pointRead.structuralStretchVertical = 0.5f * ADBSetting.structuralStretchVerticalScaleGlobal;
                    point.pointRead.structuralShrinkHorizontal = 0.5f * ADBSetting.structuralShrinkHorizontalScaleGlobal;
                    point.pointRead.structuralStretchHorizontal = 0.5f * ADBSetting.structuralStretchHorizontalScaleGlobal;
                    point.pointRead.shearShrink = 0.5f * ADBSetting.shearShrinkScaleGlobal;
                    point.pointRead.shearStretch = 0.5f * ADBSetting.shearStretchScaleGlobal;
                    point.pointRead.bendingShrinkVertical = 0.5f * ADBSetting.bendingShrinkVerticalScaleGlobal;
                    point.pointRead.bendingStretchVertical = 0.5f * ADBSetting.bendingStretchVerticalScaleGlobal;
                    point.pointRead.bendingShrinkHorizontal = 0.5f * ADBSetting.bendingShrinkHorizontalScaleGlobal;
                    point.pointRead.bendingStretchHorizontal = 0.5f * ADBSetting.bendingStretchHorizontalScaleGlobal;
                }
                pointReadList[i] = point.pointRead;
                pointReadWriteList[i] = allPointList[i].pointReadWrite;
                pointTransformsList[i] = allPointList[i].trans;
            }

        }

        private void ComputeQuality(List<ADBConstraintRead> constraintsStructuralHorizontal, List<ADBConstraintRead> constraintsStructuralVertical)
        {
            //OYM：Use Area 
            float[] nodeWeight = new float[allNodeList.Count];
            if (ADBSetting.isComputeQuantityByArea)
            {
                Vector3[] HorizontalVector = new Vector3[allNodeList.Count];
                Vector3[] VerticalVector = new Vector3[allNodeList.Count];
                for (int i = 0; i < constraintsStructuralHorizontal.Count; i++)
                {
                    HorizontalVector[constraintsStructuralHorizontal[i].pointA.index] += constraintsStructuralHorizontal[i].direction;
                    HorizontalVector[constraintsStructuralHorizontal[i].pointB.index] += constraintsStructuralHorizontal[i].direction;
                }
                for (int i = 0; i < constraintsStructuralVertical.Count; i++)
                {
                    VerticalVector[constraintsStructuralVertical[i].pointA.index] += constraintsStructuralVertical[i].direction;
                    VerticalVector[constraintsStructuralVertical[i].pointB.index] += constraintsStructuralVertical[i].direction;
                }
                for (int i = 0; i < nodeWeight.Length; i++)
                {
                    nodeWeight[i] = 1000 * 0.5f * Vector3.Cross(HorizontalVector[i], VerticalVector[i]).magnitude;
                }
            }
            else
            //OYM：use Length
            {
                for (int i = 0; i < constraintsStructuralHorizontal.Count; i++)
                {
                    nodeWeight[constraintsStructuralHorizontal[i].pointA.index] +=  constraintsStructuralHorizontal[i].constraintRead.length*0.5f;
                    nodeWeight[constraintsStructuralHorizontal[i].pointB.index] +=  constraintsStructuralHorizontal[i].constraintRead.length * 0.5f;
                }
                if (ADBSetting.isComputeStructuralVertical)
                {
                    for (int i = 0; i < constraintsStructuralVertical.Count; i++)
                    {
                        nodeWeight[constraintsStructuralVertical[i].pointA.index] += constraintsStructuralVertical[i].constraintRead.length * 0.5f;
                        nodeWeight[constraintsStructuralVertical[i].pointB.index] += constraintsStructuralVertical[i].constraintRead.length;
                    }
                }
            }
            ComputeWeight(nodeWeight, allNodeList);
        }

        private void ComputeWeight(float[] nodeWeight, List<ADBRuntimePoint> allNodeList)
        {
            for (int i = allNodeList.Count - 1; i >= 0; i--)
            {
                if (allNodeList[i].isFixed)
                {
                    allNodeList[i].pointRead.weight = 1000000;
                }
                else
                {
                    float weight= nodeWeight[i];
                    if (allNodeList[i].pointRead.childFirstIndex != -1)
                    {
                        for (int j0 = allNodeList[i].pointRead.childFirstIndex; j0 < allNodeList[i].pointRead.childLastIndex; j0++)
                        {
                            weight += nodeWeight[j0];
                        }
                    }
                    if (weight <= 0.001f)
                    {
                        allNodeList[i].pointRead.weight = 0.001f;
                    }
                    else
                    {
                        allNodeList[i].pointRead.weight =  weight;
                    }
                }
            }
        }
        #region constrain
        private void CreationConstraintList()
        {
            var ConstraintReadList = new List<List<ConstraintRead>>();
            int constraintindex = 0;
            CheckAndAddConstraint(ADBSetting.isComputeStructuralVertical, ref constraintindex, constraintsStructuralVertical, ref ConstraintReadList);
            CheckAndAddConstraint(ADBSetting.isComputeStructuralHorizontal,ref  constraintindex, constraintsStructuralHorizontal, ref ConstraintReadList);
            CheckAndAddConstraint(ADBSetting.isComputeShear, ref constraintindex, constraintsShear, ref ConstraintReadList);
            CheckAndAddConstraint(ADBSetting.isComputeBendingVertical, ref constraintindex, constraintsBendingVertical, ref ConstraintReadList);
            CheckAndAddConstraint(ADBSetting.isComputeBendingHorizontal, ref constraintindex, constraintsBendingHorizontal, ref ConstraintReadList);
            CheckAndAddConstraint(ADBSetting.isComputeCircumference, ref constraintindex,constraintsCircumference, ref ConstraintReadList);
            CheckAndAddConstraint(ADBSetting.isComputeVirtual, ref constraintindex, constraintsVirtual, ref ConstraintReadList);
            constraintList = new ConstraintRead[ConstraintReadList.Count][];
            for (int i = 0; i < ConstraintReadList.Count; i++)
            {
                constraintList[i] = ConstraintReadList[i].ToArray();
            }
        }

        private void CheckAndAddConstraint(bool isCompute,ref int constraintIndex, List<ADBConstraintRead> constraintList, ref List<List<ConstraintRead>> ConstraintReadList)
        {
            if (isCompute)
            {
                for (int i = 0; i < constraintList.Count; i++)
                {
                    var isAdd = false;
                    for (int j0 = 0; j0 < ConstraintReadList.Count; j0++)
                    {
                        if (!ConstraintReadList[j0].Contains(constraintList[i].constraintRead))
                        {
                            ConstraintReadList[j0].Add(constraintList[i].constraintRead);
                            isAdd = true;
                            break;
                        }
                    }
                    if (!isAdd)
                    {//if all table contain this 
                        var list = new List<ConstraintRead>();
                        list.Add(constraintList[i].constraintRead);
                        ConstraintReadList.Add(list);
                    }
                }
            }
        }

        private void UpdateJointConnection(List<ADBRuntimePoint> fixedPointList)
        {
            //OYM：这一段....真叫人掉头发

            int HorizontalRootCount = fixedPointList.Count;
            //OYM：这是所有竖着排列的节点之间的杆件
            #region Vertical_Structural
            constraintsStructuralVertical = new List<ADBConstraintRead>();
            {
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    CreateConstraintStructuralVertical(fixedPointList[i], ref constraintsStructuralVertical, ADBSetting.structuralShrinkVertical, ADBSetting.structuralStretchVertical);
                }
            }
            #endregion
            //OYM：所有横着排列的节点之间的杆件
            #region Vertical_Horizontal
            constraintsStructuralHorizontal = new List<ADBConstraintRead>();
            if (ADBSetting.isLoopRootPoints && HorizontalRootCount > 2)//OYM：循环获取？
            {
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    CreationConstraintHorizontal(fixedPointList[(i + 0) % HorizontalRootCount], fixedPointList[(i + 1) % HorizontalRootCount], ref constraintsStructuralHorizontal, ADBSetting.structuralShrinkHorizontal, ADBSetting.structuralStretchHorizontal);
                }
            }
            else
            {
                for (int i = 0; i < HorizontalRootCount - 1; ++i)//OYM：同上，但是不会循环
                {
                    CreationConstraintHorizontal(fixedPointList[i + 0], fixedPointList[i + 1], ref constraintsStructuralHorizontal, ADBSetting.structuralShrinkHorizontal, ADBSetting.structuralStretchHorizontal);
                }
            }
            #endregion
            //OYM：所有对角线上(就是正方形的四个角交叉相连)的杆件
            #region Shear
            constraintsShear = new List<ADBConstraintRead>();
            if (ADBSetting.isLoopRootPoints && HorizontalRootCount > 2)
            {
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    CreationConstraintShear(fixedPointList[(i + 0) % HorizontalRootCount], fixedPointList[(i + 1) % HorizontalRootCount], ref constraintsShear, ADBSetting.shearShrink, ADBSetting.shearStretch);
                }
            }
            else
            {
                for (int i = 0; i < HorizontalRootCount - 1; ++i)
                {
                    CreationConstraintShear(fixedPointList[i + 0], fixedPointList[i + 1], ref constraintsShear, ADBSetting.shearShrink, ADBSetting.shearStretch);
                }
            }
            #endregion
            //OYM：所有竖着排列的跨一个节点的杆件
            #region Bending_Vertical
            constraintsBendingVertical = new List<ADBConstraintRead>();
            for (int i = 0; i < HorizontalRootCount; ++i)
            {
                CreationConstraintBendingVertical(fixedPointList[i], ref constraintsBendingVertical, ADBSetting.bendingShrinkVertical, ADBSetting.bendingStretchVertical);
            }
            #endregion
            //OYM：所有横着排列的跨一个节点的杆件
            #region Bending_Horizontal
            constraintsBendingHorizontal = new List<ADBConstraintRead>();
            CreationConstraintBendingHorizontal(constraintsStructuralHorizontal, ref constraintsBendingHorizontal, ADBSetting.bendingShrinkHorizontal, ADBSetting.bendingStretchHorizontal, ADBSetting.isLoopRootPoints);//OYM：写的话太难了,要三个节点一起循环,改为遍历一下算了

            #endregion
            //OYM：所有从root点出到所有节点的杆件
            #region Circumference
            constraintsCircumference = new List<ADBConstraintRead>();
            for (int i = 0; i < HorizontalRootCount; ++i)
            {
                CreationConstraintCircumference(fixedPointList[i], ref constraintsCircumference, ADBSetting.circumferenceShrink, ADBSetting.circumferenceStretch);//OYM：横向跨一个进行循环搜索
            }

            #endregion
        }

        private static void CreateConstraintStructuralVertical(ADBRuntimePoint Point, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch)
        {
            if (Point == null || Point.childNode == null) return;

            for (int i = 0; i < Point.childNode.Count; ++i)
            {
                var constraint = new ADBConstraintRead(ConstraintType.Structural_Vertical, Point, Point.childNode[i], shrink, stretch);

                ConstraintList.Add(constraint);
                CreateConstraintStructuralVertical(Point.childNode[i], ref ConstraintList, shrink, stretch);
            }
        }

        private static void CreationConstraintHorizontal(ADBRuntimePoint PointA, ADBRuntimePoint PointB, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch)//OYM：我建议你不要去看他,你只要相信他能够正常工作就好了
        {
            if ((PointA == null) || (PointB == null)) return;//OYM：判空
            if (PointA == PointB) return;//OYM：这里是如果只有一条子列的话防止赋值自身

            var childPointAList = PointA.childNode;
            var childPointBList = PointB.childNode;//OYM：获取子节点上的点


            if ((childPointAList != null) && (childPointBList != null))
            {
                if (childPointAList[0].isVirtual || childPointBList[0].isVirtual) return;//OYM：虚点不参与其中

                if (childPointAList.Count >= 2)//OYM：存在多个子节点
                {
                    sortByDistance(childPointBList[0], ref childPointAList, false);
                    sortByDistance(childPointAList[childPointAList.Count - 1], ref childPointBList, true);//OYM：好吧就这么写吧以后谁倒霉谁来改
                    for (int i = 0; i < childPointAList.Count - 1; i++)
                    {
                        ConstraintList.Add(new ADBConstraintRead(ConstraintType.Structural_Horizontal, childPointAList[i], childPointAList[i + 1], shrink, stretch));
                        CreationConstraintHorizontal(childPointAList[i], childPointAList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBConstraintRead(ConstraintType.Structural_Horizontal, childPointAList[childPointAList.Count - 1], childPointBList[0], shrink, stretch));
                CreationConstraintHorizontal(childPointAList[childPointAList.Count - 1], childPointBList[0], ref ConstraintList, shrink, stretch);//OYM：递归
            }
            else if ((childPointAList != null) && (childPointBList == null))//OYM：为了防止互相连接,只允许向序号增大的方向进行连接
            {
                if (childPointAList[0].isVirtual) return;//OYM：虚点不参与其中

                sortByDistance(PointB, ref childPointAList, false);
                if (childPointAList.Count >= 2)//OYM：存在多个子节点
                {
                    for (int i = 0; i < childPointAList.Count - 1; i++)
                    {
                        ConstraintList.Add(new ADBConstraintRead(ConstraintType.Structural_Horizontal, childPointAList[i], childPointAList[i + 1], shrink, stretch));
                        CreationConstraintHorizontal(childPointAList[i], childPointAList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBConstraintRead(ConstraintType.Structural_Horizontal, childPointAList[childPointAList.Count - 1], PointB, shrink, stretch));
                CreationConstraintHorizontal(childPointAList[childPointAList.Count - 1], PointB, ref ConstraintList, shrink, stretch);

            }
        }

        private void CreationConstraintShear(ADBRuntimePoint PointA, ADBRuntimePoint PointB, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch)//OYM：查找交叉节点
        {
            if ((PointA == null) || (PointB == null)) return;
            if (PointA == PointB) return;

            var childPointAList = PointA.childNode;
            var childPointBList = PointB.childNode;

            if (childPointAList != null && childPointBList != null)
            {
                if (childPointAList[0].isVirtual || childPointBList[0].isVirtual) return;//OYM：虚点不参与其中

                sortByDistance(PointB, ref childPointAList, false);
                sortByDistance(PointA, ref childPointBList, true);
                if (childPointAList.Count >= 2)//OYM：存在多个子节点
                {
                    for (int i = 0; i < childPointAList.Count - 1; i++)
                    {
                        CreationConstraintShear(childPointAList[i], childPointAList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                if (childPointBList.Count >= 2)//OYM：存在多个子节点
                {
                    for (int i = 0; i < childPointBList.Count - 1; i++)
                    {
                        CreationConstraintShear(childPointBList[i], childPointBList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBConstraintRead(ConstraintType.Shear, childPointAList[childPointAList.Count - 1], PointB, shrink, stretch));
                ConstraintList.Add(new ADBConstraintRead(ConstraintType.Shear, childPointBList[0], PointA, shrink, stretch));
                CreationConstraintShear(childPointAList[childPointAList.Count - 1], childPointBList[0], ref ConstraintList, shrink, stretch);
            }
            else if ((childPointAList != null ^ childPointBList != null) && !ADBSetting.isComputeStructuralHorizontal)//OYM：如果横向创建了,那么斜对角再创建就没有必要了
            {
                var existPoint = childPointAList == null ? PointA : PointB;
                var existList = childPointAList == null ? childPointBList : childPointAList;

                if (existPoint.isVirtual || existList[0].isVirtual) return;//OYM：虚点不参与其中

                sortByDistance(existPoint, ref existList, false);
                if (existList.Count >= 2)//OYM：存在多个子节点
                {
                    for (int i = 0; i < childPointAList.Count - 1; i++)
                    {
                        CreationConstraintHorizontal(existList[i], existList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBConstraintRead(ConstraintType.Shear, existList[existList.Count - 1], existPoint, shrink, stretch));
                CreationConstraintShear(existList[existList.Count - 1], existPoint, ref ConstraintList, shrink, stretch);
            }
        }

        private static void CreationConstraintBendingVertical(ADBRuntimePoint Point, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch)
        {
            if (Point.childNode == null) return;
            foreach (var child in Point.childNode)
            {
                if (child.childNode == null) continue;
                foreach (var grandSon in child.childNode)
                {
                    ConstraintList.Add(new ADBConstraintRead(ConstraintType.Bending_Vertical, Point, grandSon, shrink, stretch));
                }
                CreationConstraintBendingVertical(child, ref ConstraintList, shrink, stretch);
            }
        }

        private static void CreationConstraintBendingHorizontal(List<ADBConstraintRead> horizontalConstraintList, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch, bool isLoop)
        {
            for (int i = 0; i < horizontalConstraintList.Count; i++)
            {

                ADBConstraintRead ConstraintA = horizontalConstraintList[i];

                int j0= isLoop ? 0 : i;
                for (; j0< horizontalConstraintList.Count; j0++)
                {
                    ADBConstraintRead ConstraintB = horizontalConstraintList[j0];
                    if (ConstraintA.pointB == ConstraintB.pointA)
                    {
                        ConstraintList.Add(new ADBConstraintRead(ConstraintType.Bending_Horizontal, ConstraintA.pointA, ConstraintB.pointB, shrink, stretch));
                    }
                }
            }
        }

        private void CreationConstraintCircumference(ADBRuntimePoint fixedPoint, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch)
        {
            if (fixedPoint == null || fixedPoint.childNode == null) return;

            for (int i = 0; i < fixedPoint.childNode.Count; i++)
            {
                if (fixedPoint.childNode[i].isVirtual) continue;

                ConstraintList.Add(new ADBConstraintRead(ConstraintType.Circumference, fixedPoint, fixedPoint.childNode[i], shrink, stretch));
                CreationConstraintCircumference(fixedPoint, fixedPoint.childNode[i], ref ConstraintList, shrink, stretch);//OYM：递归
            }
        }

        private void CreationConstraintCircumference(ADBRuntimePoint PointA, ADBRuntimePoint PointB, ref List<ADBConstraintRead> ConstraintList, float shrink, float stretch)
        {
            if (PointB == null || PointA == null) return;//OYM：判空

            var childPointB = PointB.childNode;
            if ((childPointB != null))
            {
                for (int i = 0; i < childPointB.Count; i++)
                {
                    var isRepetA = ADBSetting.isComputeStructuralVertical && (childPointB[i].depth == 1);
                    var isRepetB = ADBSetting.isComputeBendingVertical && (childPointB[i].depth == 2);
                    if (!isRepetA && !isRepetB)
                    {
                        ConstraintList.Add(new ADBConstraintRead(ConstraintType.Circumference, PointA, childPointB[i], shrink, stretch));
                    }
                    CreationConstraintCircumference(PointA, childPointB[i], ref ConstraintList, shrink, stretch);//OYM：递归
                }
            }
        }

        private static void sortByDistance(ADBRuntimePoint target, ref List<ADBRuntimePoint> List, bool isInverse)
        {
            //OYM：这里请允许我花点时间啰嗦一下,如果不颠倒,则距离短的在后,如果颠倒的话,则距离短的在前
            if (List.Count < 2 || target == null) return;

            int fore = isInverse ? 1 : -1;
            List.Sort((point1, point2) =>
            {
                return (Vector3.Distance(point1.trans.position, target.trans.position) > Vector3.Distance(point2.trans.position, target.trans.position)) ? -fore : fore;
            });
        }
        #endregion
        private void SerializeAndSearchAllPoints(ADBRuntimePoint point, ref List<ADBRuntimePoint> allPointList, out  int maxPointDepth)//OYM：在这里递归搜索
        {
            if (point == null)
            {
                maxPointDepth = 0;
                return;
            }

            if (point.childNode == null)
            {//OYM：没有子节点
                if (point.depth == 0||ADBSetting.isComputeVirtual)
                {
                    //OYM：如果只有一个节点,而且还是一个fix点
                    addVirtualPointList.Add(point);
                }
                else
                {
                    //OYM：不是的话就当做最后一个节点处理
                    point.pointRead.childFirstIndex = -1;
                    point.pointRead.childLastIndex = -1;
                }
                point.pointDepthRateMaxPointDepth = 1;
                maxPointDepth = point.depth; 
                return;
            }  
            else
            //OYM：有子节点的情况
            {
                point.pointRead.childFirstIndex = allPointList.Count;//OYM：记录第一个子节点的位置
                point.pointRead.childLastIndex = point.pointRead.childFirstIndex + point.childNode.Count;//OYM：记录最后一个子节点的位置

                maxPointDepth = point.depth;
                //OYM：这里容我花点时间啰嗦一下为什么要for两次
                //OYM：这里有一个关键的方法就是allPointList.Add(point.childNode[i]);,一定要在添加完所有子节点才可以向子节点递归
                //OYM：这么做是因为这样子才能让firstindex和lastindex生效,而不是中途插入莫名其妙的更深的节点
                //OYM：同时,在遍历到最底层时
                for (int i = 0; i < point.childNode.Count; i++)
                {
                    var childPoint = point.childNode[i];

                    childPoint.pointRead.parent = point.index;
                    childPoint.pointRead.boneAxis = point.trans.InverseTransformPoint(childPoint.trans.position).normalized;
                    childPoint.pointRead.localRotation = childPoint.trans.localRotation;
                    childPoint.index = allPointList.Count;
                    childPoint.pointRead.fixedIndex = childPoint.isFixed? childPoint.index: point.pointRead.fixedIndex;


                    allPointList.Add(point.childNode[i]);

                }

                for (int i = 0; i < point.childNode.Count; i++)
                {
                    int maxDeep = point.depth;
                    SerializeAndSearchAllPoints(point.childNode[i], ref allPointList, out maxDeep);
                    if (maxDeep > maxPointDepth)
                    {
                        maxPointDepth = maxDeep;
                        point.pointDepthRateMaxPointDepth = point.depth / (float)maxDeep;
                    }
                }
            }
        }

        public void GetData(ref DataPackage dataPackage)
        {
            dataPackage.SetPointAndConstraintpackage(constraintList, pointReadList, pointReadWriteList, pointTransformsList);
        }

        #region Static Generate Func

        public static ADBConstraintReadAndPointControll[] GetJointAndPointControllList(Transform transform, List<string> generateKeyWordWhiteList, List<string> generateKeyWordBlackList, List<Transform> blackListOfGenerateTransform, ADBGlobalSetting settings)//OYM：一个巨啰嗦的方法
        {
            List<ADBRuntimePoint> FixedADBRuntimePoint = searchFixedADBRuntimePoint(transform, generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform, 0);//OYM：获取所有的固定点,,这里的固定时指那些能动的点的父节点,顺便一提,这个方法顺便把所有节点的子节点赋值了
            
            if (FixedADBRuntimePoint == null)
                return null;

            //OYM：在这里对收集好的子节点进行分类与识别
            List<ADBConstraintReadAndPointControll> ADBRuntimeJointAndPointControlls = new List<ADBConstraintReadAndPointControll>();//OYM：
            for (int i = 0; i < FixedADBRuntimePoint.Count; i++)
            {
                Transform parentPoint = FixedADBRuntimePoint[i].trans.parent;
                string keyWord = FixedADBRuntimePoint[i].keyWord;
                bool isContain = false;
                for (int j0 = 0; j0 < ADBRuntimeJointAndPointControlls.Count; j0++)
                {
                    //OYM：如果包含,就把它加到包含它的controll里面去
                    if (ADBRuntimeJointAndPointControlls[j0].rootNode.trans == parentPoint && ADBRuntimeJointAndPointControlls[j0].keyWord == keyWord)
                    {
                        ADBRuntimeJointAndPointControlls[j0].fixedNodeList.Add(FixedADBRuntimePoint[i]);
                        isContain = true;
                    }
                }
                //OYM：不包含就新建立一个controll,然后加进去
                if (!isContain)
                {
                    ADBSetting setting;
                    if (settings == null)
                    {
                        Debug.Log("Setting File is Lost,check the Resources folder");
                        setting = (ADBSetting)ScriptableObject.CreateInstance(typeof(ADBSetting));
                    }
                    else
                    {
                        setting = settings.GetSetting(keyWord);
                    }

                    ADBRuntimeJointAndPointControlls.Add(new ADBConstraintReadAndPointControll(parentPoint, keyWord, setting));
                    ADBRuntimeJointAndPointControlls[ADBRuntimeJointAndPointControlls.Count - 1].fixedNodeList.Add(FixedADBRuntimePoint[i]);//OYM：给
                }
            }
            //OYM：顺便把所有的fixednode添加给rootnode的childNode
            for (int i = 0; i < ADBRuntimeJointAndPointControlls.Count; i++)
            {
                ADBRuntimeJointAndPointControlls[i].rootNode.childNode = ADBRuntimeJointAndPointControlls[i].fixedNodeList;
            }
            return ADBRuntimeJointAndPointControlls.ToArray();
        }
        //OYM：deep search the fixed point ,get they childpoint and add it to their point data 
        private static List<ADBRuntimePoint> searchFixedADBRuntimePoint(Transform transform, List<string> generateKeyWordWhiteList, List<string> generateKeyWordBlackList, List<Transform> blackListOfGenerateTransform, int depth)
        {       //OYM：利用深度搜索,能很快找到所有的固定点,
                //OYM：如果是子节点与父节点匹配,则父节点添加子节点坐标
            //OYM：
            if (transform == null || transform.childCount == 0) return null;
            //OYM：防空
            List<ADBRuntimePoint> ADBRuntimePoint = new List<ADBRuntimePoint>();

            for (int i = 0; i < transform.childCount; i++)//OYM：这里就很有意思了
            {
                var childNodeTarns = transform.GetChild(i);//OYM：遍历每一个子节点
                var childName = childNodeTarns.name.ToLower();//OYM：获取他们的名字
                ADBRuntimePoint point = null;

                //OYM：[判断是否属于黑名单
                bool isblack =false;
                for (int j0= 0; j0< blackListOfGenerateTransform.Count; j0++)
                {
                    if (isblack) break;

                    isblack = childNodeTarns.Equals(blackListOfGenerateTransform[j0]);
                }
                for (int j0= 0; j0< generateKeyWordBlackList.Count; j0++)
                {
                    if (isblack) break;

                    isblack = childName.Contains(generateKeyWordBlackList[j0]);
                }
                if (isblack)
                {
                    continue;
                }

                foreach (var whiteKey in generateKeyWordWhiteList)
                {
                    if (whiteKey == null || whiteKey.Length == 0) continue;
                    if (childName.Contains(whiteKey))
                    {
                        point = new ADBRuntimePoint(childNodeTarns, depth, whiteKey);
                        break;
                    }
                }

                //OYM：get point child
                //OYM：注意,这个递归非常有意思,值得好好看看
                if (point != null)
                {
                    List<ADBRuntimePoint> childPoint = searchFixedADBRuntimePoint(point.trans, new List<string> { point.keyWord }, generateKeyWordBlackList, blackListOfGenerateTransform, depth + 1);
                    //OYM：以单一的关键词进行搜索
                    point.childNode = childPoint;//OYM：注意,这里是设置父节点的子节点,而子节点添加下一层节点搜索到的更深层的子节点
                    ADBRuntimePoint.Add(point);//OYM：result添加这个父节点,返回给上一级
                }
                //OYM：search next
                else
                {
                    List<ADBRuntimePoint> nextNode = searchFixedADBRuntimePoint(childNodeTarns, generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform, depth);//OYM：这个list会包含所有最顶层的父节点
                    //OYM：不搜索匹配的子节点,而搜索子节点的所有子节点中是否有匹配的
                    if (nextNode != null)
                    {
                        ADBRuntimePoint.AddRange(nextNode);
                    }
                }
            }
            ADBRuntimePoint = ADBRuntimePoint.Count == 0 ? null : ADBRuntimePoint;
            return ADBRuntimePoint;
        }
        #endregion
        public void OnDrawGizmos()
        {
            if (!ADBSetting.isDebugDraw) return;

            foreach (var point in allNodeList)
            {
                point.OnDrawGizmos();
            }
            if (ADBSetting.isComputeStructuralVertical)
            {
                DrawConstraint(constraintsStructuralVertical);
            }
            if (ADBSetting.isComputeStructuralHorizontal)
            {
                DrawConstraint(constraintsStructuralHorizontal);
            }
            if (ADBSetting.isComputeShear)
            {
                DrawConstraint(constraintsShear);
            }
            if (ADBSetting.isComputeCircumference)
            {
                DrawConstraint(constraintsCircumference);
            }
            if (ADBSetting.isComputeBendingHorizontal)
            {
                DrawConstraint(constraintsBendingHorizontal);
            }
            if (ADBSetting.isComputeBendingVertical)
            {
                DrawConstraint(constraintsBendingVertical);
            }
        }
        public void DrawConstraint(List<ADBConstraintRead> constraints)
        {
            if (constraints == null) return;
            for (int i = 0; i < constraints.Count; i++)
            {
                constraints[i].OnDrawGizmos();
            }
        }
    }
}


