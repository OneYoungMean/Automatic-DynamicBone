//#define ADB_DEBUG

using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;
using Unity.Jobs;
using System;

namespace ADBRuntime
{
    using Internal;
    using Mono;
    using static ADBRuntime.Internal.ADBRunTimeJobsTable;

    public unsafe class DataPackage
    {
        static int batchLength = 64;
        private JobHandle Hjob;
        private ADBRunTimeJobsTable ADBRunTimeJobsTable;

        private ADBRunTimeJobsTable.ColliderGetAABB colliderCalcAABB;
        private ADBRunTimeJobsTable.PointGetTransform pointGet;
        private ADBRunTimeJobsTable.PointUpdate pointUpdate;
        private ADBRunTimeJobsTable.ConstraintUpdate constraintUpdates;
        private ADBRunTimeJobsTable.ConstraintForceUpdateByPoint constraintForceUpdateByPoint;
        private ADBRunTimeJobsTable.JobPointToTransform pointToTransform;

        private NativeArray<ColliderRead> collidersReadNativeArray;
        private NativeMultiHashMap<int, ConstraintRead> ConstraintReadMultiHashMap;//OYM:对应的粒子的index 与ConstraintRead

        private List<ConstraintRead[]> m_constraintList;
        private List<PointRead> m_pointReadList;
        private List<PointReadWrite> m_pointReadWriteList;
        private NativeArray<ConstraintRead> constraintReadList;
        private NativeArray<PointRead> pointReadNativeArray;
        private NativeArray<PointReadWrite> pointReadWriteListNativeArray;
        // private NativeArray<PointReadWrite> pointReadWriteListCopy;
        private TransformAccessArray pointTransformsAccessArray;
        private List<Transform> pointTransformsListTest = new List<Transform>();
        private bool isInitialize = false;


        private NativeList<JobHandle> CompleteHandleArray;
        public DataPackage()
        {
            Hjob = new JobHandle();
            m_constraintList = new List<ConstraintRead[]>();
            m_pointReadList = new List<PointRead>();
            m_pointReadWriteList = new List<PointReadWrite>();
            CompleteHandleArray = new NativeList<JobHandle>(8,Allocator.Persistent);
            pointTransformsAccessArray = new TransformAccessArray(0);
        }
        /// <summary>
        /// 物理接口,如果要更新物理数据,需要在里面填入相关的信息
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <param name="scale"></param>
        /// <param name="iteration"></param>
        /// <param name="addForce"></param>
        /// <param name="colliderCollisionType"></param>
        /// <param name="isOptimize"></param>
        /// <param name="detectAsync"></param>
        /// <param name="isParallel"></param>
        /// <returns></returns>
        internal bool SetRuntimeData(float deltaTime, float scale, ref int iteration, Vector3 addForce, ColliderCollisionType colliderCollisionType, bool isOptimize, bool isRunAsync, bool isParallel)
        {
            if (!isInitialize||!Hjob.IsCompleted)
            {
                return false;
            }
            //OYM:优先更新坐标 
            CompleteHandleArray.Clear();
            //OYM：当我用ADBRunTimeJobsTable.returnHJob时候,任务会在我调用的时候被强制完成,当我用本地的Hjob的时候,任务会在异步进行
            //OYM:  注意,JH底层很可能也是单例
            //OYM:  赋参
            float oneDivideIteration = 1.0f / iteration;
            constraintForceUpdateByPoint.oneDivideIteration = constraintUpdates.oneDivideIteration = pointUpdate.oneDivideIteration = colliderCalcAABB.oneDivideIteration = pointGet.oneDivideIteration = oneDivideIteration;
            pointUpdate.deltaTime = deltaTime;
            colliderCalcAABB.globalScale= pointUpdate.globalScale = scale;
            pointUpdate.isOptimize = isOptimize;
            pointUpdate.addForcePower = addForce;
            pointUpdate.isCollision = (colliderCollisionType == ColliderCollisionType.Both || colliderCollisionType == ColliderCollisionType.Point);
            
            //OYM:  下面就是随机顺序了
            constraintForceUpdateByPoint.globalScale = constraintUpdates.globalScale = scale;
            constraintUpdates.isCollision = (colliderCollisionType == ColliderCollisionType.Both || colliderCollisionType == ColliderCollisionType.Constraint); ;

            #region LifeCycle
            //OYM:Collider
            CompleteHandleArray.Add(colliderCalcAABB.Schedule(collidersReadNativeArray.Length, batchLength));
            //OYM:pointGet
            CompleteHandleArray.Add(pointGet.Schedule(pointTransformsAccessArray));


            for (int i = 0; i < iteration; i++)
            {
                if (!isRunAsync)//OYM:单线程
                {
                    pointUpdate.Run(pointReadNativeArray.Length);

                    if (colliderCollisionType == ColliderCollisionType.Constraint || colliderCollisionType == ColliderCollisionType.Both)
                    {
                        constraintUpdates.Run(constraintReadList.Length);
                    }
                    else
                    {
                        constraintForceUpdateByPoint.Run(pointReadNativeArray.Length);
                    }
                }

                else if (!isParallel) //OYM:多线程异步(中等)
                {
                    Hjob = pointUpdate.Schedule(pointReadNativeArray.Length, batchLength, Hjob);
                    if (colliderCollisionType == ColliderCollisionType.Constraint || colliderCollisionType == ColliderCollisionType.Both)
                    {
                        Hjob = constraintUpdates.Schedule(constraintReadList.Length, batchLength, Hjob);
                    }
                    else
                    {
                        Hjob = constraintForceUpdateByPoint.Schedule(pointReadNativeArray.Length, batchLength, Hjob);
                    }
                }
                else //OYM:多线程并行(最快)
                {
                    CompleteHandleArray.Add(pointUpdate.Schedule(pointReadNativeArray.Length, batchLength));

                    if (colliderCollisionType == ColliderCollisionType.Constraint || colliderCollisionType == ColliderCollisionType.Both)
                    {
                        CompleteHandleArray.Add(constraintUpdates.Schedule(constraintReadList.Length, batchLength));
                    }
                    else
                    {
                        CompleteHandleArray.Add(constraintForceUpdateByPoint.Schedule(pointReadNativeArray.Length, batchLength));
                    }

                }
            }
            
            Hjob = pointToTransform.Schedule(pointTransformsAccessArray, Hjob);
            CompleteHandleArray.Add(Hjob);
            #endregion
            return true;
        }

        internal void SetRuntimeCollider(ColliderRead[] collidersReadList)
        {
            if (collidersReadNativeArray.IsCreated)
            {
                collidersReadNativeArray.Dispose();
            }
            
            collidersReadNativeArray = new NativeArray<ColliderRead>(collidersReadList, Allocator.Persistent);
            colliderCalcAABB.pReadColliders = (ColliderRead*)collidersReadNativeArray.GetUnsafePtr();

            pointUpdate.pReadColliders = (ColliderRead*)collidersReadNativeArray.GetUnsafePtr();
            pointUpdate.colliderCount = collidersReadNativeArray.Length;

            constraintUpdates.pReadColliders = (ColliderRead*)collidersReadNativeArray.GetUnsafePtr();
            constraintUpdates.colliderCount = collidersReadNativeArray.Length;
        }
        public void SetPointAndConstraintpackage(ConstraintRead[][] constraintList, PointRead[] pointReadList, PointReadWrite[] pointReadWriteList, Transform[] pointTransformsList)
        {
            int offset = m_pointReadList.Count;

            for (int i = 0; i < pointReadList.Length; i++)
            {
                if (pointReadList[i].parentIndex != -1)
                {
                    pointReadList[i].parentIndex += offset;
                }
                if (pointReadList[i].childFirstIndex != -1)
                {
                    pointReadList[i].childFirstIndex += offset;
                    pointReadList[i].childLastIndex += offset;
                }
                pointReadList[i].fixedIndex += offset;
            }

            for (int i = 0; i < constraintList.Length; i++)
            {
                for (int j0 = 0; j0 < constraintList[i].Length; j0++)
                {
                    constraintList[i][j0].indexA += offset;
                    constraintList[i][j0].indexB += offset;
                }
            }
            this.m_constraintList.AddRange(constraintList);
            this.m_pointReadList.AddRange(pointReadList);
            this.m_pointReadWriteList.AddRange(pointReadWriteList);
            for (int i = 0; i < pointTransformsList.Length; i++)
            {
                this.pointTransformsAccessArray.Add(pointTransformsList[i]);
                pointTransformsListTest.Add(pointTransformsList[i]);
            }
        }
        public void SetNativeArray()
        {
            //OYM:  创建各种实例
            pointReadNativeArray = new NativeArray<PointRead>(m_pointReadList.ToArray(), Allocator.Persistent);
            pointReadWriteListNativeArray = new NativeArray<PointReadWrite>(m_pointReadWriteList.ToArray(), Allocator.Persistent);


            List<ConstraintRead> constraintReadListTarget = new List<ConstraintRead>();
            ConstraintReadMultiHashMap = new NativeMultiHashMap<int, ConstraintRead>(8, Allocator.Persistent);
            for (int i = 0; i < m_constraintList.Count; i++)
            {
                constraintReadListTarget.AddRange(m_constraintList[i]);
                for (int j = 0; j < m_constraintList[i].Length; j++)
                {
                    ConstraintRead temp = m_constraintList[i][j];
                    ConstraintReadMultiHashMap.Add(m_constraintList[i][j].indexA, temp);
                    //OYM:  考虑到只会读取indexB,及相对的点,这里要交换一下顺序,避免读取到自己
                    int exchange = temp.indexA;
                    temp.indexA = temp.indexB;
                    temp.indexB = exchange;
                    ConstraintReadMultiHashMap.Add(m_constraintList[i][j].indexB, temp);
                }
            }
            constraintReadList = new NativeArray<ConstraintRead>(constraintReadListTarget.ToArray(), Allocator.Persistent);

            colliderCalcAABB = new ADBRunTimeJobsTable.ColliderGetAABB();
            pointGet = new ADBRunTimeJobsTable.PointGetTransform();
            pointUpdate = new ADBRunTimeJobsTable.PointUpdate();
            constraintUpdates = new ADBRunTimeJobsTable.ConstraintUpdate();
            constraintForceUpdateByPoint = new ADBRunTimeJobsTable.ConstraintForceUpdateByPoint();
            pointToTransform = new ADBRunTimeJobsTable.JobPointToTransform();

            pointGet.pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr();
            pointGet.pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr();

            pointUpdate.pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr();
            pointUpdate.pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr();

            constraintUpdates.pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr();
            constraintUpdates.pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr();
            constraintUpdates.pConstraintsRead = (ConstraintRead*)constraintReadList.GetUnsafePtr();

            constraintForceUpdateByPoint.pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr();
            constraintForceUpdateByPoint.pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr();
            constraintForceUpdateByPoint.constraintsRead = ConstraintReadMultiHashMap;

            pointToTransform.pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr();
            pointToTransform.pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr();
            isInitialize = true;
        }
        /// <summary>
        /// 重置所有点
        /// </summary>
        public void restorePoint()
        {
            JobHandle.CompleteAll(CompleteHandleArray.AsDeferredJobArray());

            if (pointTransformsAccessArray.isCreated) //OYM:优先
            {
                ADBRunTimeJobsTable.InitiralizePoint1 initialpoint = new ADBRunTimeJobsTable.InitiralizePoint1
                {
                    pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr(),
                    pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr(),
                };
                Hjob = initialpoint.Schedule(pointTransformsAccessArray, Hjob);

                ADBRunTimeJobsTable.InitiralizePoint2 initialpoint2 = new ADBRunTimeJobsTable.InitiralizePoint2
                {
                    pReadPoints = (PointRead*)pointReadNativeArray.GetUnsafePtr(),
                    pReadWritePoints = (PointReadWrite*)pointReadWriteListNativeArray.GetUnsafePtr(),
                };
                Hjob = initialpoint2.Schedule(pointTransformsAccessArray, Hjob);
            }
            Hjob.Complete();
        }
        /// <summary>
        /// 释放,如果为true,则重新加载数据
        /// 注意,该操作会释放大量GC
        /// </summary>
        /// <param name="isReset"></param>
        public void Dispose(bool isReset)
        {
            isInitialize = false;
            JobHandle.CompleteAll(CompleteHandleArray.AsDeferredJobArray());


            pointReadNativeArray.Dispose();
            pointReadWriteListNativeArray.Dispose();
            pointTransformsAccessArray.Dispose();
            if (collidersReadNativeArray.IsCreated)
            {
                collidersReadNativeArray.Dispose();
            }
            constraintReadList.Dispose();

            if (isReset)
            {
                ConstraintReadMultiHashMap.Clear();
                pointTransformsAccessArray = new TransformAccessArray(0);
                m_constraintList.Clear();
                m_pointReadList.Clear();
                m_pointReadWriteList.Clear();
            }
            else
            {
                ConstraintReadMultiHashMap.Dispose();
            }

        }
    }
}
