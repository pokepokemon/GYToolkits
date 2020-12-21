#define RECEIPT_VALIDATION
#if OPEN_EDITOR_COMPILER

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using GYLib;
using UnityEngine.Events;


public interface IStoreListener
{

}


public class IDs
{
    public void Add(params object[] args)
    {

    }
}

public class ProductType
{
    public const string NonConsumable = "";
    public const string Consumable = "";
}

public class AppleAppStore
{
    public const string Name = "";
}

public class GooglePlay
{
    public const string Name = "";
}

public class Product
{
    public string receipt;
    public string transactionID;
    public Definition definition;
    public Meta metadata;

    public class Definition
    {
        public string id;
    }

    public class Meta
    {
        public string localizedPriceString;
    }
}

public class InitializationFailureReason
{

}

public class PurchaseFailureReason
{

}

public class IPurchaseReceipt
{

}

public class ConfigurationBuilder
{
    public void AddProduct(params object[] args)
    {

    }
}

/// <summary>
/// Unity IAP Support Base on version 1.2.0
/// </summary>
public class IAPManager : MonoSingleton<IAPManager>, IStoreListener
{
    public bool isInited = false;
    public bool isIniting = false;

    public string initFailReason = null;

    public string versionStr = "";

    /// <summary>
    /// 支付成功时调用
    /// </summary>
    public UnityAction OnSuccessInitedCall;

    /// <summary>
    /// 开始支付某物时调用
    /// </summary>
    public UnityAction<Product> OnStartPurchaseCall;

    /// <summary>
    /// 初始化失败时调用
    /// </summary>
    public UnityAction<InitializationFailureReason> OnInitFailedCall;

    /// <summary>
    /// 支付失败回调
    /// </summary>
    public UnityAction<Product, PurchaseFailureReason> OnPurchaseFailedCall;

    /// <summary>
    /// 支付成功回调
    /// </summary>
    public UnityAction<Product> OnPurchaseSuccessCall;

    /// <summary>
    /// 服务器校验回调
    /// </summary>
    public UnityAction<IPurchaseReceipt> OnPurchaseValidate;

    /// <summary>
    /// 添加产品
    /// </summary>
    public UnityAction<ConfigurationBuilder> OnAddProductionList;

#pragma warning disable 0414
    private bool _isGooglePlayStoreSelected;
#pragma warning restore 0414

    private string _lastTransactionID;
    private bool _purchaseInProgress;
    
    //origin subscription here
    

    /// <summary>
    /// This will be called if an attempted purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
    }

    /// <summary>
    /// 开始初始化
    /// </summary>
    public void StartInit()
    {
    }

#if INTERCEPT_PROMOTIONAL_PURCHASES
    private void OnPromotionalPurchase(Product item) {
        Debug.Log("Attempted promotional purchase: " + item.definition.id);

        // Promotional purchase has been detected. Handle this event by, e.g. presenting a parental gate.
        // Here, for demonstration purposes only, we will wait five seconds before continuing the purchase.
        StartCoroutine(ContinuePromotionalPurchases());
    }

    private System.Collections.IEnumerator ContinuePromotionalPurchases()
    {
        Debug.Log("Continuing promotional purchases in 5 seconds");
        yield return new WaitForSeconds(5);
        Debug.Log("Continuing promotional purchases now");
        m_AppleExtensions.ContinuePromotionalPurchases (); // iOS and tvOS only; does nothing on Mac
    }
#endif

    public void PurchaseButtonClick(string productID)
    {
    }

    /// <summary>
    /// 新用户安装回包,重置
    /// </summary>
    public void RestoreButtonClick()
    {
    }
    
    /// <summary>
    /// 是否需要恢复按钮
    /// </summary>
    /// <returns></returns>
    public bool NeedRestoreButton()
    {
        return Application.platform == RuntimePlatform.IPhonePlayer ||
               Application.platform == RuntimePlatform.OSXPlayer ||
               Application.platform == RuntimePlatform.tvOS ||
               Application.platform == RuntimePlatform.WSAPlayerX86 ||
               Application.platform == RuntimePlatform.WSAPlayerX64 ||
               Application.platform == RuntimePlatform.WSAPlayerARM;
    }

    /// <summary>
    /// 获取品项
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Product GetProduct(string id)
    {
        return null;
    }

    /// <summary>
    /// 是否在支付进程中
    /// </summary>
    /// <returns></returns>
    public bool GetPurchaseInProgress()
    {
        return _purchaseInProgress;
    }
}

#endif