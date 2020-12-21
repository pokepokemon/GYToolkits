using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using GYLib.GYFrame;

/// <summary>
/// 多语言切换图片组件
/// </summary>
public class ImageLocalizeSwitcher : MonoBehaviour
{
    [SerializeField]
    public ImageLocalizeSwitcherDictionary spMap = new ImageLocalizeSwitcherDictionary();
    
    public Image image;

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        Sprite sp;
        if (image != null)
        {
            if (spMap.TryGetValue(LanguageData.Language, out sp) && sp != null)
            {
                image.sprite = sp;
                image.SetNativeSize();
            }
            else if (spMap.TryGetValue(LanguageEnum.English, out sp) && sp != null)
            {
                image.sprite = sp;
                image.SetNativeSize();
            }
        }
    }

    private void Reset()
    {
        Image tmpImage = this.GetComponent<Image>();
        if (this.image == null && tmpImage != null)
        {
            this.image = tmpImage;
        }
        
        if (!this.spMap.ContainsKey(LanguageEnum.English)) this.spMap[LanguageEnum.English] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.Chinese)) this.spMap[LanguageEnum.Chinese] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.Korean)) this.spMap[LanguageEnum.Korean] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.Japanese)) this.spMap[LanguageEnum.Japanese] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.Russian)) this.spMap[LanguageEnum.Russian] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.German)) this.spMap[LanguageEnum.German] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.Spanish)) this.spMap[LanguageEnum.Spanish] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.French)) this.spMap[LanguageEnum.French] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.Portuguese)) this.spMap[LanguageEnum.Portuguese] = this.image.sprite;
        if (!this.spMap.ContainsKey(LanguageEnum.ChineseTraditional)) this.spMap[LanguageEnum.Portuguese] = this.image.sprite;
    }
}

[Serializable]
public class ImageLocalizeSwitcherDictionary : SerializableDictionary<string, Sprite> { }
