using SkiaSharp;

namespace SmartDocHub.Web.Captcha;

public class CaptchaGenerator
{
    public static string CreateValidCode(int len)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        Random random = new Random();

        string code = new string(Enumerable.Repeat(chars, len).Select(s => s[random.Next(s.Length)]).ToArray());
        return code;
    }

    public static byte[] GenerateCode(string code, int width, int height)
    {
        using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
        {
            var canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            using (var textPaint = new SKPaint())
            using (var font = new SKFont())
            {
                textPaint.Color = SKColors.Black;
                textPaint.IsAntialias = true;
                textPaint.StrokeWidth = 3;

                font.Size = height * 0.8f;
                font.Typeface = SKTypeface.Default; 

                var textBounds = new SKRect();
                font.MeasureText(code, out textBounds, textPaint);

                var xText = (width - textBounds.Width) / 2;
                var yText = (height - textBounds.Height) / 2 - textBounds.Top;

                canvas.DrawText(code, xText, yText, SKTextAlign.Left, font, textPaint);
            }

            using (var linePaint = new SKPaint())
            {
                linePaint.Color = new SKColor(0, 0, 0, 128);
                linePaint.StrokeWidth = 1;
                linePaint.IsAntialias = true;

                var random = new Random();
                for (int i = 0; i < 5; i++)
                {
                    float x1 = 0;
                    float y1 = random.Next(height);
                    float x2 = width;
                    float y2 = random.Next(height);
                    canvas.DrawLine(x1, y1, x2, y2, linePaint);
                }
            }
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                return data.ToArray();
            }
        }
    }
}
