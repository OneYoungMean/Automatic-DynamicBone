using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ADBRuntime.Mono.Tool
{
    public enum ChainGeneratorMode
    {
         DynamicBone,
         ADBChain,
         Clear
    }
    public class ADBChainGenerateTool :MonoBehaviour
    {
        [SerializeField]
        public List<string> generateKeyWordWhiteList = new List<string> { "skirt" };// "hair", "tail", 
        [SerializeField]
        public List<string> generateKeyWordBlackList = new List<string> { "ik" };
        [SerializeField]
        public List<Transform> blackListOfGenerateTransform = new List<Transform>();
        [SerializeField]
        public List<Transform> generateTransformList = new List<Transform>() {};
        [SerializeField]
        public ADBSettingLinker linker;
        [SerializeField]
        public ADBPhysicsSetting setting;
        [SerializeField]
        public ADBChainProcessor[] allChain;
        [SerializeField]
        public bool isGenerateOnStart;
        [SerializeField]
        public ChainGeneratorMode generatorMode;

        private void ListCheck()
        {
            if (generateKeyWordWhiteList == null || generateKeyWordWhiteList.Count == 0)
            {
                generateKeyWordWhiteList = linker.AllKeyWord;
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
        public  void ClearBoneChain()
        {
            for (int i = 0; i < allChain.Length; i++)
            {
                var targetChain = allChain[i];
                for (int j0 = 0; j0 < targetChain.allPointList.Count; j0++)
                {
                    DestroyImmediate(targetChain.allPointList[j0]);
                }
                DestroyImmediate(targetChain);
            }

            if (gameObject.TryGetComponent<ADBRuntimeController>(out ADBRuntimeController target))
            {
                target.ListCheck();
            }
            allChain = null;
        }
            public string GetPointCount()
        {
            int count = 0;
            for (int i = 0; i < allChain?.Length; i++)
            {
                if (allChain[i].allPointList!=null)
                {
                    count += allChain[i].allPointList.Count;
                }

            }
            return count.ToString();
        }

        public void InitializeChain()
        {
            ListCheck();
            List<ADBChainProcessor> generateChain = new List<ADBChainProcessor>();
            switch (generatorMode)
            {
                case ChainGeneratorMode.DynamicBone:
                    List<string> everyKey = new List<string>() { "" };
                    var tempLinker = ScriptableObject.CreateInstance<ADBSettingLinker>();
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
                        if (generateTransformList[i]==null||(!generateTransformList[i].gameObject.GetComponentsInParent<Transform>().Contains(transform)))
                        {
                            continue;
                        }
                        GenerateBoneChainImporter(generateTransformList[i], everyKey, new List<string>(), new List<Transform>(), tempLinker, ref generateChain);
                    }
                    break;
                case ChainGeneratorMode.ADBChain:
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        GenerateBoneChainImporter(transform.GetChild(i), generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform, linker, ref generateChain);
                    }
                    break;
                default:
                    break;
            }
            allChain = generateChain.ToArray();
            //RefreshBoneChain();
            for (int i = 0; i < allChain.Length; i++)
            {
                allChain[i].Initialize();
            }

        }
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
        public  ADBChainProcessor[] GetChainsInChildren()
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
                    break;
            }

        }
        public static void GenerateBoneChainImporter(Transform transform, List<string> generateKeyWordWhiteList, List<string> generateKeyWordBlackList, List<Transform> blackListOfGenerateTransform, ADBSettingLinker settings,ref List<ADBChainProcessor> chainProcessors)//OYM：一个巨啰嗦的方法
        {
            if (transform == null||
                (transform.TryGetComponent<ADBRuntimePoint>(out ADBRuntimePoint point)&&!point.isRoot)) return ;

            bool isblack = false;
            do
            {
                if (!transform.gameObject.activeInHierarchy|| transform.parent==null)//OYM:没有打开,或者是根节点
                {
                    continue;
                }

                for (int j0 = 0; j0 < blackListOfGenerateTransform.Count; j0++)//OYM：是否在节点黑名单
                {
                    if (isblack) break;
                    isblack = transform.Equals(blackListOfGenerateTransform[j0]);
                }
                if (isblack) return;//OYM:节点在黑名单时,不再向下搜索

                string transformName = transform.name.ToLower();
                for (int j0 = 0; j0 < generateKeyWordBlackList.Count; j0++)//OYM：是否在名字黑名单
                {
                    if (isblack) break;
                    isblack = transformName.Contains(generateKeyWordBlackList[j0]);

                }
                if (isblack) break;//OYM:名字在黑名单时,仍然继续向下搜索

                for (int i = 0; i < generateKeyWordWhiteList.Count; i++)
                {
                    string whiteKey = generateKeyWordWhiteList[i];
                    if (whiteKey == null) continue;

                    if (transformName.Contains(whiteKey))
                    {
                       var fixedNode =  SearchADBRuntimePoint(transform, new List<string>() { whiteKey }, generateKeyWordBlackList, blackListOfGenerateTransform, 0);
                        var parent = transform.parent;
                        ADBChainProcessor chainProcessor= chainProcessors.FirstOrDefault(x => x.CanMerge(fixedNode, settings.GetSetting(whiteKey)));
                        if (chainProcessor == null)
                        {
                            chainProcessor = ADBChainProcessor.CreateADBChainProcessor(parent, whiteKey, settings.GetSetting(whiteKey));
                            chainProcessors.Add(chainProcessor);
                        }
                        chainProcessor.AddChild(fixedNode);
                    }
                }
            } while (false);

            for (int i = 0; i < transform.childCount; i++)
            {
                GenerateBoneChainImporter(transform.GetChild(i), generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform, settings,ref chainProcessors);
            }
        }

        //OYM：deep search the fixed point ,get they childpoint and add it to their point data 
        private static ADBRuntimePoint SearchADBRuntimePoint(Transform transform, List<string> generateKeyWordWhiteList, List<string> generateKeyWordBlackList, List<Transform> blackListOfGenerateTransform, int depth)
        {       //OYM：利用深度搜索,能很快找到所有的固定点,
                //OYM：如果是子节点与父节点匹配,则父节点添加子节点坐标
                //OYM：

            ADBRuntimePoint point = null;
            do
            {
                if (transform == null) break;

                if (!transform.gameObject.activeInHierarchy) break;//OYM:关闭了就不再进行添加
                bool isblack = false;

                for (int i = 0; i < blackListOfGenerateTransform.Count; i++)//OYM：是否在节点黑名单
                {
                    if (isblack) break;
                    isblack = transform.Equals(blackListOfGenerateTransform[i]);
                }
                string transformName = transform.name.ToLower();
                for (int i = 0; i < generateKeyWordBlackList.Count; i++)//OYM：是否在名字黑名单
                {
                    if (isblack) break;
                    isblack = transformName.Contains(generateKeyWordBlackList[i]);

                }
                if (isblack) break;

                for (int i = 0; i < generateKeyWordWhiteList.Count; i++)//OYM:搜索白名单以内的骨骼
                {
                    string whiteKey = generateKeyWordWhiteList[i];
                    if (whiteKey == null) continue;
                    if (transformName.Contains(whiteKey))
                    {
                        point =  ADBRuntimePoint.CreateRuntimePoint(transform, depth, whiteKey, !transformName.Contains(ADBChainProcessor.virtualKey));//OYM: 创建活动节点
                        break;
                    }
                }
            } while (false);


            //OYM：get point child
            //OYM：注意,这个递归非常有意思,值得好好看看
            if (point != null)
            {

                for (int i = 0; i < transform.childCount; i++)
                {
                    var childPoint = SearchADBRuntimePoint(point.transform.GetChild(i), generateKeyWordWhiteList, generateKeyWordBlackList, blackListOfGenerateTransform, depth + 1);
                    if (childPoint != null)
                    {
                        point.AddChild(childPoint);
                    }
                }
            }
            return point;
        }
        #endregion
    }
}
