using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopUI : MonoBehaviour
{
    #region ConvertMoney
    public static string ConvertMoneyToString(long money)
    {
        if (money >= 1000000)
        {
            return (money / 1000000) + "M";
        }
        return money.ToString();
    }
    #endregion
}
