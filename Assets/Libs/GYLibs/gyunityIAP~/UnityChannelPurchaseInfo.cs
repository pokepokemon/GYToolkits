using System;
using UnityEngine;

[Serializable]
public class UnityChannelPurchaseInfo
{
    /// <summary>
    /// Corresponds to storeSpecificId
    /// </summary>
    public string productCode;

    /// <summary>
    /// Corresponds to transactionId
    /// </summary>
    public string gameOrderId;

    public string orderQueryToken;
}