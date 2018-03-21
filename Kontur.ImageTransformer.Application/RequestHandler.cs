using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Kontur.ImageTransformer.Application
{
    /*
        Привет, дорогой разработчик. При написании этого класса я руководствовался исключительно
        требованиями ТЗ. Поэтому постарался сделать его максимально простым, из-за чего решение 
        получилось совершенно не масштабируемым. Изначально было желание свелосипедировать, 
        и с использованием рефлексии сделать свой конвеер обработки в стиле ASP.NET, 
        но мне показалось это излишним :)
     */

    /// <summary>
    /// Этот класс отвечает за обработку HTTP запросов
    /// </summary>
    public class RequestHandler
    {
        /// <summary>
        /// Инициализирует новый объект класса RequestHandler
        /// </summary>
        /// <param name="context">Контекст объекта HttpListener</param>
        public RequestHandler(HttpListenerContext context)
        {
            listenerContext = context;
        }

        /// <summary>
        /// Обрабатывает HTTP запрос
        /// </summary>
        public void HandleRequest()
        {
            string[] requestSegments = null;
            int[] imageParametrs = null;
            string filterName = null;

            try
            {
                if (!listenerContext.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                    listenerContext.Request.Url.Segments.Length != 4 ||
                    listenerContext.Request.ContentLength64 > maxContentLength)
                {
                    throw new ArgumentException();
                }

                requestSegments = listenerContext.Request.Url.Segments.Select(v => v.Trim('/')).ToArray();

                if (!requestSegments[1].Equals("process", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException();

                filterName = requestSegments[2];
                imageParametrs = requestSegments[3].Split(',').Select(v => Convert.ToInt32(v)).ToArray();

                if (imageParametrs.Length != 4)
                    throw new ArgumentException();

                //image processing
                Bitmap bitmap = new Bitmap(listenerContext.Request.InputStream);
                ImageProcessor.Crop(ref bitmap, imageParametrs[0], imageParametrs[1], imageParametrs[2], imageParametrs[3]);

                if (filterName.Equals("grayscale", StringComparison.OrdinalIgnoreCase))
                {
                    ImageProcessor.SetGrayscale(bitmap);
                }
                else if (filterName.Equals("sepia", StringComparison.OrdinalIgnoreCase))
                {
                    ImageProcessor.SetSepia(bitmap);
                }
                else
                {
                    int x = 0;
                    var match = Regex.Match(filterName.ToLower(), @"(?<=threshold\().*?(?=\))");
                    if (!(match.Success && Int32.TryParse(match.Value, out x) && (x >= 0 && x <= 100)))
                        throw new ArgumentException();

                    ImageProcessor.SetThreshold(bitmap, x);
                }

                EndRequest(HttpStatusCode.OK, bitmap);
            }
            catch (FormatException)
            {
                EndRequest(HttpStatusCode.BadRequest, null);
            }
            catch (ArgumentOutOfRangeException)
            {
                EndRequest(HttpStatusCode.NoContent, null);
            }
            catch (ArgumentException)
            {
                EndRequest(HttpStatusCode.BadRequest, null);
            }
            catch(Exception e)
            {
                EndRequest(HttpStatusCode.InternalServerError, null);
            }
        }

        /// <summary>
        /// Завершает обработку запроса. Пишет в выходной поток
        /// </summary>
        /// <param name="status">HTTP код результата</param>
        /// <param name="bitmap">Данные для записи в выходной поток</param>
        private void EndRequest(HttpStatusCode status, Bitmap bitmap)
        {
            listenerContext.Response.StatusCode = (int)status;
            listenerContext.Response.AddHeader("Content-Type", responseContentType);
            
            if(status == HttpStatusCode.OK)
            {
                using (BufferedStream writer = new BufferedStream(listenerContext.Response.OutputStream))
                {
                    try
                    {
                        bitmap.Save(writer, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch
                    {

                    }
                }
            }

            listenerContext.Response.Close();
            if (bitmap != null)
                bitmap.Dispose();
        }

        private const long maxContentLength = 102400;
        private const string responseContentType = "application/octet-stream";
        private readonly HttpListenerContext listenerContext;
    }
}
