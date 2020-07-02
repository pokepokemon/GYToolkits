using System;
using System.Collections.Generic;
using UnityEngine;

namespace GYLib.Utils
{
	/**
	 * 读取CSV配置的工具类
	 * 配置约束：
	 * 1 不允许带双引号字符
	 * 2 不允许第一行带逗号
	 */
	public class CSVInit
	{
		private Dictionary<string, string[]> _dict = new Dictionary<string, string[]>();
		public Dictionary<string, string[]> dict
		{
			get { return _dict; }
		}

        public CSVInit()
        {

        }

        /// <summary>
        /// 已经content.Split(new string[1]{"\r\n"}, StringSplitOptions.None)分离好的逐行语句
        /// </summary>
        /// <param name="arr"></param>
        public CSVInit (string[] arr)
        {
            if (arr.GetLength(0) > 0)
            {
                string[] rowNames = arr[0].Split(new string[1] { "," }, StringSplitOptions.None);
                for (int j = 0; j < rowNames.GetLength(0); j++)
                {
                    _dict.Add(rowNames[j], new string[arr.GetLength(0) - 1]);
                }
                for (int i = 1; i < arr.GetLength(0); i++)
                {
                    string[] lineData = this.analyseLine(arr[i]);
                    for (int j = 0; j < rowNames.GetLength(0) && j < lineData.GetLength(0); j++)
                    {
                        _dict[rowNames[j]][i - 1] = lineData[j];
                    }
                }
            }
        }

        /// <summary>
        /// 通过Resource.Load读取路径解析
        /// </summary>
        /// <param name="path"></param>
		public CSVInit (string path)
		{
			TextAsset asset = (TextAsset)Resources.Load(path, typeof(TextAsset));

			if (asset != null)
			{
				string content = asset.text;
				string[] arr = content.Split(new string[1]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
				if (arr.GetLength(0) > 0)
				{
					string[] rowNames = arr[0].Split(new string[1] { "," }, StringSplitOptions.None);
					for (int j = 0; j < rowNames.GetLength(0); j++)
					{
						_dict.Add(rowNames[j], new string[arr.GetLength(0) - 1]);
					}
					for (int i = 1; i < arr.GetLength(0); i++) {
						string[] lineData = this.analyseLine(arr[i]);
						for (int j = 0; j < rowNames.GetLength(0) && j < lineData.GetLength(0); j++)
						{
							_dict[rowNames[j]][i - 1] = lineData[j];
						}
					}
				}
			}
		}

		/**
		 * 逐个分析，切割成数组
		 */
		private string[] analyseLine(string line)
		{
			List<string> list = new List<string> ();
			string[] elements = line.Split (',');
			string element = "";
			bool inString = false;
			for (int i= 0; i < elements.GetLength(0); i++) {
				string token = elements[i];
				if (inString)
				{
					if (token.Length > 0 && token[token.Length - 1] == '\"')
					{
						string tmpStr = token.Substring(0, token.Length - 1);
						element = element + tmpStr;
						list.Add(element);
						inString = false;
						continue;
					}
					element = element + token + ",";
					continue;
				}
				if (token.Length > 0 && token[0] == '\"' && !inString)
				{
                    if (token[token.Length - 1] == '\"')
                    {
                        element = token.Substring(1, token.Length - 1);
                        inString = false;
                    }
                    else
                    {
                        element = token.Substring(1) + ",";
                        inString = true;
                        continue;
                    }
				}
				element = elements[i];
				list.Add(element);
			}
			return list.ToArray();
		}

        public void Dispose()
        {
            dict.Clear();
            _dict = null;
        }
	}
}

