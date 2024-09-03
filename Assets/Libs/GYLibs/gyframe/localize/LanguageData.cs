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
            else
            {
                LanguageData.Language = LanguageEnum.English;
            }
        }
    }
}