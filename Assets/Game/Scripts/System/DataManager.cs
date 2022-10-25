using System.Collections.Generic;
using UnityEngine;
using mygame.sdk;
using System.Linq;
using System;

public class DataManager : Singleton<DataManager>
{
    public static event System.Action<int, int, float, bool> OnAddCoin = delegate { };
    public static event System.Action<int, int, float> OnAddGem = delegate { };

    [Header("Player Stats")]
    [SerializeField] private int baseCoin = 0;

    private void Start()
    {

    }
    public int coin
    {
        get
        {
            return PlayerPrefs.GetInt("coin", baseCoin);
        }
        set
        {
            PlayerPrefs.SetInt("coin", value);
        }
    }

    public void AddCoin(int value, float delayTime, string where, bool showanim = true)
    {
        OnAddCoin(coin, coin + value, delayTime, showanim);
        GameRes.AddRes(RES_type.GOLD, value, where, true, 0);
        coin += value;
    }
}
