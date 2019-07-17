using System;
using System.Drawing;
using System.Threading.Tasks;
using mzxrules.Helper;
using System.Threading;

namespace Spectrum
{
    static class FramebufferUtil
    {
        const int BUFFER_SIZE = 320 * 240 * 2;

        public static void SaveFrameBufferBitmap(N64Ptr frameBuffer)
        {
            Bitmap frame = GetFrameBufferBitmap(frameBuffer);
            frame.Save($"dump/frame{frameBuffer:X6}.png", System.Drawing.Imaging.ImageFormat.Png);
        }
        

        static Bitmap GetFrameBufferBitmap(N64Ptr framebuffer)
        {
            byte[] pixelData = Zpr.ReadRam(framebuffer, BUFFER_SIZE);
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

        public static ConsoleKey ViewFrameBuffer(N64Ptr addr)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write($"{addr}");
            
            var cancellationTokenSource = new CancellationTokenSource();

            void _draw_loop()
            {
                Point location = new Point(0, 2);

                Size fontSize = NativeMethods.GetConsoleFontSize();
                Size imageSize = new Size(320 * 2, 240 * 2); // desired image size

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                    using (Graphics g = Graphics.FromHwnd(NativeMethods.GetConsoleWindow()))
                    {
                        Image image = GetFrameBufferBitmap(addr); 

                        // translating the character positions to pixels
                        Rectangle imageRect = new Rectangle(
                                location.X * fontSize.Width,
                                location.Y * fontSize.Height,
                                imageSize.Width,
                                imageSize.Height);
                        g.DrawImage(image, imageRect);
                    }
                }
            }

            Task task = Task.Factory.StartNew(_draw_loop, cancellationTokenSource.Token);

            ConsoleKey key;
            do
            {
                key = Console.ReadKey().Key;
            }
            while (!(key == ConsoleKey.DownArrow
                    || key == ConsoleKey.UpArrow));

            cancellationTokenSource.Cancel();


            try
            {
                Task.WaitAll(task);
            }
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
                cancellationTokenSource.Dispose();
            }
            return key;
        }
    }
}
