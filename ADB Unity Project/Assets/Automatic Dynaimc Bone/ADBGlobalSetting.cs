﻿using System.Collections;
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
                    {
                        if (settings[i].setting == null)
                        {
                            Debug.LogError("you global setting file has lost the setting file ,please check the " +
                              keyword +" keyword");
                            settings[i].setting = (ADBSetting)ScriptableObject.CreateInstance("ADBSetting");
                        }
                        return settings[i].setting;
                    }                       
                }
            }

            Debug.Log("You dont add the keyword : "+ keyword + " In ADBGlobalSetting! Check the ADBGlobalSetting File ");
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
            if (keyWord != null)
            {
                for (int i = 0; i < keyWord?.Count; i++)
                {
                    if (keyWord[i].Length>0&& keyWord[i].ToLower().Contains( key))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
