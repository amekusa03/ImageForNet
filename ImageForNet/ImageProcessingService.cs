using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 画像処理を行うサービスクラスの例
/// </summary>
public class ImageProcessingService
{
    public enum WatermarkPosition
    {
        BottomRight,
        BottomLeft,
        TopRight,
        TopLeft
    }

    /// <summary>
    /// 画像からEXIF情報を削除し、指定されたパスに保存します。
    /// </summary>
    /// <param name="inputPath">元の画像ファイルのパス</param>
    /// <param name="outputPath">保存先の画像ファイルのパス</param>
    /// <returns>Task</returns>
    public async Task RemoveExifAsync(string inputPath, string outputPath)
    {
        try
        {
            // 1. usingステートメントで画像を読み込む
            //    これにより、処理後にリソースが正しく解放されます。
            using (Image image = await Image.LoadAsync(inputPath))
            {
                // 2. EXIF情報が存在するか確認
                if (image.Metadata.ExifProfile != null)
                {
                    Debug.WriteLine("EXIF情報が見つかりました。削除します。");
                    // 3. EXIFプロファイルをnullに設定して削除
                    image.Metadata.ExifProfile = null;
                }
                else
                {
                    Debug.WriteLine("この画像にはEXIF情報が含まれていません。");
                }

                // 4. 変更を新しいファイルに非同期で保存
                await image.SaveAsync(outputPath);
                Debug.WriteLine($"処理済みの画像を {outputPath} に保存しました。");
            }
        }
        catch (UnknownImageFormatException)
        {
            // ImageSharpがサポートしていない画像フォーマットの場合
            Debug.WriteLine($"エラー: サポートされていない画像形式です: {inputPath}");
            // TODO: ユーザーにエラーを通知する処理
        }
        catch (System.Exception ex)
        {
            // その他の予期せぬエラー
            Debug.WriteLine($"エラーが発生しました: {ex.Message}");
            // TODO: ユーザーにエラーを通知する処理
        }
    }

    /// <summary>
    /// 画像に透かしテキストを追加し、指定されたパスに保存します。
    /// </summary>
    /// <param name="inputPath">元の画像ファイルのパス</param>
    /// <param name="outputPath">保存先の画像ファイルのパス</param>
    /// <param name="watermarkText">透かし文字</param>
    /// <param name="color">文字色</param>
    /// <param name="position">位置</param>
    /// <param name="fontSize">フォントサイズ（オプション）</param>
    /// <param name="opacity">不透明度 (0.0 - 1.0)</param>
    public async Task AddWatermarkAsync(string inputPath, string outputPath, string watermarkText, Color color, WatermarkPosition position, float fontSize = 36, float opacity = 1.0f)
    {
        try
        {
            using (Image image = await Image.LoadAsync(inputPath))
            {
                FontFamily family = SystemFonts.Families.FirstOrDefault(); // デフォルトフォントを準備

                // 各OSで標準的に搭載されている可能性が高い日本語フォントのリスト
                var japaneseFontNames = new[]
                {
                    "Yu Gothic UI", // Windows
                    "Meiryo", // Windows
                    "Hiragino Kaku Gothic ProN", // macOS
                    "Noto Sans CJK JP", // Linux, Android
                    "MS Gothic" // Windows (Fallback)
                };

                // 日本語フォントがシステムに存在するかチェックし、最初に見つかったものを使用
                foreach (var name in japaneseFontNames)
                {
                    if (SystemFonts.TryGet(name, out family))
                    {
                        Debug.WriteLine($"日本語対応フォントとして '{name}' を使用します。");
                        break; // フォントが見つかったのでループを抜ける
                    }
                }

                var font = family.CreateFont(fontSize, FontStyle.Bold);
                
                // テキストのサイズを計測
                var textOptions = new TextOptions(font);
                var textSize = TextMeasurer.MeasureSize(watermarkText, textOptions);

                // 描画位置の計算 (マージンを20pxとする)
                float padding = 20f;
                float x = 0;
                float y = 0;

                switch (position)
                {
                    case WatermarkPosition.TopLeft:
                        x = padding;
                        y = padding;
                        break;
                    case WatermarkPosition.TopRight:
                        x = image.Width - textSize.Width - padding;
                        y = padding;
                        break;
                    case WatermarkPosition.BottomLeft:
                        x = padding;
                        y = image.Height - textSize.Height - padding;
                        break;
                    case WatermarkPosition.BottomRight:
                        x = image.Width - textSize.Width - padding;
                        y = image.Height - textSize.Height - padding;
                        break;
                }

                var drawingOptions = new DrawingOptions
                {
                    GraphicsOptions = new GraphicsOptions
                    {
                        BlendPercentage = opacity
                    }
                };

                // テキストを描画
                image.Mutate(ctx => ctx.DrawText(drawingOptions, watermarkText, font, color, new PointF(x, y)));

                await image.SaveAsync(outputPath);
                Debug.WriteLine($"透かし付き画像を {outputPath} に保存しました。");
            }
        }
        catch (System.Exception ex)
        {
            Debug.WriteLine($"透かし処理エラー: {ex.Message}");
            throw;
        }
    }
}
