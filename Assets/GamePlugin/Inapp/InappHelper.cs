//#define ENABLE_INAPP
//#define ENABLE_ADJUST

using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Myapi;
#if ENABLE_ADJUST
using com.adjust.sdk;
#endif
using System.Linq;
#if ENABLE_INAPP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

#endif


namespace mygame.sdk
{
#if ENABLE_INAPP
    public class InappHelper : MonoBehaviour, IStoreListener
    {
#else
    public class InappHelper : MonoBehaviour
    {
#endif
        public static InappHelper Instance { get; private set; }

        public static event Action<string> SubFirstCallback = null;
        public static event Action<string, int> SubDailyCallback = null;

        public static event Action<string, int> FakeSubDailyCallback = null;

#if ENABLE_INAPP
        private IStoreController m_StoreController; // The Unity Purchasing system.
        private IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
        private IAppleExtensions m_AppleExtensions;
        private CrossPlatformValidator validator;
        private Product productValidbyAppsflyer = null;
#endif
        public HandleSub _handleSub;
        public InappCountryOb listCurr { get; private set; }
        public Dictionary<string, InappCountryOb> listAll = new Dictionary<string, InappCountryOb>();
        private string buyWhere = "";
        private DateTime mPurchaseDate;
        int countTryCheckFakeSub = 0;
        public static int isPurchase
        {
            get { return PlayerPrefs.GetInt("iap_isPurchase", 0); }
        }

        private Action<PurchaseCallback> _callback;
        int CurrStatePurchase = 0;//0-none, 1-purchasing
        long tCurrPurchase = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                CurrStatePurchase = 0;
                tCurrPurchase = 0;
                string data = "";
                string path = Application.persistentDataPath + "/files/cf_ina_ga.txt";
                if (File.Exists(path))
                {
                    data = File.ReadAllText(path);
                }

                if (data == null || data.Length < 10)
                {
#if UNITY_ANDROID
                    TextAsset txt = (TextAsset)Resources.Load("Inapp/Android/data", typeof(TextAsset));
#else
                    TextAsset txt = (TextAsset)Resources.Load("Inapp/iOS/data", typeof(TextAsset));
#endif
                    data = txt.text;
                }

                InappUtil.parserDataSkus(data, listAll);
                string countrycode = PlayerPrefs.GetString("mem_countryCode", "");
                countrycode = CountryCodeUtil.convertToCountryCode(countrycode);
                if (listAll.ContainsKey(countrycode))
                {
                    listCurr = listAll[countrycode];
                }
                else
                {
                    listCurr = listAll[GameHelper.CountryDefault];
                }

                //getMemPrice();
            }
            else
            {
                // if (this != Instance) Destroy(gameObject);
            }
        }

        private void Start()
        {
#if ENABLE_INAPP
            Debug.Log("mysdk: IAP Start1");
            ConfigurationBuilder configurationBuilder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            configurationBuilder.Configure<IMicrosoftConfiguration>().useMockBillingSystem = true;
            //configurationBuilder.Configure<IGooglePlayConfiguration>().SetPublicKey(lincenseKey);
            foreach (var skuitem in listCurr.listWithId)
            {
                ProductType tpr = ProductType.Consumable;
                if (skuitem.Value.typeInapp == 0)
                {
                    tpr = ProductType.Consumable;
                }
                else if (skuitem.Value.typeInapp == 1)
                {
                    tpr = ProductType.NonConsumable;
                }
                else if (skuitem.Value.typeInapp == 2)
                {
                    tpr = ProductType.Subscription;
                }
                Debug.Log("mysdk: IAP Start1 sku=" + skuitem.Value.sku);

                configurationBuilder.AddProduct(skuitem.Value.sku, tpr);
            }
            Debug.Log("mysdk: IAP Start2");
            UnityPurchasing.Initialize(this, configurationBuilder);
            Debug.Log("mysdk: IAP Start3");
            string appidentifier = Application.identifier;
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE)
            validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleTangle.Data(),
                appidentifier);
#endif

#if !UNITY_EDITOR && ENABLE_AppsFlyer
#if UNITY_ANDROID
Debug.Log("mysdk: IAP Start41");
            AppsFlyerSDK.AppsFlyerAndroid.initInAppPurchaseValidatorListener(this);
#elif UNITY_IOS || UNITY_IPHONE
Debug.Log("mysdk: IAP Start42");
#endif
#endif

#endif
            Debug.Log("mysdk: IAP Start4");
        }

        void savePrice()
        {
            try
            {
                string dataPrice = "{";
                string pathPrice = Application.persistentDataPath + "/files/mem_iap_price.txt";
                bool isbegin = true;
                foreach (var item in listCurr.listWithSku)
                {
                    if (isbegin)
                    {
                        isbegin = false;
                        dataPrice += ",";
                    }
                    dataPrice += $"\"{item.Value.sku}\":\"{item.Value.price}\"";
                }
                dataPrice += "}";
                File.WriteAllText(pathPrice, dataPrice);
            }
            catch (Exception ex)
            {
                Debug.Log("mysdk: IAP ex=" + ex.ToString());
            }
        }

        void getMemPrice()
        {
            string dataPrice = "";
            string pathPrice = Application.persistentDataPath + "/files/mem_iap_price.txt";
            if (File.Exists(pathPrice))
            {
                dataPrice = File.ReadAllText(pathPrice);
                var listmemPrice = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(dataPrice);
                if (listmemPrice != null || listmemPrice.Count > 0)
                {
                    foreach (KeyValuePair<string, object> itemdata in listmemPrice)
                    {
                        if (listCurr.listWithSku.ContainsKey(itemdata.Key))
                        {
                            listCurr.listWithSku[itemdata.Key].price = (string)itemdata.Value;
                        }
                    }
                }
            }
        }

        public void onclickPolicy()
        {
#if UNITY_ANDROID
            Application.OpenURL("https://dinoproduct.com/privacypolicy");
#elif UNITY_IOS || UNITY_IPHONE
            Application.OpenURL("https://habuistudio.com/privacypolicy");
#endif
        }

        public void onclickTermsofuse()
        {
#if UNITY_ANDROID
            Application.OpenURL("https://dinoproduct.com/termsofuse");
#elif UNITY_IOS || UNITY_IPHONE
            Application.OpenURL("https://habuistudio.com/privacypolicy");
#endif
        }

        #region PUBLIC METHODS

        public bool BuyPackage(string skuid, string where, Action<PurchaseCallback> cb)
        {
            string realSku = "";
            buyWhere = where;
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                realSku = listCurr.listWithId[skuid].sku;
            }
            else
            {
                if (skuid != null)
                {
                    Debug.LogError("mysdk: IAP err " + skuid);
                }

                return false;
            }

            long t = SdkUtil.systemCurrentMiliseconds() / 1000;
            if (CurrStatePurchase != 0)
            {
                if ((t - tCurrPurchase) < 1200)
                {
                    Debug.LogError("mysdk: IAP err in pre processing");
                    return false;
                }
            }
            _callback = cb;

#if ENABLE_TEST_INAPP
		if (realSku.Length > 0) {
			Debug.Log("mysdk: IAP test inapp" + realSku);
            PlayerPrefs.SetInt("iap_isPurchase", 1);
            getRewardInapp(realSku);
            if (_callback != null)
            {
                PurchaseCallback pcb = new PurchaseCallback(1, realSku);
                _callback(pcb);
            }
            return true;
		} else {
            return false;
        }
		
#endif
#if ENABLE_INAPP
            string skulog = realSku.Replace('.', '_');
            if (skulog.Length > 25)
            {
                skulog = skulog.Substring(skulog.Length - 25);
            }
            FIRhelper.logEvent($"IAP_click_{skulog}");
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(realSku);
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log($"mysdk: IAP Purchasing product asychronously: {product.definition.id}");
                    tCurrPurchase = t;
                    CurrStatePurchase = 1;
                    SDKManager.Instance.showWaitCommon();
                    m_StoreController.InitiatePurchase(product);
                    return true;
                }
                else
                {
                    Debug.LogError(
                        $"mysdk: IAP BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"mysdk: IAP BuyProductID FAIL. Not initialized.");
                return false;
            }
#else
            return false;
#endif
        }

        public bool BuySubscription(string skuid, string where, Action<PurchaseCallback> cb)
        {
            if (!_handleSub.hasSub(skuid))
            {
                return BuyPackage(skuid, where, cb);
            }
            else
            {
                return false;
            }
        }

        public void RestorePurchases()
        {
            Debug.Log("mysdk: IAP RestorePurchases Click ...");
            if (!IsInitialized())
            {
                Debug.Log("mysdk: IAP RestorePurchases FAIL. Not initialized.");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                     Application.platform == RuntimePlatform.OSXPlayer)
            {
                Debug.Log("mysdk: IAP RestorePurchases started ...");
#if ENABLE_INAPP
                IAppleExtensions extension = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                extension.RestoreTransactions(delegate (bool result)
                {
                    if (result)
                    {
                        for (int i = 0; i < m_StoreController.products.all.Length; i++)
                        {
                            if (m_StoreController.products.all[i].hasReceipt)
                            {
                                Debug.Log("mysdk: IAP Receipt " + m_StoreController.products.all[i].receipt);
                                if (listCurr.listWithSku.ContainsKey(m_StoreController.products.all[i].definition.id))
                                {
                                    PkgObject obsku =
                                        listCurr.listWithSku[m_StoreController.products.all[i].definition.id];
                                    if (obsku.rcv.removeads != 0)
                                    {
                                        PlayerPrefs.SetInt("RemoveAdsNew", 1);
                                        mygame.sdk.AdsHelper.setRemoveAds();
                                    }

                                    getRewardInapp(m_StoreController.products.all[i]);
                                    //TODO VVV
                                }
                            }
                        }
                    }

                    Debug.Log("mysdk: IAP RestorePurchases continuing: " + result +
                              ". If no further messages, no purchases available to restore.");
                });
#endif
            }
            else
            {
                Debug.Log("mysdk: IAP RestorePurchases FAIL. Not supported on this platform. Current = " +
                          Application.platform);
            }
        }

        public int GetPurchaseDate(string sku)
        {
            return listCurr.listWithId[sku].dayPurchased;
        }

        public void handleFirstSub(string skuid)
        {
            if (SubFirstCallback != null)
            {
                SubFirstCallback(skuid);
            }
        }

        public void handleDailySub(string skuid, int dayRcv)
        {
            if (SubDailyCallback != null)
            {
                SubDailyCallback(skuid, dayRcv);
            }
        }

        void handleFakeSub(string skuid, int dayRcv)
        {
            if (FakeSubDailyCallback != null)
            {
                FakeSubDailyCallback(skuid, dayRcv);
            }
        }

        public bool isExpireSub(string skuid)
        {
#if ENABLE_INAPP
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                if (listCurr.listWithId[skuid].typeInapp == 2)
                {
                    return _handleSub.isExpired(skuid);
                }
            }
            return true;
#else
            return true;
#endif
        }
        public string getPrice(string skuid)
        {
#if UNITY_EDITOR
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                return listCurr.listWithId[skuid].price;
            }
#elif ENABLE_INAPP
            if (m_StoreController != null)
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    string realSku = listCurr.listWithId[skuid].sku;
                    if (m_StoreController.products.WithID(realSku) != null)
                    {
                        return m_StoreController.products.WithID(realSku).metadata.localizedPriceString;
                    } 
                    else 
                    {
                        if (listCurr.listWithId.ContainsKey(skuid))
                        {
                            return listCurr.listWithId[skuid].price;
                        }
                    }
                }
            }
            else 
            {
                if (listCurr.listWithId.ContainsKey(skuid))
                {
                    return listCurr.listWithId[skuid].price;
                }
            }
#endif
            return "";
        }

        public int getDayReward(string skuid)
        {
            if (listCurr.listWithId.ContainsKey(skuid))
            {
                return listCurr.listWithId[skuid].periodSub;
            }
            return 0;
        }

        void checkTime4FakeSub()
        {
            Debug.Log("mysdk: IAP checkTime4FakeSub");
            countTryCheckFakeSub++;
            foreach (var skuitem in listCurr.listWithId)
            {
                if (skuitem.Value.typeInapp == 0 && skuitem.Value.periodSub > 0 && skuitem.Value.dayPurchased > 0)
                {
                    if (SDKManager.Instance.timeOnline <= 0)
                    {
                        Myapi.ApiManager.Instance.getTimeOnline((status, time) =>
                        {
                            if (status)
                            {
                                SDKManager.Instance.timeOnline = (int)(time / 60000);
                                SDKManager.Instance.timeWhenGetOnline = (int)(SdkUtil.systemCurrentMiliseconds() / 60000);
                                checkFakeSub();
                            }
                            else
                            {
                                if (countTryCheckFakeSub <= 3)
                                {
                                    Invoke("checkTime4FakeSub", 60);
                                }
                            }
                        });
                    }
                    else
                    {
                        checkFakeSub();
                    }
                    break;
                }
            }
        }

        void checkFakeSub()
        {
            DateTime nd = SdkUtil.DateTimeFromTimeStamp((long)SDKManager.Instance.timeOnline * 60);
            int ncday = nd.Year * 365 + nd.DayOfYear;
            Debug.Log($"mysdk: IAP checkFakeSub curr day={ncday}:{nd}");
            foreach (var skuitem in listCurr.listWithId)
            {
                if (skuitem.Value.typeInapp == 0 && skuitem.Value.periodSub > 0 && skuitem.Value.dayPurchased > 0)
                {
                    int dd = ncday - skuitem.Value.dayPurchased;
                    if (dd >= skuitem.Value.periodSub)
                    {
                        Debug.Log($"mysdk: IAP checkFakeSub pack:{skuitem.Value.id} daybuy={skuitem.Value.dayPurchased} out of date");
                        skuitem.Value.dayPurchased = 0;
                        PlayerPrefsBase.Instance().setInt($"iap_{skuitem.Value.id}_daypur", 0);
                    }
                    else
                    {
                        int drcvmem = PlayerPrefsBase.Instance().getInt($"iap_{skuitem.Value.id}_dayrcvpur", 0);
                        if (drcvmem < ncday)
                        {
                            Debug.Log($"mysdk: IAP checkFakeSub pack:{skuitem.Value.id} daybuy={skuitem.Value.dayPurchased} rcv day={dd}");
                            skuitem.Value.countRcvDaily++;
                            skuitem.Value.dayRcvFakeSub = ncday;
                            PlayerPrefsBase.Instance().setInt($"iap_{skuitem.Value.id}_dayrcv", skuitem.Value.countRcvDaily);
                            PlayerPrefsBase.Instance().setInt($"iap_{skuitem.Value.id}_dayrcvpur", ncday);

                            handleFakeSub(skuitem.Value.id, dd + 1);
                        }
                        else
                        {
                            Debug.Log($"mysdk: IAP checkFakeSub pack:{skuitem.Value.id} daybuy={skuitem.Value.dayPurchased} dayrcv={drcvmem} has rcv");
                        }
                    }
                }
            }
        }

        public InappRcvObject getReceiver(string skuId)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv;
            }
            return null;
        }
        public int getMoenyRcv(string skuId, string key)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv.getMoney(key);
            }
            return 0;
        }
        public int getItemRcv(string skuId, string key)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv.getItem(key);
            }
            return 0;
        }
        public InappRcvItemsObject getEquipmentRcv(string skuId, string key)
        {
            if (listCurr.listWithId.ContainsKey(skuId))
            {
                InappRcvObject rcv = listCurr.listWithId[skuId].rcv;
                return rcv.getEquipment(key);
            }
            return null;
        }

        #endregion

        #region PRIVATE METHODS

        public bool IsInitialized()
        {
#if ENABLE_INAPP
            // Only say we are initialized if both the Purchasing references are set.
            return (m_StoreController != null && m_StoreExtensionProvider != null);
#else
            return false;
#endif
        }

        #endregion

#if ENABLE_INAPP

        #region IMPLEMENTION IStoreListener

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("mysdk: IAP OnInitializeFailed " + error);
            //this.InitializePurchasing();
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("mysdk: IAP OnInitialized 1");
            countTryCheckFakeSub = 0;
            _handleSub.checkStartSub();
            checkTime4FakeSub();
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            //Debug.Log("OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;
            m_AppleExtensions = extensions.GetExtension<IAppleExtensions>();
            Dictionary<string, string> introductoryPriceDictionary = m_AppleExtensions.GetIntroductoryPriceDictionary();

            Product[] all = controller.products.all;
            bool isHanSub = false;
            foreach (Product product in all)
            {
                if (!product.availableToPurchase)
                {
                    continue;
                }

                if (listCurr.listWithSku.ContainsKey(product.definition.id))
                {
                    if (listCurr.listWithSku[product.definition.id].memPrice == 1)
                    {
                        PlayerPrefs.SetString(product.definition.id, product.metadata.localizedPriceString);
                    }
                }

                if (product.receipt != null)
                {
                    if (product.definition.type == ProductType.Subscription)
                    {
                        if (checkIfProductIsAvailableForSubscriptionManager(product.receipt))
                        {
                            string intro_json =
                                (introductoryPriceDictionary != null &&
                                 introductoryPriceDictionary.ContainsKey(product.definition.storeSpecificId))
                                    ? introductoryPriceDictionary[product.definition.storeSpecificId]
                                    : null;
                            SubscriptionManager subscriptionManager = new SubscriptionManager(product, intro_json);
                            SubscriptionInfo subscriptionInfo = subscriptionManager.getSubscriptionInfo();
                            Debug.Log("mysdk: IAP product id is: " + subscriptionInfo.getProductId());
                            Debug.Log("mysdk: IAP purchase date is: " + subscriptionInfo.getPurchaseDate());
                            Debug.Log("mysdk: IAP subscription next billing date is: " +
                                      subscriptionInfo.getExpireDate());
                            Debug.Log("mysdk: IAP is subscribed? " + subscriptionInfo.isSubscribed());
                            Debug.Log("mysdk: IAP is expired? " + subscriptionInfo.isExpired());
                            Debug.Log("mysdk: IAP is cancelled? " + subscriptionInfo.isCancelled());
                            Debug.Log("mysdk: IAP product is in free trial peroid? " +
                                      subscriptionInfo.isFreeTrial());
                            Debug.Log("mysdk: IAP product is auto renewing? " + subscriptionInfo.isAutoRenewing());
                            Debug.Log("mysdk: IAP subscription remaining valid time until next billing date is: " +
                                      subscriptionInfo.getRemainingTime());
                            Debug.Log("mysdk: IAP is this product in introductory price period? " +
                                      subscriptionInfo.isIntroductoryPricePeriod());
                            Debug.Log("mysdk: IAP the product introductory localized price is: " +
                                      subscriptionInfo.getIntroductoryPrice());
                            Debug.Log("mysdk: IAP the product introductory price period is: " +
                                      subscriptionInfo.getIntroductoryPricePeriod());
                            Debug.Log("mysdk: IAP the product introductory price period is: " +
                                      subscriptionInfo.getIntroductoryPricePeriod());
                            Debug.Log("mysdk: IAP the number of product introductory price period cycles is: " +
                                      subscriptionInfo.getIntroductoryPricePeriodCycles());
                            isHanSub = true;
                            _handleSub.ReceiveSubscriptionProduct(subscriptionInfo, listCurr.listWithSku[product.definition.id]);
                        }
                        else
                        {
                            Debug.Log(
                                "mysdk: IAP This product is not available for SubscriptionManager class, only products that are purchase by 1.19+ SDK can use this class." +
                                product.definition.id);
                        }
                    }
                    else
                    {
                        Debug.Log("mysdk: IAP the product is not a subscription product" + product.definition.id);
                    }
                }
                else
                {
                    Debug.Log("mysdk: IAP valid sku=" + product.definition.id + ", price=" + product.metadata.localizedPriceString);
                }
            }
            if (!isHanSub)
            {
                _handleSub.checkHandSub();
            }
        }

        private bool checkIfProductIsAvailableForSubscriptionManager(string receipt)
        {
            // Debug.Log("mysdk: IAP checkIfProductIsAvailableForSubscriptionManager");

            // var listAndroidReceipt = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(receipt);
            // if (listAndroidReceipt != null || listAndroidReceipt.Count > 0)
            // {
            //     foreach (KeyValuePair<string, object> itemReceipt in listAndroidReceipt)
            //     {
            //         Debug.Log("mysdk: IAP itemReceipt key={itemReceipt.Key}");
            //         Debug.Log("mysdk: IAP itemReceipt val={itemReceipt.Value}");
            //         try
            //         {
            //             var listAndPayload = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText((string)itemReceipt.Value);
            //             if (listAndPayload != null || listAndPayload.Count > 0)
            //             {
            //                 foreach (KeyValuePair<string, object> itemPayload in listAndPayload)
            //                 {
            //                     Debug.Log("mysdk: IAP itemPayload key={itemPayload.Key}");
            //                     Debug.Log("mysdk: IAP itemPayload val={itemPayload.Value}");
            //                 }
            //             }
            //         }
            //         catch (Exception ex)
            //         {

            //         }
            //     }
            // }
            // Debug.Log("mysdk: IAP checkIfProductIsAvailableForSubscriptionManager 1111");

            var dicReceipt = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(receipt);
            if (!dicReceipt.ContainsKey("Store") || !dicReceipt.ContainsKey("Payload"))
            {
                Debug.Log("mysdk: IAP The product receipt does not contain enough information ");
                return false;
            }

            string txtStore = (string)dicReceipt["Store"];
            string textPayload = (string)dicReceipt["Payload"];
            if (txtStore != null && textPayload != null)
            {
                if (txtStore == "GooglePlay")
                {
                    var dicPayload = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(textPayload);
                    if (!dicPayload.ContainsKey("json"))
                    {
                        Debug.Log("mysdk: IAP The product receipt does not contain enough information, the 'json' field is missing");
                        return false;
                    }
                    // string txtjson = (string)dicPayload["json"];
                    // Debug.Log("mysdk: IAP txtjson={txtjson}");
                    // var dicJson = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(txtjson);
                    // if (dicJson == null || !dicJson.ContainsKey("developerPayload"))
                    // {
                    //     Debug.Log("mysdk: IAP The product receipt does not contain enough information, the 'developerPayload' field is missing");
                    //     return false;
                    // }

                    // string txtDevPlayload = (string)dicJson["developerPayload"];
                    // Debug.Log("mysdk: IAP txtDevPlayload={txtDevPlayload}");
                    // var dicDevPay = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(txtDevPlayload);
                    // if (dicDevPay == null || !dicDevPay.ContainsKey("is_free_trial") || !dicDevPay.ContainsKey("has_introductory_price_trial"))
                    // {
                    //     Debug.Log("mysdk: IAP The product receipt does not contain enough information, the product is not purchased using 1.19 or later");
                    //     return false;
                    // }

                    return true;
                }
                else if (txtStore == "AppleAppStore" || txtStore == "AmazonApps" || txtStore == "MacAppStore")
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            Debug.Log(string.Format("mysdk: OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}",
                i.definition.storeSpecificId, p));
            handlePurchaseFaild(i, p.ToString());
        }
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            bool validPurchase = true;

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
			try {
				// On Google Play, result has a single product ID.
				// On Apple stores, receipts contain multiple products.
				var result = validator.Validate(args.purchasedProduct.receipt);
				// For informational purposes, we list the receipt(s)
				foreach (IPurchaseReceipt productReceipt in result) {
					Debug.Log("mysdk: IAP Product ID: " + productReceipt.productID);
					Debug.Log("mysdk: IAP Purchase date: " + productReceipt.purchaseDate);
					Debug.Log("mysdk: IAP Receipt: " + productReceipt);
                    mPurchaseDate = productReceipt.purchaseDate;
				}
                
			} catch (IAPSecurityException) {
				Debug.Log("mysdk: IAP Invalid receipt, not unlocking content");
				validPurchase = false;
			}
#elif UNITY_EDITOR
            mPurchaseDate = SdkUtil.DateTimeFromTimeStamp(SdkUtil.systemCurrentMiliseconds() / 1000);
#endif

            Debug.Log("mysdk: IAP ProcessPurchase:1" + validPurchase);
            if (validPurchase)
            {
                Debug.Log("mysdk: IAP ProcessPurchase:2" + args.purchasedProduct.definition.id);
#if !UNITY_EDITOR && ENABLE_AppsFlyer && ENABLE_VALIDATE_IAP

#if UNITY_ANDROID
                Debug.Log("mysdk: IAP Start31");
                productValidbyAppsflyer = args.purchasedProduct;
                appsflyerValidateAndroid(args.purchasedProduct.receipt, args.purchasedProduct.metadata.localizedPriceString, args.purchasedProduct.metadata.isoCurrencyCode);
#elif UNITY_IOS || UNITY_IPHONE
                Debug.Log("mysdk: IAP Start32");
                CurrStatePurchase = 0;
                handlePurchaseSuc(args.purchasedProduct);
#endif

#else
                CurrStatePurchase = 0;
                handlePurchaseSuc(args.purchasedProduct);
#endif
            }
            else
            {
                CurrStatePurchase = 0;
                handlePurchaseFaild(args.purchasedProduct, "validPurchase faild");
            }

            // Return a flag indicating whether this product has completely been received, or if the application needs 
            // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
            // saving purchased products to the cloud, and when that save is delayed. 
            return PurchaseProcessingResult.Complete;
        }

#if UNITY_ANDROID && ENABLE_AppsFlyer
        bool appsflyerValidateAndroid(string androidReceipt, string price, string currency)
        {
            try
            {
                byte[] makey = { 75, 71, 71, 64, 71, 104, 63, 76, 55, 92, 96, 102, 93, 96, 94, 60, 46, 108, 37, 55, 54, 70, 58, 59, 54, 57, 71, 59, 57, 73, 48, 57, 69, 65, 65, 65, 66, 102, 74, 66, 64, 80, 68, 64, 97, 38, 85, 34, 94, 73, 61, 62, 40, 75, 42, 87, 58, 32, 93, 105, 72, 77, 102, 106, 103, 90, 107, 88, 79, 108, 109, 98, 40, 49, 63, 46, 75, 83, 114, 71, 94, 35, 65, 82, 75, 92, 85, 33, 69, 51, 74, 68, 39, 89, 35, 38, 51, 37, 90, 59, 69, 66, 61, 93, 95, 83, 98, 31, 97, 49, 74, 98, 44, 90, 56, 88, 75, 80, 112, 96, 111, 38, 69, 106, 88, 101, 39, 96, 39, 81, 112, 43, 52, 108, 81, 113, 118, 72, 121, 97, 120, 83, 78, 75, 104, 101, 52, 116, 71, 83, 57, 56, 111, 47, 119, 66, 108, 97, 56, 81, 80, 69, 98, 78, 69, 57, 97, 119, 53, 77, 43, 65, 54, 48, 112, 84, 43, 80, 120, 80, 104, 51, 116, 119, 52, 81, 97, 118, 70, 48, 77, 69, 115, 83, 102, 82, 106, 122, 48, 48, 88, 74, 83, 105, 102, 101, 67, 65, 86, 54, 47, 116, 48, 68, 101, 67, 49, 118, 74, 84, 70, 100, 43, 116, 98, 74, 89, 80, 50, 99, 52, 57, 71, 69, 102, 117, 85, 117, 49, 65, 100, 89, 55, 50, 119, 75, 49, 85, 48, 103, 121, 106, 104, 76, 65, 119, 69, 87, 97, 74, 109, 79, 53, 80, 113, 66, 115, 102, 121, 55, 89, 75, 76, 112, 90, 68, 70, 107, 121, 74, 87, 117, 54, 73, 102, 122, 81, 84, 66, 85, 117, 67, 81, 90, 107, 104, 52, 117, 87, 98, 106, 70, 115, 66, 109, 77, 114, 68, 69, 77, 122, 100, 77, 78, 112, 108, 122, 108, 52, 54, 66, 65, 76, 76, 105, 85, 115, 84, 56, 117, 75, 47, 112, 117, 99, 117, 112, 80, 74, 116, 71, 83, 76, 75, 55, 110, 72, 48, 116, 107, 48, 100, 52, 85, 75, 67, 100, 69, 102, 57, 78, 54, 55, 118, 120, 53, 54, 50, 101, 111, 74, 77, 103, 72, 68, 112, 82, 108, 122, 98, 108, 103, 53, 107, 73, 81, 73, 68, 65, 81, 65, 66 };
                int[] paskey = { 8, 25, 35, 44, 61, 70, 80, 93, 111, 118, 131 };
                byte[] pasva = { 2, 11, 8, 1, 15, 10, 3, 15, 16, 10, 9 };
                string pkey = "";
                string psig = "";
                string pdata = "";
                if (makey != null && makey.Length > 20 && paskey != null && paskey.Length > 0 && pasva != null && pasva.Length == paskey.Length)
                {
                    pkey = "";
                    var sb = new System.Text.StringBuilder();
                    int idxp = 0;
                    for (int i = 0; i < makey.Length; i++)
                    {
                        char ch = (char)makey[i];
                        if (idxp < paskey.Length)
                        {
                            if (i >= paskey[idxp])
                            {
                                idxp++;
                            }
                        }
                        if (idxp < paskey.Length)
                        {
                            if (i < paskey[idxp])
                            {
                                ch = (char)(makey[i] + pasva[idxp]);
                            }
                        }
                        sb.Append(ch);
                    }
                    pkey = sb.ToString();
                    var listAndroidReceipt = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText(androidReceipt);
                    if (listAndroidReceipt != null || listAndroidReceipt.Count > 0)
                    {
                        foreach (KeyValuePair<string, object> itemReceipt in listAndroidReceipt)
                        {
                            if (itemReceipt.Key.Equals("Payload"))
                            {
                                var listAndPayload = (IDictionary<string, object>)MyJson.JsonDecoder.DecodeText((string)itemReceipt.Value);
                                if (listAndPayload != null || listAndPayload.Count > 0)
                                {
                                    foreach (KeyValuePair<string, object> itemPayload in listAndPayload)
                                    {
                                        if (itemPayload.Key.Equals("json"))
                                        {
                                            pdata = (string)itemPayload.Value;
                                        }
                                        else if (itemPayload.Key.Equals("signature"))
                                        {
                                            psig = (string)itemPayload.Value;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    AppsFlyerSDK.AppsFlyerAndroid.validateAndSendInAppPurchase(pkey, psig, pdata, price, currency, null, this);
                    return true;
                }
                else
                {
                    CurrStatePurchase = 0;
                    handlePurchaseSuc(productValidbyAppsflyer);
                    return true;
                }
            }
            catch (Exception ex)
            {
                CurrStatePurchase = 0;
                handlePurchaseFaild(null, "Exception validate appsflyer");
                return true;
            }
        }
#endif

        public void didFinishValidateReceipt(string description)
        {
            Debug.Log("mysdk: IAP appsflyer didFinishValidateReceipt=" + description);
            CurrStatePurchase = 0;
            handlePurchaseSuc(productValidbyAppsflyer);
        }

        public void didFinishValidateReceiptWithError(string description)
        {
            Debug.Log("mysdk: IAP appsflyer didFinishValidateReceiptWithError=" + description);
            CurrStatePurchase = 0;
            handlePurchaseFaild(null, description);
        }

        private void handlePurchaseFaild(Product p, string err)
        {
            Debug.Log(string.Format("mysdk: handlePurchaseFaild: Product: '{0}', PurchaseFailureReason: {1}",
                p.definition.storeSpecificId, err));
            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
            CurrStatePurchase = 0;
            string skulog = p.definition.id.Replace('.', '_');
            if (skulog.Length > 25)
            {
                skulog = skulog.Substring(skulog.Length - 25);
            }
            FIRhelper.logEvent($"IAP_fail_{skulog}");

            if (_callback != null)
            {
                PurchaseCallback pcb = new PurchaseCallback(0, p.definition.id);
                _callback(pcb);
            }
        }

        private void handlePurchaseSuc(Product p)
        {
            SDKManager.Instance.PopupShowFirstAds.gameObject.SetActive(false);
            CurrStatePurchase = 0;
            if (!SDKManager.Instance.isDeviceTest())
            {
#if FIRBASE_ENABLE && UNITY_ANDROID && !UNITY_EDITOR
                Dictionary<string, object> iapAndParam = new Dictionary<string, object>();
                iapAndParam.Add("product_id", p.definition.id);
                iapAndParam.Add("quantity", p.definition.id);
                iapAndParam.Add("price", (double)p.metadata.localizedPrice);
                iapAndParam.Add("currency", p.metadata.isoCurrencyCode);
                iapAndParam.Add("value", (double)p.metadata.localizedPrice);
                FIRhelper.logEvent("in_app_purchase" , iapAndParam);
#endif
                Dictionary<string, object> iapParam = new Dictionary<string, object>();
                iapParam.Add("product_id", p.definition.id);
                iapParam.Add("quantity", p.definition.id);
                iapParam.Add("price", (double)p.metadata.localizedPrice);
                iapParam.Add("currency", p.metadata.isoCurrencyCode);
                iapParam.Add("value", (double)p.metadata.localizedPrice);
                FIRhelper.logEvent($"IAP_suc_purchase", iapParam);

                string skulog = p.definition.id.Replace('.', '_');
                if (skulog.Length > 25)
                {
                    skulog = skulog.Substring(skulog.Length - 25);
                }
                FIRhelper.logEvent($"IAP_suc_{skulog}");
                AppsFlyerHelperScript.logPurchase(p.definition.id, p.metadata.localizedPrice.ToString(), p.metadata.isoCurrencyCode);
                Myapi.LogEventApi.Instance().logInApp(p.definition.id, p.transactionID, "", p.metadata.isoCurrencyCode, (float)p.metadata.localizedPrice, buyWhere);
                Dictionary<string, string> dicparam = new Dictionary<string, string>();
                dicparam.Add("producId", p.definition.id);
                AdjustHelper.LogEvent(AdjustEventName.Inapp, dicparam);
            }
            PlayerPrefs.SetInt("iap_isPurchase", 1);
            getRewardInapp(p);
            if (_callback != null)
            {
                PurchaseCallback pcb = new PurchaseCallback(1, p.definition.id);
                _callback(pcb);
            }
        }

        private void getRewardInapp(Product pro)
        {
            if (listCurr.listWithSku.ContainsKey(pro.definition.id))
            {
                PkgObject obPur = listCurr.listWithSku[pro.definition.id];
                if (obPur.typeInapp == 2)
                {
                    InappRcvObject rcv = obPur.rcv;
                    if (rcv.removeads == 1)
                    {
                        AdsHelper.setRemoveAds();
                    }
                    _handleSub.onBuySubSuccess(obPur, pro);
                }
                else
                {
                    if (obPur.periodSub > 0)
                    {
                        obPur.countRcvDaily = 1;
                        obPur.dayPurchased = mPurchaseDate.Year * 365 + mPurchaseDate.DayOfYear;
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_dayrcv", obPur.countRcvDaily);
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_daypur", obPur.dayPurchased);
                        PlayerPrefsBase.Instance().setInt($"iap_{obPur.id}_dayrcvpur", obPur.dayPurchased);
                        Debug.Log($"mysdk: IAP getRewardInapp day buy={mPurchaseDate} daycount={obPur.dayPurchased}");
                        handleFakeSub(obPur.id, 1);
                    }
                    InappRcvObject rcv = obPur.rcv;
                    if (rcv.removeads == 1)
                    {
                        AdsHelper.setRemoveAds();
                    }

                    int va = rcv.getMoney("gold");
                    if (va > 0)
                    {
                        //add gold
                    }

                    var items = rcv.getItems();
                    if (items != null)
                    {
                        //for (int i = 0; i < items.Count; i++)
                        //{
                        //    KeyValuePair<string, int> key = items.ElementAt(i);
                        //    ItemData.ItemName itemName = ItemData.ItemName.ItemIronHammer;
                        //    if (Enum.TryParse(key.Key, out itemName))
                        //    {
                        //        //success
                        //        PlayerInfo.SetItemAmount(itemName, key.Value);
                        //    }
                        //    else
                        //    {
                        //        Debug.Log($"Parse enum fail: {key.Key} : {key.Value}");
                        //        continue;  //fail
                        //    }
                        //}
                    }
                }
            }
            else
            {
                Debug.Log("mysdk: IAP buy success but list not contain sku = " + pro.definition.id);
            }
        }

        private string parIdItem(string pkg)
        {
            string idItem = "";
            int n = pkg.LastIndexOf('.');
            if (n >= 0 && n < (pkg.Length - 1))
            {
                idItem = pkg.Substring(n + 1);
            }
            else
            {
                n = pkg.Length;
                n = n - 15;
                if (n > 0)
                {
                    idItem = pkg.Substring(n);
                }
                else
                {
                    idItem = pkg;
                }
            }

            return idItem;
        }

        string getDateFromReceipt(string receipt)
        {
            string re = "";

            return re;
        }

        #endregion

#endif
    }
}
