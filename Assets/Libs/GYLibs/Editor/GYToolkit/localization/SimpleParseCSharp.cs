using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SimpleParseCSharp {

    private string _code;

    public string GetCode()
    {
        return _code;
    }

    private ParseStatus _status = ParseStatus.None;

    private HashSet<string> _strSet;

    public SimpleParseCSharp(string code, HashSet<string> stringSet)
    {
        _code = code;
        _strSet = stringSet;
        DeleteStartLineComment();
    }

    private void DeleteStartLineComment()
    {
        StringBuilder sb = new StringBuilder();
        string[] strArr = _code.Split('\n');
        foreach (string str in strArr)
        {
            string tmpStr = str.Trim();
            if (!tmpStr.StartsWith("//") && !tmpStr.StartsWith("#"))
            {
                sb.AppendLine(tmpStr);
            }
        }
        _code = sb.ToString();
    }

    public void ParseCode()
    {
        StringBuilder sb = new StringBuilder();
        string tmpResult;
        for (int i = 0; i < _code.Length; i++)
        {
            char tmpChar = _code[i];
            switch (_status)
            {
                case ParseStatus.None:
                    if (i + 1 < _code.Length)
                    {
                        char nextChar = _code[i + 1];
                        if (tmpChar == '/')
                        {
                            if (nextChar == '*')
                            {
                                _status = ParseStatus.InComment;
                            }
                            else if (nextChar == '/')
                            {
                                _status = ParseStatus.InCommentLine;
                            }
                        }
                        else if (tmpChar == '"')
                        {
                            _status = ParseStatus.InStr;
                        }
                    }
                    break;
                case ParseStatus.InCommentLine:
                    if (tmpChar == '\n')
                    {
                        _status = ParseStatus.None;
                    }
                    break;
                case ParseStatus.InComment:
                    if (i + 1 < _code.Length)
                    {
                        char nextChar = _code[i + 1];
                        if (tmpChar == '*' && nextChar == '/')
                        {
                            i = i + 1;
                            _status = ParseStatus.None;
                        }
                    }
                    break;
                case ParseStatus.InStr:
                    if (tmpChar == '\\')
                    {
                        _status = ParseStatus.InStrSkip;
                        sb.Append(tmpChar);
                    }
                    else if (tmpChar == '"')
                    {
                        tmpResult = sb.ToString();
                        if (!_strSet.Contains(tmpResult))
                        {
                            tmpResult = tmpResult.Replace("\\n", "\n");
                            tmpResult = tmpResult.Replace("\\r", "\r");
                            _strSet.Add(tmpResult);
                        }
                        sb = new StringBuilder();
                        _status = ParseStatus.None;
                    }
                    else
                    {
                        sb.Append(tmpChar);
                    }
                    break;
                case ParseStatus.InStrSkip:
                    sb.Append(tmpChar);
                    _status = ParseStatus.InStr;
                    break;
            }
            
        }
    }

    enum ParseStatus
    {
        None, InComment, InCommentLine, InMacro, InStr, InStrSkip,
    }
}
