#define RECEIPT_VALIDATION

using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.Store; // UnityChannel
using GYLib;
using UnityEngine.Events;

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

    // Unity IAP objects
    private IStoreController _controller;

    private IAppleExtensions _appleExtensions;
    private ITransactionHistoryExtensions _transactionHistoryExtensions;
    private IGooglePlayStoreExtensions _googlePlayStoreExtensions;

#pragma warning disable 0414
    private bool _isGooglePlayStoreSelected;
#pragma warning restore 0414

    private string _lastTransactionID;
    private bool _purchaseInProgress;

#if RECEIPT_VALIDATION
    private CrossPlatformValidator validator;
#endif

    /// <summary>
    /// This will be called when Unity IAP has finished initialising.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _controller = controller;
        _appleExtensions = extensions.GetExtension<IAppleExtensions>();
        _transactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();
        _googlePlayStoreExtensions = extensions.GetExtension<IGooglePlayStoreExtensions>();
        // Sample code for expose product sku details for google play store
        // Key is product Id (Sku), value is the skuDetails json string
        //Dictionary<string, string> google_play_store_product_SKUDetails_json = _googlePlayStoreExtensions.GetProductJSONDictionary();
        // Sample code for manually finish a transaction (consume a product on GooglePlay store)
        //_googlePlayStoreExtensions.FinishAdditionalTransaction(productId, transactionId);
        
        //Here record Unity IAP version
        versionStr = "Unity version: " + Application.unityVersion + "\n" +
                           "IAP version: " + StandardPurchasingModule.k_PackageVersion;

        // 苹果 家长控制 Ask to buy 功能 异步询问调用
        _appleExtensions.RegisterPurchaseDeferredListener(OnDeferred);

        //origin subscription here

        // Sample code for expose product sku details for apple store
        //Dictionary<string, string> product_details = _appleExtensions.GetProductDetails();


        Debug.Log("Available items:");
        foreach (var item in controller.products.all)
        {
            if (item.availableToPurchase)
            {
                Debug.Log(string.Join(" - ",
                    new[]
                    {
                        item.metadata.localizedTitle,
                        item.metadata.localizedDescription,
                        item.metadata.isoCurrencyCode,
                        item.metadata.localizedPrice.ToString(),
                        item.metadata.localizedPriceString,
                        item.transactionID,
                        item.receipt
                    }));
#if INTERCEPT_PROMOTIONAL_PURCHASES
                // Set all these products to be visible in the user's App Store according to Apple's Promotional IAP feature
                // https://developer.apple.com/library/content/documentation/NetworkingInternet/Conceptual/StoreKitGuide/PromotingIn-AppPurchases/PromotingIn-AppPurchases.html
                m_AppleExtensions.SetStorePromotionVisibility(item, AppleStorePromotionVisibility.Show);
#endif

                //origin  subscription here
            }
        }

        isInited = true;
        isIniting = false;
        LogProductDefinitions();
        if (OnSuccessInitedCall != null)
        {
            OnSuccessInitedCall();
        }
    }

    //origin subscription here

    /// <summary>
    /// This will be called when a purchase completes.
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        Debug.Log("Purchase OK: " + e.purchasedProduct.definition.id);
        Debug.Log("Receipt: " + e.purchasedProduct.receipt);

        _lastTransactionID = e.purchasedProduct.transactionID;

#if RECEIPT_VALIDATION // Local validation is available for GooglePlay, Apple, and UnityChannel stores
        if (_isGooglePlayStoreSelected ||
            Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.tvOS) {
            try {
                var result = validator.Validate(e.purchasedProduct.receipt);
                Debug.Log("Receipt is valid. Contents:");
                foreach (IPurchaseReceipt productReceipt in result) {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate);
                    Debug.Log(productReceipt.transactionID);

                    if (OnPurchaseValidate != null)
                    {
                        OnPurchaseValidate(productReceipt);
                    }

                    GooglePlayReceipt google = productReceipt as GooglePlayReceipt;
                    if (null != google) {
                        Debug.Log(google.purchaseState);
                        Debug.Log(google.purchaseToken);
                    }

                    AppleInAppPurchaseReceipt apple = productReceipt as AppleInAppPurchaseReceipt;
                    if (null != apple) {
                        Debug.Log(apple.originalTransactionIdentifier);
                        Debug.Log(apple.subscriptionExpirationDate);
                        Debug.Log(apple.cancellationDate);
                        Debug.Log(apple.quantity);
                    }

                    // For improved security, consider comparing the signed
                    // IPurchaseReceipt.productId, IPurchaseReceipt.transactionID, and other data
                    // embedded in the signed receipt objects to the data which the game is using
                    // to make this purchase.
                }
            } catch (IAPSecurityException ex) {
                Debug.Log("Invalid receipt, not unlocking content. " + ex);
                return PurchaseProcessingResult.Complete;
            }
        }
#endif

        // Unlock content from purchases here.
        // Indicate if we have handled this purchase.
        //   PurchaseProcessingResult.Complete: ProcessPurchase will not be called
        //     with this product again, until next purchase.
        //   PurchaseProcessingResult.Pending: ProcessPurchase will be called
        //     again with this product at next app launch. Later, call
        //     _controller.ConfirmPendingPurchase(Product) to complete handling
        //     this purchase. Use to transactionally save purchases to a cloud
        //     game service.
#if DELAY_CONFIRMATION
        StartCoroutine(ConfirmPendingPurchaseAfterDelay(e.purchasedProduct));
        return PurchaseProcessingResult.Pending;
#else
        if (OnPurchaseSuccessCall != null)
        {
            OnPurchaseSuccessCall(e.purchasedProduct);
        }

        _purchaseInProgress = false;
        return PurchaseProcessingResult.Complete;
#endif
    }

#if DELAY_CONFIRMATION
    private HashSet<string> _pendingProducts = new HashSet<string>();

    private System.Collections.IEnumerator ConfirmPendingPurchaseAfterDelay(Product p)
    {
        _pendingProducts.Add(p.definition.id);
        Debug.Log("Delaying confirmation of " + p.definition.id + " for 5 seconds.");

		var end = Time.time + 5f;

		while (Time.time < end) {
			yield return null;
			var remaining = Mathf.CeilToInt (end - Time.time);
		}

        Debug.Log("Confirming purchase of " + p.definition.id);
        _controller.ConfirmPendingPurchase(p);
        _pendingProducts.Remove(p.definition.id);
        if (OnPurchaseSuccessCall != null)
        {
            OnPurchaseSuccessCall(p);
        }
    }

    /// <summary>
    /// 该品项是否正在确认支付锁定中
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private bool IsProductPendingTime(string id)
    {
        return _pendingProducts.Contains(id);
    }
#endif

    /// <summary>
    /// This will be called if an attempted purchase fails.
    /// </summary>
    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        Debug.Log("Purchase failed: " + item.definition.id);
        Debug.Log(r);


        // Detailed debugging information
        Debug.Log("Store specific error code: " + _transactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode());
        if (_transactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
        {
            Debug.Log("Purchase failure description message: " +
                      _transactionHistoryExtensions.GetLastPurchaseFailureDescription().message);
        }
        if (OnPurchaseFailedCall != null)
        {
            OnPurchaseFailedCall(item, r);
        }
        // Unity IAP objects
        _purchaseInProgress = false;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        initFailReason = "Billing failed to initialize! error code = " + error;
        Debug.Log(initFailReason);
        switch (error)
        {
            case InitializationFailureReason.AppNotKnown:
                Debug.LogError("Is your App correctly uploaded on the relevant publisher console?");
                break;
            case InitializationFailureReason.PurchasingUnavailable:
                // Ask the user if billing is disabled in device settings.
                Debug.Log("Billing disabled!");
                break;
            case InitializationFailureReason.NoProductsAvailable:
                // Developer configuration error; check product metadata.
                Debug.Log("No products available for purchase!");
                break;
        }
        isInited = false;
        isIniting = false;
        if (OnInitFailedCall != null)
        {
            OnInitFailedCall(error);
        }
    }

    /// <summary>
    /// 可以重载增加商品品项列表
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void AddCustomProduceList(ConfigurationBuilder builder)
    {
        // In this case our products have the same identifier across all the App stores,
        // except on the Mac App store where product IDs cannot be reused across both Mac and
        // iOS stores.
        // So on the Mac App store our products have different identifiers,
        // and we tell Unity IAP this by using the IDs class.
        builder.AddProduct(PayIDs.NoAD, ProductType.NonConsumable, new IDs
            {
                {"noad01", AppleAppStore.Name},
                {"com.tan.tan.tun.noad", GooglePlay.Name },
            }
        );
    }

    /// <summary>
    /// 开始初始化
    /// </summary>
    public void StartInit()
    {
        if (!isIniting && !isInited)
        {
            var module = StandardPurchasingModule.Instance();

            // The FakeStore supports: no-ui (always succeeding), basic ui (purchase pass/fail), and
            // developer ui (initialization, purchase, failure code setting). These correspond to
            // the FakeStoreUIMode Enum values passed into StandardPurchasingModule.useFakeStoreUIMode.
            module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;

            var builder = ConfigurationBuilder.Instance(module);

            _isGooglePlayStoreSelected =
                Application.platform == RuntimePlatform.Android && module.appStore == AppStore.GooglePlay;

            // Define our products.
            // Either use the Unity IAP Catalog, or manually use the ConfigurationBuilder.AddProduct API.
            // Use IDs from both the Unity IAP Catalog and hardcoded IDs via the ConfigurationBuilder.AddProduct API.

            // Use the products defined in the IAP Catalog GUI.
            // E.g. Menu: "Window" > "Unity IAP" > "IAP Catalog", then add products, then click "App Store Export".
            var catalog = ProductCatalog.LoadDefaultCatalog();

            foreach (var product in catalog.allValidProducts)
            {
                if (product.allStoreIDs.Count > 0)
                {
                    var ids = new IDs();
                    foreach (var storeID in product.allStoreIDs)
                    {
                        ids.Add(storeID.id, storeID.store);
                    }
                    builder.AddProduct(product.id, product.type, ids);
                }
                else
                {
                    builder.AddProduct(product.id, product.type);
                }
            }

            AddCustomProduceList(builder);

#if INTERCEPT_PROMOTIONAL_PURCHASES
        // On iOS and tvOS we can intercept promotional purchases that come directly from the App Store.
        // On other platforms this will have no effect; OnPromotionalPurchase will never be called.
        builder.Configure<IAppleConfiguration>().SetApplePromotionalPurchaseInterceptorCallback(OnPromotionalPurchase);
        Debug.Log("Setting Apple promotional purchase interceptor callback");
#endif

#if RECEIPT_VALIDATION
            string appIdentifier = Application.identifier;
            validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(),
                UnityChannelTangle.Data(), appIdentifier);
#endif

            Action initializeUnityIap = () =>
            {
                // Now we're ready to initialize Unity IAP.
                UnityPurchasing.Initialize(this, builder);
            };

            initializeUnityIap();
            Debug.Log("Start init IAP : is Google = " + _isGooglePlayStoreSelected);
        }
    }

    /// <summary>
    /// This will be called after a call to IAppleExtensions.RestoreTransactions().
    /// </summary>
    private void OnTransactionsRestored(bool success)
    {
        Debug.Log("Transactions restored." + success);
    }

    /// <summary>
    /// 苹果 家长控制 Ask to buy 功能 异步询问调用
    /// When the purchase is approved or rejected, the normal purchase events
    /// will fire.
    /// </summary>
    /// <param name="item">Item.</param>
    private void OnDeferred(Product item)
    {
        Debug.Log("Purchase deferred: " + item.definition.id);
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
        if (_purchaseInProgress == true)
        {
            Debug.Log("Please wait, purchase in progress");
            return;
        }

        if (_controller == null)
        {
            Debug.LogError("Purchasing is not initialized");
            return;
        }

        Product product = _controller.products.WithID(productID);
        if (product == null)
        {
            Debug.LogError("No product has id " + productID);
            return;
        }

        // Don't need to draw our UI whilst a purchase is in progress.
        // This is not a requirement for IAP Applications but makes the demo
        // scene tidier whilst the fake purchase dialog is showing.
        _purchaseInProgress = true;

        //Sample code how to add accountId in developerPayload to pass it to getBuyIntentExtraParams
        //Dictionary<string, string> payload_dictionary = new Dictionary<string, string>();
        //payload_dictionary["accountId"] = "Faked account id";
        //payload_dictionary["developerPayload"] = "Faked developer payload";
        //_controller.InitiatePurchase(_controller.products.WithID(productID), MiniJson.JsonEncode(payload_dictionary));
        //payload 是用于校验的
        _controller.InitiatePurchase(product, "developerPayload");
        if (OnStartPurchaseCall != null)
        {
            OnStartPurchaseCall(product);
        }
    }

    /// <summary>
    /// 新用户安装回包,重置
    /// </summary>
    public void RestoreButtonClick()
    {
        if (_isGooglePlayStoreSelected)
        {
            _googlePlayStoreExtensions.RestoreTransactions(OnTransactionsRestored);
        }
        else
        {
            _appleExtensions.RestoreTransactions(OnTransactionsRestored);
        }
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

    private void LogProductDefinitions()
    {
        var products = _controller.products.all;
        foreach (var product in products)
        {
            Debug.Log(string.Format("id: {0}\nstore-specific id: {1}\ntype: {2}\nenabled: {3}\n", product.definition.id, product.definition.storeSpecificId, product.definition.type.ToString(), product.definition.enabled ? "enabled" : "disabled"));
        }
    }

    /// <summary>
    /// 获取品项
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Product GetProduct(string id)
    {
        if (_controller == null || _controller.products == null)
        {
            return null;
        }
        return _controller.products.WithID(id);
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
