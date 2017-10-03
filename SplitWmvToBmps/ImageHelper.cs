using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SplitWmvToBmps
{
    static class ImageHelper
    {
        private static byte[, ,] _rgb;

        static ImageHelper()
        {
            OptimizeCalculations();
        }

        private static void OptimizeCalculations()
        {
            _rgb = new byte[256, 256, 256];
            for (int r = 0; r <= 255; r++)
            {
                for (int g = 0; g <= 255; g++)
                {
                    for (int b = 0; b <= 255; b++)
                    {
                        _rgb[r, g, b] = (byte)(.3 * r + .59 * b + .11 * b);
                    }
                }
            }
        }

        public static void MakeGrayscale(string inputFile, string outFile)
        {
            if (Path.IsPathRooted(outFile))
            {
                var dir = Path.GetDirectoryName(outFile);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            using (var image = new Bitmap(inputFile))
            {
                if (File.Exists(outFile))
                {
                    File.Delete(outFile);
                }

                var result = MakeGrayscale2(image);
                result.Save(outFile);
                result.Dispose();
            }
        }

        // taken from excelent article http://www.switchonthecode.com/tutorials/csharp-tutorial-convert-a-color-image-to-grayscale
        // optimized for mass processing
        public static Bitmap MakeGrayscale2(Bitmap original)
        {
            unsafe
            {
                //create an empty bitmap the same size as original
                var newBitmap = new Bitmap(original.Width, original.Height, PixelFormat.Format24bppRgb);

                //lock the original bitmap in memory
                BitmapData originalData = original.LockBits(
                   new Rectangle(0, 0, original.Width, original.Height),
                   ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

                //lock the new bitmap in memory
                BitmapData newData = newBitmap.LockBits(
                   new Rectangle(0, 0, original.Width, original.Height),
                   ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

                //set the number of bytes per pixel
                int pixelSize = 3;

                for (int y = 0; y < original.Height; y++)
                {
                    //get the data from the original image
                    byte* oRow = (byte*)originalData.Scan0 + (y * originalData.Stride);

                    //get the data from the new image
                    byte* nRow = (byte*)newData.Scan0 + (y * newData.Stride);

                    for (int x = 0; x < original.Width; x++)
                    {
                        var xps = x * pixelSize;

                        //create the grayscale version

                        byte grayScale = _rgb[oRow[xps + 2], oRow[xps + 1], oRow[xps]];

                        //set the new image's pixel to the grayscale version
                        nRow[xps] =
                            nRow[xps + 1] =
                            nRow[xps + 2] = grayScale;
                    }
                }

                //unlock the bitmaps
                newBitmap.UnlockBits(newData);
                original.UnlockBits(originalData);

                return newBitmap;
            }
        }

    }
}
