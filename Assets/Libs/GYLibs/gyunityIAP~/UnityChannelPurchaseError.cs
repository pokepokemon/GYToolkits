using System;
using static IAPManager;

[Serializable]
public class UnityChannelPurchaseError
{
    public string error;
    public UnityChannelPurchaseInfo purchaseInfo;
}