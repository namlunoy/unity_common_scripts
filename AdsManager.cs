using UnityEngine;
using System.Collections;
using System;

public enum AdsOption { BannerHide, BannerShow, ShowBillboard, RefreshBanner }

public class AdsManager : MonoBehaviour
{
    public static event Action<AdsOption> AdsAction;
    public static void DoAction(AdsOption option)
    {
        if(AdsAction != null)
            AdsAction(option);
        else
            print("AdsAction is null");
    }
}
