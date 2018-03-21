using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Kontur.ImageTransformer.Application
{
    /// <summary>
    /// Содержит набор методов для обработки изображения
    /// </summary>
    static class ImageProcessor
    {
        /// <summary>
        /// Применяет фильтр grayscale к изображению
        /// </summary>
        /// <param name="bitmap">Входное изображение</param>
        public static void SetGrayscale(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            using (FastBitmap fbitmap = new FastBitmap(bitmap))
            {
                unsafe
                {
                    byte* row = (byte*)fbitmap.Scan0;
                    byte* bb = row;

                    for (int yy = 0; yy < fbitmap.Height; yy++, bb = (row += fbitmap.Stride))
                    {
                        for (int xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            byte intensity = (byte)((*(bb + 0) + *(bb + 1) + *(bb + 2)) / 3);
                            *(bb + 0) = *(bb + 1) = *(bb + 2) = intensity;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Применяет фильтр threshold к изображению
        /// </summary>
        /// <param name="bitmap">Входное изображение</param>
        /// <param name="x">Значение порога</param>
        public static void SetThreshold(Bitmap bitmap, int x)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            int ww = bitmap.Width / 8;
            int hh = bitmap.Height / 8;
            using (FastBitmap fbitmap = new FastBitmap(bitmap))
            {
                unsafe
                {
                    byte* row = (byte*)fbitmap.Scan0;
                    byte* bb = row;

                    for (int yy = 0; yy < fbitmap.Height; yy++, bb = (row += fbitmap.Stride))
                    {
                        for (int xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            byte intensity = (byte)((*(bb + 0) + *(bb + 1) + *(bb + 2)) / 3);

                            if (intensity >= (byte)(255 * x / 100))
                            {
                                *(bb + 0) = *(bb + 1) = *(bb + 2) = 255;
                            }
                            else
                            {
                                *(bb + 0) = *(bb + 1) = *(bb + 2) = 0;
                            }

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Применяет фильтр sepia к изображению
        /// </summary>
        /// <param name="bitmap">Входное изображение</param>
        public static void SetSepia(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            using (FastBitmap fbitmap = new FastBitmap(bitmap))
            {
                unsafe
                {
                    byte* row = (byte*)fbitmap.Scan0;
                    byte* bb = row;

                    float blue;
                    float green;
                    float red;

                    for (int yy = 0; yy < fbitmap.Height; yy++, bb = (row += fbitmap.Stride))
                    {
                        for (int xx = 0; xx < fbitmap.Width; xx++, bb += fbitmap.PixelSize)
                        {
                            blue = (*(bb + 2) * .272F) + (*(bb + 1) * .534F) + (*(bb + 0) * .131F);
                            green = (*(bb + 2) * .349F) + (*(bb + 1) * .686F) + (*(bb + 0) * .168F);
                            red = (*(bb + 2) * .393F) + (*(bb + 1) * .769F) + (*(bb + 0) * .189F);

                            *(bb + 0) = (blue > 255) ? (byte)255 : (byte)blue;
                            *(bb + 1) = (green > 255) ? (byte)255 : (byte)green;
                            *(bb + 2) = (red > 255) ? (byte)255 : (byte)red;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Обрезает изображение
        /// </summary>
        /// <param name="bitmap">Входное изображение</param>
        /// <param name="xPosition">Координата X верхнео левого угола области</param>
        /// <param name="yPosition">Координата Y верхнео левого угола области</param>
        /// <param name="width">Ширина области</param>
        /// <param name="height">Высота области</param>
        public static void Crop(ref Bitmap bitmap, int xPosition, int yPosition, int width, int height)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb && bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            try
            {
                if (width < 0)
                {
                    xPosition += width;
                    width = -width;
                }

                if (height < 0)
                {
                    yPosition += height;
                    height = -height;
                }

                if (xPosition + width <= 0 || xPosition >= bitmap.Width || yPosition + height <= 0 || yPosition >= bitmap.Height)
                    throw new ArgumentOutOfRangeException();

                if (xPosition < 0)
                {
                    width += xPosition;
                    xPosition = 0;
                }

                if (yPosition < 0)
                {
                    height += yPosition;
                    yPosition = 0;
                }

                if (xPosition + width > bitmap.Width - xPosition)
                    width = bitmap.Width - xPosition;

                if (yPosition + height > bitmap.Height - yPosition)
                    height = bitmap.Height - yPosition;

                Bitmap buffer = new Bitmap(width, height, bitmap.PixelFormat);

                using (FastBitmap fbitmapOld = new FastBitmap(bitmap, xPosition, yPosition, width, height),
                    fbitmapNew = new FastBitmap(buffer))
                {
                    unsafe
                    {
                        byte* rowOld = (byte*)fbitmapOld.Scan0;
                        byte* bbOld = rowOld;
                        byte* rowNew = (byte*)fbitmapNew.Scan0;
                        byte* bbNew = rowNew;

                        for (int yy = 0; yy < fbitmapOld.Height; yy++, bbOld = (rowOld += fbitmapOld.Stride), bbNew = (rowNew += fbitmapNew.Stride))
                        {
                            for (int xx = 0; xx < fbitmapOld.Width; xx++, bbOld += fbitmapOld.PixelSize, bbNew += fbitmapNew.PixelSize)
                            {
                                *(bbNew + 0) = *(bbOld + 0);
                                *(bbNew + 1) = *(bbOld + 1);
                                *(bbNew + 2) = *(bbOld + 2);
                                *(bbNew + 3) = *(bbOld + 3);
                            }
                        }
                    }
                }

                bitmap = buffer;
                buffer = null;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }  
        }
    }
}