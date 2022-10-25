using System;
using System.Collections;
using System.Collections.Generic;
using MyJson;
using UnityEngine;
using UnityEngine.Analytics;

#if FIRBASE_ENABLE
using Firebase.RemoteConfig;
#elif ENABLE_GETCONFIG
using Firebase.RemoteConfig;
#endif

namespace mygame.sdk
{
    public class FIRParserOtherConfig
    {
        public static void parserInGameConfig()
        {
#if FIRBASE_ENABLE || ENABLE_GETCONFIG
            string keycfframerate = "cf_game_mode";
            ConfigValue v = FirebaseRemoteConfig.DefaultInstance.GetValue(keycfframerate);
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_game_mode", per);
            }
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_type_game_lose");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_type_game_lose", per);
            }

            // Finish Time
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_player_type");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                string data = v.StringValue;
                PlayerPrefs.SetString("cf_player_type", data);
                PlayerIQCtrl.Instance.playerIQConfig = JsonUtility.FromJson<PlayerIQConfig>(data);
            }

            // Show ads
            v = FirebaseRemoteConfig.DefaultInstance.GetValue("cf_show_ad_in_main");
            if (v.StringValue != null && v.StringValue.Length > 0)
            {
                int per = (int)v.LongValue;
                PlayerPrefs.SetInt("cf_show_ad_in_main", per);
    
            }

#endif
        }
    }
}