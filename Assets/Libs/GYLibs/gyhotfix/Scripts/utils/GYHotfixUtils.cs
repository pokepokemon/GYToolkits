using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace GYLib.Hotfix
{
    /// <summary>
    /// 热更工具集
    /// </summary>
    public class GYHotfixUtils
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="originName"></param>
        /// <returns></returns>
        public static string GetMD5(string originName)
        {
            return GetMD5(Encoding.UTF8.GetBytes(originName));
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="inBytes"></param>
        /// <returns></returns>
        public static string GetMD5(byte[] inBytes)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] encryptedBytes = md5.ComputeHash(inBytes);
            StringBuilder sb = new StringBuilder();
            for (int j = 0; j < encryptedBytes.Length; j++)
            {
                sb.AppendFormat("{0:x2}", encryptedBytes[j]);
            }
            string newName = sb.ToString();
            return newName;
        }
    }
}