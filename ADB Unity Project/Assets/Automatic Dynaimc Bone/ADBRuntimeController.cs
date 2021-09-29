using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.ComponentModel;
using UnityEngine.SceneManagement;

namespace ADBRuntime.Mono
{

    public enum ColliderCollisionType
    {
        /// <summary
        /// Accurate but slow
        /// </summary>
        Both = 1,
        /// <summary>
        ///  fast but no radius
        /// </summary>

        Constraint = 2,
        /// <summary>
        /// little faster than constraint
        /// </summary>

        Point = 3,
        /// <summary>
        /// 没有碰撞(最快)
        /// </summary>

        Null = 4
    }

    public enum UpdateMode
    {
        Update=1,
        FixedUpdate=2,
        LateUpdate=3,
    }
    [DisallowMultipleComponent]
    public class ADBRuntimeController : MonoBehaviour
    {
        [Serializable]
        private class ConnectWithADBSettingAndADBRuntimePoint
        {
            public ADBSetting setting;
            public Transform[] points;
            public ConnectWithADBSettingAndADBRuntimePoint(ADBSetting setting, Transform[] points)
            {
                this.setting = setting;
                this.points = points;
            }
        }

        [SerializeField]
        private ConnectWithADBSettingAndADBRuntimePoint[] inspectorPointList;
        [SerializeField]
        public bool isGenerateColliderAutomaitc=false;
        [SerializeField]
        public bool isGenerateByAllPoint = true;
        [SerializeField]
        public bool isGenerateFinger = false;
        [SerializeField]
        public bool isGenerateColliderOpenTrigger=true;
        [SerializeField]
        public float bufferTime=1f;
        [SerializeField]
        public int iteration=4;
        [SerializeField]
        public float windForceScale=0f;
        public bool isDebug;
        public bool isOptimize = false;
        public bool isResetPoint;
        [SerializeField]
        public ADBGlobalSetting settings;

        public ColliderCollisionType colliderCollisionType = ColliderCollisionType.Constraint;
        [SerializeField]
        public List<string> generateKeyWordWhiteList = new List<string> { "skirt" };// "hair", "tail", 
        [SerializeField]
        public List<string> generateKeyWordBlackList = new List<string> { "ik" };
        [SerializeField]
        public List<Transform> blackListOfGenerateTransform=new List<Transform>();
        [SerializeField]
        public bool isAdvance;
        public Transform generateTransform;
        public List<Transform> allPointTrans;

        [SerializeField]
        public bool isRunAsync = false;
        [SerializeField]
        public bool isParallel=false;
        public ADBAvatarReader colliderControll;

        [SerializeField]
        public List<ADBColliderReader> overlapsColliderList;
        [SerializeField]
        public List<ADBColliderReader> generateColliderList;
        [SerializeField]
        public MinMaxAABB OverlapBox;

        public UpdateMode updateMode=UpdateMode.Update;
        private ADBConstraintReadAndPointControll[] jointAndPointControlls;
        private DataPackage dataPackage;
        private bool isInitialize = false;

        private float deltaTime=0;
        private float initializeScale;
        private float scale;
        private Vector3 addForce;
        public bool isAsync;
        private Collider[] colliders;


        //OYM:Advanced
        private const int maxColliderCount = 512;
     

        private void Start()//OYM：滚回来自己来趟这趟屎山
        {
            if (!isInitialize)
            {
                ParentCheck();
                ListCheck();
                InitializePoint();
                initializeCollider();
            }

            if (jointAndPointControlls == null) return;

            dataPackage = new DataPackage();
            if (Application.isPlaying)
            {
                
                isInitialize = true;
                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    jointAndPointControlls[i].GetData( dataPackage);//OYM：在这里对各种joint和point进行分类与编号
                }
                //colliderControll.GetData(ref dataPackage);
                dataPackage.SetNativeArray();
                initializeScale = generateTransform.lossyScale.x;
                isInitialize = true;
            }
            bufferTime = bufferTime < 0.017f ? 0.017f : bufferTime;
            isResetPoint = true;
        }
        private void Update()
        {
            if (updateMode==UpdateMode.Update)
            {
                Run(Time.deltaTime);
            }
        }
        private void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
            {
                Run(Time.fixedDeltaTime);
            }

        }

        private void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate)
            {
                Run(Time.deltaTime);
            }
        }
        private void OnDisable()
        {
            if (dataPackage != null)
            {
                RestoreRuntimePoint();
            }
        }
        private void OnDestroy()
        {
            if (dataPackage != null)
            {
                RestoreRuntimePoint();
                dataPackage.Dispose(false);
            }

        }

        private void Run(float inputDeltaTime)
        {
            if (jointAndPointControlls == null) return;
            deltaTime = Mathf.Lerp(deltaTime, inputDeltaTime, 1 / (bufferTime * 60));

            if (isResetPoint)
            {
                isResetPoint = false;
                RestoreRuntimePoint();
                return;
            }

            //  deltaTime =Mathf.Lerp(deltaTime, Mathf.Min(Time.deltaTime,0.0166f),0.1f);//OYM：用time.deltaTime并不理想,或许是我笔记本太烂的缘故?

            scale = generateTransform.lossyScale.x;
            addForce += ADBWindZone.getaddForceForce(generateTransform.position) * windForceScale * deltaTime;
            UpdateOverlapsCollider();
            UpdateDataPakage();

            //OYM：理论上你多执行几次UpdateDataPakage()也没啥关系
        }
        private void UpdateDataPakage()
        {
            bool isSuccessfulRun = dataPackage.SetRuntimeData(deltaTime,
                                                              scale / initializeScale,
                                                              ref iteration,
                                                              addForce,
                                                              colliderCollisionType,
                                                              isOptimize,
                                                              isRunAsync,
                                                              isParallel
                                                              );

            if (isSuccessfulRun)
            {
                addForce = Vector3.zero;
            }
        }

        public void UpdateOverlapsCollider()
        {
            overlapsColliderList.Clear();
            if (colliders==null||colliders.Length != maxColliderCount)
            {
                colliders = new Collider[maxColliderCount];
            }

            if (!Application.isPlaying)
            {
                overlapsColliderList.AddRange(gameObject.GetComponents<ADBColliderReader>());
                overlapsColliderList.AddRange(gameObject.GetComponentsInChildren<ADBColliderReader>());
                return;
            }

            int count = Physics.OverlapBoxNonAlloc(
                                generateTransform.position+ generateTransform.rotation* (Vector3)OverlapBox.Center,
                                scale / initializeScale * OverlapBox.HalfExtents,
                                colliders, Quaternion.identity, int.MaxValue, QueryTriggerInteraction.Collide
                                );
            ColliderRead[] colliderReads =new ColliderRead[count];
            for (int i = 0; i < count; i++)
            {
                if (ADBColliderReader.ColliderTokenDic.TryGetValue(colliders[i].GetInstanceID(),out ADBColliderReader colliderToken))
                {
                    if (colliderToken.IsOwner(this))
                    {
                        colliderReads[overlapsColliderList.Count] = colliderToken.runtimeCollider.colliderRead;
                        overlapsColliderList.Add(colliderToken);
                    }
                }
            }
            

            if (isInitialize)
            {
                Array.Resize(ref colliderReads, overlapsColliderList.Count);
                dataPackage.SetRuntimeCollider(colliderReads);
            }
            
        }
        public void Reset()
        {
            if (Application.isPlaying)
            {
                try
                {
                    RestoreRuntimePoint();
                    InitializePoint();
                    dataPackage.Dispose(true);
                    for (int i = 0; i < jointAndPointControlls.Length; i++)
                    {
                        jointAndPointControlls[i].GetData(dataPackage);//OYM：在这里对各种joint和point进行分类与编号
                    }
                    dataPackage.SetNativeArray();
                    isResetPoint = true;
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        public void SetPhysicData(ADBAvatarReader colliderControll, ADBConstraintReadAndPointControll[] jointAndPointControlls)
        {
            this.colliderControll = colliderControll;
            this.jointAndPointControlls = jointAndPointControlls;
        }
        public void RestoreRuntimePoint()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("Use it On Runtime!");
                return;
            }
            dataPackage.restorePoint();
        }
        public void ParentCheck()
        {
            var parentRuntimeController = generateTransform.parent?.GetComponentInParent<ADBRuntimeController>();
            if (parentRuntimeController != null)
            {
                Debug.Log(generateTransform.name + " find the parent has  ADB Runtime Controller in" + parentRuntimeController.generateTransform.name + ", if it is not you want ,check it ");
            }
        }
        public bool PointCheck()
        {
            return jointAndPointControlls?.Length > 0;
        }
        public void ListCheck()
        {//OYM：一个简单的防报错和把关键词tolower的方法

            if (generateKeyWordWhiteList==null||generateKeyWordWhiteList.Count == 0)
            {
                generateKeyWordWhiteList = settings.defaultKeyWord;
                if (generateKeyWordWhiteList == null)
                {
                    Debug.Log("The white key is null!Check the RuntimeController or Global Setting!");
                    return;
                }
            }
            else
            {
                for (int i = 0; i < generateKeyWordWhiteList.Count; i++)
                {
                    if (generateKeyWordWhiteList[i] == "")
                    {
                        generateKeyWordWhiteList[i] = null;
                    }
                    else
                    {
                        generateKeyWordWhiteList[i] = generateKeyWordWhiteList[i].ToLower();
                    }
                }
            }
            for (int i = 0; i < generateKeyWordBlackList.Count; i++)
            {
                generateKeyWordBlackList[i] = generateKeyWordBlackList[i].ToLower();
            }
            if (!generateTransform)
            {
                generateTransform = transform;
            }
            if (settings == null)
            {
                settings = Resources.Load("Setting/ADBGlobalSettingFile") as ADBGlobalSetting;
            }
            if (overlapsColliderList == null)
            {
                overlapsColliderList = new List<ADBColliderReader>();
            }
            if (colliders==null)
            {
                colliders = new Collider[maxColliderCount];
            }
            
        }
        public void InitializePoint()
        {
            jointAndPointControlls = ADBConstraintReadAndPointControll.GetJointAndPointControllList(generateTransform, generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform,settings);//OYM：在这里搜索所有的节点和杆件的controll

            if (jointAndPointControlls != null)
            {
                allPointTrans = new List<Transform>();

                inspectorPointList = new ConnectWithADBSettingAndADBRuntimePoint[jointAndPointControlls.Length];

                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    jointAndPointControlls[i].Initialize();//OYM：在这里对各种joint和point进行分类与编号
                    List<Transform> transformArray = new List<Transform>();
                    for (int j = 0; j < jointAndPointControlls[i].allNodeList.Count; j++)
                    {
                        transformArray.Add(jointAndPointControlls[i].allNodeList[j].trans);
                    }
                    inspectorPointList[i] = new ConnectWithADBSettingAndADBRuntimePoint(jointAndPointControlls[i].aDBSetting, transformArray.ToArray());
                    allPointTrans.AddRange(transformArray);
                }
            }
            else
            {
                Debug.Log( "no point found , check the white key word");
            }
        }
        public void initializeCollider()
        {
            if (colliderControll==null)
            {
                colliderControll = new ADBAvatarReader(this);//OYM：在这里生成collider和AABB;
            }

            OverlapBox = colliderControll.CaculateAABB();//OYM:生成AABB

            if (!isGenerateColliderAutomaitc)
            {
                return;
            }

            if (generateColliderList != null&& generateColliderList.Count>0)
            {
                Debug.LogWarning("Pleace delete old generate collider before you want to generate new!");
                return;
            }

            if (!PointCheck())
            {
                ListCheck();
                InitializePoint();
            }

            List<ADBRuntimePoint> pointList = new List<ADBRuntimePoint>();

            if (jointAndPointControlls == null || jointAndPointControlls.Length == 0)
            {
                Debug.LogWarning("Lost point data.try to use <generate Point> buttom to generate accurate collider!");
            }
            else
            {
                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    if (isGenerateByAllPoint)
                    {
                        pointList.AddRange(jointAndPointControlls[i].allNodeList);
                    }
                    else
                    {
                        pointList.AddRange(jointAndPointControlls[i].
                            fixedNodeList);
                    }
                }
            }
            generateColliderList= colliderControll.GenerateBodyCollidersData(pointList,isGenerateFinger,isGenerateColliderOpenTrigger);

            isGenerateColliderAutomaitc = false;
        }
        public void AddForce(Vector3 force)
        {
            addForce += force;
        }
        public bool GetConstraintByKey(string key, ConstraintType constraintType, ref ADBRuntimeConstraint[] returnConstraint)
        {
            List<ADBRuntimeConstraint> constraints = new List<ADBRuntimeConstraint>();
            bool isFind = false;
            for (int i = 0; i < jointAndPointControlls.Length; i++)
            {
                if (jointAndPointControlls[i].keyWord == key)
                {
                    constraints.AddRange( jointAndPointControlls[i].GetConstraint(constraintType));
                    isFind = true;
                }
            }
            returnConstraint = constraints.ToArray();
            return isFind;
        }

        private void OnDrawGizmos()
        {
            if (!isDebug && isActiveAndEnabled) return;

            if (jointAndPointControlls != null)
            {
                foreach (var controll in jointAndPointControlls)
                {
                    controll.OnDrawGizmos(colliderCollisionType);
                }
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(generateTransform.position + generateTransform.rotation * OverlapBox.Center, OverlapBox.Extents);
        }
    }
}