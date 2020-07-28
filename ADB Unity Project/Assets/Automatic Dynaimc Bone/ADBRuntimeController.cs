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
        [SerializeField]
        public bool isGenerateColliderAutomaitc=true;
        [SerializeField]
        public bool isGenerateByFixedPoint = true;
        [SerializeField]
        public float delayTime=0.0001f;
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
                initializePoint();
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
            delayTime = delayTime < 0.017f ? 0.017f : delayTime;
        }

        private void Update()
        {
            if (jointAndPointControlls == null) return;

            if (delayTime - Time.deltaTime > 0)
            {
                delayTime -= Time.deltaTime;

                return;
            }
            else if ((delayTime > 0 && delayTime - Time.deltaTime < 0) || isResetPoint)
            {
                delayTime -= Time.deltaTime;
                isResetPoint = false;
                RestorePoint();
                return;
            }

            deltaTime += 0.0166f;//OYM：用time.deltaTime并不理想,或许是我笔记本太烂的缘故?
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
                                                              iteration,
                                                              addForce,
                                                              colliderCollisionType,
                                                              isOptimize);
            if (isSuccessfulRun)
            {
                deltaTime = 0;
                addForce = Vector3.zero;
            }
        }
        public void Reset()
        {
            if (Application.isPlaying)
            {
                RestorePoint();
                initializePoint();
                dataPackage.Dispose(true);
                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    jointAndPointControlls[i].GetData(dataPackage);//OYM：在这里对各种joint和point进行分类与编号
                }
                dataPackage.SetNativeArray();
                delayTime = delayTime < 0.017f ? 0.017f : delayTime;
            }
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
        public void initializeList()
        {//OYM：一个简单的防报错和把关键词tolower的方法

            if (!(generateKeyWordWhiteList?.Count != 0))
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
                    generateKeyWordWhiteList[i] = generateKeyWordWhiteList[i].ToLower();
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
        public void initializePoint()
        {
            initializeList();
            jointAndPointControlls = ADBConstraintReadAndPointControll.GetJointAndPointControllList(generateTransform, generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform,settings);//OYM：在这里搜索所有的节点和杆件的controll
            allPointTrans = new List<Transform>();
            if (jointAndPointControlls != null)
            {

                for (int i = 0; i < jointAndPointControlls.Length; i++)
                {
                    jointAndPointControlls[i].Initialize();//OYM：在这里对各种joint和point进行分类与编号
                    for (int j0 = 0; j0 < jointAndPointControlls[i].allNodeList.Count; j0++)
                    {
                        allPointTrans.Add(jointAndPointControlls[i].allNodeList[j0].trans);
                    }
                }
            }
            else
            {
                Debug.Log( "no point found , check the white key word");
            }
        }
        public void initializeCollider()
        {
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
                        if (isGenerateByFixedPoint)
                        {
                            pointList.AddRange(jointAndPointControlls[i].fixedNodeList);
                        }
                        else
                        {
                            pointList.AddRange(jointAndPointControlls[i].allNodeList);
                        }
                    }
                }
            }
            colliderControll = new ADBRuntimeColliderControll(generateTransform.gameObject, pointList, isGenerateColliderAutomaitc,!(Application.isPlaying),out editorColliderList);//OYM：在这里获取collider
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