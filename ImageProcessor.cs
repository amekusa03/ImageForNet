using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageForNet.Services
{
    public enum WatermarkPosition
    {
        TopLeft, TopRight, BottomLeft, BottomRight
    }

    public class ImageProcessor
    {
        /// <summary>
        /// 画像を処理し、EXIF削除と透かし追加を行います。
        /// </summary>
        /// <param name="inputPath">入力ファイルパス</param>
        /// <param name="outputPath">出力ファイルパス</param>
        /// <param name="text">透かしテキスト</param>
        /// <param name="position">配置位置</param>
        /// <param name="color">色</param>
        /// <param name="fontSizeRatio">画像の高さに対するフォントサイズの比率 (例: 0.05 = 5%)</param>
        /// <param name="opacity">不透明度 (0.0 - 1.0)</param>
        public void ProcessImage(string inputPath, string outputPath, string text, WatermarkPosition position, Color color, float fontSizeRatio, float opacity)
        {
            using var image = Image.Load(inputPath);

            // 1. EXIF情報の削除
            image.Metadata.ExifProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.XmpProfile = null;

            // 2. 画像の高さ(Y)に基づいてフォントサイズを動的に計算
            // これにより、4K画像でも低解像度画像でも、透かしの相対的な大きさが統一されます
            float calculatedFontSize = image.Height * fontSizeRatio;

            // フォントの取得
            var font = SystemFonts.CreateFont("Arial", calculatedFontSize); 
            
            image.Mutate(ctx =>
            {
                var size = TextMeasurer.MeasureSize(text, new TextOptions(font));
                var point = GetRenderPoint(image.Width, image.Height, size.Width, size.Height, position);
                
                // 指定された不透明度を適用した色を作成
                var colorWithOpacity = color.WithAlpha(opacity);

                ctx.DrawText(text, font, colorWithOpacity, point);
            });

            image.Save(outputPath);
        }

        private PointF GetRenderPoint(int imgW, int imgH, float textW, float textH, WatermarkPosition pos)
        {
            // 余白も画像の高さに比例させる（例: 高さの2%）ことで、全体のレイアウトバランスを維持します
            float margin = imgH * 0.02f;
            return pos switch
            {
                WatermarkPosition.TopLeft => new PointF(margin, margin),
                WatermarkPosition.TopRight => new PointF(imgW - textW - margin, margin),
                WatermarkPosition.BottomLeft => new PointF(margin, imgH - textH - margin),
                WatermarkPosition.BottomRight => new PointF(imgW - textW - margin, imgH - textH - margin),
                _ => new PointF(margin, margin)
            };
        }
    }
}