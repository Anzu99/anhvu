//#define ENABLE_Gadsme
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using mygame.sdk;
#if ENABLE_Gadsme
using Gadsme;
#endif
public class GadsmeHelper : MonoBehaviour
{
    public const int maxdis = 90;
    public const int mindis = 50;
    public const int radiousCheck = 6;
    private static GadsmeHelper Instance = null;
    private Dictionary<GadsmeType, GadsmeObjectWithType> listAdsFree;
    private Dictionary<GadsmeType, GadsmeObjectWithType> listAdsUse;
    private List<Vector3> listMem4Check;

    public Dictionary<GadsmeType, TTWithTypeGadsme> dicDefault = new Dictionary<GadsmeType, TTWithTypeGadsme>();

    public Camera mainCamera;
    public Texture2D textureDefault;
    public float lenghtRayClick { get; private set; }

    public GadsmeObject[] AdsPrefabs;
    public List<GadsmePlacementIdWithType> listPlacementsWithType = new List<GadsmePlacementIdWithType>();


    private Vector3 lastPos;
    private Vector3 currPos;

    private string _gdprString;

    public static int cf_gadsme_alshow = 1;
    public static bool Gadsme_clicking = false;
    int stateClick = 0;

    bool isEnableClick = false;
    Vector2 posClick = Vector2.zero;
    string nameUiObIgnore = "";
#if ENABLE_Gadsme
    GadsmePlacement placementClick = null;
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            configDefault();
            listAdsFree = new Dictionary<GadsmeType, GadsmeObjectWithType>();
            listAdsUse = new Dictionary<GadsmeType, GadsmeObjectWithType>();
            listMem4Check = new List<Vector3>();
#if ENABLE_Gadsme
            GameAdsHelperBridge.CBRequestGDPR += onShowCmp;
#endif
            for (int i = 0; i < AdsPrefabs.Length; i++)
            {
                AdsPrefabs[i].initAds(textureDefault);
                GadsmeObjectWithType awtfree = new GadsmeObjectWithType();
                awtfree.listAds.Add(AdsPrefabs[i]);
                listAdsFree.Add(AdsPrefabs[i].adType, awtfree);
                AdsPrefabs[i].gameObject.SetActive(false);
                GadsmeObjectWithType awtuse = new GadsmeObjectWithType();
                listAdsUse.Add(AdsPrefabs[i].adType, awtuse);
            }
            textureDefault.name = "gadsmemygame";

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            if (this != Instance) Destroy(gameObject);
        }
    }
    void Start()
    {
#if ENABLE_Gadsme
        GadsmeSDK.SetMainCamera(mainCamera);
        GadsmeSDK.Init();
        GadsmeSDK.SetInteractionsEnabled(false);
        if (PlayerPrefs.GetInt("cf_gadsme_enable", 1) == 1 && PlayerPrefs.GetInt("cf_gadsme_CMP", 1) == 1 && PlayerPrefs.GetInt("mem_show_CMP", 0) <= 0)
        {
            Debug.Log($"mysdk: gadsme Start");
#if UNITY_IOS || UNITY_IPHONE

#if !UNITY_EDITOR
            GadsmeSDK.SetAllowConsentDialog(false);
            //mygame.sdk.GameHelper.showCMP();
#endif

#elif UNITY_ANDROID

#if !UNITY_EDITOR
            mygame.sdk.GameHelper.showCMP();
#endif

#endif
        }
#endif
    }

    public static void onGamePhaseChange(Camera newCamera)
    {
#if ENABLE_Gadsme
        if (Instance != null)
        {
            Instance.mainCamera = newCamera;
        }
        GadsmeSDK.SetMainCamera(newCamera);
#endif
    }

#if ENABLE_Gadsme
    private void Update()
    {
        if (listAdsUse.Count > 0 && isEnableClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                placementClick = null;
                posClick = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = new Vector2(posClick.x, posClick.y);

                //check ui elements
                bool isIgnoreClick = false;
                if (EventSystem.current != null)
                {
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                    foreach (var re in results)
                    {
                        Debug.Log($"mysdk: gadsme click 000 ui=" + re.gameObject.name);
                        if (!nameUiObIgnore.Contains(re.gameObject.name))
                        {
                            isIgnoreClick = true;
                            break;
                        }
                    }
                }
                if (!isIgnoreClick)
                {
                    Ray ray;
                    if (mainCamera != null)
                    {
                        ray = mainCamera.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                    }
                    else
                    {
                        if (Camera.main != null)
                        {
                            ray = Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
                        }
                        else
                        {
                            return;
                        }
                    }
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, lenghtRayClick, LayerMask.GetMask("AdmixClick")))
                    {
                        placementClick = hit.collider.GetComponent<GadsmePlacement>();
                    }
                }
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (placementClick != null)
                {
                    Vector2 dclick = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - posClick;
                    if (Mathf.Abs(dclick.x) <= 10 && Mathf.Abs(dclick.y) <= 10)
                    {
                        placementClick.Interact();
                        onclickGadsme();
                    }
                }
            }
        }
    }
#endif
    private void OnDestroy()
    {
        Instance = null;
        listAdsFree.Clear();
        listAdsUse.Clear();
        listMem4Check = null;
    }

    public static GadsmePlacementChannel getPlacementId(GadsmePlacementType type)
    {
        GadsmePlacementChannel re = null;
        foreach (var item in Instance.listPlacementsWithType)
        {
            if (item.type == type)
            {
                if (item.idx < item.listPlacements.Count)
                {
                    re = item.listPlacements[item.idx];
                    item.idx++;
                    if (item.idx >= item.listPlacements.Count)
                    {
                        item.idx = 0;
                    }
                }
                break;
            }
        }
        return re;
    }

    public static void initClick(bool isClick, float lenghtRay, string nameUiIgnore = "")
    {
#if ENABLE_Gadsme
        if (Instance != null)
        {
            Debug.Log($"mysdk: gadsme initClick isClick={isClick} lenghtRay={lenghtRay}");
            int tclick = PlayerPrefs.GetInt("cf_gadsme_click", 1);
            if (tclick <= 0)
            {
                Instance.isEnableClick = false;
                return;
            }
            if (mygame.sdk.AdsHelper.isRemoveAds == 1)
            {
                // isClick = false;
            }
            Debug.Log($"mysdk: gadsme initClick 2 isClick={isClick} lenghtRay={lenghtRay}");
            if (!isClick)
            {
                Instance.isEnableClick = false;
                Instance.lenghtRayClick = lenghtRay;
            }
            else
            {
                Instance.isEnableClick = true;
                Instance.lenghtRayClick = lenghtRay;
                Instance.nameUiObIgnore = nameUiIgnore;
                Debug.Log($"mysdk: gadsme initClick 22");
            }
        }
#endif
    }

    public void onclickGadsme()
    {
        mygame.sdk.FIRhelper.logEvent("Gadsme_click_ui");
        if (PlayerPrefs.GetInt("cf_gadsme_click", 1) > 0)
        {
            if (GameHelper.Instance != null)
            {
                Gadsme_clicking = true;
                stateClick = 1;
                GameHelper.Instance.setAllowShowOpenAd(false);
                GameHelper.Instance.isAlowShowOpen = false;
                StartCoroutine(waitResetFlagShowOpenAds());
                Debug.Log($"mysdk: gadsme onclickGadsme Gadsme_clicking={Gadsme_clicking}");
            }
        }
    }

    IEnumerator waitResetFlagShowOpenAds()
    {
        yield return new WaitForSeconds(3.5f);
        if (stateClick == 1)
        {
            stateClick = 0;
            Gadsme_clicking = false;
            GameHelper.Instance.setAllowShowOpenAd(true);
            GameHelper.Instance.isAlowShowOpen = true;
            Debug.Log($"mysdk: gadsme click but not openLink");
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            if (Gadsme_clicking)
            {
                Debug.Log($"mysdk: gadsme OnApplicationPause Gadsme_clicking={Gadsme_clicking}");
                Gadsme_clicking = false;
                stateClick = 0;
                StartCoroutine(waitResetFlagShowOpenAdsAfterOpenLink());
            }
        }
        else
        {
            if (stateClick == 1 && Gadsme_clicking)
            {
                Debug.Log($"mysdk: gadsme OnApplicationPause Gadsme openLink");
                stateClick = 0;
            }
        }
    }

    IEnumerator waitResetFlagShowOpenAdsAfterOpenLink()
    {
        yield return new WaitForSeconds(0.35f);
        GameHelper.Instance.setAllowShowOpenAd(true);
        GameHelper.Instance.isAlowShowOpen = true;
        Debug.Log($"mysdk: gadsme setFlag ads open");
    }

    public static void ShowCMPIOS()
    {
#if ENABLE_Gadsme
        if (PlayerPrefs.GetInt("cf_gadsme_enable", 1) == 1 && PlayerPrefs.GetInt("cf_gadsme_CMP", 1) == 1 && PlayerPrefs.GetInt("mem_show_CMP", 0) <= 0)
        {
#if (UNITY_IOS || UNITY_IPHONE) && !UNITY_EDITOR
            Debug.Log($"mysdk: gadsme ShowCMPIOS");
            mygame.sdk.GameHelper.showCMP();
#endif
        }
#endif
    }

    public static GadsmeObject genAd(GadsmeType type, Vector3 pos, Vector3 forward, Transform _target = null, int stateLookat = 0, bool isFloowY = false)
    {
#if ENABLE_Gadsme
        if (Instance != null && PlayerPrefs.GetInt("cf_gadsme_enable", 1) == 1)
        {
            GadsmeObject ad = Instance.getAdsWithType(type, pos, forward, _target, stateLookat, isFloowY);
            Debug.Log($"mysdk: gadsme genAd count={Instance.listAdsUse.Count}");
            if (ad != null)
            {
                ad.transform.localScale = new Vector3(1, 1, 1);
            }

            return ad;
        }
        else
        {
            return null;
        }
#else
        return null;
#endif
    }
    private GadsmeObject getAdsWithType(GadsmeType type, Vector3 pos, Vector3 forward, Transform _target, int stateLookat, bool isFloowY)
    {
        if (listAdsUse[type].listAds.Count < 15)
        {
            if (listAdsFree[type].listAds.Count > 0)
            {
                GadsmeObject re = null;
                for (int i = 0; i < listAdsFree[type].listAds.Count; i++)
                {
                    if (listAdsFree[type].listAds[i].isLoaded())
                    {
                        re = listAdsFree[type].listAds[i];
                        listAdsFree[type].listAds.RemoveAt(i);
                        break;
                    }
                }
                if (re == null)
                {
                    re = listAdsFree[type].listAds[0];
                    listAdsFree[type].listAds.RemoveAt(0);
                }
                re.transform.position = pos;
                re.pos = pos;
                re.forward = forward;
                re.target = _target;
                re.stateLoockat = stateLookat;
                listAdsUse[type].listAds.Add(re);
                re.gameObject.SetActive(true);
                re.setFollowY(isFloowY);
                re.enableMesh();
                return re;
            }
            else
            {
                var prefabsads = getPrefab(type);
                if (prefabsads != null)
                {
                    var re = Instantiate(prefabsads, pos, Quaternion.identity, prefabsads.transform.parent);
                    re.initAds(textureDefault);
                    for (int i = 0; i < re.listAdInfo.Count; i++)
                    {
                        re.listAdInfo[i].isLoadDefault = false;
                    }
                    re.transform.position = pos;
                    re.pos = pos;
                    re.forward = forward;
                    re.target = _target;
                    re.stateLoockat = stateLookat;
                    listAdsUse[type].listAds.Add(re);
                    re.gameObject.SetActive(true);
                    re.setFollowY(isFloowY);
                    re.enableMesh();
                    return re;
                }
            }
        }
        return null;
    }

    GadsmeObject getPrefab(GadsmeType type)
    {
        GadsmeObject re = null;
        for (int i = 0; i < AdsPrefabs.Length; i++)
        {
            if (AdsPrefabs[i].adType == type)
            {
                re = AdsPrefabs[i];
                break;
            }
        }

        return re;
    }
    public static void freeAll()
    {
        if (Instance != null)
        {
            foreach (var item in Instance.listAdsUse)
            {
                for (int i = (item.Value.listAds.Count - 1); i >= 0; i--)
                {
                    Instance.listAdsFree[item.Value.listAds[i].adType].listAds.Add(item.Value.listAds[i]);
                    item.Value.listAds[i].gameObject.SetActive(false);
                    item.Value.listAds.RemoveAt(i);
                }
            }

        }
    }
    public static void freeAd(GadsmeObject ad)
    {
        if (Instance != null)
        {
            if (ad != null)
            {
                if (Instance.listAdsUse[ad.adType].listAds.Contains(ad))
                {
                    Instance.listAdsUse[ad.adType].listAds.Remove(ad);
                    Instance.listAdsFree[ad.adType].listAds.Add(ad);
                    ad.gameObject.SetActive(false);
                }
            }
        }
    }


    public static void configDefault()
    {
        if (Instance != null)
        {
            Instance.dicDefault.Clear();
            string stringcf = PlayerPrefs.GetString("admix_v1_6x5", "");
            if (stringcf.Length > 0)
            {
                Debug.Log($"mysdk: gadsme admix_6x5=" + stringcf);
                string[] arrtype = stringcf.Split('#');
                TTWithTypeGadsme obType = new TTWithTypeGadsme();
                for (int i = 0; i < arrtype.Length; i++)
                {
                    string[] arrad = arrtype[i].Split(';');
                    if (arrad.Length > 0)
                    {
                        TTMyGadsme dd = new TTMyGadsme();
                        obType.list.Add(dd);
                        string[] imgs = arrad[0].Split(',');
                        for (int j = 0; j < imgs.Length; j++)
                        {
                            dd.urlImgs.Add(imgs[j]);
                        }
                        if (arrad.Length == 2)
                        {
                            dd.urlStore = arrad[1];
                        }
                    }
                }
                obType.idx = PlayerPrefs.GetInt("gadsme_6x5_idx", 0);
                if (obType.idx >= obType.list.Count)
                {
                    obType.idx = 0;
                }
                Instance.dicDefault.Add(GadsmeType.Size6x5, obType);
            }
            if (Instance.dicDefault != null && Instance.dicDefault.Count > 0)
            {
                cf_gadsme_alshow = PlayerPrefs.GetInt("cf_gadsme_alshow", 1);
            }
            else
            {
                cf_gadsme_alshow = 0;
            }
        }
    }

    public static TTMyGadsme getTTDefault(GadsmeType type, Action<TTMyGadsme, Texture2D> cb = null)
    {
        TTMyGadsme re = null;
        Debug.Log($"mysdk: gadsme getTTDefault");
        if (Instance != null)
        {
            if (Instance.dicDefault.ContainsKey(type))
            {
                TTWithTypeGadsme ob = Instance.dicDefault[type];
                if (ob.list.Count > 0)
                {
                    if (ob.idx >= ob.list.Count)
                    {
                        ob.idx = 0;
                    }
                    Debug.Log($"mysdk: gadsme getTTDefault idx=" + ob.idx);
                    re = ob.list[ob.idx];
                    if (ob.list[ob.idx].textures != null && ob.list[ob.idx].textures.Count > 0 && ob.list[ob.idx].textures[0] != null)
                    {
                        Debug.Log($"mysdk: gadsme getTTDefault 1");
                        cb?.Invoke(ob.list[ob.idx], ob.list[ob.idx].textures[0]);
                    }
                    else
                    {
                        int idx = ob.idx;
                        Debug.Log($"mysdk: gadsme getTTDefault url=" + ob.list[ob.idx].urlImgs[0]);
                        ImageLoader.loadImageTexture(ob.list[idx].urlImgs[0], 60, 50, (tt) =>
                        {
                            tt.name = "gadsmemygame";
                            if (ob.list[idx].textures.Count > 0)
                            {
                                ob.list[idx].textures[0] = tt;
                            }
                            else
                            {
                                ob.list[idx].textures.Add(tt);
                            }
                            cb?.Invoke(ob.list[idx], tt);
                        });
                    }
                    ob.idx++;
                    if (ob.idx >= ob.list.Count)
                    {
                        ob.idx = 0;
                    }
                    if (type == GadsmeType.Size6x5)
                    {
                        PlayerPrefs.SetInt("gadsme_6x5_idx", ob.idx);
                    }
                }
            }
        }
        return re;
    }

    public void onShowCmp(int state, string des)
    {
        if (state == 0)
        {
            onShowCmpNative();
        }
        else if (state == 1)
        {
            if (des != null && des.Length > 5)
            {
                onCMPOK(des);
            }
        }
    }

    public void onShowCmpNative()
    {
        Debug.Log($"mysdk: gadsme onShowCmpiOS");
        PlayerPrefs.SetInt("mem_show_CMP", 1);
    }

    public void onCMPOK(string iABTCv2String)
    {
        Debug.Log($"mysdk: gadsme onCMPOK=" + iABTCv2String);
#if ENABLE_Gadsme && !UNITY_EDITOR
        Debug.Log($"mysdk: gadsme onCMPOK call set");
        GadsmeSDK.SetGdprConsentString(iABTCv2String);
#endif
    }
}

[Serializable]
public class GadsmePlacementIdWithType
{
    public GadsmePlacementType type = GadsmePlacementType.P_300x250;
    public List<GadsmePlacementChannel> listPlacements = new List<GadsmePlacementChannel>();
    public int idx = 0;
}

[Serializable]
public class GadsmePlacementChannel
{
    public string placementId;
    public int channel;
}
public class TTWithTypeGadsme
{
    public int idx = 0;
    public List<TTMyGadsme> list = new List<TTMyGadsme>();
}

public class TTMyGadsme
{
    public List<string> urlImgs = new List<string>();
    public string urlStore = "";
    public List<Texture2D> textures = new List<Texture2D>();
}

public class GadsmeObjectWithType
{
    public List<GadsmeObject> listAds = new List<GadsmeObject>();
}
