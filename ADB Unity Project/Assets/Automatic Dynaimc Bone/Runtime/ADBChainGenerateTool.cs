using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ADBRuntime.Mono.Tool
{
    public enum ChainGeneratorMode
    {
        Select,
        DynamicBone,
        ADBChain,
        Clear,


    }

    [SerializeField]
    public class SelectHelper
    {
        public enum SelectType
        {

            ClothBone=0,
            InvalidBone = 1,
            SelectBone =2,
            SelectFixedBone = 3,
            GenerateBone=4,
            GenerateFixedBone=5,
            OtherBone = 6,
        }

        public bool isOpen { get => 
                selectType == SelectType.SelectBone ||
                selectType == SelectType.SelectFixedBone ||
                selectType == SelectType.GenerateBone ||
                selectType == SelectType.GenerateFixedBone 
                ; }

        public SelectType selectType;
        public Transform parent;
        public Transform target;
        public List<Transform> child = new List<Transform>();
        public Color GetColor()
        {
            switch (selectType)
            {
                case SelectType.InvalidBone:
                    return Color.gray;
                case SelectType.ClothBone:
                    return Color.white;
                case SelectType.SelectBone:
                    return Color.Lerp(Color.white, Color.green, 0.5f);
                case SelectType.SelectFixedBone:
                    return Color.Lerp(Color.white, Color.red, 0.5f);
                case SelectType.GenerateBone:
                    return Color.green;
                case SelectType.GenerateFixedBone:
                    return Color.red;
                case SelectType.OtherBone:
                    return Color.Lerp(Color.gray, Color.yellow, 0.5f);
                default:
                    return Color.black;

            }
        }
        public float GetScale()
        {
            switch (selectType)
            {
                case SelectType.InvalidBone:
                    return 1;
                case SelectType.ClothBone:
                    return 1.1f;
                case SelectType.SelectBone:
                    return 1;
                case SelectType.SelectFixedBone:
                    return 1;
                case SelectType.GenerateBone:
                    return 0.9f;
                case SelectType.GenerateFixedBone:
                    return 0.9f;
                case SelectType.OtherBone:
                    return 1;
                default:
                    return 1;
            }
        }
    }

    /// <summary>
    /// Physics bone's generate tool
    /// </summary>
    [RequireComponent(typeof(ADBRuntimeController))]
    public class ADBChainGenerateTool : MonoBehaviour
    {
        #region  Field&Property
        [SerializeField]
        public List<string> generateKeyWordWhiteList = new List<string> { };// "hair", "tail", 
        [SerializeField]
        public List<string> generateKeyWordBlackList = new List<string> { "ik", "mesh" };
        [SerializeField]
        public List<Transform> blackListOfGenerateTransform = new List<Transform>();
        [SerializeField]
        public List<Transform> generateTransformList = new List<Transform>() { };
        [SerializeField]
        public ADBSettingLinker linker;
        [SerializeField]
        public ADBPhysicsSetting setting;
        [SerializeField]
        public List<ADBChainProcessor> allChain = new List<ADBChainProcessor>();
        [SerializeField]
        public bool isGenerateOnStart;
        [SerializeField]
        public ChainGeneratorMode generatorMode;
        [SerializeField]
        public bool isOpenMonitor;
        [SerializeField]
        public SelectHelper[] selectHelpers;

        private Dictionary<Transform, SelectHelper> selectDictionary = new Dictionary<Transform, SelectHelper>();
        private static List<string> everyKey = new List<string>() { "" };

        #endregion

        #region UnityFunc

        private void OnEnable()
        {
            ListCheck();
        }

        #endregion

        #region LocalFunc

        /// <summary>
        /// Check value
        /// </summary>
        private void ListCheck()
        {
            if (allChain == null)
            {
                allChain = new List<ADBChainProcessor>();
            }

            if (generatorMode != ChainGeneratorMode.ADBChain)
            {
                return;
            }

            if (generateKeyWordWhiteList == null || generateKeyWordWhiteList.Count == 0)
            {
                generateKeyWordWhiteList = linker?.AllKeyWord;
                if (generateKeyWordWhiteList == null)
                {
                    Debug.Log("The white key is null!Check the ADBChainGenerateTool or Value Setting!");
                    return;
                }
            }
            else
            {
                for (int i = 0; i < generateKeyWordWhiteList.Count; i++)
                {
                    if (!string.IsNullOrEmpty(generateKeyWordWhiteList[i]))
                    {
                        generateKeyWordWhiteList[i] = generateKeyWordWhiteList[i].ToLower();
                    }
                    else
                    {
                        generateKeyWordWhiteList[i] = null;
                    }
                }
            }
            for (int i = 0; i < generateKeyWordBlackList.Count; i++)
            {
                generateKeyWordBlackList[i] = generateKeyWordBlackList[i].ToLower();
            }
            if (linker == null)
            {
                linker = Resources.Load("Setting/ADBDefaultSettingLinker") as ADBSettingLinker;
            }
        }
        /*        public void RefreshBoneChain()
                {
                    allChain = GetChainsInChildren();
                }*/
        /// <summary>
        /// Clear generate bone chain
        /// </summary>
        public void ClearBoneChain()
        {
            for (int i = 0; i < allChain.Count; i++)
            {
                var targetChain = allChain[i];
                for (int j0 = 0; j0 < targetChain.allPointList.Count; j0++)
                {
                    if (targetChain.allPointList[j0]!=null)
                    {
                        DestroyImmediate(targetChain.allPointList[j0]);
                    }
                }
                DestroyImmediate(targetChain);
            }

            if (gameObject.TryGetComponent<ADBRuntimeController>(out ADBRuntimeController target))
            {
                target.ListCheck();
            }
            allChain.Clear();
        }
        public string GetPointCount()
        {
            int count = 0;
            for (int i = 0; i < allChain?.Count; i++)
            {
                if (allChain[i].allPointList != null)
                {
                    count += allChain[i].allPointList.Count;
                }
            }
            return count.ToString();
        }
        /// <summary>
        /// Initialize
        /// </summary>
        public void InitializeChain()
        {
            ListCheck();
            var tempLinker = ScriptableObject.CreateInstance<ADBSettingLinker>();
            switch (generatorMode)
            {
                case ChainGeneratorMode.DynamicBone:

                    tempLinker.settings = new List<KeyWordSetting>() {
                        new KeyWordSetting()
                        {
                            setting=setting,
                            keyWord =everyKey
                        }
                    };
                    if (generateTransformList.Count == 0)
                    {
                        generateTransformList.Add(transform);
                    }
                    for (int i = 0; i < generateTransformList.Count; i++)
                    {
                        if (generateTransformList[i] == null || (!generateTransformList[i].gameObject.GetComponentsInParent<Transform>().Contains(transform)))
                        {
                            continue;
                        }
                        GenerateBoneChainImporter(generateTransformList[i], everyKey, new List<string>(), new List<Transform>(), tempLinker, ref allChain);
                    }
                    Debug.Log("Create " + setting.name + " DynamicBone " + allChain.Count + " Chain Successful;");
                    break;
                case ChainGeneratorMode.ADBChain:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GenerateBoneChainImporter(transform.GetChild(i), generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform, linker, ref allChain);
                    }
                    Debug.Log("Create " + linker.name + " DynamicBone " + allChain.Count + " Chain Successful;");
                    break;
                case ChainGeneratorMode.Select:
                    tempLinker.settings = new List<KeyWordSetting>() {
                        new KeyWordSetting()
                        {
                            setting=setting,
                            keyWord =everyKey,
                        }
                    };
                    var blackList = new List<Transform>();
                    foreach (var item in selectDictionary)
                    {
                        if (!item.Value.isOpen)
                        {
                            blackList.Add(item.Key);
                        }
                    }
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GenerateBoneChainImporter(transform.GetChild(i), everyKey, new List<string>(), blackList, tempLinker, ref allChain);
                    }
                    UpdateSelectDictionary(true);
                    Debug.Log("Create " + setting.name + " DynamicBone " + allChain.Count + " Chain Successful;");
                    break;
                default:
                    break;
            }
            //RefreshBoneChain();


        }
        #endregion
        #region Static Generate Func
        public static void ClearBoneChain(Transform transform)
        {
            var boneChainsArray = transform.gameObject.GetComponentsInChildren<ADBChainProcessor>();
            for (int i = 0; i < boneChainsArray.Length; i++)
            {
                Component.DestroyImmediate(boneChainsArray[i]);
            }
            var points = transform.gameObject.GetComponentsInChildren<ADBRuntimePoint>();
            for (int i = 0; i < points.Length; i++)
            {
                Component.DestroyImmediate(points[i]);
            }

        }
        public ADBChainProcessor[] GetChainsInChildren()
        {
            switch (generatorMode)
            {
                case ChainGeneratorMode.DynamicBone:
                    List<ADBChainProcessor> list = new List<ADBChainProcessor>();
                    for (int i = 0; i < generateTransformList.Count; i++)
                    {
                        var target = generateTransformList[i].parent ?? generateTransformList[i];
                        if (target.TryGetComponent<ADBChainProcessor>(out ADBChainProcessor value))
                        {
                            if (!list.Contains(value))
                            {
                                list.Add(value);
                            }
                        }
                    }
                    return list.ToArray();
                case ChainGeneratorMode.ADBChain:
                    return gameObject.GetComponentsInChildren<ADBChainProcessor>();
                default:
                    throw new InvalidOperationException();
            }

        }
        public Dictionary<Transform, SelectHelper> UpdateSelectDictionary(bool isDirty = false)
        {
            if (!isDirty && selectDictionary != null && selectDictionary.Count != 0)
            {
                return selectDictionary;
            }
            if (!isDirty &&selectHelpers != null && selectHelpers.Length != 0)
            {
                for (int i = 0; i < selectHelpers.Length; i++)
                {
                    selectDictionary.Add(selectHelpers[i].target, selectHelpers[i]);
                }
                selectHelpers = null;
            }
            else
            {
                selectDictionary.Clear();
                Transform[] allTransform = GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < allTransform.Length; i++)
                {
                    SelectHelper selectHelper = new SelectHelper();
                    selectHelper.selectType = SelectHelper.SelectType.ClothBone;
                    selectDictionary.Add(allTransform[i], new SelectHelper());
                }

                Animator animator = GetComponent<Animator>();
                if (animator != null)
                {
                    List<Transform> animatorTransforms = new List<Transform>();
                    for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                    {
                        Transform humanBone = animator.GetBoneTransform((HumanBodyBones)i);
                        if (humanBone != null)
                        {
                            animatorTransforms.Add(humanBone);
                        }
                    }
                    for (int i = 0; i < allTransform.Length; i++)
                    {
                        Transform target = allTransform[i];
                        Transform[] childs = target.GetComponentsInChildren<Transform>(true);
                        for (int ii = 0; ii < childs.Length; ii++)
                        {
                            Transform child = childs[ii];
                            if (animatorTransforms.Contains(child))
                            {
                                selectDictionary[target].selectType = SelectHelper.SelectType.InvalidBone;
                                break;
                            }
                        }
                    }
                }


                for (int i = 0; i < allTransform.Length; i++)
                {
                    var target = allTransform[i];
                    int childCount = target.childCount;
                    for (int ii = 0; ii < childCount; ii++)
                    {
                        var child = target.GetChild(ii);
                        if (selectDictionary.ContainsKey(child))
                        {
                            var parent = selectDictionary[target];
                            selectDictionary[target].child.Add(child);
                        }
                    }
                }
                for (int i = 0; i < allTransform.Length; i++)
                {
                    ADBRuntimePoint point = allTransform[i].gameObject.GetComponent<ADBRuntimePoint>();
                    if (point != null)
                    {
                        selectDictionary[allTransform[i]].selectType = SelectHelper.SelectType.OtherBone;
                    }
                }
                for (int i = 0; i < allChain.Count; i++)
                {
                    for (int ii = 0; ii < allChain[i].allPointList.Count; ii++)
                    {
                        try
                        {
                            selectDictionary[allChain[i].allPointList[ii].transform].selectType = SelectHelper.SelectType.GenerateBone;
                        }
                        catch (Exception)
                        {

                            throw;
                        }

                    }
                    for (int ii = 0; ii < allChain[i].fixedPointList.Count; ii++)
                    {
                        selectDictionary[allChain[i].allPointList[ii].transform].selectType = SelectHelper.SelectType.GenerateFixedBone;
                    }
                }

            }
            return selectDictionary;
        }

        public void SaveSelectData()
        {
            if (selectDictionary != null)
            {
                selectHelpers = selectDictionary.Values.ToArray();
            }
        }

        #endregion

        public static void GenerateBoneChainImporter(Transform root, List<string> generateKeyWordWhiteList, List<string> generateKeyWordBlackList, List<Transform> blackListOfGenerateTransform, ADBSettingLinker settings, ref List<ADBChainProcessor> chainProcessors)
        {
            Transform[] bfsSortTransform = GetBFS(root);

            for (int i = 0; i < bfsSortTransform.Length; i++)
            {
                Transform target = bfsSortTransform[i];

                if (target == null || target.parent == null) continue;
                ADBRuntimePoint appendOther = target.GetComponent<ADBRuntimePoint>();
                if (appendOther!=null&&!appendOther.isRoot) continue;

                Transform parent = target.parent;

                bool isInvaild = false;
                string transformName = target.name;
                for (int ii = 0; ii < blackListOfGenerateTransform.Count; ii++)
                {
                    if (target.Equals(blackListOfGenerateTransform[ii]))
                    {
                        isInvaild = true;
                        break;
                    }
                }

                for (int ii = 0; ii < generateKeyWordBlackList.Count; ii++)
                {
                    if (transformName.Contains(generateKeyWordBlackList[ii]))
                    {
                        isInvaild = true;
                        break;
                    }
                }

                if (isInvaild)
                {
                    continue;
                }
                else
                {
                    for (int ii = 0; ii < generateKeyWordWhiteList.Count; ii++)
                    {
                        string whiteKey = generateKeyWordWhiteList[ii];
                        if (whiteKey == null) continue;

                        if (transformName.Contains(whiteKey))
                        {
                           var fixedNode= SearchADBRuntimePoint(target, new List<string>() { whiteKey }, generateKeyWordBlackList, blackListOfGenerateTransform);

                            ADBChainProcessor chainProcessor = chainProcessors.FirstOrDefault(x => x.CanMerge(fixedNode, settings.GetSetting(whiteKey)));

                            if (chainProcessor == null)
                            {
                                chainProcessor = ADBChainProcessor.CreateADBChainProcessor(parent, whiteKey, settings.GetSetting(whiteKey));
                                chainProcessors.Add(chainProcessor);
                            }
                            chainProcessor.AddChild(fixedNode);
                        }
                    }
                }
            }

            for (int i = 0; i < chainProcessors.Count; i++)
            {
                chainProcessors[i].Initialize();
            }
        }

        //deep search the fixed point ,get they childpoint and add it to their point data 
        private static ADBRuntimePoint SearchADBRuntimePoint(Transform transform, List<string> generateKeyWordWhiteList, List<string> generateKeyWordBlackList, List<Transform> blackListOfGenerateTransform)
        {
            Transform[] bfsSortTransform = GetBFS(transform);
            List<ADBRuntimePoint> pointResult = new List<ADBRuntimePoint>();

            for (int i = 0; i < bfsSortTransform.Length; i++)
            {
                Transform target = bfsSortTransform[i];

                if (target == null) continue ;

                if (!target.gameObject.activeInHierarchy) continue;
                bool isblack = false;

                for (int ii = 0; ii < blackListOfGenerateTransform.Count; ii++)
                {
                    if (isblack) break;
                    isblack = target.Equals(blackListOfGenerateTransform[ii]);
                }
                string transformName = target.name.ToLower();
                for (int ii = 0; ii < generateKeyWordBlackList.Count; ii++)
                {
                    if (isblack) break;
                    isblack = transformName.Contains(generateKeyWordBlackList[ii]);

                }
                if (isblack) break;

                for (int ii = 0; ii < generateKeyWordWhiteList.Count; ii++)
                {
                    string whiteKey = generateKeyWordWhiteList[ii];
                    if (whiteKey == null) continue;
                    if (transformName.Contains(whiteKey))
                    {
                        pointResult.Add( ADBRuntimePoint.CreateRuntimePoint(target,0, whiteKey, !transformName.Contains(ADBChainProcessor.virtualKey)));
                    }
                }
            }
            BuildParentChildRelation(pointResult);
            return pointResult[0];
        }

        private static void BuildParentChildRelation(List<ADBRuntimePoint> pointList)
        {
            Transform[] transformList = pointList.Select(x => x.transform).ToArray();
            List<ADBRuntimePoint> rootPoint = new List<ADBRuntimePoint>();
            for (int i = 0; i < transformList.Length; i++)
            {
                Transform parentIter = transformList[i].parent;
                while (parentIter!=null)
                {
                    int parentIndex = Array.IndexOf(transformList, parentIter);
                    if (parentIndex != -1)
                    {
                        pointList[parentIndex].AddChild(pointList[i]);
                        break;
                    }
                    parentIter = parentIter.parent;
                }
                if (parentIter == null)
                {
                    rootPoint.Add(pointList[i]);
                }
            }

            for (int i = 0; i < rootPoint.Count; i++)
            {
                SetDepth(rootPoint[i], 0);
            }
        }
        private static void SetDepth(ADBRuntimePoint target,int depth)
        {
            target.depth = depth;
            for (int i = 0; i < target.ChildPoints?.Count; i++)
            {
                SetDepth(target.ChildPoints[i], depth + 1);
            }
        }


        private static Transform[] GetBFS(Transform root)
        {
            Queue<Transform> targetQueue = new Queue<Transform>();
            List<Transform> bfsResult = new List<Transform>();

            targetQueue.Enqueue(root);
            while (targetQueue.Count>0)
            {
                Transform target = targetQueue.Dequeue();
                bfsResult.Add(target);
                int childCount = target.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    targetQueue.Enqueue(target.GetChild(i));
                }
            }
            return bfsResult.ToArray();
        }
    }

}
