using System.Text;
using UnityEngine;

namespace Core
{
    [ExecuteAlways]
    public static class ConsoleLogHelper
    {
        public static bool LogIsStarted { get; private set; } = false;
    
        private static StringBuilder logBuilder = null;

        public static bool TryStartNewLog(bool forceNew = false)
        {
            if (LogIsStarted)
            {
                if (forceNew) EndLog();
                else return false;
            }
            else if(logBuilder != null) 
                EndLog();

            logBuilder = new StringBuilder();
            LogIsStarted = true;
            return LogIsStarted;
        }
        public static void EndLog()
        {
            if (!LogIsStarted && logBuilder == null)
                return;
            
            Debug.Log(logBuilder.ToString());
            
            logBuilder = null;
            LogIsStarted = false;
        }

        public static void Log(string message, bool newLine = true)
        {
            if (newLine) logBuilder.AppendLine(message);
            else logBuilder.Append(message);
        }
        public static void LogIndent(int indent, string message, bool newLine = true)
        {
            var indentBuilder = new StringBuilder();
            for (int i = 0; i < indent; i++)
                indentBuilder.Append("        ");

            Log(indentBuilder.ToString() + message, newLine);
        }
    }
}
