using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mzxrules.Helper;
using System.Collections.Concurrent;
using System.Threading;

namespace Spectrum
{
    static class ShitBit
    {
        const int BUFFER_SIZE = 320 * 240 * 2;

        public static void SaveFrameBufferBitmap(int frameBuffer)
        {
            Bitmap frame = GetFrameBufferBitmap(frameBuffer);
            frame.Save($"dump/frame{frameBuffer:X6}.png", System.Drawing.Imaging.ImageFormat.Png);
        }
        

        static Bitmap GetFrameBufferBitmap(int frameBuffer)
        {
            byte[] pixelData = Zpr.ReadRam(frameBuffer, BUFFER_SIZE);
            //pixelData.Reverse32();
            frameBuffer &= 0xFFFFFF;
            return DrawRGB5A1(pixelData, 320, 240);
        }

        static Bitmap DrawRGB5A1(byte[] data, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);


            for (int h = 0; h < height; h++)
                for (int w = 0; w < width; w++)
                {
                    short pix = Endian.ConvertInt16(BitConverter.ToInt16(data, (h * width + w) * 2));
                    int r = ((pix >> 8) & 0xF8); r += (r >> 5);
                    int g = ((pix >> 3) & 0xF8); g += (g >> 5);
                    int b = ((pix << 2) & 0xF8); b += (b >> 5);
                    int a = ((pix) & 1) * 255;
                    Color c = Color.FromArgb(a, r, g, b);
                    image.SetPixel(w, h, c);
                }
            image.MakeTransparent(Color.SkyBlue);
            return image;
        }

        public static void ViewFrameBuffer(int addr)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write($"{addr:X8}");
            
            Point location = new Point(0, 2);

            Size fontSize = NativeMethods.GetConsoleFontSize();
            //Size imageSize = new Size(320*2/fontSize.Width, 240*2/fontSize.Height); // desired image size in characters
            Size imageSize = new Size(320 * 2, 240 * 2);

            var tokenSource = new CancellationTokenSource();
            Task task;
            

            //Console.SetCursorPosition(0, imageSize.Height);

            //// draw some placeholders
            //Console.SetCursorPosition(location.X - 1, location.Y);
            //Console.Write(">");
            //Console.SetCursorPosition(location.X + imageSize.Width, location.Y);
            //Console.Write("<");
            //Console.SetCursorPosition(location.X - 1, location.Y + imageSize.Height - 1);
            //Console.Write(">");
            //Console.SetCursorPosition(location.X + imageSize.Width, location.Y + imageSize.Height - 1);
            //Console.WriteLine("<");

            //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), @"Sample Pictures\tulips.jpg");

            task = Task.Factory.StartNew(() =>
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                    using (Graphics g = Graphics.FromHwnd(NativeMethods.GetConsoleWindow()))
                    {
                        Image image = GetFrameBufferBitmap(addr); //bitmap;
                        //using (Image image = bitmap)//Image.FromFile(path))
                        {
                            // translating the character positions to pixels
                            Rectangle imageRect = new Rectangle(
                                location.X * fontSize.Width,
                                location.Y * fontSize.Height,
                                imageSize.Width, //* fontSize.Width,
                                imageSize.Height);// * fontSize.Height);
                            g.DrawImage(image, imageRect);
                        }
                    }
                }
            }, tokenSource.Token);

            ConsoleKey key = ConsoleKey.Q;

            while (key != ConsoleKey.DownArrow)
            {
                key = Console.ReadKey().Key;
            }

            tokenSource.Cancel();


            try
            { Task.WaitAll(task); }
            catch (AggregateException e)
            {
                Console.WriteLine("\nAggregateException thrown with the following inner exceptions:");
                // Display information about each exception. 
                foreach (var v in e.InnerExceptions)
                {
                    if (v is TaskCanceledException)
                        Console.WriteLine("   TaskCanceledException: Task {0}",
                                          ((TaskCanceledException)v).Task.Id);
                    else
                        Console.WriteLine("   Exception: {0}", v.GetType().Name);
                }
                Console.WriteLine();
            }
            finally
            {
                tokenSource.Dispose();
            }

        }
    }
}
