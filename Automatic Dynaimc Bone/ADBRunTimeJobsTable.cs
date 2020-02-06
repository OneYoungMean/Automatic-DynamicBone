#define DEBUG 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Jobs.LowLevel;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System;



namespace ADBRuntime.Internal
{

    public unsafe class ADBRunTimeJobsTable : MonoBehaviour
    {
        #region Single
        private static ADBRunTimeJobsTable instance_ADBRunTimeJobsTable;//OYM：单例模式
        private ADBRunTimeJobsTable() { }
        public static ADBRunTimeJobsTable GetRunTimeJobsTable()
        {
            if (instance_ADBRunTimeJobsTable == null)
            {
                instance_ADBRunTimeJobsTable = new GameObject("ADBRunTimeJobsTable").AddComponent<ADBRunTimeJobsTable>();
                DontDestroyOnLoad(instance_ADBRunTimeJobsTable.gameObject);
                instance_ADBRunTimeJobsTable.needRestart = true;

            }
            return instance_ADBRunTimeJobsTable;
        }

        public void AddGlobalCollider(ADBRuntimeCollider ADBRuntimeCollider)
        {
            allGlobalColliderReadTable.Add(ADBRuntimeCollider.colliderRead);
            allGlobalColliderReadWriteTable.Add(ADBRuntimeCollider.colliderReadWrite);
            allGlobalColliderTransformTable.Add(ADBRuntimeCollider.appendTransform);
        }

        #endregion
        internal JobHandle returnHJob;
        // private int complexHJobBatchCount=8;
        //先预留在这里看下性能
        internal NativeList<ColliderRead> allGlobalColliderReadTable;
        internal NativeList<ColliderReadWrite> allGlobalColliderReadWriteTable;
        internal TransformAccessArray allGlobalColliderTransformTable;
        private ColliderUpdate colliderUpdate;
        private const float EPSILON = 0.001f;
        public bool needRestart;
        public int computeCount { get; private set; }

        private void OnDestroy()
        {
            allGlobalColliderReadTable.Dispose();
            allGlobalColliderReadWriteTable.Dispose();
            allGlobalColliderTransformTable.Dispose();
        }
        #region Jobs
        [BurstCompile]
        public struct InitiralizeCollider : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            public void Execute(int index, TransformAccess transform)
            {
                /*
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }
            public void Execute(int index, Transform transform)
            {
                */
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                pReadWriteCollider->position = pReadWriteCollider->positionForward = transform.position + transform.rotation * pReadCollider->positionOffset;
                pReadWriteCollider->direction = pReadWriteCollider->directionForward = transform.rotation * pReadCollider->staticDirection;
                pReadWriteCollider->normal = pReadWriteCollider->normalForward = transform.rotation * pReadCollider->staticNormal;
            }
        }
        [BurstCompile]
        public struct InitiralizePoint : IJobParallelForTransform

        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;

            public void Execute(int index, TransformAccess transform)
            {
                /*
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }
            void Execute(int index, Transform transform)
            {
            */
                var pReadWritePoint = pReadWritePoints + index;
                var pReadPoint = pReadPoints + index;


                if ((pReadPoints + index)->isVirtual)//OYM：virtual point
                {
                    (pReadWritePoints + index)->position = transform.position + Vector3.down * 0.1f;
                }
                else if (pReadPoint->fixedIndex == index)
                {
                    (pReadWritePoints + index)->position = transform.position;
                }
                else
                {
                    var pFixReadWritePoint = pReadWritePoints + (pReadPoint->fixedIndex);
                    pReadWritePoint->position = pFixReadWritePoint->position + pReadPoint->initialPosition;
                }
            }
        }
        [BurstCompile]
        public struct PointGetTransform : IJobParallelForTransform
        //OYM：把job的点转换成实际的点
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;

            public void Execute(int index, TransformAccess transform)
            {
                /*
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }

            public void Execute(int index, Transform transform)
            {
            */
                (pReadWritePoints + index)->velocity *= (pReadPoints + index)->mass;

                if ((pReadPoints + index)->fixedIndex == index)//OYM：fixedpoint
                {
                    (pReadWritePoints + index)->velocity = transform.position - (pReadWritePoints + index)->position;
                    (pReadWritePoints + index)->position = transform.position;
                }
            }
        }
        [BurstCompile]
        public struct ColliderGetTransform : IJobParallelForTransform
        //OYM：把job的点转换成实际的点
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            public void Execute(int index, TransformAccess transform)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        pReadWriteCollider->positionForward = transform.position + transform.rotation * pReadCollider->positionOffset;
                        break;
                    case ColliderType.Capsule:
                        pReadWriteCollider->positionForward = transform.position + transform.rotation * pReadCollider->positionOffset;
                        pReadWriteCollider->directionForward = transform.rotation * pReadCollider->staticDirection;
                        break;
                    case ColliderType.OBB:
                        pReadWriteCollider->positionForward = transform.position + transform.rotation * pReadCollider->positionOffset;
                        pReadWriteCollider->normalForward = transform.rotation * pReadCollider->staticNormal;
                        break;
                    default:
                        break;
                }
            }
        }
        [BurstCompile]
        public struct ColliderUpdate : IJobParallelFor
        //OYM：把job的点转换成实际的点
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            [ReadOnly]
            internal float step;

            public void Execute(int index)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        pReadWriteCollider->position = Vector3.Lerp(pReadWriteCollider->position, pReadWriteCollider->positionForward, step);
                        break;
                    case ColliderType.Capsule:
                        pReadWriteCollider->position = Vector3.Lerp(pReadWriteCollider->position, pReadWriteCollider->positionForward, step);
                        pReadWriteCollider->direction = Vector3.Lerp(pReadWriteCollider->direction, pReadWriteCollider->directionForward, step);
                        break;
                    case ColliderType.OBB:
                        pReadWriteCollider->position = Vector3.Lerp(pReadWriteCollider->position, pReadWriteCollider->positionForward, step);
                        pReadWriteCollider->normal = Quaternion.Lerp(pReadWriteCollider->normal, pReadWriteCollider->normalForward, step);
                        break;
                    default:
                        break;
                }
            }
        }
        [BurstCompile]
        public struct PointUpdate : IJobParallelFor
        {

            [NativeDisableUnsafePtrRestriction]
            internal PointReadWrite* pReadWritePoints;
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal PointRead* pReadPoints;
            [ReadOnly]
            internal Vector3 windForcePower;
            [ReadOnly]
            internal float scale;
            [ReadOnly]
            internal float deltaTime;
            [ReadOnly]
            internal float iteration;
            public void TryExecute(int index, int _, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
            }
            //OYM：
            public void Execute(int index)
            {
                //OYM：找到固定的节点,如果移动速度过快,就修正距离
                //OYM：默认移动距离超过1就采用修正,防止裙子螺旋升天

                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;
                if (pReadPoint->fixedIndex != index)
                {
                    PointReadWrite* pFixedPointReadWrite = (pReadWritePoints + pReadPoint->fixedIndex);
                    /*
                    if (pFixedPointReadWrite->velocity.sqrMagnitude > 0.01f)
                    {
                       // pReadWritePoint->position += pFixedPointReadWrite->velocity*(1 - 0.1f/pFixedPointReadWrite->velocity.magnitude);//OYM：问题是                                                                                                                                    //OYM：有没有更快的方法?
                    }
                    */
                        pReadWritePoint->velocity += pReadPoint->gravity * scale * (0.5f * deltaTime * deltaTime) /iteration;//OYM：重力(要计算iteration次所以除以个iteration)
                    pReadWritePoint->position += pReadWritePoint->velocity/iteration ;
                }
            }
        }
        [BurstCompile]
        public struct JobConstraintUpdate : IJobParallelFor
        {
            /// <summary>
            /// 指向所有可读的点
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            /// <summary>
            /// 指向所有可读写的点
            /// </summary>);
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            /// <summary>
            /// 所有可读的碰撞体
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pGlobalReadColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pGlobalReadWriteColliders;

            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ConstraintRead* pConstraintsRead;
            [ReadOnly]
            /// <summary>
            /// 碰撞体序号
            /// </summary>);
            public int colliderCount;
            [ReadOnly]
            public float scale;
            [ReadOnly]
            public float iteration;
            [ReadOnly]
            public int globalColliderCount;
            [ReadOnly]
            public bool isCollision;

            public void TryExecute(int index, int _, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
            }
            public void Execute(int index)
            {

                // public void Executea(int index)

                //OYM：获取约束
                ConstraintRead* constraint = pConstraintsRead + index;

                //OYM：获取约束的节点AB
                PointRead* pPointReadA = pReadPoints + constraint->indexA;
                PointRead* pPointReadB = pReadPoints + constraint->indexB;

                //OYM：任意一点都不能小于极小值
                //OYM：if ((WeightA <= EPSILON) && (WeightB <= EPSILON))
                //OYM：获取可读写的点A
                PointReadWrite* pReadWritePointA = pReadWritePoints + constraint->indexA;

                //OYM：获取可读写的点B
                PointReadWrite* pReadWritePointB = pReadWritePoints + constraint->indexB;
                //OYM：获取约束的朝向
                var Direction = pReadWritePointB->position - pReadWritePointA->position;

      
                float Distance = Direction.magnitude;
                //OYM：力度等于距离减去长度除以弹性，这个值可以不存在，可以大于1但是没有什么卵用
                float Force = (Distance - constraint->length * scale);
                //OYM：是否收缩，意味着力大于0
                bool IsShrink = Force >= 0.0f;
                float ConstraintPower;//OYM：这个值等于
                switch (constraint->type)
                //OYM：这下面都是一个意思，就是确认约束受到的力，然后根据这个获取杆件约束的属性，计算 ConstraintPower
                //OYM：Shrink为杆件全局值，另外两个值为线性插值获取的值，同理Stretch，所以这里大概可以猜中只是一个简单的不大于1的值
                {
                    case ConstraintType.Structural_Vertical:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->structuralShrinkVertical + pPointReadB->structuralShrinkVertical)
                            : constraint->stretch * (pPointReadA->structuralStretchVertical + pPointReadB->structuralStretchVertical);
                        break;
                    case ConstraintType.Structural_Horizontal:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->structuralShrinkHorizontal + pPointReadB->structuralShrinkHorizontal)
                            : constraint->stretch * (pPointReadA->structuralStretchHorizontal + pPointReadB->structuralStretchHorizontal);
                        break;
                    case ConstraintType.Shear:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->shearShrink + pPointReadB->shearShrink)
                            : constraint->stretch * (pPointReadA->shearStretch + pPointReadB->shearStretch);
                        break;
                    case ConstraintType.Bending_Vertical:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->bendingShrinkVertical + pPointReadB->bendingShrinkVertical)
                            : constraint->stretch * (pPointReadA->bendingStretchVertical + pPointReadB->bendingStretchVertical);
                        break;
                    case ConstraintType.Bending_Horizontal:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->bendingShrinkHorizontal + pPointReadB->bendingShrinkHorizontal)
                            : constraint->stretch * (pPointReadA->bendingStretchHorizontal + pPointReadB->bendingStretchHorizontal);
                        break;
                    case ConstraintType.Circumference:
                        ConstraintPower = IsShrink
                            ? constraint->shrink * (pPointReadA->circumferenceShrink + pPointReadB->circumferenceShrink)
                            : constraint->stretch * (pPointReadA->circumferenceStretch + pPointReadB->circumferenceStretch);
                        break;
                    case ConstraintType.Virtual:
                        ConstraintPower = 1;
                        break;
                    default:
                        ConstraintPower = 0.0f;
                        break;
                }


                //OYM：获取AB点重量比值

                float WeightProportion = pPointReadB->weight / (pPointReadA->weight + pPointReadB->weight);

                if (ConstraintPower > 0.0f)//OYM：这里不可能小于0吧（除非有人搞破坏）
                {
                    Vector3 Displacement = Direction.normalized * (Force * ConstraintPower);

                    pReadWritePointA->position += Displacement * WeightProportion;
                    pReadWritePointA->velocity += Displacement * WeightProportion;
                    pReadWritePointB->position -= Displacement * (1 - WeightProportion);
                    pReadWritePointB->velocity -= Displacement * (1 - WeightProportion);

                }

                if (isCollision && constraint->isCollider)
                {
                    for (int i = 0; i < colliderCount; ++i)
                    {
                        ColliderRead* pReadCollider = pReadColliders + i;//OYM：终于到碰撞这里了
                        ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;//OYM：啊啊啊我好激动
                        if (pReadCollider->isOpen)
                        {
                            ComputeCollider(pReadCollider, pReadWriteCollider, pReadWritePointA, pReadWritePointB, WeightProportion);
                        }

                    }
                }
            }

            private void ComputeCollider(ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, float WeightProportion)
            {

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        {
                            Vector3 pointOnLine = ConstrainToSegment(pReadWriteCollider->position, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out float t);
                            DistributionPower(pointOnLine - pReadWriteCollider->position, pReadCollider->radius, pReadWritePointA, pReadWritePointB, WeightProportion, t);
                        }

                        break;
                    case ColliderType.Capsule:
                        {
                            SqrComputeNearestPoints(pReadWriteCollider->position, pReadWriteCollider->direction, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out _, out float t, out Vector3 pointOnCollider, out Vector3 pointOnLine);
                            DistributionPower(pointOnLine - pointOnCollider, pReadCollider->radius, pReadWritePointA, pReadWritePointB, WeightProportion, t);
                        }

                        break;
                    case ColliderType.OBB:
                        {
                            
                            SegmentToOBB(pReadWritePointA->position, pReadWritePointB->position, pReadWriteCollider->position, - pReadCollider->boxSize,  pReadCollider->boxSize, Quaternion.Inverse(pReadWriteCollider->normal), out float t1, out float t2);
                            Vector3 dir = pReadWritePointB->position - pReadWritePointA->position;
                            t1 = Clamp01(t1);
                            t2 = Clamp01(t2);
                            Vector3 pointOnLineA = pReadWritePointA->position + dir * t1;
                            Vector3 pointOnLineB = pReadWritePointA->position + dir * t2;
                            bool bHit = t1 >= 0f && t2 > t1 && t2 <= 1.0f;
                            if (bHit)
                            {
                                float t = (t1 + t2) * 0.5f;
                                Vector3 nearestPoint = pReadWritePointA->position + dir * t;
                                Vector3 pushout = Quaternion.Inverse(pReadWriteCollider->normal)*( nearestPoint - pReadWriteCollider->position);
                                float pushoutX = pushout.x > 0 ? pReadCollider->boxSize.x - pushout.x : -pReadCollider->boxSize.x - pushout.x;
                                float pushoutY = pushout.y > 0 ? pReadCollider->boxSize.y - pushout.y :- pReadCollider->boxSize.y - pushout.y;
                                float pushoutZ = pushout.z > 0 ? pReadCollider->boxSize.z - pushout.z : -pReadCollider->boxSize.z - pushout.z;

                                if (Abs(pushoutZ) < Abs(pushoutY) && Abs(pushoutZ) < Abs(pushoutX))
                                {
                                    pushout = pReadWriteCollider->normal*new Vector3(0, 0, pushoutZ);

                                }
                                else if (Abs(pushoutY) < Abs(pushoutX) && Abs(pushoutY) < Abs(pushoutZ))
                                {
                                    pushout = pReadWriteCollider->normal* new Vector3(0, pushoutY, 0);
                                }
                                else
                                {
                                    pushout = pReadWriteCollider->normal* new Vector3(pushoutX, 0, 0);
                                }
                                if (pushout.sqrMagnitude != 0)
                                {

                                        float inverse1Velocity = Vector3.Dot(pushout, pReadWritePointA->velocity) / pushout.sqrMagnitude;
                                        pReadWritePointA->velocity -= pushout * inverse1Velocity;
                                        pReadWritePointB->velocity -= pushout * inverse1Velocity;

                                        float Propotion = WeightProportion * t / (1 - WeightProportion - t + 2 * WeightProportion * t);
                                        if (WeightProportion > 0)
                                        {
                                            pReadWritePointA->position += (pushout* Propotion);
                                            pReadWritePointA->velocity += (pushout * Propotion);
                                        }
                                        else
                                        {
                                            Propotion = 1;
                                        }
                                        pReadWritePointB->position += (pushout*(1- Propotion) );
                                        pReadWritePointB->velocity += (pushout *(1- Propotion));
                                    
                                }
                            }
                            break;
                        }
                    default:
                        return;

                }
            }

            void DistributionPower(Vector3 pushout, float radius, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, float WeightProportion, float lengthPropotion)
            {
                float sqrPushout = pushout.sqrMagnitude;
                if (sqrPushout < radius * radius && sqrPushout != 0)
                {
                    //OYM：把pushout方向多余的力给减掉
                    float inverse1Velocity = Vector3.Dot(pushout, pReadWritePointA->velocity) / sqrPushout;
                    pReadWritePointA->velocity -= pushout * inverse1Velocity;
                    pReadWritePointB->velocity -= pushout * inverse1Velocity;

                    pushout = pushout * (radius / Mathf.Sqrt(sqrPushout) - 1);
                    float Propotion = WeightProportion * lengthPropotion / (1 - WeightProportion - lengthPropotion + 2 * WeightProportion * lengthPropotion);
                    if (WeightProportion > 0)
                    {
                        pReadWritePointA->position += (pushout * (1 - Propotion));
                        pReadWritePointA->velocity += (pushout * (1 - Propotion)) * 0.5f;
                    }
                    else
                    {
                        Propotion = 1;
                    }

                    pReadWritePointB->position += (pushout * Propotion);
                    pReadWritePointB->velocity += (pushout * Propotion) * 0.5f;

                }
            }
            //OYM：https://zalo.github.io/blog/closest-point-between-segments/#line-segments
            //OYM：目前是我见过最快的方法
            float SqrComputeNearestPoints(
                Vector3 posP,//OYM：碰撞体的位置起点位置
                Vector3 dirP,//OYM：碰撞体的朝向
                Vector3 posQ,//OYM：约束的起点坐标
                Vector3 dirQ,//OYM：约束的起点朝向
out float tP, out float tQ, out Vector3 pointOnP, out Vector3 pointOnQ)
            {
                float lineDirSqrMag = dirQ.sqrMagnitude;
                Vector3 inPlaneA = posP - ((Vector3.Dot(posP - posQ, dirQ) / lineDirSqrMag) * dirQ);
                Vector3 inPlaneB = posP + dirP - ((Vector3.Dot(posP + dirP - posQ, dirQ) / lineDirSqrMag) * dirQ);
                Vector3 inPlaneBA = inPlaneB - inPlaneA;

                float t1 = Vector3.Dot(posQ - inPlaneA, inPlaneBA) / inPlaneBA.sqrMagnitude;
                t1 = (inPlaneA != inPlaneB) ? t1 : 0f; // Zero's t if parallel
                Vector3 L1ToL2Line = posP + dirP * Clamp01(t1);

                pointOnQ = ConstrainToSegment(L1ToL2Line, posQ, dirQ, out tQ);
                pointOnP = ConstrainToSegment(pointOnQ, posP, dirP, out tP);
                return (pointOnP - pointOnQ).sqrMagnitude;
            }

            Vector3 ConstrainToSegment(Vector3 tag, Vector3 pos, Vector3 dir, out float t)
            {
                t = Vector3.Dot(tag - pos, dir) / dir.sqrMagnitude;
                t = Clamp01(t);
                return pos + dir * t;
            }
            void SegmentToOBB(Vector3 start, Vector3 end, Vector3 center, Vector3 min, Vector3 max, Quaternion InverseNormal, out float t1, out float t2)
            {
                Vector3 startP = InverseNormal * (center - start);
                Vector3 endP = InverseNormal * (center - end);
                SegmentToAABB(startP, endP, center, min, max, out t1, out t2);
            }

            void SegmentToAABB(Vector3 start, Vector3 end, Vector3 center, Vector3 min, Vector3 max, out float t1, out float t2)
            {
                Vector3 dir = end - start;
                t1 = Max(Min((min.x - start.x) / dir.x, (max.x - start.x) / dir.x), Min((min.y - start.y) / dir.y, (max.y - start.y) / dir.y), Min((min.z - start.z) / dir.z, (max.z - start.z) / dir.z));
                t2 = Min(Max((min.x - start.x) / dir.x, (max.x - start.x) / dir.x), Max((min.y - start.y) / dir.y, (max.y - start.y) / dir.y), Max((min.z - start.z) / dir.z, (max.z - start.z) / dir.z));
            }
            float Abs(float A)
            {
                return A > 0 ? A : -A;
            }
            float Clamp01(float A)
            {
                return A > 0 ? (A < 1 ? A : 1) : 0;
            }
            float Min(float A, float B,float C)
            {
                return A<B?(A<C?A:C):(B<C?B:C);
            }
            float Min(float A, float B)
            {
                return A > B ? B : A;
            }
            float Max(float A, float B, float C)
            {
                return A > B ? (A > C ? A : C) : (B > C ? B : C);
            }
            float Max(float A, float B)
            {
                return A > B ? A : B;
            }
        }
        [BurstCompile]
        public struct JobCollisionPoint : IJobParallelFor//OYM：点碰撞
        {
            /// <summary>
            /// 所有可读写的点的指针
            /// </summary>);
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            /// <summary>
            /// 所有碰撞体的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            /// <summary>
            /// 所有碰撞体坐标的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [ReadOnly]
            public int colliderCount;
            [ReadOnly]
            internal bool isCollider;
            [ReadOnly]
            internal float step;
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal ColliderRead* pGlobalReadColliders;
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal ColliderReadWrite* pGlobalReadWriteColliders;
            internal int globalColliderCount;
            public void TryExecute(int index, int _, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < index; i++)
                {
                    Execute(i);
                }
            }
            public void Execute(int index)
            {
                if (!isCollider) return;

                var pReadWritePoint = pReadWritePoints + index;

                //OYM：获取可写点
                for (int i = 0; i < colliderCount; ++i)
                {
                    ColliderRead* pReadCollider = pReadColliders + i;
                    ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;

                    Vector3 pushout;
                    float sqrPushout;
                    switch (pReadCollider->colliderType)
                    {
                        case ColliderType.Sphere:
                            pushout = pReadWritePoint->position- pReadWriteCollider->position ;
                            sqrPushout = pushout.sqrMagnitude;
                            if (sqrPushout < pReadCollider->radius * pReadCollider->radius)
                            {
                                pushout = pushout * (pReadCollider->radius / Mathf.Sqrt(sqrPushout) - 1);
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->velocity += pushout ;
                            }
                            break;
                        case ColliderType.Capsule:
                            pushout = pReadWritePoint->position - ConstrainToSegment(pReadWritePoint->position, pReadWriteCollider->position, pReadWriteCollider->direction, out _);
                            sqrPushout = pushout.sqrMagnitude;
                            if (sqrPushout < pReadCollider->radius * pReadCollider->radius)
                            {
                                pReadWritePoint->position += pushout * (pReadCollider->radius / pushout.magnitude - 1);
                                pReadWritePoint->velocity = pushout * Vector3.Dot(pushout, pReadWritePoint->velocity) / sqrPushout;
                            }
                            break;
                        case ColliderType.OBB:
                            pushout = Quaternion.Inverse(pReadWriteCollider->normal) * (pReadWritePoint->position - pReadWriteCollider->position);
                            if (-pReadCollider->boxSize.x< pushout.x&&pushout.x< pReadCollider->boxSize.x&&
                                -pReadCollider->boxSize.y < pushout.y && pushout.y < pReadCollider->boxSize.y&&
                                -pReadCollider->boxSize.z < pushout.z && pushout.z < pReadCollider->boxSize.z
                                )
                            {
                                float pushoutX = pushout.x > 0 ? pReadCollider->boxSize.x - pushout.x : -pReadCollider->boxSize.x - pushout.x;
                                float pushoutY = pushout.y > 0 ? pReadCollider->boxSize.y - pushout.y : -pReadCollider->boxSize.y - pushout.y;
                                float pushoutZ = pushout.z > 0 ? pReadCollider->boxSize.z - pushout.z : -pReadCollider->boxSize.z - pushout.z;

                                if (Abs(pushoutZ) < Abs(pushoutY) && Abs(pushoutZ) < Abs(pushoutX))
                                {
                                    pushout = pReadWriteCollider->normal * new Vector3(0, 0, pushoutZ);

                                }
                                else if (Abs(pushoutY) < Abs(pushoutX) && Abs(pushoutY) < Abs(pushoutZ))
                                {
                                    pushout = pReadWriteCollider->normal * new Vector3(0, pushoutY, 0);
                                }
                                else
                                {
                                    pushout = pReadWriteCollider->normal * new Vector3(pushoutX, 0, 0);
                                }
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->velocity += pushout ;
                            }
                            break;
                        default:
                            return;
                    }
                }
            }
            Vector3 ConstrainToSegment(Vector3 tag, Vector3 pos, Vector3 dir, out float t)
            {
                t = Vector3.Dot(tag - pos, dir) / dir.sqrMagnitude;
                return pos + dir * Clamp01(t);
            }
            void SegmentToOBB(Vector3 start, Vector3 end, Vector3 center, Vector3 min, Vector3 max, Quaternion InverseNormal, out float t1, out float t2)
            {
                Vector3 startP = InverseNormal * (center - start);
                Vector3 endP = InverseNormal * (center - end);
                SegmentToAABB(startP, endP, center, min, max, out t1, out t2);
            }

            void SegmentToAABB(Vector3 start, Vector3 end, Vector3 center, Vector3 min, Vector3 max, out float t1, out float t2)
            {
                Vector3 dir = end - start;
                t1 = Max(Min((min.x - start.x) / dir.x, (max.x - start.x) / dir.x), Min((min.y - start.y) / dir.y, (max.y - start.y) / dir.y), Min((min.z - start.z) / dir.z, (max.z - start.z) / dir.z));
                t2 = Min(Max((min.x - start.x) / dir.x, (max.x - start.x) / dir.x), Max((min.y - start.y) / dir.y, (max.y - start.y) / dir.y), Max((min.z - start.z) / dir.z, (max.z - start.z) / dir.z));
            }
            float Abs(float A)
            {
                return A > 0 ? A : -A;
            }
            float Clamp01(float A)
            {
                return A > 0 ? (A < 1 ? A : 1) : 0;
            }
            float Min(float A, float B, float C)
            {
                return Min(Min(A, B), Min(B, C));
            }
            float Min(float A, float B)
            {
                return A > B ? B : A;
            }
            float Max(float A, float B, float C)
            {
                return Max(Max(A, B), Max(B, C));
            }
            float Max(float A, float B)
            {
                return A > B ? A : B;
            }
        }
        [BurstCompile]
        public struct JobPointToTransform2 : IJobParallelForTransform
        //OYM：把job的点转换成实际的点
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly]
            public float deltaTime;

            public void Execute(int index, TransformAccess transform)
            {
                /*
            }
            public void TryExecute(TransformAccessArray transforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < transforms.length; i++)
                {
                    Execute(i, transforms[i]);
                }
            }
            public void Execute(int index, Transform transform)
            {
                */
                PointReadWrite* pReadWrite = pReadWritePoints + index;//OYM：获取每个读写点
                PointRead* pRead = pReadPoints + index;//OYM：获取每个只读点

                if (!(pRead->fixedIndex == index || pRead->isVirtual))//OYM：不是fix点
                {
                    transform.position = pReadWrite->position;
                }

                if (pRead->childFirstIndex > -1)
                {
                    SetRotation(index, transform, pReadWrite, pRead);//OYM：设置旋转
                }
            }
            void SetRotation(int index, TransformAccess transform, PointReadWrite* pReadWrite, PointRead* pRead)
            {
                transform.localRotation = pRead->localRotation;
                var child = pReadWritePoints + pRead->childFirstIndex;
                var childRead = pReadPoints + pRead->childFirstIndex;

                var Direction = child->position - pReadWrite->position;//OYM：朝向等于面向子节点的方向
                if (Direction.sqrMagnitude > EPSILON * EPSILON)//OYM：两点不再一起
                {
                    Vector3 AimVector = transform.rotation * childRead->boneAxis;//OYM：将BoneAxis按照transform.rotation进行旋转

                    Quaternion AimRotation = Quaternion.FromToRotation(AimVector, Direction);//OYM：我觉得这里应该用lookRotation
                    transform.rotation = AimRotation * transform.rotation;//OYM：旋转过去
                }
            }
        }
        #endregion
    }
}



