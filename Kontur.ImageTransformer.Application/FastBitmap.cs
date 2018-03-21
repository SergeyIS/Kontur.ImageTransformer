using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kontur.ImageTransformer.Application
{
    /// <summary>
    /// Реализация Bitmap с блокировками
    /// </summary>
    public class FastBitmap : IDisposable
    {
        /// <summary>
        /// Инициализирует новый объект FastBitmap
        /// </summary>
        /// <param name="bitmap">Исходное изображение</param>
        /// <param name="xx">Координата X верхнего левого угла блокируемой области</param>
        /// <param name="yy">Координата Y верхнего левого угла блокируемой области</param>
        /// <param name="width">Ширина блокируемой области</param>
        /// <param name="height">Высота блокируемой области</param>
        public FastBitmap(Bitmap bitmap, int xx, int yy, int width, int height)
        {
            this.Bitmap = bitmap;
            XX = xx;
            YY = yy;
            Width = width;
            Height = height;
            Data = this.Bitmap.LockBits(new Rectangle(xx, yy, width, height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
            PixelSize = FindPixelSize();
            Stride = Data.Stride;
            Scan0 = Data.Scan0;
        }

        /// <summary>
        /// Инициализирует новый объект FastBitmap
        /// </summary>
        /// <param name="bitmap">Исходное изображение</param>
        public FastBitmap(Bitmap bitmap) : this(bitmap, 0, 0, bitmap.Width, bitmap.Height)
        {
        }

        /// <summary>
        /// Возвращает размер пикселя в байтах
        /// </summary>
        /// <returns>Размер пикселя в байтах</returns>
        private int FindPixelSize()
        {
            if (Data.PixelFormat == PixelFormat.Format24bppRgb)
            {
                return 3;
            }
            if (Data.PixelFormat == PixelFormat.Format32bppArgb)
            {
                return 4;
            }
            return 4;
        }

        public void Dispose()
        {
            try
            {
                Bitmap.UnlockBits(Data);
            }
            catch
            {
            }
        }

        private Bitmap Bitmap;
        public readonly BitmapData Data;
        public readonly int PixelSize;
        public readonly int XX;
        public readonly int YY;
        public readonly int Width;
        public readonly int Height;
        public readonly int Stride;
        public readonly IntPtr Scan0;
    }
}
