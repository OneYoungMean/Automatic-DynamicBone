using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ADBRuntime
{
    [CreateAssetMenu(fileName = "ADBGlobalSettingFile", menuName = "ADB/GlobalSettingFile")]
    public class ADBGlobalSetting : ScriptableObject
    {

        public List<KeyWordSetting> settings;
        public List<string> defaultKeyWord;
        public ADBSetting GetSetting(string keyword)
        {
            if (!(settings == null || settings.Count == 0))
            {
                for (int i = 0; i < settings.Count; i++)
                {
                    if (settings[i].HasKey(keyword))
                        return settings[i].setting;
                }
            }
            Debug.Log("You dont add the keyword In ADBSetting!check the Automatic Dynamic Bone/Resource/GlobalSettingFile !");
            return (ADBSetting)ScriptableObject.CreateInstance(typeof(ADBSetting));
        }

    }
    [System.Serializable]
    public class KeyWordSetting
    {

        public ADBSetting setting;
        [SerializeField]
        List<string> keyWord;
        public bool HasKey(string key)
        {
            return (keyWord != null && keyWord.Contains(key));
        }
    }
}
