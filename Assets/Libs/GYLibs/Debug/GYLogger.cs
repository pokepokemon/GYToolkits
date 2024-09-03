using UnityEngine;
using System.Text;

namespace GYLibs
{
    public static class GYLogger
    {
        private static bool _isInited = false;
        private static bool _canLog = true;
        private static bool _canWarning = true;
        private static bool _canError = true;
        private static StringBuilder _logBuilder = new StringBuilder();
        private static readonly string _normalColor = "#" + ColorUtility.ToHtmlStringRGBA(Color.black);
        private static readonly string _warnColor = "#" + ColorUtility.ToHtmlStringRGBA(Color.yellow);
        private static readonly string _errorColor = "#" + ColorUtility.ToHtmlStringRGBA(Color.red);
        private static readonly string _forceColor = "#" + ColorUtility.ToHtmlStringRGBA(Color.cyan);

        public static bool CanLog
        {
            get => _canLog;
            set => _canLog = value;
        }

        public static bool CanWarning
        {
            get => _canWarning;
            set => _canWarning = value;
        }

        public static bool CanError
        {
            get => _canError;
            set => _canError = value;
        }

        public static void Init()
        {
            if (_isInited) return;

            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);

            _canError = true;
            _canWarning = _canLog = true;

            _isInited = true;
        }

        /// <summary>
        /// 开关三种级别的log
        /// </summary>
        /// <param name="normal">普通打印</param>
        /// <param name="warn">警告</param>
        /// <param name="error">错误</param>
        public static void OpenLog(bool normal, bool warn, bool error)
        {
            _canLog = normal;
            _canWarning = warn;
            _canError = error;
        }

        [System.Diagnostics.Conditional("GY_DEBUG")]
        public static void Log(object msg)
        {
            if (!_canLog)
            {
                return;
            }
            // msg = _ChangeMsgColor(msg, _normalColor);
            Debug.Log(msg);
        }

        public static void ForceLog(object msg)
        {
            if (!_canLog)
            {
                return;
            }
            msg = ChangeMsgColor(msg, _forceColor);
            Debug.Log(msg);
        }

        public static void LogWarning(object msg)
        {
            if (!_canWarning)
            {
                return;
            }
            msg = ChangeMsgColor(msg, _warnColor);
            Debug.LogWarning(msg);
        }

        public static void LogError(object msg)
        {
            if (!_canError)
            {
                return;
            }
            msg = ChangeMsgColor(msg, _errorColor);
            Debug.LogError(msg);
        }

        public static void LogException(System.Exception exp)
        {
            if (!_canError)
            {
                return;
            }
            string msg = ChangeMsgColor(exp.Message + "\n" + exp.StackTrace, _errorColor);
            Debug.LogError(msg);
        }

        /// <summary>
        /// 修改打印内容的颜色
        /// </summary>
        /// <param name="msg">打印对象</param>
        /// <param name="colorCode">颜色代码</param>
        /// <returns></returns>
        private static string ChangeMsgColor(object msg, string colorCode)
        {
#if UNITY_EDITOR
            _logBuilder.Clear();
            _logBuilder.Append("<color=" + colorCode + ">");
            _logBuilder.Append(msg.ToString());
            _logBuilder.Append("</color>");
            return _logBuilder.ToString();
#else
            return msg.ToString();
#endif
        }
    }
}
