namespace WeatherStation.Utilities;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.IO;

public static class ImageUtility {
    public static byte[] AddTextToImage(byte[] imageBytes, params (string text, (float x, float y) position, int fontSize, string colorHex)[] texts) {
        var memoryStream = new MemoryStream();

        var image = Image.Load(imageBytes);

        image.Clone(img => {
            foreach (var (text, (x, y), fontSize, colorHex) in texts) {
                var font = SystemFonts.CreateFont("Verdana", fontSize);
                var color = Rgba32.ParseHex(colorHex);

                img.DrawText(text, font, color, new PointF(x, y));
            }
        }).SaveAsPng(memoryStream);
        return memoryStream.ToArray();
    }
}