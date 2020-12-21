using UnityEngine;
using System.Collections;

namespace GYLib.GYFrame
{
    public class LanguageData
    {
        public static string Language = LanguageEnum.English;

        public static void InitLanguage()
        {
            LanguageData.Language = Application.systemLanguage.ToString();
            if (LanguageData.Language == SystemLanguage.Chinese.ToString() ||
                LanguageData.Language == SystemLanguage.ChineseTraditional.ToString())
            {
                LanguageData.Language = LanguageEnum.ChineseTraditional;
            }
            else if (LanguageData.Language == SystemLanguage.ChineseSimplified.ToString())
            {
                LanguageData.Language = LanguageEnum.Chinese;
            }
            else if (LanguageData.Language == SystemLanguage.Korean.ToString())
            {
                LanguageData.Language = LanguageEnum.Korean;
            }
            else if (LanguageData.Language == SystemLanguage.Japanese.ToString())
            {
                LanguageData.Language = LanguageEnum.Japanese;
            }
            else if (LanguageData.Language == SystemLanguage.Russian.ToString())
            {
                LanguageData.Language = LanguageEnum.Russian;
            }
            else if (LanguageData.Language == SystemLanguage.Spanish.ToString())
            {
                LanguageData.Language = LanguageEnum.Spanish;
            }
            else if (LanguageData.Language == SystemLanguage.German.ToString())
            {
                LanguageData.Language = LanguageEnum.German;
            }
            else if (LanguageData.Language == SystemLanguage.French.ToString())
            {
                LanguageData.Language = LanguageEnum.French;
            }
            else
            {
                LanguageData.Language = LanguageEnum.English;
            }
        }
    }
}