using UnityEngine;
using GYLib.Utils;
using System.Collections.Generic;
using System.IO;
using System;

namespace GYLib.Utils
{
    public class CSVSaveHelper
    {
        /// <summary>
        /// 保存成CSV
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="values"></param>
        /// <param name="path"></param>
        public static void SaveToCSV(string[] keys, List<List<object>> values, string path)
        {
            string content = "";
            int keyLength = keys.Length;
            for (int i = 0; i < keyLength; i++)
            {
                content += keys;
                if (i != keyLength - 1)
                    content += ",";
            }
            content += "\r\n";

            for (int i = 0; i < values.Count; i++)
            {
                List <object> value = values[i];
                for (int j = 0; j < keyLength; j++)
                {
                    string result = value[j].ToString();
                    if (result.IndexOf(",") != -1)
                        content += '\"' + result + "\"";
                    else
                        content += result;
                    if (j != (keyLength - 1))
                        content += ",";
                }
                content += "\r\n";
            }
            FileManager.Instance.Save(path, System.Text.Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 同步保存(回写)CSVInit
        /// </summary>
        /// <param name="csv"></param>
        public static void SaveCSVInit(CSVInit csv, string path)
        {
            string content = GetSaveCSVInitFile(csv);
            FileManager.Instance.Save(path, System.Text.Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 获得CSV文件内容
        /// </summary>
        /// <param name="csv"></param>
        /// <returns></returns>
        public static string GetSaveCSVInitFile(CSVInit csv)
        {
            string content = "";
            int keyLength = csv.dict.Count;
            var keys = csv.dict.Keys;
            List<string> keyList = new List<string>();
            int counter = 0;
            int maxLength = 0;
            //遍历列名
            foreach (string key in keys)
            {
                content += key;
                keyList.Add(key);
                if (counter != (keyLength - 1))
                    content += ",";
                counter++;
                string[] tmpContent = csv.dict[key];
                if (content.Length > maxLength)
                    maxLength = tmpContent.Length;
            }
            content += "\r\n";
            //逐行构造
            if (keyLength > 0)
            {
                for (int i = 0; i < maxLength; i++)
                {
                    for (int j = 0; j < keyLength; j++)
                    {
                        string[] tmpContent = csv.dict[keyList[j]];
                        if (tmpContent.Length > i)
                        {
                            string result = tmpContent[i].ToString();
                            if (result.IndexOf(",") != -1)
                                content += '\"' + result + "\"";
                            else
                                content += result + "";
                            if (j != (keyLength - 1))
                                content += ",";
                        }
                        else
                        {
                            if (j != (keyLength - 1))
                                content += ",";
                        }
                    }
                    content += "\r\n";
                }
            }
            return content;
        }

        /// <summary>
        /// 同步存储键值为CSV文件
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="path"></param>
        public static void SaveKeyValue(Dictionary<string, object> dict, string path)
        {
            string content = GetSaveKeyValueFile(dict);
            FileManager.Instance.Save(path, System.Text.Encoding.UTF8.GetBytes(content));
        }

        /// <summary>
        /// 获得键值存储的CSV文件内容
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static string GetSaveKeyValueFile(Dictionary<string, object> dict)
        {
            string content = "";
            string values = "";
            var keys = dict.Keys;
            List<string> keyList = new List<string>();
            int size = dict.Count;
            int i = 0;
            foreach (string key in keys)
            {
                content += key;
                string value = dict[key].ToString();
                if (value.IndexOf(",") != -1)
                    values += "\"" + value + "\"";
                else
                    values += value;
                if (i != size - 1)
                {
                    content += ",";
                    values += ",";
                }
                i++;
            }
            content = content + "\r\n" + values;
            return content;
        }

        public static Dictionary<string, object> GetKeyValue(string path)
        {
            FileStream fs = FileManager.Instance.GetFile(path);
            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);
            fs.Close();
            string content = System.Text.Encoding.UTF8.GetString(bytes);
            string[] arr = content.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            CSVInit csv = new CSVInit(arr);
            Dictionary<string, object> dict = new Dictionary<string, object>();

            var keys = csv.dict.Keys;
            List<string> keyList = new List<string>();
            foreach (string key in keys)
            {
                dict.Add(key, csv.dict[key][0]);
            }
            return dict;
        }
    }
}