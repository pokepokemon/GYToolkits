using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayIDs
{
    public const string NoAD = "no_ad";
    public const string GetDiamond1 = "getDiamond1";
    public const string GetDiamond2 = "getDiamond2";
    public const string GetDiamond3 = "getDiamond3";
    public const string GetDiamond4 = "getDiamond4";
    public const string GetDiamond5 = "getDiamond5";
    public const string Newcomer = "beginner_package";
    public const string SmallGift = "small_gift_packs";
    public const string BigGift = "grand_gift_packs";
    public const string Idle = "offline_revenue";
    
    public static HashSet<string> nonConsumeSet = new HashSet<string>()
    {
        NoAD, Idle
    };

    public static bool IsNonConsume(string id)
    {
        return nonConsumeSet.Contains(id);
    }
}
