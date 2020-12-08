#define ADB_DEBUG

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Jobs.LowLevel;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using System;
using Unity.Mathematics;

namespace ADBRuntime.Internal
{
    public unsafe class ADBRunTimeJobsTable
    {
        private const float EPSILON = 0.001f;
        private const float SQRT_2 = 1.41421356f;

        #region Jobs
        /// <summary>
        /// 初始化所有点的位置
        /// </summary>
        [BurstCompile]
        public struct InitiralizePoint : IJobParallelForTransform

        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;

            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
            }
            public void TryExecute(TransformAccessArray PoinTransforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < PoinTransforms.length; i++)
                {
                    Execute(i, PoinTransforms[i]);
                }
            }
            void Execute(int index, Transform transform)
            {
#endif

                var pReadWritePoint = pReadWritePoints + index;
                var pReadPoint = pReadPoints + index;


                if (pReadPoint->fixedIndex == index)
                {
                    //Debug.Log(pReadPoint->localRotation==Quaternion.Inverse(transform.localRotation));这里没问题
                    transform.localRotation = pReadPoint->initialLocalRotation;//OYM：这里改变之后,rotation也会改变

                    pReadWritePoint->rotation = transform.rotation;
                    pReadWritePoint->rotationY = pReadPoint->initialRotation;//OYM:  注意,这个rotationY是参照原始的旋转,然后乘以deltaRotationY算出来的,是依赖rotation算出来,而不是真实存在的
                    //Debug.Log(pReadWritePoint->rotation+" "+index);
                    pReadWritePoint->position = transform.position;

                    pReadWritePoint->deltaRotationY = pReadWritePoint->deltaRotation = Quaternion.identity;//OYM:  fixed坐标不需要这些花里胡哨的
                    pReadWritePoint->deltaPosition = Vector3.zero;

                }
                else
                {
                    //OYM:  这里先要把所有乱动的点恢复到原来的坐标上去(比如你需要修复穿模,调用一下清零),再重新记录数据

                    var pFixReadWritePoint = pReadWritePoints + (pReadPoint->fixedIndex);
                    var pFixReadPoint = pReadPoints + (pReadPoint->fixedIndex);
                    transform.localRotation = pReadPoint->initialLocalRotation;//OYM: 修复localrotation
                    pReadWritePoint->position = pFixReadWritePoint->position + pFixReadWritePoint->rotation * pReadPoint->initialPosition;//OYM:  修复position

                    transform.position = pReadWritePoint->position;
                    pReadWritePoint->deltaPosition = Vector3.zero;
                    pReadWritePoint->deltaRotationY = pReadWritePoint->deltaRotation = Quaternion.identity;

                }
            }
        }
        /// <summary>
        /// 初始化所有的colldier
        /// </summary>
        [BurstCompile]
        public struct InitiralizeCollider : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;

            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
            }
            public void TryExecute(TransformAccessArray ColliderTransforms, JobHandle job)
            {
                if (!job.IsCompleted)
                {
                    job.Complete();
                }
                for (int i = 0; i < ColliderTransforms.length; i++)
                {
                    Execute(i, ColliderTransforms[i]);
                }
            }
            public void Execute(int index, Transform transform)
            {
#endif
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                pReadWriteCollider->position =transform.position + transform.rotation * pReadCollider->positionOffset;
                pReadWriteCollider->direction =transform.rotation * pReadCollider->staticDirection;
                pReadWriteCollider->rotation =  transform.rotation * pReadCollider->staticRotation;
                pReadWriteCollider->deltaPosition = Vector3.zero;
                pReadWriteCollider->deltaDirection = Vector3.zero;
                pReadWriteCollider->deltaRotation = Quaternion.identity;
            }
        }

        [BurstCompile]
        public struct ColliderGetTransform : IJobParallelForTransform
        //OYM：获取collider的deltaPostion
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            [ReadOnly]
            public float oneDivideIteration;
            public void Execute(int index, TransformAccess transform)
            {
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        pReadWriteCollider->deltaPosition = oneDivideIteration*( transform.position + transform.rotation * pReadCollider->positionOffset- pReadWriteCollider->position);
                        break;
                    case ColliderType.Capsule:
                        pReadWriteCollider->deltaPosition = oneDivideIteration*(transform.position + transform.rotation * pReadCollider->positionOffset - pReadWriteCollider->position);
                        pReadWriteCollider->deltaDirection = oneDivideIteration*( transform.rotation * pReadCollider->staticDirection- pReadWriteCollider->direction);
                        break;
                    case ColliderType.OBB:
                        pReadWriteCollider->deltaPosition = oneDivideIteration * (transform.position + transform.rotation * pReadCollider->positionOffset - pReadWriteCollider->position);
                        pReadWriteCollider->deltaRotation = Quaternion.Lerp(Quaternion.identity, ((transform.rotation * pReadCollider->staticRotation) * Quaternion.Inverse(pReadWriteCollider->rotation)), oneDivideIteration);
                        break;
                    default:
                        break;
                }
            }
        }
        [BurstCompile]
        public struct ColliderUpdate : IJobParallelFor
        //OYM:  获取坐标,迭代
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            [NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
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
                ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + index;
                ColliderRead* pReadCollider = pReadColliders + index;
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        pReadWriteCollider->position +=pReadWriteCollider->deltaPosition ; 
                        break;
                    case ColliderType.Capsule:
                        pReadWriteCollider->position += pReadWriteCollider->deltaPosition ;
                        pReadWriteCollider->direction +=  pReadWriteCollider->deltaDirection ;
                        break;
                    case ColliderType.OBB:
                        pReadWriteCollider->position += pReadWriteCollider->deltaPosition ;
                        pReadWriteCollider->rotation =pReadWriteCollider->deltaRotation* pReadWriteCollider->rotation;
                        break;
                    default:
                        break;

                }
            }
        }

        /// <summary>
        /// 获取点的位置,同时处理速度上的一些调整
        /// </summary>
        [BurstCompile]
        public struct PointGetTransform : IJobParallelForTransform
        {
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public PointRead* pReadPoints;
            [NativeDisableUnsafePtrRestriction]
            public PointReadWrite* pReadWritePoints;
            [ReadOnly]
            public float oneDivideIteration;
            public void Execute(int index, TransformAccess transform)
            {
#if ADB_DEBUG
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

            public void Execute(int index, Transform transform)//OYM：注意,这里只获取delta的值与fixed点的坐标,不记录其他的数据
            {
#endif

                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;

                if (pReadPoint->fixedIndex == index)//OYM：fixedpoint
                {
                    pReadWritePoint->deltaPosition = oneDivideIteration * (transform.position - pReadWritePoint->position);


                    //OYM：做笔记 unity当中 child.rotation =parent.rotation*child.localrotation;
                    Quaternion rotationTemp =  ( transform.rotation* Quaternion.Inverse(transform.localRotation))* pReadPoint->initialLocalRotation;
                    pReadWritePoint->deltaRotation = Quaternion.LerpUnclamped(Quaternion.identity, rotationTemp * Quaternion.Inverse(pReadWritePoint->rotation), oneDivideIteration);
                    pReadWritePoint->deltaRotationY = Quaternion.AngleAxis(pReadWritePoint->deltaRotation.eulerAngles.y, Vector3.up);

                }
                else
                {
                    //OYM:  写成这样是为了在迭代的方式下尽可能的减少计算,所以用近似的方法减少速度,而不是damp
                    if (pReadWritePoint->deltaPosition.sqrMagnitude > (pReadPoint->mass ) * (pReadPoint->mass )*0.1f)//OYM：>0.1*mass*mass,注意这里的mass被乘以了0.2f
                    {
                        pReadWritePoint->deltaPosition *=(0.8f+pReadPoint->mass);
                    }
                    else
                    {
                        pReadWritePoint->deltaPosition *= 0.97f;//OYM：测了一下感觉这个数最好,有需求自己改
                    }
                }
            }
        }

        [BurstCompile]
        public struct PointUpdate : IJobParallelFor
        {
            /// <summary>
            /// 所有点位置的指针
            /// </summary>
            [NativeDisableUnsafePtrRestriction]
            internal PointReadWrite* pReadWritePoints;
            /// <summary>
            /// 所有点的指针
            /// </summary>
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            internal PointRead* pReadPoints;
            /// <summary>
            /// 所有碰撞体坐标的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            /// <summary>
            /// 所有碰撞体的指针
            /// </summary>);
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderRead* pReadColliders;
            /// <summary>
            /// 碰撞体数量
            /// </summary>
            [ReadOnly]
            public int colliderCount;
            /// <summary>
            /// 风力
            /// </summary>
            [ReadOnly]
            internal Vector3 addForcePower;
            /// <summary>
            /// 大小
            /// </summary>
            [ReadOnly]
            internal float globalScale;
            /// <summary>
            /// 1/迭代次数,为什么不用迭代次数,因为除法比乘法慢
            /// </summary>
            [ReadOnly]
            internal float oneDivideIteration;
            [ReadOnly]
            internal float deltaTime;
            [ReadOnly]
            internal bool isCollision;
            [ReadOnly]
            internal bool isOptimize;
 #if ADB_DEBUG
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
#endif
            public void Execute(int index)
            {
                PointRead* pReadPoint = pReadPoints + index;
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;
                if (pReadPoint->fixedIndex != index)
                {
                    EvaluatePosition(index, pReadPoint, pReadWritePoint);
                    if (isCollision)
                    {
                        for (int i = 0; i < colliderCount; ++i)
                        {
                            ColliderRead* pReadCollider = pReadColliders + i;
                            ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;

                            if (pReadCollider->isOpen && (pReadPoint->colliderChoice & pReadCollider->colliderChoice)!= 0)
                            {
                                ColliderCheck(pReadPoint, pReadWritePoint, pReadCollider, pReadWriteCollider);
                            }

                        }
                    }
                }
                else
                {
                        //OYM：计算渐进的fixed点坐标
                        pReadWritePoint->position += pReadWritePoint->deltaPosition;
                        pReadWritePoint->rotationY = pReadWritePoint->deltaRotationY * pReadWritePoint->rotationY;
                        pReadWritePoint->rotation = pReadWritePoint->deltaRotation * pReadWritePoint->rotation;
                }
            
            }
            private void EvaluatePosition(int index ,PointRead* pReadPoint, PointReadWrite* pReadWritePoint)
            {
                //OYM：如果你想要添加什么奇怪的力的话,可以在这底下添加

                Vector3 deltaPosition = Vector3.zero;
                //OYM：获取固定点的信息
                PointReadWrite* pFixedPointReadWrite = (pReadWritePoints + pReadPoint->fixedIndex);
                PointRead* pFixedPointRead = (pReadPoints + pReadPoint->fixedIndex);
                //OYM：获取以fixed位移进行为参考进行距离补偿(这里的delta已经乘以过onedivideItertation了)
                pReadWritePoint->position +=pFixedPointReadWrite->deltaPosition * pReadPoint->distanceCompensation;
                //OYM：获取当前相对fixed的向量
                Vector3 direction = pReadWritePoint->position - pFixedPointReadWrite->position;
                Vector3 back;
                //OYM：获取归位的向量

                if (pReadPoint->isFixGravityAxis)//OYM:  固定重力方向,这个值会受到fix节点初始rotation的影响
                {

                    deltaPosition += oneDivideIteration * ((pFixedPointReadWrite->rotation *Quaternion.Inverse(pFixedPointRead->initialRotation) )*pReadPoint->gravity) * (0.5f * deltaTime * deltaTime) * globalScale;//OYM：重力
                    back = pFixedPointReadWrite->rotation * pReadPoint->initialPosition * globalScale - direction;
                }
                else
                {
                    deltaPosition += oneDivideIteration * pReadPoint->gravity * (0.5f * deltaTime * deltaTime) * globalScale;//OYM：重力
                    back = pFixedPointReadWrite->rotationY * pReadPoint->initialPosition * globalScale - direction;
                }

                //OYM：计算外来力
                Vector3 addForce = oneDivideIteration * addForcePower * pReadPoint->addForceScale / pReadPoint->weight;
                deltaPosition += GetRotateForce(addForce, direction)+addForce;
                //OYM：计算弹性形变的内部应力,当freeze很小的时候设置上限,很大的时候乘以系数
                deltaPosition += oneDivideIteration* deltaTime * pReadPoint->freeze* Vector3.ClampMagnitude(back, pReadPoint->freeze * 0.1f);
                //OYM：计算离心力(理想状态
                deltaPosition += deltaTime *((pFixedPointReadWrite->deltaRotation * direction)-direction );
                //OYM：计算以fixed位移进行为参考进行速度补偿
                deltaPosition -= pFixedPointReadWrite->deltaPosition * pReadPoint->moveByFixedPoint * 0.2f;//OYM：测试了一下,0.2是个恰到好处的值,不会显得太大也不会太小
                //OYM：计算重力

                if (isOptimize)
                {
                    //OYM：减少对于骨骼的拉长
                    //OYM:  注意,这个操作会让你的骨骼看上去更加的卷,更加的带有动漫风
                    deltaPosition -= Vector3.Dot(deltaPosition, direction) / direction.sqrMagnitude * direction;
                    deltaPosition += GetRotateForce(deltaPosition, direction);
                }

                //OYM：赋值给累积的速度
                pReadWritePoint->deltaPosition += deltaPosition;
                //OYM：赋值给距离
                pReadWritePoint->position += oneDivideIteration * pReadWritePoint->deltaPosition;//OYM：这里我想了很久,应该是这样,如果是迭代n次的话,那么deltaposition将会被加上n次,正规应该是只加一次


                //Debug.Log(index + " : " + pFixedPointReadWrite->position + " " + positionA + " " + pFixedPointReadWrite->deltaPosition * pReadPoint->distanceCompensation + "  " + positionB);
            }
            Vector3 GetRotateForce(Vector3 force, Vector3 direction)//OYM：返回一个不存在的力,使得其向受力方向卷曲,在一些动漫里面会经常出现这种曲线的头发
            {
                return (Quaternion.Euler(-Mathf.Rad2Deg * force.z, 0, -Mathf.Rad2Deg * force.x) * direction - direction);
            }
            private void ColliderCheck(PointRead* pPointRead, PointReadWrite* pReadWritePoint, ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider)
            {

                //OYM：条件判断
                Vector3 pushout;
                float sqrPushout;
                float scale = pReadCollider->isConnectWithBody ? globalScale : 1;
                float radius;

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        radius = pPointRead->radius * globalScale + pReadCollider->radius * scale;
                        if (Abs(pReadWriteCollider->position.y - pReadWritePoint->position.y) < radius &&
                            Abs(pReadWriteCollider->position.x - pReadWritePoint->position.x) < radius &&
                            Abs(pReadWriteCollider->position.z - pReadWritePoint->position.z) < radius)//OYM：快速检查
                        {
                            pushout = pReadWritePoint->position - pReadWriteCollider->position;
                            sqrPushout = pushout.sqrMagnitude;

                            if (sqrPushout < radius* radius)
                            {
                                pushout = pushout * (radius / Mathf.Sqrt(sqrPushout) - 1);
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->deltaPosition += pushout;
                            }
                        }
                        break;

                    case ColliderType.Capsule:
                        radius = pPointRead->radius * globalScale + pReadCollider->radius * scale;
                        Vector3 centerA = pReadWriteCollider->position + scale * pReadWriteCollider->direction * 0.5f;
                        if (Abs(centerA.y - pReadWritePoint->position.y) < Abs(pReadWriteCollider->direction.y) * 0.5f + radius &&
                            Abs(centerA.x - pReadWritePoint->position.x) < Abs(pReadWriteCollider->direction.x) * 0.5f + radius &&
                             Abs(centerA.z - pReadWritePoint->position.z) < Abs(pReadWriteCollider->direction.z) * 0.5f + radius)//OYM：快速检查
                        {
                            pushout = pReadWritePoint->position - ConstrainToSegment(pReadWritePoint->position, pReadWriteCollider->position, pReadWriteCollider->direction * scale);
                            sqrPushout = pushout.sqrMagnitude;
                            if (sqrPushout < radius* radius)
                            {
                                pushout = pushout * (radius / Mathf.Sqrt(sqrPushout) - 1);
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->deltaPosition += pushout;
                            }
                        }
                        break;
                    case ColliderType.OBB:
                        radius = pPointRead->radius * globalScale + Max(pReadCollider->boxSize.x, pReadCollider->boxSize.y, pReadCollider->boxSize.z) * scale * SQRT_2;
                        if (Abs(pReadWriteCollider->position.x - pReadWritePoint->position.x) < radius &&
                            Abs(pReadWriteCollider->position.y - pReadWritePoint->position.y) < radius &&
                            Abs(pReadWriteCollider->position.z - pReadWritePoint->position.z) < radius)//OYM：快速检查
                        {
                            pushout = Quaternion.Inverse(pReadWriteCollider->rotation) * (pReadWritePoint->position - pReadWriteCollider->position);
                            Vector3 boxSize = scale * pReadCollider->boxSize+new Vector3( pPointRead->radius, pPointRead->radius, pPointRead->radius);
                            if ( boxSize.x >Abs( pushout.x)&&
                                boxSize.y >Abs(pushout.y)  &&
                                boxSize.z > Abs(pushout.z )
                                )
                            {
                                float pushoutX = ( pushout.x > 0 ? boxSize.x - pushout.x :  -boxSize.x - pushout.x);
                                float pushoutY =  (pushout.y > 0 ? boxSize.y - pushout.y :- boxSize.y - pushout.y);
                                float pushoutZ = (pushout.z > 0 ? boxSize.z - pushout.z : -boxSize.z - pushout.z);

                                if (Abs(pushoutZ) <= Abs(pushoutY) && Abs(pushoutZ) <= Abs(pushoutX))
                                {
                                    pushout = pReadWriteCollider->rotation * new Vector3(0, 0, pushoutZ);

                                }
                                else if (Abs(pushoutY) <=Abs(pushoutX) && Abs(pushoutY) <= Abs(pushoutZ))
                                {
                                    pushout = pReadWriteCollider->rotation * new Vector3(0, pushoutY, 0);
                                }
                                else
                                {
                                    pushout = pReadWriteCollider->rotation * new Vector3(pushoutX, 0, 0);
                                }
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->deltaPosition += pushout;
                            }
                        }
                        break;
                    default:
                        return;
                }
            }
            Vector3 ConstrainToSegment(Vector3 tag, Vector3 pos, Vector3 dir)
            {
                float  t = Vector3.Dot(tag - pos, dir) / dir.sqrMagnitude;
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
                return A < B ? (A < C ? A : C) : (B < C ? B : C);
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
        public struct ConstraintUpdate : IJobParallelFor
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
            /// <summary>
            /// 所有读写碰撞体
            /// </summary>
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ColliderReadWrite* pReadWriteColliders;
            /// <summary>
            /// 所有杆件
            /// 
            /// </summary>
            [ReadOnly, NativeDisableUnsafePtrRestriction]
            public ConstraintRead* pConstraintsRead;
            [ReadOnly]
            /// <summary>
            /// 碰撞体序号
            /// </summary>);
            public int colliderCount;
            [ReadOnly]
            public float globalScale;
            [ReadOnly]
            public int globalColliderCount;
            [ReadOnly]
            public bool isCollision;
            [ReadOnly]
            internal float oneDivideIteration;
 #if ADB_DEBUG
            public void TryExecute(int index, int temp, JobHandle job)
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
#endif
            public void Execute(int index)
            {
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
                float Force = Distance - constraint->length * globalScale;
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
                    default:
                        ConstraintPower = 0.0f;
                        break;
                }


                //OYM：获取AB点重量比值的比值,由于重量越大移动越慢,所以A的值实际上是B的重量的比

                float WeightProportion = pPointReadB->weight / (pPointReadA->weight + pPointReadB->weight);

                if (ConstraintPower > 0.0f)//OYM：这里不可能小于0吧（除非有人搞破坏）
                {
                    Vector3 Displacement = Direction.normalized * (Force * ConstraintPower);

                    pReadWritePointA->position += Displacement * WeightProportion;
                    pReadWritePointA->deltaPosition += Displacement * WeightProportion;
                    pReadWritePointB->position += -Displacement * (1 - WeightProportion);
                    pReadWritePointB->deltaPosition +=- Displacement * (1 - WeightProportion);
                }

                if (isCollision && constraint->isCollider)
                {
                    for (int i = 0; i < colliderCount; ++i)
                    {
                        ColliderRead* pReadCollider = pReadColliders + i;//OYM：终于到碰撞这里了

                        if (pReadCollider->isOpen && (pPointReadA->colliderChoice & pReadCollider->colliderChoice) != 0)
                        {//OYM：collider是否打开,且pPointReadA->colliderChoice是否包含 pReadCollider->colliderChoice的位
                            ColliderReadWrite* pReadWriteCollider = pReadWriteColliders + i;
                            ComputeCollider(
                                pReadCollider, pReadWriteCollider,
                                pReadWritePointA, pReadWritePointB,
                                constraint,
                                WeightProportion,
                                pPointReadA->friction, pPointReadB->friction
                                );
                        }
                    }
                }
            }
            Vector3 GetRotateForce(Vector3 force, Vector3 direction)//OYM：返回一个不存在的力,使得其向受力方向卷曲,在一些动漫里面会经常出现这种曲线的头发
            {
                return (Quaternion.Euler(Mathf.Rad2Deg * (-force.z), 0, Mathf.Rad2Deg * (-force.x) )* direction - direction);
            }
            private void ComputeCollider(ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB,ConstraintRead* constraint, float WeightProportion,
                float frictionA, float frictionB)
            {
                float throwTemp;
                float t,radius;
                Vector3 constraintCenter, colliderCenter;
                float scale = pReadCollider->isConnectWithBody ? globalScale : 1;

                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        {
                            constraintCenter = (pReadWritePointA->position + pReadWritePointB->position) * 0.5f;
                            radius = scale * pReadCollider->radius + globalScale* constraint->radius ;
                            if (Abs(pReadWriteCollider->position.y - constraintCenter.y) < (Abs(pReadWritePointA->position.y - constraintCenter.y) + radius )&&
                                Abs(pReadWriteCollider->position.x - constraintCenter.x) < (Abs(pReadWritePointA->position.x - constraintCenter.x) + radius) &&
                                Abs(pReadWriteCollider->position.z - constraintCenter.z) < (Abs(pReadWritePointA->position.z - constraintCenter.z) + radius))
                            {
                                Vector3 pointOnLine = ConstrainToSegment(pReadWriteCollider->position, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out t);
                                DistributionPower(pointOnLine - pReadWriteCollider->position, radius, pReadWritePointA, pReadWritePointB, WeightProportion, t, frictionA, frictionB, pReadCollider->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.Capsule:
                        {
                            constraintCenter = pReadWriteCollider->position + pReadWriteCollider->direction * 0.5f;
                            colliderCenter = (pReadWritePointA->position + pReadWritePointB->position) * 0.5f;
                            radius = scale * pReadCollider->radius + globalScale * constraint->radius;
                            if (Abs(constraintCenter.y - colliderCenter.y) < (Abs(pReadWriteCollider->direction.y) * 0.5f + Abs(pReadWritePointA->position.y - colliderCenter.y) + radius) &&
                                Abs(constraintCenter.x - colliderCenter.x) < (Abs(pReadWriteCollider->direction.x) * 0.5f + Abs(pReadWritePointA->position.x - colliderCenter.x) + radius) &&
                                Abs(constraintCenter.z - colliderCenter.z) < (Abs(pReadWriteCollider->direction.z) * 0.5f + Abs(pReadWritePointA->position.z - colliderCenter.z) + radius))
                            {
                                Vector3 pointOnCollider, pointOnLine;
                                SqrComputeNearestPoints(pReadWriteCollider->position, pReadWriteCollider->direction * scale, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out throwTemp, out t, out pointOnCollider, out pointOnLine);
                                DistributionPower(pointOnLine - pointOnCollider, radius, pReadWritePointA, pReadWritePointB, WeightProportion, t, frictionA, frictionB, pReadCollider->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.OBB:
                        {
                            constraintCenter = (pReadWritePointA->position + pReadWritePointB->position) * 0.5f;
                            radius = globalScale * constraint->radius+ Max(pReadCollider->boxSize.x, pReadCollider->boxSize.y, pReadCollider->boxSize.z) * scale * SQRT_2;

                            if (Abs(pReadWriteCollider->position.x - constraintCenter.x) < Abs(pReadWritePointA->position.x - constraintCenter.x) + radius &&
                                Abs(pReadWriteCollider->position.y - constraintCenter.y) < Abs(pReadWritePointA->position.y - constraintCenter.y) + radius &&
                                Abs(pReadWriteCollider->position.z - constraintCenter.z) < Abs(pReadWritePointA->position.z - constraintCenter.z) + radius)
                            {
                                Vector3 boxSize = scale * pReadCollider->boxSize + new Vector3( globalScale * constraint->radius, globalScale * constraint->radius, globalScale * constraint->radius);
                                float t1, t2;
                                //OYM：这个方法可以求出直线与obbbox的两个交点
                                SegmentToOBB(pReadWritePointA->position, pReadWritePointB->position, pReadWriteCollider->position, boxSize, Quaternion.Inverse(pReadWriteCollider->rotation), out t1, out t2);

                                t1 = Clamp01(t1);
                                t2 = Clamp01(t2);
                                //OYM：如果存在,那么t2>t1,且至少有一个点不在边界上
                                bool bHit = t1 >= 0f && t2 > t1 && t2 <= 1.0f;
                                if (bHit)
                                {
                                    //OYM：这里不是取最近的点,而是取中点,最近的点效果并不理想
                                    t = (t1 + t2) * 0.5f;
                                    Vector3 dir = pReadWritePointB->position - pReadWritePointA->position;
                                    Vector3 nearestPoint = pReadWritePointA->position + dir * t;
                                    Vector3 pushout = Quaternion.Inverse(pReadWriteCollider->rotation) * (nearestPoint - pReadWriteCollider->position);
                                    float pushoutX = pushout.x > 0 ? boxSize.x - pushout.x : -boxSize.x- pushout.x;
                                    float pushoutY = pushout.y > 0 ? boxSize.y - pushout.y : -boxSize.y - pushout.y;
                                    float pushoutZ = pushout.z > 0 ? boxSize.z - pushout.z : -boxSize.z - pushout.z;
                                    //OYM：这里我自己都不太记得了 XD
                                    //OYM：这里是选推出点离的最近的位置,然后推出
                                    //OYM：Abs(pushoutZ) < Abs(pushoutY)是错的 ,可能会出现两者都为0的情况
                                    if (Abs(pushoutZ) <= Abs(pushoutY) && Abs(pushoutZ) <= Abs(pushoutX))
                                    {
                                        pushout = pReadWriteCollider->rotation * new Vector3(0, 0, pushoutZ);

                                    }
                                    else if (Abs(pushoutY) <= Abs(pushoutX) && Abs(pushoutY) <= Abs(pushoutZ))
                                    {
                                        pushout = pReadWriteCollider->rotation * new Vector3(0, pushoutY, 0);
                                    }
                                    else
                                    {
                                        pushout = pReadWriteCollider->rotation * new Vector3(pushoutX, 0, 0);
                                    }
                                    if (pushout.sqrMagnitude != 0)
                                    {
                                        //float inverse1Velocity = Vector3.Dot(pushout, pReadWritePointA->velocity) / pushout.sqrMagnitude;
                                        //pReadWritePointA->velocity -= pushout * inverse1Velocity;
                                        //pReadWritePointB->velocity -= pushout * inverse1Velocity;
                                        pReadWritePointA->deltaPosition *= (1 - frictionA);
                                        pReadWritePointB->deltaPosition *= (1 - frictionB);

                                        //float Propotion = WeightProportion * t / (1 - WeightProportion - t + 2 * WeightProportion * t);
                                        if (WeightProportion > EPSILON)
                                        {
                                            if (pReadCollider->collideFunc == CollideFunc.InsideNoLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit)
                                            {
                                                pReadWritePointA->deltaPosition += 0.01f * oneDivideIteration * (pushout *(1- t));
                                            }
                                            else
                                            {
                                                pReadWritePointA->position += (pushout *(1- t));
                                                pReadWritePointA->deltaPosition += (pushout * (1-t));
                                            }
             
                                        }
                                        else
                                        {
                                            t = 1;
                                        }
                                        if (pReadCollider->collideFunc == CollideFunc.InsideNoLimit || pReadCollider->collideFunc == CollideFunc.OutsideNoLimit)
                                        {
                                            pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * (pushout * t);
                                        }
                                        else
                                        {
                                            pReadWritePointB->position += (pushout *  t);
                                            pReadWritePointB->deltaPosition += (pushout *  t);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    default:
                        return;

                }
            }

            void DistributionPower(Vector3 pushout, float radius, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, float WeightProportion, float lengthPropotion, float frictionA, float frictionB, CollideFunc collideFunc)
            {

                float sqrPushout = pushout.sqrMagnitude;
                switch (collideFunc)
                {
                    //OYM：整片代码里面最有趣的一块
                    //OYM：反正我现在不想回忆当时怎么想的了XD
                    case CollideFunc.Freeze:
                        break;//OYM：猜猜为啥这样写
                    case CollideFunc.OutsideLimit:
                        if (!(sqrPushout < radius * radius )&& sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.OutsideNoLimit:
                        if (!(sqrPushout < radius * radius) && sqrPushout != 0)
                        { return; }
                        break;
                    case CollideFunc.InsideNoLimit:
                        if (sqrPushout < radius * radius && sqrPushout != 0)
                        { return; }
                        break;

                }
                //OYM：把pushout方向多余的力给减掉
                //OYM：没有也不需要
                    // pReadWritePointA->velocity -= pushout * (Vector3.Dot(pushout, pReadWritePointA->velocity) / sqrPushout);
                    //pReadWritePointB->velocity -= pushout * (Vector3.Dot(pushout, pReadWritePointB->velocity) / sqrPushout);
                pReadWritePointA->deltaPosition *= (1 - frictionA);
                pReadWritePointB->deltaPosition *= (1 - frictionB);

                pushout = pushout * (radius / Mathf.Sqrt(sqrPushout) - 1);//OYM：这里简单解释一下,首先我要计算的是推出的距离,及半径长度减去原始的pushout度之后剩下的值,即pushout/pushout.magnitude*radius-pushout.即pushout*((radius/magnitude -1));

                //  float Propotion = WeightProportion * lengthPropotion / (1 - WeightProportion - lengthPropotion + 2 * WeightProportion * lengthPropotion);

                if (WeightProportion > EPSILON)
                {
                    if (collideFunc ==CollideFunc.InsideNoLimit|| collideFunc == CollideFunc.OutsideNoLimit)
                    {
                        pReadWritePointA->deltaPosition +=0.01f*oneDivideIteration * (1 - lengthPropotion)* pushout;
                    }
                    else
                    {
                        pReadWritePointA->position += (pushout * (1 - lengthPropotion));
                        pReadWritePointA->deltaPosition += (pushout * (1 - lengthPropotion));
                    }

                }
                else
                {
                    lengthPropotion = 1;
                }

                if (collideFunc == CollideFunc.InsideNoLimit || collideFunc == CollideFunc.OutsideNoLimit)
                {
                    pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * (lengthPropotion) * pushout;
                }
                else
                {
                    pReadWritePointB->position += (pushout * lengthPropotion);
                    pReadWritePointB->deltaPosition += (pushout * lengthPropotion);
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
            void SegmentToOBB(Vector3 start, Vector3 end, Vector3 center, Vector3 size, Quaternion InverseNormal, out float t1, out float t2)
            {
                Vector3 startP = InverseNormal * (center - start);
                Vector3 endP = InverseNormal * (center - end);
                SegmentToAABB(startP, endP, center, -size, size, out t1, out t2);
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
                return A < B ? (A < C ? A : C) : (B < C ? B : C);
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
        public struct JobPointToTransform : IJobParallelForTransform
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
#if ADB_DEBUG
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
#endif
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;//OYM：获取每个读写点
                PointRead* pReadPoint = pReadPoints + index;//OYM：获取每个只读点

                if (pReadPoint->fixedIndex != index)//OYM：不是fix点
                {
                    transform.position = pReadWritePoint->position;
                }

                //OYM:  旋转节点
                //OYM:  这里有个bug,当初考虑的时候是存在多个子节点的,但是实际上并没有
                if (pReadPoint->childFirstIndex > -1)
                {
                    transform.localRotation = pReadPoint->initialLocalRotation;
                    var child = pReadWritePoints + pReadPoint->childFirstIndex;
                    var parent = pReadWritePoints + pReadPoint->parentIndex;
                    var childRead = pReadPoints + pReadPoint->childFirstIndex;

                    Vector3 ToDirection = child->position - pReadWritePoint->position;//OYM：朝向等于面向子节点的方向
                    Vector3 FixedDirection = parent->position - pReadWritePoint->position;
                    if (ToDirection.sqrMagnitude > EPSILON * EPSILON)//OYM：两点不再一起
                    {
                        Vector3 FromDirection = transform.rotation * childRead->initialLocalPosition;//OYM：将BoneAxis按照transform.rotation进行旋转

                        Quaternion AimRotation = Quaternion.FromToRotation(FromDirection, ToDirection);//OYM：我仔细考虑了下,fromto用在这里不一定是最好,但是一定是最快

                        transform.rotation = AimRotation * transform.rotation;
                    }
                }
            }
        }
#endregion
    }
}



