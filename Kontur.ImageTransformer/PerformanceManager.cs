using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Kontur.ImageTransformer.Log;


namespace Kontur.ImageTransformer.Performance
{
    /// <summary>
    /// Счетчик производительности сервиса. Выполняет опрос сервера и замеряет среднее время ответа
    /// </summary>
    public class PerformanceManager
    {
        /// <summary>
        /// Запускает счетчик производительности
        /// </summary>
        /// <param name="maxWaitTime">Максимальное время ожидание получения ответа от сервера</param>
        /// <param name="poolingTime">Время задержки между опросами сервера</param>
        /// <param name="url">Адрес и порт, на котором запущен сервер</param>
        public static void Start(int maxWaitTime, int poolingTime, string url)
        {
            if (maxWaitTime < 0 || poolingTime < 0 || String.IsNullOrEmpty(url))
                throw new ArgumentException("It's not allowed to use empty arguments");

            if (!isCounterRunning)
            {
                try
                {
                    counterThread = new Thread(() => { PerformanceWatching(maxWaitTime, poolingTime, url); });
                    counterThread.Start();
                }
                catch (Exception)
                {
                    throw;
                }

                isCounterRunning = true;
            }
        }

        public static void Start()
        {
            Start(500, 500, "http://localhost:8080");
        }

        /// <summary>
        /// Останавливает счетчик производительности
        /// </summary>
        public static void Stop()
        {
            if (counterThread != null)
            {
                if (counterThread.IsAlive)
                {
                    counterThread.Abort();
                    counterThread.Join();
                }

                counterThread = null;
            }

            isCounterRunning = false;
        }

        private static void PerformanceWatching(int maxWaitTime, int poolingTime, string url)
        {
            double dt;//Текущее время ответа
            double avtime;//Среднее время ответа
            DateTime startRequestTime;//Время начала запроса

            while (true)
            {
                startRequestTime = DateTime.Now;
                try
                {
                    WebRequest request = WebRequest.Create(url);
                    request.Timeout = maxWaitTime;
                    request.GetResponse();
                }
                catch (WebException)
                {
                }
                catch (Exception e)
                {
                    using (var evlog = new EventLogWrapper())
                    {
                        evlog.WriteLog("An error was occured on aplication server starting", EventLogEntryType.Error, e);
                    }
                }
                finally
                {
                    dt = (DateTime.Now - startRequestTime).TotalMilliseconds;
                    commonQueueRequestTime += dt;
                    countOfQueueRequests++;
                    avtime = commonQueueRequestTime / countOfQueueRequests;

                    IsHighLoad = avtime > maxWaitTime || avtime > avtime * 2;

                    if (countOfQueueRequests > counterLimit) //Фактически, учитываем только counterLimit последних запросов
                    {
                        commonQueueRequestTime = avtime;
                        countOfQueueRequests = 1;
                    }

                    Console.WriteLine($"time={(int)dt}, avr={(int)avtime}");

                    Thread.Sleep(poolingTime);
                }
            }
        }
        
        /// <summary>
        /// Показывает загружен ли сервис в текущий момент
        /// </summary>
        public static bool IsHighLoad { get; private set; }

        private static double commonQueueRequestTime;//Общее время всех хапросов. Необходимо для вычисл. ср.
        private static int countOfQueueRequests;//Текущее количество запросов. Необходимо для вычисл. ср.
        private static int counterLimit = 10;//Макс. кол. запросов, котор. нужно учитывать при подсчете ср.
        private static Thread counterThread;
        private static bool isCounterRunning;
    }
}
