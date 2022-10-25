using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using mygame.sdk;
#if ENABLE_Gadsme
using Gadsme;
#endif
public enum GadsmeType
{
    Size6x5 = 0,
    Size_3f_6x5,
    Size32x5
}

public enum GadsmePlacementType
{
    P_300x250 = 0,
    P_300x50
}

public class GadsmeObject : MonoBehaviour
{
    public GadsmePlacementType placementType;
    public GadsmeType adType;
    [HideInInspector] public Vector3 pos;
    [HideInInspector] public Vector3 forward;
    public List<GadsmeInfo> listAdInfo;
    public List<GameObject> listObBoard;
    Dictionary<GameObject, GadsmeInfo> dicAds = null;
    [HideInInspector] public Transform target;
    [HideInInspector] public int stateLoockat = 0;
    bool isAdLoaded = false;
    Vector3 postouch;
    TTMyGadsme obTTDefault = null;
    bool isgetAllImg = false;
    int idxAniDf = 0;
    public bool disableObj = false;
    bool isFollowY = false;
    float yTagetOrigin = 0;
    float preYtaget = 0;
    float yWillSet = 0;
    float tLookAt = 2000;
    static bool isSetTouch = false;

    public void initAds(Texture2D ttdf)
    {
        if (dicAds == null)
        {
            isgetAllImg = false;
            isFollowY = false;
            dicAds = new Dictionary<GameObject, GadsmeInfo>();
            GadsmePlacementChannel pidc = GadsmeHelper.getPlacementId(placementType);
            for (int i = 0; i < listAdInfo.Count; i++)
            {
                dicAds.Add(listAdInfo[i].mesh.gameObject, listAdInfo[i]);
#if ENABLE_Gadsme
                listAdInfo[i].placement = listAdInfo[i].mesh.GetComponent<GadsmePlacement>();
                if (pidc != null)
                {
                    listAdInfo[i].placement.placementId = pidc.placementId;
                    listAdInfo[i].placement.adChannelNumber = pidc.channel;
                }
                listAdInfo[i].placement.RenderChangeEvent += onRenderChangeEvent;
                listAdInfo[i].placement.InteractEvent += onInteractEvent;
                listAdInfo[i].placement.EnableChangeEvent += onEnableChangeEvent;
                listAdInfo[i].placement.ContentFailedEvent += onContentFailedEvent;
                listAdInfo[i].placement.ContentLoadedEvent += onContentLoadedEvent;
#endif
            }
            hideAds();
        }
#if ENABLE_Gadsme
        if (ttdf != null)
        {
            for (int i = 0; i < listAdInfo.Count; i++)
            {
                GadsmePlacement pp = listAdInfo[i].placement;
                if (pp != null)
                {
                    //vvvpp.fallbackTexture = ttdf;
                }
                if (listAdInfo[i].mesh.material.mainTexture != null)
                {
                    //vvvlistAdInfo[i].mesh.material.mainTexture = ttdf;
                }
            }
        }
#endif
        isAdLoaded = false;
        foreach (var item in dicAds)
        {
            if (item.Value.isAdLoaded)
            {
                isAdLoaded = true;
                break;
            }
        }
        if (GadsmeHelper.cf_gadsme_alshow == 1)
        {
            isAdLoaded = true;
        }
        if (isAdLoaded)
        {
            showAds();
        }
        else
        {
            hideAds();
        }
    }

    // private void OnEnable()
    // {
    //     Invoke("setTextDefault", 1);
    //     Debug.Log($"mysdk: gadsme OnEnable isAdLoaded={isAdLoaded}");
    // }

    public void enableMesh()
    {
        // if (listMesh != null)
        // {
        //     for (int i = 0; i < listMesh.Count; i++)
        //     {
        //         listMesh[i].enabled = true;
        //     }
        // }

        yWillSet = 0;
    }

    public void setFollowY(bool isfl, float yTaget = -10000)
    {
        isFollowY = isfl;
        if (target != null)
        {
            if (yTaget <= -10000)
            {
                yTagetOrigin = target.transform.position.y;
            }
            else
            {
                yTagetOrigin = yTaget;
            }

            preYtaget = yTagetOrigin;
        }
    }

    void setTextDefault()
    {
#if ENABLE_Gadsme
        for (int i = 0; i < listAdInfo.Count; i++)
        {
            int idx = i;
            if (!listAdInfo[i].isLoadDefault)
            {
                obTTDefault = GadsmeHelper.getTTDefault(adType, (obad, tt) =>
                {
                    showAds();
                    if (listAdInfo[idx].placement != null)
                    {
                        //vvvlistAdInfo[idx].placement.fallbackTexture = tt;
                    }
                    if (listAdInfo[idx].mesh.material.mainTexture != null)
                    {
                        listAdInfo[idx].isLoadDefault = true;
                        if (listAdInfo[idx].mesh.material.mainTexture.name.CompareTo("gadsmemygame") == 0)
                        {
                            //vvvlistAdInfo[idx].mesh.material.mainTexture = tt;
                        }
                    }
                    if (obad != null && obad.urlImgs.Count > 1)
                    {
                        for (int p = 1; p < obad.urlImgs.Count; p++)
                        {
                            int mp = p;
                            obad.textures.Add(null);
                            ImageLoader.loadImageTexture(obad.urlImgs[p], 100, 100, (ntt) =>
                            {
                                ntt.name = "gadsmemygame";
                                obad.textures[mp] = ntt;
                                bool isall = true;
                                for (int ll = 0; ll < obad.textures.Count; ll++)
                                {
                                    if (obad.textures[ll] == null)
                                    {
                                        isall = false;
                                        break;
                                    }
                                }
                                isgetAllImg = isall;
                                if (isall)
                                {
                                    StartCoroutine(AniDefault());
                                }
                            });
                        }
                    }
                });
            }
        }
#endif
    }

    void showAds()
    {
        if (!disableObj)
        {
            foreach (var item in listObBoard)
            {
                item.SetActive(true);
            }
        }
        foreach (var item in dicAds)
        {
            item.Value.mesh.transform.parent.GetComponent<MeshRenderer>().enabled = true;
            item.Value.mesh.enabled = true;
        }
    }

    void hideAds()
    {
        // Debug.Log($"mysdk: gadsme hideAds {gameObject.name}");
        foreach (var item in dicAds)
        {
            item.Value.mesh.transform.parent.GetComponent<MeshRenderer>().enabled = false;
            item.Value.mesh.enabled = false;
        }
        foreach (var item in listObBoard)
        {
            item.SetActive(false);
        }
    }

    public bool isLoaded()
    {
        isAdLoaded = false;
        foreach (var item in dicAds)
        {
            if (item.Value.isAdLoaded)
            {
                isAdLoaded = true;
                break;
            }
        }
        return isAdLoaded;
    }

    private void OnDestroy()
    {
        foreach (var item in dicAds)
        {
            item.Value.mesh = null;
        }
        dicAds.Clear();
        dicAds = null;
    }

#if ENABLE_Gadsme

    IEnumerator AniDefault()
    {
        yield return new WaitForSeconds(0.1f);
        idxAniDf++;
        bool isc = false;
        if (idxAniDf >= obTTDefault.textures.Count)
        {
            idxAniDf = 0;
            isc = true;
        }
        if (obTTDefault.textures[idxAniDf] != null)
        {
            for (int i = 0; i < listAdInfo.Count; i++)
            {
                if (listAdInfo[i].placement != null)
                {
                    //vvvlistAdInfo[i].placement.fallbackTexture = obTTDefault.textures[idxAniDf];
                }
                if (listAdInfo[i].mesh.material.mainTexture != null)
                {
                    listAdInfo[i].isLoadDefault = true;
                    if (listAdInfo[i].mesh.material.mainTexture.name.CompareTo("gadsmemygame") == 0)
                    {
                        //vvvlistAdInfo[i].mesh.material.mainTexture = obTTDefault.textures[idxAniDf];
                    }
                }
            }
        }
        if (isc)
        {
            yield return new WaitForSeconds(3.5f);
        }
        StartCoroutine(AniDefault());
    }

    private void Update()
    {
        //transform.position += forward * 2 * Time.deltaTime;
        if (target != null)
        {
            if (isFollowY)
            {
                float dy = target.transform.position.y - yTagetOrigin;
                if (dy > 1f || dy < -1f)
                {
                    dy = target.transform.position.y - preYtaget;
                    preYtaget = target.transform.position.y;
                    yWillSet += dy;
                }
                if (yWillSet > 0.1f || yWillSet < -0.1f)
                {
                    if (yWillSet > 0)
                    {
                        dy = 7.5f * Time.deltaTime;
                        if (yWillSet < dy)
                        {
                            dy = yWillSet;
                            yWillSet = 0;
                        }
                        else
                        {
                            yWillSet -= dy;
                        }
                    }
                    else
                    {
                        dy = -7.5f * Time.deltaTime;
                        if (yWillSet > dy)
                        {
                            dy = yWillSet;
                            yWillSet = 0;
                        }
                        else
                        {
                            yWillSet -= dy;
                        }
                    }
                    transform.position = new Vector3(transform.position.x, transform.position.y + dy, transform.position.z);
                }
            }
            if (stateLoockat > 0)
            {
                transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
                if (stateLoockat == 2)
                {
                    stateLoockat = 0;
                }
            }
        }
    }
    public void onRenderChangeEvent(GadsmePlacement ad, bool ischange)
    {
        Debug.Log($"mysdk: gadsme onRenderChangeEvent={ad.placementId}, ischange={ischange}");
    }

    public void onInteractEvent(GadsmePlacement ad)
    {
        Debug.Log($"mysdk: gadsme onInteractEvent={ad.placementId}");
    }

    public void onEnableChangeEvent(GadsmePlacement ad, bool ischange)
    {
        Debug.Log($"mysdk: gadsme onEnableChangeEvent={ad.placementId}, ischange={ischange}");
    }

    public void onContentFailedEvent(GadsmePlacement ad)
    {
        Debug.Log($"mysdk: gadsme onContentFailedEvent={ad.placementId}");
        if (dicAds.ContainsKey(ad.gameObject))
        {
            //dicAds[ad.gameObject].isAdLoaded = false;
        }
        isAdLoaded = false;
        foreach (var item in dicAds)
        {
            if (item.Value.isAdLoaded)
            {
                isAdLoaded = true;
                break;
            }
        }
        if (isAdLoaded)
        {
            showAds();
        }
        else
        {
            // hideAds();
        }
    }

    public void onContentLoadedEvent(GadsmePlacement ad)
    {
        Debug.Log($"mysdk: gadsme onContentLoadedEvent={ad.placementId}");
        isAdLoaded = true;
        if (dicAds.ContainsKey(ad.gameObject))
        {
            dicAds[ad.gameObject].isAdLoaded = true;
        }
        showAds();
    }
#endif

}

[Serializable]
public class GadsmeInfo
{
    public MeshRenderer mesh;
#if ENABLE_Gadsme
    public GadsmePlacement placement;
#endif
    [HideInInspector] public bool isLoadDefault = false;
    [HideInInspector] public bool isAdLoaded;
}
