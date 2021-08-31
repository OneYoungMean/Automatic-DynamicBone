using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.ComponentModel;

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
        public float bufferTime=1f;
        [SerializeField]
        public int iteration=4;
        [SerializeField]
        public float windForceScale=0.5f;
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
        public List<ADBEditorCollider> editorColliderList;

        [SerializeField]
        public bool isDetectAsync = false;
        [SerializeField]
        public bool isFuzzyCompute=false;
        private ADBRuntimeColliderControll colliderControll;
        private ADBConstraintReadAndPointControll[] jointAndPointControlls;
        private DataPackage dataPackage;
        private bool isInitialize = false;

        private float deltaTime=0;
        private float initializeScale;
        private float scale;
        private Vector3 addForce;


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
                colliderControll.GetData(ref dataPackage);
                dataPackage.SetNativeArray();
                initializeScale = transform.lossyScale.x;
                isInitialize = true;
            }
            bufferTime = bufferTime < 0.017f ? 0.017f : bufferTime;
            isResetPoint = true;
        }

        private void FixedUpdate()
        {
            if (jointAndPointControlls == null) return;
             deltaTime = Mathf.Lerp(deltaTime,Time.deltaTime, 1 / (bufferTime * 60));
 
             if (isResetPoint)
            {
                isResetPoint = false;
                RestorePoint();
                return;
            }

            //  deltaTime =Mathf.Lerp(deltaTime, Mathf.Min(Time.deltaTime,0.0166f),0.1f);//OYM：用time.deltaTime并不理想,或许是我笔记本太烂的缘故?

            scale = transform.lossyScale.x;
            addForce += ADBWindZone.getaddForceForce(transform.position ) * windForceScale* deltaTime;

             UpdateDataPakage();

            //OYM：理论上你多执行几次UpdateDataPakage()也没啥关系

        }
        private void OnDisable()
        {
            if (dataPackage != null)
            {
                RestorePoint();
            }
        }
        private void OnDestroy()
        {
            if (dataPackage != null)
            {
                RestorePoint();
                dataPackage.Dispose(false);
            }

        }
        private void UpdateDataPakage()
        {
            bool isSuccessfulRun = dataPackage.SetRuntimeData(deltaTime,
                                                              scale / initializeScale,
                                                              ref iteration,
                                                              addForce,
                                                              colliderCollisionType,
                                                              isOptimize,
                                                              !isDetectAsync,
                                                              isFuzzyCompute
                                                              );

            if (isSuccessfulRun)
            {
                addForce = Vector3.zero;
            }
        }
        public void Reset()
        {
            if (Application.isPlaying)
            {
                RestorePoint();
                InitializePoint();
                dataPackage.Dispose(true);
                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    jointAndPointControlls[i].GetData(dataPackage);//OYM：在这里对各种joint和point进行分类与编号
                }
                dataPackage.SetNativeArray();
                isResetPoint = true;
            }
        }
        public void SetPhysicData(ADBRuntimeColliderControll colliderControll, ADBConstraintReadAndPointControll[] jointAndPointControlls)
        {
            this.colliderControll = colliderControll;
            this.jointAndPointControlls = jointAndPointControlls;
        }
        public void RestorePoint()
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
            var parentRuntimeController = transform.parent?.GetComponentInParent<ADBRuntimeController>();
            if (parentRuntimeController != null)
            {
                Debug.Log(transform.name + " find the parent has  ADB Runtime Controller in" + parentRuntimeController.transform.name + ", if it is not you want ,check it ");
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
        }
        public void InitializePoint()
        {

            jointAndPointControlls = ADBConstraintReadAndPointControll.GetJointAndPointControllList(generateTransform, generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform,settings);//OYM：在这里搜索所有的节点和杆件的controll

            if (jointAndPointControlls != null)
            {
                allPointTrans = new List<Transform>();
                /*
                 * 这段代码被我废弃掉了,作用是允许你修改所有节点中对应的设置,但是由于无法确切的知道用户到底要怎么改(比如改了Global又改这里),为了保持正常的工作流,这段代码不会被采纳.
                 * 如果你希望能够自由的更改你设置的面板,你可以启用它.
                if (inspectorPointList != null && inspectorPointList.Length == jointAndPointControlls.Length)//OYM：两者存在且相等
                { 
                    for (int i = 0; i < jointAndPointControlls.Length; i++)
                    {
                        if (jointAndPointControlls[i].aDBSetting != inspectorPointList[i].setting)
                        {
                            jointAndPointControlls[i].SetADBSetting ( inspectorPointList[i].setting);
                        }
                        jointAndPointControlls[i].Initialize();
                        for (int j = 0; j < jointAndPointControlls[i].allNodeList.Count; j++)
                        {
                            allPointTrans.Add(jointAndPointControlls[i].allNodeList[j].trans);
                        }
                    }
                }
                else
                {
                */
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
            if (!PointCheck())
            {
                ListCheck();
                InitializePoint();
            }
            List<ADBRuntimePoint> pointList = null;
            if (jointAndPointControlls == null || jointAndPointControlls.Length == 0)
            {
                Debug.Log("If you want to generate body Collider,try to use <generate Point> Buttom again !");
            }
            else
            {
                pointList = new List<ADBRuntimePoint>();
                if (isGenerateColliderAutomaitc)
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
            }
            colliderControll = new ADBRuntimeColliderControll(generateTransform.gameObject, pointList, isGenerateColliderAutomaitc,!(Application.isPlaying),isGenerateFinger,out editorColliderList);//OYM：在这里获取collider
            for (int i = 0; i < editorColliderList.Count; i++)
            {
                editorColliderList[i].Refresh();
            }
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
            if (!isDebug) return;

            if (jointAndPointControlls != null)
            {
                foreach (var controll in jointAndPointControlls)
                {
                    controll.OnDrawGizmos(colliderCollisionType);
                }
            }


            if (colliderControll != null && Application.isPlaying)
            {
                Gizmos.color = Color.red;
                colliderControll.OnDrawGizmos();
            }
        }
    }
}