using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ADBRuntime
{
    /// <summary>
    /// A class used to store multiple settings and keyword pairs
    /// Used to auto generate.
    /// </summary>
    [CreateAssetMenu(fileName = "ADBSettingLinker", menuName = "ADB/ADBSettingLinker")]
    public class ADBSettingLinker : ScriptableObject
    {
        public ADBPhysicsSetting defaultSetting;
        public List<KeyWordSetting> settings;
        public List<string> AllKeyWord { get { return settings.SelectMany(x => x.keyWord, (x, y) => y).ToList(); } }
        /// <summary>
        /// Get setting by keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool TryGetSetting(string keyword,out ADBPhysicsSetting setting)
        {
            if (!(settings == null || settings.Count == 0))
            {
                for (int i = 0; i < settings.Count; i++)
                {
                    if (settings[i].HasKey(keyword))
                    {
                        if (settings[i].setting == null)
                        {
                            Debug.LogError(string.Format( $"the linker file{0} has lost the setting file ,please check the {1} keyword",this.name, settings[i].keyWord));
                        }
                        else
                        {
                            setting = settings[i].setting;
                            return true;
                        }

                    }                       
                }
            }
             if (defaultSetting!=null)
            {

                Debug.Log(string.Format($"the keyworld {0} Use linker{1} default Setting", keyword, this.name));
                setting = defaultSetting;
                return false;
            }
            else
            {
                Debug.LogError(string.Format($"the linker file {0} does not containing the {1} keyword", this.name, keyword));
                setting = (ADBPhysicsSetting)ScriptableObject.CreateInstance(typeof(ADBPhysicsSetting));
                return false;
            }
        }
        /// <summary>
        /// Get Setting by keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ADBPhysicsSetting GetSetting(string keyword)
        {
            TryGetSetting(keyword, out ADBPhysicsSetting setting);
            return setting;
        }
        public bool isContain(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return false;

            for (int i = 0; i < settings.Count; i++)
            { 
                if (settings[i].HasKey(keyword))
                {
                    if (settings[i].setting == null)
                    {
                        Debug.LogError("you Linker setting file has lost the setting file ,please check the " +
                            keyword + " keyword");
                        settings[i].setting = (ADBPhysicsSetting)ScriptableObject.CreateInstance("ADBSetting");
                    }
                    return true;
                }
                
            }
            return false;
        }

    }
    /// <summary>
    /// Word list-setting pair 
    /// </summary>
    [System.Serializable]
    public class KeyWordSetting
    {

        public ADBPhysicsSetting setting;
        [SerializeField]
        public List<string> keyWord;
        public bool HasKey(string key)
        {
            if (key==null) return false;

            key = key.ToLower();
            if (keyWord != null)
            {
                for (int i = 0; i < keyWord.Count; i++)
                {
                    if (key.Contains( keyWord[i].ToLower()))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
