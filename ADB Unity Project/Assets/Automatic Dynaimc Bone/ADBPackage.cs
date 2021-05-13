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

        private ADBRunTimeJobsTable.ColliderGetTransform colliderGet;
        private ADBRunTimeJobsTable.PointGetTransform pointGet;
        private ADBRunTimeJobsTable.PointUpdate pointUpdate;
        private ADBRunTimeJobsTable.ColliderUpdate colliderUpdate;
        private ADBRunTimeJobsTable.ConstraintUpdate[] constraintUpdates;
        private ADBRunTimeJobsTable.ConstraintUpdate constraintUpdates1;
        private ADBRunTimeJobsTable.ConstraintForceUpdateByPoint constraintForceUpdateByPoint;
        private ADBRunTimeJobsTable.JobPointToTransform pointToTransform;

        private NativeArray<ColliderRead> collidersReadList;
        private NativeMultiHashMap<int, ConstraintRead> ConstraintReadByPointIndex;
        private NativeArray<ColliderReadWrite> collidersReadWriteList;
        private TransformAccessArray colliderTransformsList;
        private List<ConstraintRead[]> m_constraintList;
        private List<PointRead> m_pointReadList;
        private List<PointReadWrite> m_pointReadWriteList;
        private NativeArray<ConstraintRead>[] constraintReadList;
        private NativeArray<ConstraintRead> constraintReadList1;
        private NativeArray<PointRead> pointReadList;
        private NativeArray<PointReadWrite> pointReadWriteList;
        // private NativeArray<PointReadWrite> pointReadWriteListCopy;
        private TransformAccessArray pointTransformsList;
        private NativeList<JobHandle> handleList = new NativeList<JobHandle>(8, Allocator.TempJob);

        public DataPackage()
        {
            Hjob = new JobHandle();
            m_constraintList = new List<ConstraintRead[]>();
            m_pointReadList = new List<PointRead>();
            m_pointReadWriteList = new List<PointReadWrite>();

            pointTransformsList = new TransformAccessArray(0);
            colliderTransformsList = new TransformAccessArray(0);

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
        /// <param name="isFuzzyCompute"></param>
        /// <returns></returns>
        internal bool SetRuntimeData(float deltaTime, float scale, ref int iteration, Vector3 addForce, ColliderCollisionType colliderCollisionType, bool isOptimize, bool detectAsync, bool isFuzzyCompute)
        {


            if (!Hjob.IsCompleted)
            {
                if (!detectAsync)
                {
                    iteration = Mathf.CeilToInt(iteration * 0.99f);
                    Debug.Log("检测到发生异步,自动修正迭代次数到 " + iteration);
                }

                return false;
            }
            handleList.Dispose();
            handleList = new NativeList<JobHandle>(8, Allocator.TempJob);

            //OYM：当我用ADBRunTimeJobsTable.returnHJob时候,任务会在我调用的时候被强制完成,当我用本地的Hjob的时候,任务会在异步进行
            //OYM:  注意,JH底层很可能也是单例

            //OYM:  针对迭代的补偿
            //OYM:  赋参
            float oneDivideIteration = 1.0f / iteration;
            constraintForceUpdateByPoint.oneDivideIteration = constraintUpdates1.oneDivideIteration = pointUpdate.oneDivideIteration = colliderGet.oneDivideIteration = pointGet.oneDivideIteration = oneDivideIteration;
            pointUpdate.deltaTime = deltaTime;
            pointUpdate.globalScale = scale;
            pointUpdate.isOptimize = isOptimize;
            pointUpdate.addForcePower = addForce;
            pointUpdate.isCollision = (colliderCollisionType == ColliderCollisionType.Both || colliderCollisionType == ColliderCollisionType.Point);
            
            //OYM:  上面这个是防迭代顺序错乱而设置强制顺序
            for (int i = 0; i < constraintUpdates.Length; i++)
            {
                constraintUpdates[i].oneDivideIteration= 1.0f / iteration;
                constraintUpdates[i].globalScale = scale;
                constraintUpdates[i].isCollision = (colliderCollisionType == ColliderCollisionType.Both || colliderCollisionType == ColliderCollisionType.Constraint);
            }
            //OYM:  下面就是随机顺序了
            constraintForceUpdateByPoint.globalScale = constraintUpdates1.globalScale = scale;
            constraintUpdates1.isCollision = (colliderCollisionType == ColliderCollisionType.Both || colliderCollisionType == ColliderCollisionType.Constraint); ;

            #region LifeCycle

#if ADB_DEBUG
            pointGet.TryExecute(pointTransformsList, Hjob);
#else
            Hjob = colliderGet.Schedule(colliderTransformsList);
            handleList.Add(Hjob);
            Hjob = pointGet.Schedule(pointTransformsList);
            handleList.Add(Hjob);
#endif

            for (int i = 0; i < iteration; i++)
            {
#if ADB_DEBUG
                colliderUpdate.TryExecute(collidersReadList.Length, batchLength, Hjob);

                pointUpdate.TryExecute(pointReadList.Length, batchLength, Hjob);
                for (int j0 = 0; j0 < constraintUpdates.Length; j0++)
                {
                    constraintUpdates[j0].TryExecute(constraintReadList[j0].Length, batchLength, Hjob);
                }
#else
                if (isFuzzyCompute)
                {
                    Hjob = colliderUpdate.Schedule(collidersReadList.Length, batchLength);
                    handleList.Add(Hjob); //OYM:防止未完成导致释放失败
                    Hjob = pointUpdate.Schedule(pointReadList.Length, batchLength);
                    handleList.Add(Hjob);

                    if (colliderCollisionType==ColliderCollisionType.Constraint|| colliderCollisionType == ColliderCollisionType.Both)
                    {
                        Hjob = constraintUpdates1.Schedule(constraintReadList1.Length, batchLength);
                    }
                    else
                    {
                        Hjob = constraintForceUpdateByPoint.Schedule(pointReadList.Length, batchLength);
                    }
                    handleList.Add(Hjob);
                    //
                }
                else
                {
                    Hjob = colliderUpdate.Schedule(collidersReadList.Length, batchLength, Hjob);
                    handleList.Add(Hjob);
                    Hjob = pointUpdate.Schedule(pointReadList.Length, batchLength, Hjob);
                    handleList.Add(Hjob);
                    if (colliderCollisionType == ColliderCollisionType.Constraint || colliderCollisionType == ColliderCollisionType.Both)
                    {
                        Hjob = constraintUpdates1.Schedule(constraintReadList1.Length, batchLength, Hjob);
                    }
                    else
                    {
                        Hjob = constraintForceUpdateByPoint.Schedule(pointReadList.Length, batchLength, Hjob);
                    }
                    handleList.Add(Hjob);
                }
#endif
            }

#if ADB_DEBUG
            pointToTransform.TryExecute(pointTransformsList, Hjob);
#else
            Hjob = pointToTransform.Schedule(pointTransformsList, Hjob);
            handleList.Add(Hjob);
#endif
            #endregion
            return true;
        }

        public void SetColliderPackage(ColliderRead[] collidersReadList, ColliderReadWrite[] collidersReadWriteList, Transform[] collidersTransList)
        {
            this.collidersReadList = new NativeArray<ColliderRead>(collidersReadList, Allocator.Persistent);
            this.collidersReadWriteList = new NativeArray<ColliderReadWrite>(collidersReadWriteList, Allocator.Persistent);
            colliderTransformsList.SetTransforms(collidersTransList);
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
                this.pointTransformsList.Add(pointTransformsList[i]);
            }
        }
        public void SetNativeArray()
        {
            //OYM:  创建各种实例
            pointReadList = new NativeArray<PointRead>(m_pointReadList.ToArray(), Allocator.Persistent);
            pointReadWriteList = new NativeArray<PointReadWrite>(m_pointReadWriteList.ToArray(), Allocator.Persistent);
            constraintReadList = new NativeArray<ConstraintRead>[m_constraintList.Count];

            List<ConstraintRead> constraintReadList1Target = new List<ConstraintRead>();
            ConstraintReadByPointIndex = new NativeMultiHashMap<int, ConstraintRead>(8, Allocator.Persistent);
            for (int i = 0; i < m_constraintList.Count; i++)
            {
                constraintReadList1Target.AddRange(m_constraintList[i]);
                constraintReadList[i] = new NativeArray<ConstraintRead>(m_constraintList[i], Allocator.Persistent);
                for (int j = 0; j < m_constraintList[i].Length; j++)
                {
                    ConstraintRead temp = m_constraintList[i][j];
                    ConstraintReadByPointIndex.Add(m_constraintList[i][j].indexA, temp);
                    //OYM:  考虑到只会读取indexB,及相对的点,这里要交换一下顺序,避免读取到自己
                    int exchange = temp.indexA;
                    temp.indexA = temp.indexB;
                    temp.indexB = exchange;
                    ConstraintReadByPointIndex.Add(m_constraintList[i][j].indexB, temp);
                }
            }
            constraintReadList1 = new NativeArray<ConstraintRead>(constraintReadList1Target.ToArray(), Allocator.Persistent);

            colliderGet = new ADBRunTimeJobsTable.ColliderGetTransform();
            pointGet = new ADBRunTimeJobsTable.PointGetTransform();
            pointUpdate = new ADBRunTimeJobsTable.PointUpdate();
            colliderUpdate = new ADBRunTimeJobsTable.ColliderUpdate();
            constraintUpdates = new ADBRunTimeJobsTable.ConstraintUpdate[m_constraintList.Count];
            constraintUpdates1 = new ADBRunTimeJobsTable.ConstraintUpdate();
            constraintForceUpdateByPoint = new ADBRunTimeJobsTable.ConstraintForceUpdateByPoint();
            pointToTransform = new ADBRunTimeJobsTable.JobPointToTransform();

            //OYM:  获取指针与赋值
            colliderGet.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            colliderGet.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();

            colliderUpdate.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            colliderUpdate.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();

            pointGet.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointGet.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();

            pointUpdate.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointUpdate.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
            pointUpdate.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            pointUpdate.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
            pointUpdate.colliderCount = collidersReadList.Length;

            for (int i = 0; i < constraintUpdates.Length; i++)
            {
                constraintUpdates[i].pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
                constraintUpdates[i].pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
                constraintUpdates[i].pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
                constraintUpdates[i].pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
                constraintUpdates[i].pConstraintsRead = (ConstraintRead*)constraintReadList[i].GetUnsafePtr();
                constraintUpdates[i].colliderCount = collidersReadList.Length;

            }
            constraintUpdates1.pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr();
            constraintUpdates1.pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr();
            constraintUpdates1.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            constraintUpdates1.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
            constraintUpdates1.pConstraintsRead = (ConstraintRead*)constraintReadList1.GetUnsafePtr();
            constraintUpdates1.colliderCount = collidersReadList.Length;

            constraintForceUpdateByPoint.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            constraintForceUpdateByPoint.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();
            constraintForceUpdateByPoint.constraintsRead = ConstraintReadByPointIndex;

            pointToTransform.pReadPoints = (PointRead*)pointReadList.GetUnsafePtr();
            pointToTransform.pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr();

        }
        /// <summary>
        /// 重置所有点
        /// </summary>
        public void restorePoint()
        {
            Hjob.Complete();
            ADBRunTimeJobsTable.InitiralizePoint initialpoint = new ADBRunTimeJobsTable.InitiralizePoint
            {
                pReadPoints = (PointRead*)pointReadList.GetUnsafePtr(),
                pReadWritePoints = (PointReadWrite*)pointReadWriteList.GetUnsafePtr(),
            };
            Hjob = initialpoint.Schedule(pointTransformsList);

            ADBRunTimeJobsTable.InitiralizeCollider initialCollider = new ADBRunTimeJobsTable.InitiralizeCollider
            {
                pReadColliders = (ColliderRead*)collidersReadList.GetUnsafePtr(),
                pReadWriteColliders = (ColliderReadWrite*)collidersReadWriteList.GetUnsafePtr()
            };
            Hjob = initialCollider.Schedule(colliderTransformsList);



        }
        /// <summary>
        /// 释放,如果为true,则重新加载数据
        /// 注意,该操作会释放大量GC
        /// </summary>
        /// <param name="isReset"></param>
        public void Dispose(bool isReset)
        {
            JobHandle.CompleteAll(handleList.AsArray());
            pointReadList.Dispose();
            pointReadWriteList.Dispose();
            pointTransformsList.Dispose();
            constraintReadList1.Dispose();
            ConstraintReadByPointIndex.Dispose();
            for (int i = 0; i < constraintReadList.Length; i++)
            {
                constraintReadList[i].Dispose();
            }
            if (isReset)
            {
                pointTransformsList = new TransformAccessArray(0);
                m_constraintList = new List<ConstraintRead[]>();
                m_pointReadList = new List<PointRead>();
                m_pointReadWriteList = new List<PointReadWrite>();
            }
            else
            {
                handleList.Dispose();
                collidersReadList.Dispose();
                collidersReadWriteList.Dispose();
                colliderTransformsList.Dispose();
            }
        }
    }
}
