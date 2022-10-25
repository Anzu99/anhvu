//#define ENABLE_AppsFlyer

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_AppsFlyer
using AppsFlyerSDK;
#endif

namespace mygame.sdk
{

    // This class is intended to be used the the AppsFlyerObject.prefab

#if ENABLE_AppsFlyer
    public class AppsFlyerHelperScript : MonoBehaviour, IAppsFlyerConversionData
#else
    public class AppsFlyerHelperScript : MonoBehaviour
#endif
    {
        public static AppsFlyerHelperScript Instance = null;

        // These fields are set from the editor so do not modify!
        //******************************//
        public string devKey;
        public string appID;
        public string UWPAppID;
        public bool isDebug;
        public bool getConversionData;
        //******************************//

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
#if ENABLE_AppsFlyer
            // These fields are set from the editor so do not modify!
            //******************************//
            AppsFlyerAdRevenue.start();
            AppsFlyer.setIsDebug(isDebug);
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#else
            AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
            //******************************/

            AppsFlyer.startSDK();
#endif
        }

        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            string jsondata = "{";
            bool isbg = true;
            foreach (var item in conversionDataDictionary)
            {
                Debug.Log($"mysdk: appsflyer data conversion key={item.Key}, value={item.Value}");
                if (isbg)
                {
                    isbg = false;
                    jsondata += "\"" + item.Key + " \":\"" + item.Value + "\"";
                }
                else
                {
                    jsondata += ",\"" + item.Key + " \":\"" + item.Value + "\"";
                }
            }
            jsondata += "}";
            Myapi.LogEventApi.Instance().LogEvent(Myapi.MyEventLog.AdsMaxConversionData, jsondata);
#endif
        }

        public void onConversionDataFail(string error)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
#endif
        }

        public void onAppOpenAttribution(string attributionData)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here

#endif
        }

        public void onAppOpenAttributionFailure(string error)
        {
#if ENABLE_AppsFlyer
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
#endif
        }

        public static void logPurchase(string sku, string value, string currency)
        {
#if ENABLE_AppsFlyer
            Dictionary<string, string> dicParams = new Dictionary<string, string>();
            dicParams.Add(AFInAppEvents.CONTENT_ID, sku);
            dicParams.Add(AFInAppEvents.CURRENCY, currency);
            dicParams.Add(AFInAppEvents.REVENUE, value);
            logEvent(AFInAppEvents.PURCHASE, dicParams);
#endif
        }

        public static void logEvent(string eventName, Dictionary<string, string> dicPrams = null)
        {
#if ENABLE_AppsFlyer
            if (dicPrams == null)
            {
                Dictionary<string, string> dicParams = new Dictionary<string, string>();
            }
            AppsFlyer.sendEvent(eventName, dicPrams);
#endif
        }
    }
}
