using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ADBRuntime
{
    [CreateAssetMenu(fileName = "ADBGlobalSettingFile", menuName = "ADB/GlobalSettingFile")]
    public class ADBGlobalSetting : ScriptableObject
    {

        public List<KeyWordSetting> settings;
        public List<string> defaultKeyWord { get { return settings.SelectMany(x => x.keyWord, (x, y) => y).ToList(); } }
        public bool GetSetting(string keyword,out ADBSetting setting)
        {
            if (!(settings == null || settings.Count == 0))
            {
                for (int i = 0; i < settings.Count; i++)
                {
                    if (settings[i].HasKey(keyword))
                    {
                        if (settings[i].setting == null)
                        {
                            Debug.LogError("you global setting file has lost the setting file ,please check the " +
                              keyword +" keyword");
                            settings[i].setting = (ADBSetting)ScriptableObject.CreateInstance("ADBSetting");
                        }
                        setting= settings[i].setting;
                        return true;
                    }                       
                }
            }

            Debug.Log("You dont add the keyword : "+ keyword + " In ADBGlobalSetting! Check the ADBGlobalSetting File ");
            setting = (ADBSetting)ScriptableObject.CreateInstance(typeof(ADBSetting));
            return false;
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
                        Debug.LogError("you global setting file has lost the setting file ,please check the " +
                            keyword + " keyword");
                        settings[i].setting = (ADBSetting)ScriptableObject.CreateInstance("ADBSetting");
                    }
                    return true;
                }
                
            }
            return false;
        }

    }
    [System.Serializable]
    public class KeyWordSetting
    {

        public ADBSetting setting;
        [SerializeField]
        public List<string> keyWord;
        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;

            key = key.ToLower();
            if (keyWord != null)
            {
                for (int i = 0; i < keyWord.Count; i++)
                {
                    if (!string.IsNullOrEmpty(key) && keyWord[i].ToLower().Contains( key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
