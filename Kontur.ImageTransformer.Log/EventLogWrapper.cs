using System;
using System.Diagnostics;

namespace Kontur.ImageTransformer.Log
{
    /// <summary>
    /// Совсем простой логгер, пишущий в event log
    /// </summary>
    public class EventLogWrapper : IDisposable
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса EventLogWrapper
        /// </summary>
        public EventLogWrapper()
        {
            if (logname == null)
                throw new ArgumentNullException("EventLogWrapper is not initialized. Call InitLog()");

            evlog = new EventLog();
        }

        /// <summary>
        /// Пишет сообщение в лог
        /// </summary>
        /// <param name="message">Текст сообщения   </param>
        /// <param name="type">Уроввень логирования</param>
        public void WriteLog(string message, EventLogEntryType type)
        {
            if (String.IsNullOrEmpty(message))
                throw new ArgumentNullException();

            try
            {
                if (!EventLog.SourceExists(logname))
                {
                    EventLog.CreateEventSource(logname, logname);
                }
                evlog.Source = logname;
                evlog.WriteEntry(message, type);
            }
            catch { }
        }

        public void WriteLog(string message, EventLogEntryType type, Exception e)
        {
            if (String.IsNullOrEmpty(message) || e == null)
                throw new ArgumentNullException();

            try
            {
                if (!EventLog.SourceExists(logname))
                {
                    EventLog.CreateEventSource(logname, logname);
                }
                evlog.Source = logname;
                evlog.WriteEntry($"{message}: {e.Message}", type);
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                if (evlog != null)
                    evlog.Close();
            }
            catch { }
        }

        /// <summary>
        /// Инициализирует EventLogWrapper. Задает параметры для логирования
        /// </summary>
        /// <param name="loggerName">Название логера</param>
        public static void InitLog(string loggerName)
        {
            if (String.IsNullOrEmpty(loggerName))
                throw new ArgumentNullException();

            logname = loggerName;
        }

        private EventLog evlog;
        private static string logname;
    }
}
