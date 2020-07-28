
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
        #region Single
        private static ADBRunTimeJobsTable instance_ADBRunTimeJobsTable;//OYM：单例模式
        private ADBRunTimeJobsTable() { }
        public static ADBRunTimeJobsTable GetRunTimeJobsTable(bool isDebug = false)
        {
            if (instance_ADBRunTimeJobsTable == null)
            {
                instance_ADBRunTimeJobsTable = new ADBRunTimeJobsTable();
            }

            instance_ADBRunTimeJobsTable.isDebug = isDebug;
            return instance_ADBRunTimeJobsTable;
        }

        #endregion

        internal JobHandle returnHJob;
        internal bool isDebug;
        // private int complexHJobBatchCount=8;
        //先预留在这里看下性能
        private const float EPSILON = 0.001f;
        private const float SQRT_2 = 1.41421356f;
        public int computeCount = 0;
        public void Add(int count = 1)
        {
            if (isDebug)
            {
                computeCount += count;
            }
        }

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


                if (pReadPoint->fixedIndex == index)
                {
                    //Debug.Log(pReadPoint->localRotation==Quaternion.Inverse(transform.localRotation));这里没问题
                    transform.localRotation = pReadPoint->initialLocalRotation;//OYM：这里改变之后,rotation也会改变

                    pReadWritePoint->rotation = transform.rotation;
                    pReadWritePoint->rotationY = pReadPoint->initialRotation;
                    //Debug.Log(pReadWritePoint->rotation+" "+index);
                    pReadWritePoint->position = transform.position;
                    pReadWritePoint->deltaRotationY = pReadWritePoint->deltaRotation = Quaternion.identity;
                    pReadWritePoint->deltaPosition = Vector3.zero;

                }
                else
                {
                    var pFixReadWritePoint = pReadWritePoints + (pReadPoint->fixedIndex);
                    var pFixReadPoint = pReadPoints + (pReadPoint->fixedIndex);
                    if (!pReadPoint->isVirtual)
                    {
                        transform.localRotation = pReadPoint->initialLocalRotation;
                        pReadWritePoint->position = pFixReadWritePoint->position + pFixReadWritePoint->rotation * pReadPoint->initialPosition;
                        transform.position = pReadWritePoint->position;
                        pReadWritePoint->deltaPosition = Vector3.zero;
                    }
                    else
                    {
                        pReadWritePoint->position = pFixReadWritePoint->position + pFixReadWritePoint->rotation * pReadPoint->initialPosition;
                        pReadWritePoint->deltaPosition = Vector3.zero;
                    }
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
                /*
            )
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
                        pReadWriteCollider->deltaRotation = Quaternion.Lerp(Quaternion.identity, ((transform.rotation * pReadCollider->staticRotation) * Quaternion.Inverse(pReadWriteCollider->rotation)), oneDivideIteration);//OYM：写的有点啰嗦,实际上就是对backToForward做除法,计算如下
                        /*
                        Quaternion rotationForward = transform.rotation * pReadCollider->staticRotation;
                        Quaternion rotationBack = Quaternion.Inverse(pReadWriteCollider->rotation);
                        Quaternion backToForward = rotationForward * rotationBack;
                         */
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

            public void Execute(int index, Transform transform)//OYM：注意,这里只获取delta,不获取真实的坐标
            {
                */
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

                    if (pReadWritePoint->deltaPosition.sqrMagnitude > (pReadPoint->mass ) * (pReadPoint->mass )*0.1f)//OYM：大于mass*0.2f长度的速度会触发掉速
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
            /// 1/迭代次数,为什么不用int,因为除法比乘法慢
            /// </summary>
            [ReadOnly]
            internal float oneDivideIteration;
            [ReadOnly]
            internal float deltaTime;
            [ReadOnly]
            internal bool isCollision;
            [ReadOnly]
            internal bool isOptimize;
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
                //OYM：获取以fixed位移进行为参考进行距离补偿(这里的delta已经乘以过onedivideItertation了)
                pReadWritePoint->position +=pFixedPointReadWrite->deltaPosition * pReadPoint->distanceCompensation;
                //OYM：获取当前相对fixed的向量
                Vector3 direction = pReadWritePoint->position - pFixedPointReadWrite->position;
                Vector3 back;
                //OYM：获取归位的向量

                if (pReadPoint->isFixGravityAxis)
                {
                    back = pFixedPointReadWrite->rotation * pReadPoint->initialPosition * globalScale - direction;
                }
                else
                {
                    back = pFixedPointReadWrite->rotationY * pReadPoint->initialPosition * globalScale - direction;
                }

                //OYM：计算外来力
                Vector3 addForce = oneDivideIteration * addForcePower * pReadPoint->addForceScale / pReadPoint->weight;
                deltaPosition += GetRotateForce(addForce, direction)+addForce;
                //OYM：计算弹性形变的内部应力,当freeze很小的时候设置上限,很大的时候乘以系数
                deltaPosition += oneDivideIteration* deltaTime * ((pReadPoint->freeze < 1)? Vector3.ClampMagnitude(back, pReadPoint->freeze * 0.1f) : back * pReadPoint->freeze);
                //OYM：计算离心力(理想状态
                deltaPosition += deltaTime *((pFixedPointReadWrite->deltaRotation * direction)-direction );
                //OYM：计算以fixed位移进行为参考进行速度补偿
                deltaPosition -= pFixedPointReadWrite->deltaPosition * pReadPoint->moveByFixedPoint * 0.2f;//OYM：测试了一下,0.2是个恰到好处的值,不会显得太大也不会太小
                //OYM：计算重力
                deltaPosition += oneDivideIteration * pReadPoint->gravity * (0.5f * deltaTime * deltaTime) * globalScale;
                if (isOptimize)
                {
                    //OYM：减少对于骨骼的拉长
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
                float throwTemp;   //OYM：有些c#不允许使用丢弃
                Vector3 pushout;
                float sqrPushout;
                float scale = pReadCollider->isConnectWithBody ? globalScale : 1;
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        if (QuickCheck(pReadCollider, pReadWriteCollider, pReadWritePoint, scale))
                        {
                            pushout = pReadWritePoint->position - pReadWriteCollider->position;
                            sqrPushout = pushout.sqrMagnitude;

                            if (sqrPushout < pReadCollider->radius * pReadCollider->radius * scale * scale)
                            {
                                pushout = pushout * (pReadCollider->radius * scale / Mathf.Sqrt(sqrPushout) - 1);
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->deltaPosition += pushout;
                            }
                        }
                        break;

                    case ColliderType.Capsule:
                        if (QuickCheck(pReadCollider, pReadWriteCollider, pReadWritePoint, scale))
                        {
                            pushout = pReadWritePoint->position - ConstrainToSegment(pReadWritePoint->position, pReadWriteCollider->position, pReadWriteCollider->direction * scale, out throwTemp);
                            sqrPushout = pushout.sqrMagnitude;
                            if (sqrPushout < pReadCollider->radius * pReadCollider->radius * scale * scale)
                            {
                                pushout = pushout * (pReadCollider->radius * scale / Mathf.Sqrt(sqrPushout) - 1);
                                pReadWritePoint->position += pushout;
                                pReadWritePoint->deltaPosition += pushout;
                            }
                        }
                        break;
                    case ColliderType.OBB:
                        if (QuickCheck(pReadCollider, pReadWriteCollider, pReadWritePoint, scale))
                        {
                            pushout = Quaternion.Inverse(pReadWriteCollider->rotation) * (pReadWritePoint->position - pReadWriteCollider->position);
                            if (-scale * pReadCollider->boxSize.x < pushout.x && pushout.x < scale * pReadCollider->boxSize.x &&
                                -scale * pReadCollider->boxSize.y < pushout.y && pushout.y < scale * pReadCollider->boxSize.y &&
                                -scale * pReadCollider->boxSize.z < pushout.z && pushout.z < scale * pReadCollider->boxSize.z
                                )
                            {
                                float pushoutX = pushout.x > 0 ? scale * pReadCollider->boxSize.x - pushout.x : -scale * pReadCollider->boxSize.x - pushout.x;
                                float pushoutY = pushout.y > 0 ? scale * pReadCollider->boxSize.y - pushout.y : -scale * pReadCollider->boxSize.y - pushout.y;
                                float pushoutZ = pushout.z > 0 ? scale * pReadCollider->boxSize.z - pushout.z : -scale * pReadCollider->boxSize.z - pushout.z;

                                if (Abs(pushoutZ) < Abs(pushoutY) && Abs(pushoutZ) < Abs(pushoutX))
                                {
                                    pushout = pReadWriteCollider->rotation * new Vector3(0, 0, pushoutZ);

                                }
                                else if (Abs(pushoutY) < Abs(pushoutX) && Abs(pushoutY) < Abs(pushoutZ))
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

            private bool QuickCheck(ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider, PointReadWrite* pReadWritePoint, float scale)
            {
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        {
                            return Abs(pReadWriteCollider->position.y - pReadWritePoint->position.y) < pReadCollider->radius * scale &&
                                       Abs(pReadWriteCollider->position.x - pReadWritePoint->position.x) < pReadCollider->radius * scale &&
                                       Abs(pReadWriteCollider->position.z - pReadWritePoint->position.z) < pReadCollider->radius * scale;
                        }

                    case ColliderType.Capsule:
                        {
                            Vector3 centerA = pReadWriteCollider->position + scale * pReadWriteCollider->direction * 0.5f;

                            return Abs(centerA.y - pReadWritePoint->position.y) < Abs(pReadWriteCollider->direction.y) * 0.5f + pReadCollider->radius * scale &&
                                       Abs(centerA.x - pReadWritePoint->position.x) < Abs(pReadWriteCollider->direction.x) * 0.5f + pReadCollider->radius * scale &&
                                       Abs(centerA.z - pReadWritePoint->position.z) < Abs(pReadWriteCollider->direction.z) * 0.5f + pReadCollider->radius * scale;
                        }

                    case ColliderType.OBB:
                        {
                            float MaxLength = Max(pReadCollider->boxSize.x, pReadCollider->boxSize.y, pReadCollider->boxSize.z) * scale * SQRT_2;
                            return Abs(pReadWriteCollider->position.x - pReadWritePoint->position.x) < MaxLength &&
                                       Abs(pReadWriteCollider->position.y - pReadWritePoint->position.y) < MaxLength &&
                                       Abs(pReadWriteCollider->position.z - pReadWritePoint->position.z) < MaxLength;
                        }

                    default:
                        return false;
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
                    case ConstraintType.Virtual:
                        ConstraintPower = 1;
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
                                WeightProportion,
                                pPointReadA->friction, pPointReadB->friction,
                                pReadCollider->isConnectWithBody ? globalScale : 1);
                        }
                    }
                }
            }
            Vector3 GetRotateForce(Vector3 force, Vector3 direction)//OYM：返回一个不存在的力,使得其向受力方向卷曲,在一些动漫里面会经常出现这种曲线的头发
            {
                return (Quaternion.Euler(Mathf.Rad2Deg * (-force.z), 0, Mathf.Rad2Deg * (-force.x) )* direction - direction);
            }
            private void ComputeCollider(ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, float WeightProportion,
                float frictionA, float frictionB, float scale)
            {
                float throwTemp;
                float t;
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        {
                            if (QuickCheck(pReadCollider, pReadWriteCollider, pReadWritePointA, pReadWritePointB, scale))
                            {
                                Vector3 pointOnLine = ConstrainToSegment(pReadWriteCollider->position, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out t);
                                DistributionPower(pointOnLine - pReadWriteCollider->position, pReadCollider->radius * scale, pReadWritePointA, pReadWritePointB, WeightProportion, t, frictionA, frictionB, pReadCollider->collideFunc);
                            }
                        }

                        break;
                    case ColliderType.Capsule:
                        {

                            if (QuickCheck(pReadCollider, pReadWriteCollider, pReadWritePointA, pReadWritePointB, scale))
                            {
                                Vector3 pointOnCollider, pointOnLine;
                                SqrComputeNearestPoints(pReadWriteCollider->position, pReadWriteCollider->direction * scale, pReadWritePointA->position, pReadWritePointB->position - pReadWritePointA->position, out throwTemp, out t, out pointOnCollider, out pointOnLine);
                                DistributionPower(pointOnLine - pointOnCollider, pReadCollider->radius * scale, pReadWritePointA, pReadWritePointB, WeightProportion, t, frictionA, frictionB, pReadCollider->collideFunc);
                            }

                        }

                        break;
                    case ColliderType.OBB:
                        {
                            if (QuickCheck(pReadCollider, pReadWriteCollider, pReadWritePointA, pReadWritePointB, scale))
                            {
                                float t1, t2;
                                //OYM：这个方法可以求出直线与obbbox的两个交点
                                SegmentToOBB(pReadWritePointA->position, pReadWritePointB->position, pReadWriteCollider->position, scale * pReadCollider->boxSize, Quaternion.Inverse(pReadWriteCollider->rotation), out t1, out t2);

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
                                    float pushoutX = pushout.x > 0 ? scale * pReadCollider->boxSize.x - pushout.x : -scale * pReadCollider->boxSize.x - pushout.x;
                                    float pushoutY = pushout.y > 0 ? scale * pReadCollider->boxSize.y - pushout.y : -scale * pReadCollider->boxSize.y - pushout.y;
                                    float pushoutZ = pushout.z > 0 ? scale * pReadCollider->boxSize.z - pushout.z : -scale * pReadCollider->boxSize.z - pushout.z;
                                    //OYM：这里我自己都不太记得了 XD
                                    //OYM：这里是选推出点离的最近的位置,然后推出
                                    if (Abs(pushoutZ) < Abs(pushoutY) && Abs(pushoutZ) < Abs(pushoutX))
                                    {
                                        pushout = pReadWriteCollider->rotation * new Vector3(0, 0, pushoutZ);

                                    }
                                    else if (Abs(pushoutY) < Abs(pushoutX) && Abs(pushoutY) < Abs(pushoutZ))
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
                                            pReadWritePointA->position += (pushout * t);
                                            pReadWritePointA->deltaPosition += (pushout * t);
                                        }
                                        else
                                        {
                                            t = 1;
                                        }
                                        pReadWritePointB->position += (pushout * (1 - t));
                                        pReadWritePointB->deltaPosition += (pushout * (1 - t));

                                    }
                                }
                            }
                            break;
                        }
                    default:
                        return;

                }
            }
            private bool QuickCheck(ColliderRead* pReadCollider, ColliderReadWrite* pReadWriteCollider, PointReadWrite* pReadWritePointA, PointReadWrite* pReadWritePointB, float scale)
            {
                //OYM：原理很简单,用到了AABB盒相交算法,两个物体中心坐标的间隔的vector3与包裹其的AABBbox大小之和的vector3作比较,如果前者的x,y,z中任意一个大于后者,那么这两个AABB盒不想交,及两个碰撞体不想交
                switch (pReadCollider->colliderType)
                {
                    case ColliderType.Sphere:
                        {
                            Vector3 centerB = (pReadWritePointA->position + pReadWritePointB->position) * 0.5f;
                            return Abs(pReadWriteCollider->position.y - centerB.y) < (Abs(pReadWritePointA->position.y - centerB.y) + scale * pReadCollider->radius) &&
                                       Abs(pReadWriteCollider->position.x - centerB.x) < (Abs(pReadWritePointA->position.x - centerB.x) + scale * pReadCollider->radius) &&
                                       Abs(pReadWriteCollider->position.z - centerB.z) < (Abs(pReadWritePointA->position.z - centerB.z) + scale * pReadCollider->radius);
                        }

                    case ColliderType.Capsule:
                        {
                            Vector3 centerA = pReadWriteCollider->position + pReadWriteCollider->direction * 0.5f;
                            Vector3 centerB = (pReadWritePointA->position + pReadWritePointB->position) * 0.5f;

                            return Abs(centerA.y - centerB.y) < (Abs(pReadWriteCollider->direction.y) * 0.5f + Abs(pReadWritePointA->position.y - centerB.y) + scale * pReadCollider->radius) &&
                                       Abs(centerA.x - centerB.x) < (Abs(pReadWriteCollider->direction.x) * 0.5f + Abs(pReadWritePointA->position.x - centerB.x) + scale * pReadCollider->radius) &&
                                       Abs(centerA.z - centerB.z) < (Abs(pReadWriteCollider->direction.z) * 0.5f + Abs(pReadWritePointA->position.z - centerB.z) + scale * pReadCollider->radius);
                        }
                    case ColliderType.OBB:
                        {
                            Vector3 centerB = (pReadWritePointA->position + pReadWritePointB->position) * 0.5f;
                            float MaxLength = Max(pReadCollider->boxSize.x, pReadCollider->boxSize.y, pReadCollider->boxSize.z) * scale * SQRT_2;
                            return Abs(pReadWriteCollider->position.x - centerB.x) < Abs(pReadWritePointA->position.x - centerB.x) + MaxLength &&
                                       Abs(pReadWriteCollider->position.y - centerB.y) < Abs(pReadWritePointA->position.y - centerB.y) + MaxLength &&
                                       Abs(pReadWriteCollider->position.z - centerB.z) < Abs(pReadWritePointA->position.z - centerB.z) + MaxLength;
                        }

                    default:
                        return false;
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
                    pReadWritePointB->deltaPosition += 0.01f * oneDivideIteration * (1 - lengthPropotion) * pushout;
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
                PointReadWrite* pReadWritePoint = pReadWritePoints + index;//OYM：获取每个读写点
                PointRead* pReadPoint = pReadPoints + index;//OYM：获取每个只读点

                if (!(pReadPoint->fixedIndex == index || pReadPoint->isVirtual))//OYM：不是fix点
                {
                    transform.position = pReadWritePoint->position;
                }


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



