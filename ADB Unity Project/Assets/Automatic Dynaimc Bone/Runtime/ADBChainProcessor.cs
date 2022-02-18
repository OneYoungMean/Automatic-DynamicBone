using ADBRuntime.Mono;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ADBRuntime.Mono
{

    //OYM：写在开头,这里有很多种类的点,大致可以分成下面几种
    //OYM：rootpoint 最原始的节点,比如说head,只有一个逻辑上的作用
    //OYM：fixedpoint 固定的节点,通常是root的子节点,用于牵引普通节点
    //OYM：各种普通的point,可以自由活动
    //OYM：virtualpoint,不存在的点,只迭代一次,用于计算那种指起到旋转作用的节点
    public class ADBChainProcessor: ADBRuntimePoint, IADBPhysicMonoComponent
    {
        public const string virtualKey = " virtual";
        [SerializeField]
        private ADBPhysicsSetting aDBSetting;
        public Transform RootTransform { get { return transform; } }

        //OYM：pointList
        public List<ADBRuntimePoint> fixedPointList { get { return ChildPoints; } }
        public Transform[] allPointTransforms;
        public List<ADBRuntimePoint> allPointList;
        public bool isUseLocalRadiusAndColliderMask;
        public bool isInitialize;
        //OYM：constraintList
        [SerializeField]
        private List<ADBRuntimeConstraint> constraintsStructuralVertical;//OYM：所有的垂直拉约束
        [SerializeField]
        private List<ADBRuntimeConstraint> constraintsStructuralHorizontal;//OYM：所有的水平拉约束
        [SerializeField]
        private List<ADBRuntimeConstraint> constraintsShear;//OYM：剪切力约束
        [SerializeField]
        private List<ADBRuntimeConstraint> constraintsBendingVertical;//OYM：垂直弯曲约束
        [SerializeField]
        private List<ADBRuntimeConstraint> constraintsBendingHorizontal;//OYM：水平弯曲约束
        [SerializeField]
        private List<ADBRuntimeConstraint> constraintsCircumference;//OYM：圆心约束

        //OYM：struct list
        private ConstraintRead[][] constraintList;
        private PointRead[] pointReadList;
        private PointReadWrite[] pointReadWriteList;
        private int maxPointDepth;
        private float maxChainLength;

        //OYM：new一个出来
        public static ADBChainProcessor CreateADBChainProcessor(Transform rootTransform, string keyWord, ADBPhysicsSetting setting) 
        {
            ADBChainProcessor chainProcessor = rootTransform.gameObject.AddComponent<ADBChainProcessor>();
            //chainProcessor.hideFlags = HideFlags.HideInInspector;
            chainProcessor.keyWord = keyWord;
            chainProcessor.SetDepth(-1);
            chainProcessor.index = -1;
            chainProcessor.allowCreateAllConstraint = false;
            chainProcessor.initialScale = rootTransform.lossyScale;

            chainProcessor.allPointList = new List<ADBRuntimePoint>();
            chainProcessor.maxPointDepth = 1;
            chainProcessor.aDBSetting = setting;

            return chainProcessor;
        }
        public static ADBChainProcessor CreateADBChainProcessor(ADBRuntimePoint rootPoint ,ADBPhysicsSetting setting)
        {
            ADBChainProcessor chainProcessor = rootPoint.gameObject.AddComponent<ADBChainProcessor>();
            //chainProcessor.hideFlags = HideFlags.HideInInspector;
            chainProcessor.keyWord = rootPoint.keyWord;
            chainProcessor.SetDepth(-1);
            chainProcessor.index = -1;
            chainProcessor.allowCreateAllConstraint = false;
            chainProcessor.initialScale = rootPoint.transform.lossyScale;

            chainProcessor.allPointList = new List<ADBRuntimePoint>();
            chainProcessor.maxPointDepth = 1;
            chainProcessor.aDBSetting = setting;

            chainProcessor.AddChild(rootPoint.ChildPoints);
            DestroyImmediate(rootPoint);

            return chainProcessor;
        }
        public  void Clear()
        {
            allPointList.Clear();
            allPointTransforms= null;
        }
        public void Initialize()
        {
            if (isInitialize)
            {
                Clear();
            }

            SerializeAndSearchAllPoints(this, ref allPointList, out maxPointDepth);//OYM：递归搜索子节点,获取所有节点的List,计算所有节点的deepRate
            CreatePointStruct1(allPointList);//OYM：建立各种Point初始数据

            UpdateJointConnection(fixedPointList);//OYM：这里是给所有的节点进行设置，同时获取所有可以搜索到的两点中间的约束并对其进行分类
            CreationConstraintList();//OYM：建立constraint的struct供给jobs使用
            ComputeWeight();//OYM：根据constraint计算质量
            UpdatePointStruct();//OYM：由于weight要用到constraint的内容,所以把生成point结构体放最后
            GetMaxDeep();
            isInitialize = true;
        }

        private void GetMaxDeep()
        {
            maxChainLength = GetMaxDeep(this)* GetADBSetting().structuralStretchVertical * 1.1f; ;
        }

        private void UpdatePointStruct()
        {
            allPointTransforms = new Transform[allPointList.Count];
            for (int i = 0; i < allPointList.Count; i++)
            {
                pointReadList[i] = allPointList[i].pointRead;
                pointReadWriteList[i] = allPointList[i].pointReadWrite;
                allPointTransforms[i] = allPointList[i].transform;

            }
        }
        #region point

        //OYM：cratePointStruct
        private void SerializeAndSearchAllPoints(ADBRuntimePoint point, ref List<ADBRuntimePoint> allPointList, out int maxPointDepth)//OYM：在这里递归搜索
        {
            if (point == null)
            {
                maxPointDepth = 0;
                return;
            }
            if (point.ChildPoints == null || point.ChildPoints.Count == 0)
            {//OYM：没有子节点
                if (Application.isPlaying && aDBSetting.isComputeVirtual && (!point.transform.name.Contains(virtualKey)))//OYM：创建一个延长的节点
                {
                    Transform childPointTrans = new GameObject(point.transform.name + virtualKey).transform;
                    childPointTrans.position = point.transform.position + ((point.Parent != null && point.Parent.depth != -1 && !aDBSetting.ForceLookDown) ?
                        (point.transform.position - point.Parent.transform.position).normalized * aDBSetting.virtualPointAxisLength :
                        Vector3.down * aDBSetting.virtualPointAxisLength);

                    childPointTrans.parent = point.transform;

                    ADBRuntimePoint virtualPoint =  ADBRuntimePoint.CreateRuntimePoint(childPointTrans, point.depth + 1, point.keyWord, aDBSetting.isAllowComputeOtherConstraint);
                    point.AddChild(virtualPoint);
                }
                else
                {
                    //OYM：不是的话就当做最后一个节点处理
                    point.pointRead.childFirstIndex = -1;
                    point.pointRead.childLastIndex = -1;
                    point.pointDepthRateMaxPointDepth = 1;
                    maxPointDepth = point.depth;
                    return;
                }
            }
            point.pointRead.childFirstIndex = allPointList.Count;//OYM：记录第一个子节点的位置
            point.pointRead.childLastIndex = point.pointRead.childFirstIndex + point.ChildPoints.Count;//OYM：记录边界的位置

            maxPointDepth = point.depth;
            //OYM：广度遍历
            if (point.ChildPoints.Count!=0)
            
            {
                for (int i = 0; i < point.ChildPoints.Count; i++)
                {

                    ADBRuntimePoint childPoint = point.ChildPoints[i];

                    childPoint.pointRead.parentIndex = point.index;
                    childPoint.pointRead.fixedIndex = point.pointRead.fixedIndex;
                    childPoint.Parent = point;

                    childPoint.pointRead.initialLocalPosition = Quaternion.Inverse(point.transform.rotation) * (childPoint.transform.position - point.transform.position).normalized;
                    childPoint.pointRead.initialLocalPositionLength = (childPoint.transform.position - point.transform.position).magnitude;
                    childPoint.pointRead.initialLocalRotation = childPoint.transform.localRotation;
                    childPoint.pointRead.initialRotation = childPoint.transform.rotation;
                    childPoint.index = allPointList.Count;

                    if (childPoint.isFixed)
                    {
                        childPoint.pointRead.fixedIndex = childPoint.index;
                        childPoint.pointRead.initialPosition = Vector3.zero;
                    }
                    else
                    {
                        childPoint.pointRead.fixedIndex = point.pointRead.fixedIndex;
                        var fixedPoint = allPointList[childPoint.pointRead.fixedIndex];

                        childPoint.pointRead.initialPosition = Quaternion.Inverse(fixedPoint.transform.rotation) * (childPoint.transform.position - fixedPoint.transform.position);

                    }
                    allPointList.Add(childPoint);

                }

                for (int i = 0; i < point.ChildPoints.Count; i++)
                {
                    int maxDeep = point.depth;
                    SerializeAndSearchAllPoints(point.ChildPoints[i], ref allPointList, out maxDeep);
                    if (maxDeep > maxPointDepth)
                    {
                        maxPointDepth = maxDeep;
                        point.pointDepthRateMaxPointDepth = point.depth / (float)maxDeep;
                    }
                }
            }

        }
        private void CreatePointStruct1(List<ADBRuntimePoint> allPointList)
        {
            pointReadList = new PointRead[allPointList.Count];
            pointReadWriteList = new PointReadWrite[allPointList.Count];
            if (aDBSetting==null)
            {
                return;
            }

            for (int i = 0; i < allPointList.Count; ++i)
            {
                var point = allPointList[i];

                float rate = point.pointDepthRateMaxPointDepth;
                if (!isUseLocalRadiusAndColliderMask)//OYM:PMX引入:自带的mask机制
                {
                    point.pointRead.colliderMask = (int)aDBSetting.colliderChoice;
                    point.pointRead.radius = aDBSetting.ispointRadiuCurve? aDBSetting.pointRadiuCurve.Evaluate(rate): aDBSetting.pointRadiuValue;
                }

                //point.pointRead.isFixGravityAxis = aDBSetting.isFixGravityAxis;
                point.pointRead.isFixedPointFreezeRotation = aDBSetting.isFixedPointFreezeRotation;


                point.pointRead.gravity = aDBSetting.isgravityScaleCurve ? aDBSetting.gravity * aDBSetting.gravityScaleCurve.Evaluate(rate) : aDBSetting.gravity * aDBSetting.gravityScaleValue;
                point.pointRead.stiffnessWorld = aDBSetting.isstiffnessWorldCurve? aDBSetting.stiffnessWorldCurve.Evaluate(rate):aDBSetting.stiffnessWorldValue;
                point.pointRead.stiffnessLocal = aDBSetting.isstiffnessLocalCurve ? aDBSetting.stiffnessLocalCurve.Evaluate(rate) : aDBSetting.stiffnessWorldValue;
                point.pointRead.elasticity = aDBSetting.iselasticityCurve? aDBSetting.elasticityCurve.Evaluate(rate): aDBSetting.elasticityValue;
                point.pointRead.elasticityVelocity = aDBSetting.iselasticityVelocityCurve ? aDBSetting.elasticityVelocityCurve.Evaluate(rate) : aDBSetting.elasticityVelocityValue;
                point.pointRead.lengthLimitForceScale = aDBSetting.islengthLimitForceScaleCurve ? aDBSetting.lengthLimitForceScaleCurve.Evaluate(rate) : aDBSetting.lengthLimitForceScaleValue;
                point.pointRead.damping = aDBSetting.isdampingCurve ? aDBSetting.dampingCurve.Evaluate(rate) : aDBSetting.dampingValue;
                point.pointRead.moveInert = aDBSetting.ismoveInertCurve ? aDBSetting.moveInertCurve.Evaluate(rate) : aDBSetting.moveInertValue;
                point.pointRead.velocityIncrease = aDBSetting.isvelocityIncreaseCurve ? aDBSetting.velocityIncreaseCurve.Evaluate(rate) : aDBSetting.velocityIncreaseValue;
                point.pointRead.friction = aDBSetting.isfrictionCurve? aDBSetting.frictionCurve.Evaluate(rate):aDBSetting.frictionValue;
                point.pointRead.addForceScale = aDBSetting.isaddForceScaleCurve ? aDBSetting.addForceScaleCurve.Evaluate(rate) : aDBSetting.addForceScaleValue;

                point.pointRead.circumferenceShrink = aDBSetting.iscircumferenceShrinkScaleCurve?  aDBSetting.circumferenceShrinkScaleCurve.Evaluate(rate):aDBSetting.circumferenceShrinkScaleValue*0.5f;
                point.pointRead.circumferenceStretch = aDBSetting.iscircumferenceStretchScaleCurve?  aDBSetting.circumferenceStretchScaleCurve.Evaluate(rate): aDBSetting.circumferenceStretchScaleValue*0.5f;
                point.pointRead.structuralShrinkVertical = aDBSetting.isstructuralShrinkVerticalScaleCurve? aDBSetting.structuralShrinkVerticalScaleCurve.Evaluate(rate):aDBSetting.structuralShrinkVerticalScaleValue*0.5f;
                point.pointRead.structuralStretchVertical = aDBSetting.isstructuralStretchVerticalScaleCurve ?  aDBSetting.structuralStretchVerticalScaleCurve.Evaluate(rate):aDBSetting.structuralStretchVerticalScaleValue*0.5f;
                point.pointRead.structuralShrinkHorizontal = aDBSetting.isstructuralStretchVerticalScaleCurve ?  aDBSetting.structuralShrinkHorizontalScaleCurve.Evaluate(rate):aDBSetting.structuralShrinkHorizontalScaleValue*0.5f;
                point.pointRead.structuralStretchHorizontal = aDBSetting.isstructuralStretchHorizontalScaleCurve? aDBSetting.structuralStretchHorizontalScaleCurve.Evaluate(rate): aDBSetting.structuralStretchHorizontalScaleValue*0.5f;
                point.pointRead.shearShrink = aDBSetting.isshearShrinkScaleCurve ?  aDBSetting.shearShrinkScaleCurve.Evaluate(rate) : aDBSetting.shearShrinkScaleValue*0.5f;
                point.pointRead.shearStretch= aDBSetting.isshearStretchScaleCurve ?  aDBSetting.shearStretchScaleCurve.Evaluate(rate) : aDBSetting.shearStretchScaleValue*0.5f;
                point.pointRead.bendingShrinkVertical = aDBSetting.isbendingShrinkVerticalScaleCurve ?  aDBSetting.bendingShrinkVerticalScaleCurve.Evaluate(rate) : aDBSetting.bendingShrinkVerticalScaleValue*0.5f;
                point.pointRead.bendingStretchVertical = aDBSetting.isbendingStretchVerticalScaleCurve ?  aDBSetting.bendingStretchVerticalScaleCurve.Evaluate(rate) : aDBSetting.bendingStretchVerticalScaleValue*0.5f;
                point.pointRead.bendingShrinkHorizontal = aDBSetting.isbendingShrinkHorizontalScaleCurve ?  aDBSetting.bendingShrinkHorizontalScaleCurve.Evaluate(rate) : aDBSetting.bendingShrinkHorizontalScaleValue*0.5f;
                point.pointRead.bendingStretchHorizontal = aDBSetting.isbendingStretchHorizontalScaleCurve ?  aDBSetting.bendingStretchHorizontalScaleCurve.Evaluate(rate) : aDBSetting.bendingStretchHorizontalScaleValue*0.5f;

                //processed
                point.pointRead.stiffnessLocal = 1 - Mathf.Clamp01(Mathf.Cos(point.pointRead.stiffnessLocal * Mathf.PI * 0.5f));//OYM:将角度映射到距离上
                point.pointRead.damping = 0.5f + point.pointRead.damping * 0.5f;//OYM：只取0.5-1这一段
            }
        }

        private void ComputeWeight()
        {
            if (aDBSetting==null)
            {
                return;
            }
            //OYM：Use Area 
            if (aDBSetting.isAutoComputeWeight)
            {
                float[] pointWeight = new float[allPointList.Count];

                float[] HorizontalVector = new float[allPointList.Count];
                float[] VerticalVector = new float[allPointList.Count];
                if (aDBSetting.isComputeStructuralHorizontal)
                {
                    for (int i = 0; i < constraintsStructuralHorizontal.Count; i++)
                    {
                        HorizontalVector[constraintsStructuralHorizontal[i].pointA.index] += constraintsStructuralHorizontal[i].direction.magnitude;
                        HorizontalVector[constraintsStructuralHorizontal[i].pointB.index] += constraintsStructuralHorizontal[i].direction.magnitude;
                    }
                }
                if (aDBSetting.isComputeStructuralVertical)
                {
                    for (int i = 0; i < constraintsStructuralVertical.Count; i++)
                    {
                        VerticalVector[constraintsStructuralVertical[i].pointA.index] += constraintsStructuralVertical[i].direction.magnitude;
                        VerticalVector[constraintsStructuralVertical[i].pointB.index] += constraintsStructuralVertical[i].direction.magnitude;
                    }
                }


                for (int i = 0; i < pointWeight.Length; i++)
                {
                    if (!aDBSetting.isComputeStructuralHorizontal && !aDBSetting.isComputeStructuralVertical)
                    {
                        pointWeight[i] = 1f;
                    }
                    else
                    {
                        pointWeight[i] = (HorizontalVector[i] + VerticalVector[i]) * 0.5f;
                    }

                }
                ComputeWeight(pointWeight, allPointList);
            }
            else
            {
                for (int i = 0; i < allPointList.Count; i++)
                {
                    var point = allPointList[i];
                    if (point.isFixed)
                    {
                        point.pointRead.mass = 1E10f;
                    }
                    else
                    {
                        point.pointRead.mass = aDBSetting.weightCurve.Evaluate(point.pointDepthRateMaxPointDepth);
                        point.pointRead.mass = point.pointRead.mass < 1f ? 1f : point.pointRead.mass;
                    }
                }

            }


        }

        private void ComputeWeight(float[] pointWeight, List<ADBRuntimePoint> allPointList)
        {
            //OYM:normalize
            float weightSum = 0;
            for (int i = allPointList.Count - 1; i >= 0; i--)
            {

                if (allPointList[i].isFixed)
                {
                    allPointList[i].pointRead.mass = 1E10f;
                }
                else
                {
                    float weight = pointWeight[i];

                    if (weight <= 0.001f)
                    {
                        Debug.Log(allPointList[i].transform.name + " weight is too small ");
                        weight = 0.001f;
                    }

                    pointWeight[allPointList[i].pointRead.parentIndex] += weight;
                    allPointList[i].pointRead.mass = pointWeight[i];
                    weightSum += pointWeight[i];
                }
            }
            weightSum /= allPointList.Count;

            for (int i = allPointList.Count - 1; i >= 0; i--)
            {
                allPointList[i].pointRead.mass /= weightSum;//OYM：质量归一化,防止出现普遍过大或者普遍过小
            }
        }

        #endregion
        #region constraint
        private void CreationConstraintList()
        {
            if (aDBSetting == null)
            {
                constraintList = new ConstraintRead[0][];
                return;
            }
            var ConstraintReadList = new List<List<ConstraintRead>>();
            int jobcount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
            for (int i = 0; i < jobcount; i++)
            {
                ConstraintReadList.Add(new List<ConstraintRead>());
            }
            int constraintindex = 0;
            CheckAndAddConstraint(aDBSetting.isComputeStructuralVertical, ref constraintindex, constraintsStructuralVertical, ref ConstraintReadList);
            CheckAndAddConstraint(aDBSetting.isComputeStructuralHorizontal, ref constraintindex, constraintsStructuralHorizontal, ref ConstraintReadList);
            CheckAndAddConstraint(aDBSetting.isComputeShear, ref constraintindex, constraintsShear, ref ConstraintReadList);
            CheckAndAddConstraint(aDBSetting.isComputeBendingVertical, ref constraintindex, constraintsBendingVertical, ref ConstraintReadList);
            CheckAndAddConstraint(aDBSetting.isComputeBendingHorizontal, ref constraintindex, constraintsBendingHorizontal, ref ConstraintReadList);
            CheckAndAddConstraint(aDBSetting.isComputeCircumference, ref constraintindex, constraintsCircumference, ref ConstraintReadList);

            constraintList = new ConstraintRead[ConstraintReadList.Count][];
            for (int i = 0; i < ConstraintReadList.Count; i++)
            {
                constraintList[i] = ConstraintReadList[i].ToArray();
            }
        }

        private void CheckAndAddConstraint(bool isCompute, ref int constraintIndex, List<ADBRuntimeConstraint> constraintList, ref List<List<ConstraintRead>> ConstraintReadList)
        {
            if (!isCompute)
            {
                return;
            }
            int k = 0;
            for (int i = 0; i < constraintList.Count; i++)
            {

                var constraint = i % 2 == 0 ? constraintList[i / 2] : constraintList[constraintList.Count - 1 - i / 2];//OYM:打乱顺序，避免集中化

                var isAdd = false;

                for (int j = 0; j < ConstraintReadList.Count; j++, k++)
                {
                    if (k == ConstraintReadList.Count)
                    {
                        k = 0;
                    }

                    if (!ConstraintReadList[k].Contains(constraint.constraintRead))
                    {
                        ConstraintReadList[k].Add(constraint.constraintRead);
                        isAdd = true;
                        break;
                    }
                }
                if (!isAdd)
                {//if all table contain this 
                    var list = new List<ConstraintRead>();
                    list.Add(constraint.constraintRead);
                    ConstraintReadList.Add(list);
                }
            }

        }

        private void UpdateJointConnection(List<ADBRuntimePoint> fixedPointList)
        {
            //OYM：这一段....真叫人掉头发
            if (aDBSetting == null)
            {
                return;
            }
            int HorizontalRootCount = fixedPointList.Count;
            //OYM：这是所有竖着排列的节点之间的杆件
            #region Structural_Vertical
            constraintsStructuralVertical = new List<ADBRuntimeConstraint>();
            {
                for (int i = 0; i < HorizontalRootCount; ++i)
                {
                    CreateConstraintStructuralVertical(fixedPointList[i], ref constraintsStructuralVertical, aDBSetting.structuralShrinkVertical, aDBSetting.structuralStretchVertical);
                }
            }
            #endregion
            //OYM：所有横着排列的节点之间的杆件
            #region Structural_Horizontal
            constraintsStructuralHorizontal = new List<ADBRuntimeConstraint>();
            for (int i = 0; i < HorizontalRootCount - 1; ++i)//OYM:  循环创建杆件
            {
                CreationConstraintHorizontal(fixedPointList[i + 0], fixedPointList[i + 1], ref constraintsStructuralHorizontal, aDBSetting.structuralShrinkHorizontal, aDBSetting.structuralStretchHorizontal);
            }
            if (aDBSetting.isLoopRootPoints && HorizontalRootCount > 2)
            {
                CreationConstraintHorizontal(fixedPointList[HorizontalRootCount - 1], fixedPointList[0], ref constraintsStructuralHorizontal, aDBSetting.structuralShrinkHorizontal, aDBSetting.structuralStretchHorizontal);
            }

            else//OYM：如果需要断开的话则会将杆件的受力调整为0,这样既不会导致失去杆件而穿模,也不会存在约束.
            {
                CreationConstraintHorizontal(fixedPointList[HorizontalRootCount - 1], fixedPointList[0], ref constraintsStructuralHorizontal, 0, float.MaxValue);
            }
            #endregion
            //OYM：所有对角线上(就是正方形的四个角交叉相连)的杆件
            #region Shear
            constraintsShear = new List<ADBRuntimeConstraint>();

            for (int i = 0; i < HorizontalRootCount - 1; ++i)
            {
                CreationConstraintShear(fixedPointList[i + 0], fixedPointList[i + 1], ref constraintsShear, aDBSetting.shearShrink, aDBSetting.shearStretch);
            }

            if (aDBSetting.isLoopRootPoints && HorizontalRootCount > 2)
            {
                CreationConstraintShear(fixedPointList[HorizontalRootCount - 1], fixedPointList[0], ref constraintsShear, aDBSetting.shearShrink, aDBSetting.shearStretch);
            }
            else
            {
                CreationConstraintShear(fixedPointList[HorizontalRootCount - 1], fixedPointList[0], ref constraintsShear, 0, 0);
            }
            #endregion
            //OYM：所有竖着排列的跨一个节点的杆件
            #region Bending_Vertical
            constraintsBendingVertical = new List<ADBRuntimeConstraint>();
            for (int i = 0; i < HorizontalRootCount; ++i)
            {
                CreationConstraintBendingVertical(fixedPointList[i], ref constraintsBendingVertical, aDBSetting.bendingShrinkVertical, aDBSetting.bendingStretchVertical);
            }
            #endregion
            //OYM：所有横着排列的跨一个节点的杆件
            #region Bending_Horizontal
            constraintsBendingHorizontal = new List<ADBRuntimeConstraint>();
            CreationConstraintBendingHorizontal(constraintsStructuralHorizontal, ref constraintsBendingHorizontal, aDBSetting.bendingShrinkHorizontal, aDBSetting.bendingStretchHorizontal, aDBSetting.isLoopRootPoints);//OYM：写的话太难了,要三个节点一起循环,改为遍历一下算了

            #endregion
            //OYM：所有从root点出到所有节点的杆件
            #region Circumference
            constraintsCircumference = new List<ADBRuntimeConstraint>();
            for (int i = 0; i < HorizontalRootCount; ++i)
            {
                CreationConstraintCircumference(fixedPointList[i], ref constraintsCircumference, aDBSetting.circumferenceShrink, aDBSetting.circumferenceStretch);//OYM：横向跨一个进行循环搜索
            }

            #endregion
        }

        private void CreateConstraintStructuralVertical(ADBRuntimePoint Point, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch)
        {
            if (Point == null || Point.ChildPoints == null) return;

            for (int i = 0; i < Point.ChildPoints.Count; ++i)
            {
                ADBRuntimeConstraint constraint = new ADBRuntimeConstraint(ConstraintType.Structural_Vertical, Point, Point.ChildPoints[i], shrink, stretch, aDBSetting.isCollideStructuralVertical);

                ConstraintList.Add(constraint);
                CreateConstraintStructuralVertical(Point.ChildPoints[i], ref ConstraintList, shrink, stretch);
            }
        }

        private void CreationConstraintHorizontal(ADBRuntimePoint PointA, ADBRuntimePoint PointB, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch)//OYM：我建议你不要去看他,你只要相信他能够正常工作就好了
        {
            if ((PointA == null) || (PointB == null)) return;//OYM：判空
            if (PointA == PointB) return;//OYM：这里是如果只有一条子列的话防止赋值自身

            var childPointAList = PointA.ChildPoints;
            var childPointBList = PointB.ChildPoints;//OYM：获取子节点上的点


            if ((childPointAList != null&& childPointAList.Count!=0) && (childPointBList != null&& childPointBList.Count != 0))
            {
                if (!childPointAList[0].allowCreateAllConstraint || !childPointBList[0].allowCreateAllConstraint) return;//OYM：不允许创建则跳过


                if (childPointAList.Count >= 2)//OYM：存在多个子节点
                {
                    sortByDistance(childPointBList[0], ref childPointAList, false);
                    sortByDistance(childPointAList[childPointAList.Count - 1], ref childPointBList, true);//OYM：好吧就这么写吧以后谁倒霉谁来改
                    for (int i = 0; i < childPointAList.Count - 1; i++)
                    {
                        ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Structural_Horizontal, childPointAList[i], childPointAList[i + 1], shrink, stretch, aDBSetting.isCollideStructuralHorizontal));
                        CreationConstraintHorizontal(childPointAList[i], childPointAList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Structural_Horizontal, childPointAList[childPointAList.Count - 1], childPointBList[0], shrink, stretch, aDBSetting.isCollideStructuralHorizontal));
                CreationConstraintHorizontal(childPointAList[childPointAList.Count - 1], childPointBList[0], ref ConstraintList, shrink, stretch);//OYM：递归
            }
            else if ((childPointAList != null) && (childPointBList == null))//OYM：为了防止互相连接,只允许向序号增大的方向进行连接
            {
                if (!childPointAList[0].allowCreateAllConstraint) return;//OYM：不允许创建则跳过

                sortByDistance(PointB, ref childPointAList, false);
                if (childPointAList.Count >= 2)//OYM：存在多个子节点
                {
                    for (int i = 0; i < childPointAList.Count - 1; i++)
                    {
                        ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Structural_Horizontal, childPointAList[i], childPointAList[i + 1], shrink, stretch, aDBSetting.isCollideStructuralHorizontal));
                        CreationConstraintHorizontal(childPointAList[i], childPointAList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Structural_Horizontal, childPointAList[childPointAList.Count - 1], PointB, shrink, stretch, aDBSetting.isCollideStructuralHorizontal));
                CreationConstraintHorizontal(childPointAList[childPointAList.Count - 1], PointB, ref ConstraintList, shrink, stretch);
            }
        }

        private void CreationConstraintShear(ADBRuntimePoint PointA, ADBRuntimePoint PointB, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch)//OYM：查找交叉节点
        {
            if ((PointA == null) || (PointB == null)) return;
            if (PointA == PointB) return;

            var childPointAList = PointA.ChildPoints;
            var childPointBList = PointB.ChildPoints;

            if ((childPointAList != null&& childPointAList.Count!=0) && (childPointBList != null && childPointBList.Count != 0))
            {
                if (!childPointAList[0].allowCreateAllConstraint || !childPointBList[0].allowCreateAllConstraint) return;//OYM：不允许创建则跳过

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
                ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Shear, childPointAList[childPointAList.Count - 1], PointB, shrink, stretch, aDBSetting.isCollideShear));
                ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Shear, childPointBList[0], PointA, shrink, stretch, aDBSetting.isCollideShear));
                CreationConstraintShear(childPointAList[childPointAList.Count - 1], childPointBList[0], ref ConstraintList, shrink, stretch);
            }
            else if ((childPointAList != null ^ childPointBList != null) && !aDBSetting.isComputeStructuralHorizontal)//OYM：如果横向创建了,那么斜对角再创建就没有必要了
            {
                ADBRuntimePoint existPoint = childPointAList == null ? PointA : PointB;
                List<ADBRuntimePoint> existList = childPointAList == null ? childPointBList : childPointAList;

                if (!existPoint.allowCreateAllConstraint || !existList[0].allowCreateAllConstraint) return;//OYM：不允许创建则跳过

                sortByDistance(existPoint, ref existList, false);
                if (existList.Count >= 2)//OYM：存在多个子节点
                {
                    for (int i = 0; i < existList.Count - 1; i++)
                    {
                        CreationConstraintHorizontal(existList[i], existList[i + 1], ref ConstraintList, shrink, stretch);//OYM：递归
                    }
                }
                ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Shear, existList[existList.Count - 1], existPoint, shrink, stretch, aDBSetting.isCollideShear));
                CreationConstraintShear(existList[existList.Count - 1], existPoint, ref ConstraintList, shrink, stretch);
            }
        }

        private static void CreationConstraintBendingVertical(ADBRuntimePoint Point, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch)
        {
            if (Point.ChildPoints == null) return;
            foreach (var child in Point.ChildPoints)
            {
                if (child.ChildPoints == null) continue;
                foreach (var grandSon in child.ChildPoints)
                {
                    if (grandSon.allowCreateAllConstraint)
                    {
                        ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Bending_Vertical, Point, grandSon, shrink, stretch, false));
                    }
                }
                CreationConstraintBendingVertical(child, ref ConstraintList, shrink, stretch);
            }
        }

        private static void CreationConstraintBendingHorizontal(List<ADBRuntimeConstraint> horizontalConstraintList, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch, bool isLoop)
        {
            for (int i = 0; i < horizontalConstraintList.Count; i++)
            {

                ADBRuntimeConstraint ConstraintA = horizontalConstraintList[i];

                int j0 = isLoop ? 0 : i;
                for (; j0 < horizontalConstraintList.Count; j0++)
                {
                    ADBRuntimeConstraint ConstraintB = horizontalConstraintList[j0];
                    if (!(ConstraintA.constraintRead.shrink == 0 && ConstraintA.constraintRead.stretch == 2 || //OYM：排除A杆件或B杆件是isloopPoint生成的不承受力的杆件
                        ConstraintB.constraintRead.shrink == 0 && ConstraintB.constraintRead.stretch == 2) &&
                        ConstraintA.pointB == ConstraintB.pointA)
                    {
                        ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Bending_Horizontal, ConstraintA.pointA, ConstraintB.pointB, shrink, stretch, false));
                    }
                }
            }
        }

        private void CreationConstraintCircumference(ADBRuntimePoint point, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch, int deep = 0)
        {
            if (point == null || point.ChildPoints == null) return;

            for (int i = 0; i < point.ChildPoints.Count; i++)
            {

                if (!point.ChildPoints[i].allowCreateAllConstraint) continue;//OYM：不允许创建则跳过
                if (!(aDBSetting.isComputeStructuralVertical && deep == 0) && !(aDBSetting.isComputeBendingVertical && deep == 1)) //OYM:  第0层通常都有ConstraintStructuralVertical,第一层如果打开ComputeBendingVertical也会重复,所以排除这两种情况.
                {
                    ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Circumference, point, point.ChildPoints[i], shrink, stretch, false));
                }
                CreationConstraintCircumference(point, point.ChildPoints[i], ref ConstraintList, shrink, stretch);//OYM：递归
            }
        }

        private void CreationConstraintCircumference(ADBRuntimePoint PointA, ADBRuntimePoint PointB, ref List<ADBRuntimeConstraint> ConstraintList, float shrink, float stretch)
        {
            if (PointB == null || PointA == null) return;//OYM：判空

            var childPointB = PointB.ChildPoints;
            if ((childPointB != null))
            {
                for (int i = 0; i < childPointB.Count; i++)
                {
                    var isRepetA = aDBSetting.isComputeStructuralVertical && (childPointB[i].depth == 1);
                    var isRepetB = aDBSetting.isComputeBendingVertical && (childPointB[i].depth == 2);
                    if (!(isRepetA || isRepetB))
                    {
                        ConstraintList.Add(new ADBRuntimeConstraint(ConstraintType.Circumference, PointA, childPointB[i], shrink, stretch, false));
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
                return (Vector3.Distance(point1.transform.position, target.transform.position) > Vector3.Distance(point2.transform.position, target.transform.position)) ? -fore : fore;
            });
        }
        private static float GetMaxDeep(ADBRuntimePoint point)
        {
            if (point == null)
            {
                return 0;
            }
            else if (point.ChildPoints == null || point.ChildPoints.Count == 0)
            {
                return point.pointRead.initialLocalPositionLength;
            }
            else
            {
                float max = 0;
                for (int i = 0; i < point.ChildPoints.Count; i++)
                {
                    max = Mathf.Max(max, GetMaxDeep(point.ChildPoints[i]));
                }
                if (point.isFixed)
                {
                    return max;
                }
                else
                {
                    return point.pointRead.initialLocalPositionLength + max;
                }

            }
        }

        #endregion
        #region public Func
        public void SetData(ADBPhysicsKernel dataPackage)
        {

            dataPackage.SetPointAndConstraintData(constraintList, pointReadList, pointReadWriteList, allPointTransforms);
        }
        public ADBPhysicsSetting GetADBSetting()
        {
            return aDBSetting;
        }
        public void SetADBSetting(ADBPhysicsSetting aDBPhysicsSetting)
        {
            this.aDBSetting = aDBPhysicsSetting;
        }

        public ADBRuntimeConstraint[] GetConstraint(ConstraintType constrianttype)
        {
            switch (constrianttype)
            {
                case ConstraintType.Structural_Vertical:
                    return constraintsStructuralVertical.ToArray();
                case ConstraintType.Structural_Horizontal:
                    return constraintsStructuralHorizontal.ToArray();
                case ConstraintType.Shear:
                    return constraintsShear.ToArray();
                case ConstraintType.Bending_Vertical:
                    return constraintsBendingVertical.ToArray();
                case ConstraintType.Bending_Horizontal:
                    return constraintsBendingHorizontal.ToArray();
                case ConstraintType.Circumference:
                    return constraintsCircumference.ToArray();
                default:
                    Debug.LogError("can not find the constraint");
                    return null;
            }
        }

        public bool CanMerge(ADBRuntimePoint point,ADBPhysicsSetting targetSetting)
        {

            bool b = point.transform.parent == RootTransform;
            b &=  keyWord == point. keyWord;
            b&= aDBSetting.Equals(targetSetting);
            return b;

        }

        public Bounds GetCurrentRangeBounds()
        {
            return new Bounds(RootTransform.position,Vector3.one* maxChainLength * 2);
        }
        #endregion


        #region Gizmo
        public void DrawGizmos(Mono.ColliderCollisionType colliderCollisionType)
        {
          //  if (!aDBSetting.isDebugDraw) return;

            foreach (var point in allPointList)
            {
                point.DrawGizmos();
            }
            if (aDBSetting.isComputeStructuralVertical)
            {
                DrawConstraint(constraintsStructuralVertical, colliderCollisionType);
            }
            if (aDBSetting.isComputeStructuralHorizontal)
            {
                DrawConstraint(constraintsStructuralHorizontal, colliderCollisionType);
            }
            if (aDBSetting.isComputeShear)
            {
                DrawConstraint(constraintsShear, colliderCollisionType);
            }
            if (aDBSetting.isComputeCircumference)
            {
                DrawConstraint(constraintsCircumference, colliderCollisionType);
            }
            if (aDBSetting.isComputeBendingHorizontal)
            {
                DrawConstraint(constraintsBendingHorizontal, colliderCollisionType);
            }
            if (aDBSetting.isComputeBendingVertical)
            {
                DrawConstraint(constraintsBendingVertical, colliderCollisionType);
            }
        }


        private void DrawConstraint(List<ADBRuntimeConstraint> constraints, Mono.ColliderCollisionType colliderCollisionType)
        {
            if (constraints == null) return;
            for (int i = 0; i < constraints.Count; i++)
            {
                constraints[i].OnDrawGizmos(colliderCollisionType == Mono.ColliderCollisionType.Constraint || colliderCollisionType == Mono.ColliderCollisionType.Both);
            }
        }


        #endregion
    }
}


