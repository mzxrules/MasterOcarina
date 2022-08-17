using System;
using System.Drawing;
using System.Threading.Tasks;
using mzxrules.Helper;
using System.Threading;
using System.Collections.Generic;

namespace Spectrum
{
    public enum TextureFormat
    {
        invalid,
        IA4,
        IA8,
        IA16,
        RGB5A1,
        RGBA32
    }

    public class ColorBufferRequest
    {
        public TextureFormat Format { get; set; }

        public N64Ptr PixelPtr { get; set; }

        public N64Ptr PalettePtr { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Scale { get; set; }

        public static ColorBufferRequest GetFramebufferRequest(N64Ptr ptr)
        {
            return new ColorBufferRequest()
            {
                Format = TextureFormat.RGB5A1,
                PixelPtr = ptr,
                Width = 320,
                Height = 240,
                Scale = 2,
            };
        }
    }

    static class ColorBufferUtil
    {

        class TextureFormatInfo
        {
            public TextureFormat Format { get; set; }

            public int BPP { get; set; }

            public Func<byte[], int, int, Bitmap> CreateBitmap;
        }

        static readonly Dictionary<TextureFormat, TextureFormatInfo> formatInfo = new()
        {
            [TextureFormat.RGB5A1] = new TextureFormatInfo() 
            {
                Format = TextureFormat.RGB5A1, CreateBitmap = CreateRGB5A1, BPP = 16
            },
            [TextureFormat.RGBA32] = new TextureFormatInfo()
            {
                Format = TextureFormat.RGBA32, CreateBitmap = CreateRGBA, BPP = 32
            },
            [TextureFormat.IA8] = new TextureFormatInfo()
            {
                Format = TextureFormat.IA8, CreateBitmap = CreateIA8, BPP = 8
            }
        };
        public static bool IsSupported(TextureFormat format)
        {
            return formatInfo.ContainsKey(format);
        }

        //const int FRAMEBUFFER_SIZE = 320 * 240 * 2;

        public static void SaveBufferBitmap(TextureFormat format, N64Ptr start, int width, int height)
        {
            using (Bitmap frame = GetBufferBitmap(format, start, width, height))
            {
                frame.Save($"dump/img_{start:X6}_{width}x{height}.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        static Bitmap GetBufferBitmap(TextureFormat format, N64Ptr start, int width, int height)
        {
            byte[] pixelData = Zpr.ReadRam(start, GetSizeInBytes(format, width, height));
            return formatInfo[format].CreateBitmap(pixelData, width, height);
        }
        
        static int GetSizeInBytes(TextureFormat format, int width, int height)
        {
            var info = formatInfo[format];
            return (width * height * info.BPP + 7) / 8;
        }

        static Bitmap CreateRGBA(byte[] data, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    int o = (h * width + w) * 4;
                    int r = data[o + 0];
                    int g = data[o + 1];
                    int b = data[o + 2];
                    int a = data[o + 3];
                    image.SetPixel(w, h, Color.FromArgb(a, r, g, b));
                }
            }
            image.MakeTransparent(Color.SkyBlue);
            return image;
        }

        static Bitmap CreateRGB5A1(byte[] data, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    short pix = Endian.ConvertInt16(data, (h * width + w) * 2);
                    int r = (pix >> 8) & 0xF8; r += r >> 5;
                    int g = (pix >> 3) & 0xF8; g += g >> 5;
                    int b = (pix << 2) & 0xF8; b += (b >> 5);
                    int a = ((pix) & 1) * 255;
                    image.SetPixel(w, h, Color.FromArgb(a, r, g, b));
                }
            }

            image.MakeTransparent(Color.SkyBlue);
            return image;
        }

        static Bitmap CreateIA8(byte[] data, int width, int height)
        {
            Bitmap image = new Bitmap(width, height);

            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < width; w++)
                {
                    byte pix = data[h * width + w];
                    int i = (pix & 0xF0) + (pix >> 4);
                    int a = (pix & 0x0F); a += a << 4;
                    image.SetPixel(w, h, Color.FromArgb(a, i, i, i));
                }
            }
            image.MakeTransparent(Color.DarkMagenta);
            return image;
        }

        public static ConsoleKey ViewFrameBuffer(N64Ptr ptr)
        {
            return ViewColorBuffer(ColorBufferRequest.GetFramebufferRequest(ptr));
        }
        public static ConsoleKey ViewColorBuffer(ColorBufferRequest info)
        {
            Console.Clear();
            Console.SetCursorPosition(0, 0);
            Console.Write($"{info.PixelPtr}");
            
            var cancellationTokenSource = new CancellationTokenSource();

            void _draw_loop()
            {
                Point location = new Point(0, 2);

                Size fontSize = NativeMethods.GetConsoleFontSize();
                Size imageSize = new Size(info.Width * info.Scale, info.Height * info.Scale); // desired image size

                // translate character positions to pixels
                Rectangle imageRect = new Rectangle(
                        location.X * fontSize.Width,
                        location.Y * fontSize.Height,
                        imageSize.Width,
                        imageSize.Height);

                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(100);
                    using (Graphics g = Graphics.FromHwnd(NativeMethods.GetConsoleWindow()))
                    {
                        Image image = GetBufferBitmap(info.Format, info.PixelPtr, info.Width, info.Height);

                        //g.FillRectangle(new SolidBrush(Color.Black), imageRect);
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
