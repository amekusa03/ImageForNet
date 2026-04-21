using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using SixLaborsColor = SixLabors.ImageSharp.Color;

namespace ImageForNet.ViewModels;

public record NamedColor(string Name, Color Color);

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ImageProcessingService _imageService;
    private IStorageFolder? _lastUsedFolder;
    private string? _currentInputPath;
    private readonly SemaphoreSlim _previewSemaphore = new(1, 1);

    public string Greeting { get; } = "Welcome to Avalonia!";
    
    // Viewから設定されるストレージプロバイダー
    public IStorageProvider? StorageProvider { get; set; }

    [ObservableProperty]
    private string _watermarkText = "Sample";

    [ObservableProperty]
    private NamedColor _selectedColor;

    public List<NamedColor> AvailableColors { get; } = new()
    {
        new("Red", Colors.Red),
        new("Green", Colors.Green),
        new("Blue", Colors.Blue),
        new("Black", Colors.Black),
        new("White", Colors.White),
        new("Yellow", Colors.Yellow),
        new("Orange", Colors.Orange),
        new("Purple", Colors.Purple),
        new("Gray", Colors.Gray)
    };

    [ObservableProperty]
    private ImageProcessingService.WatermarkPosition _selectedPosition = ImageProcessingService.WatermarkPosition.BottomRight;

    [ObservableProperty]
    private double _watermarkFontSize = 128.0;

    [ObservableProperty]
    private double _watermarkOpacity = 0.9;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusText = "待機中";

    [ObservableProperty]
    private double _progressValue;

    // 選択されたファイルパスのリスト
    private List<string> _selectedFilePaths = new();

    [ObservableProperty]
    private Bitmap? _previewImage;

    public ImageProcessingService.WatermarkPosition[] Positions { get; } = 
        Enum.GetValues<ImageProcessingService.WatermarkPosition>();

    /// <summary>
    /// デザイン時やデフォルトの初期化用コンストラクタ
    /// </summary>
    public MainWindowViewModel() : this(new ImageProcessingService()) { }

    /// <summary>
    /// DI（依存性注入）をサポートするコンストラクタ
    /// </summary>
    public MainWindowViewModel(ImageProcessingService imageService)
    {
        _imageService = imageService;
        SelectedColor = AvailableColors.First(c => c.Name == "Gray");

        // 実行ファイルと同じ場所にあるサンプル画像を初期表示として読み込む
        var defaultImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EIA-CB.png");
        if (File.Exists(defaultImagePath))
        {
            _currentInputPath = defaultImagePath;
            _ = UpdatePreviewAsync();
        }
    }

    // プロパティが変更されたときにプレビューを更新する（CommunityToolkit.Mvvmの機能）
    partial void OnWatermarkTextChanged(string value) => _ = UpdatePreviewAsync();
    partial void OnSelectedColorChanged(NamedColor value) => _ = UpdatePreviewAsync();
    partial void OnSelectedPositionChanged(ImageProcessingService.WatermarkPosition value) => _ = UpdatePreviewAsync();
    partial void OnWatermarkFontSizeChanged(double value) => _ = UpdatePreviewAsync();
    partial void OnWatermarkOpacityChanged(double value) => _ = UpdatePreviewAsync();

    [RelayCommand(CanExecute = nameof(IsNotBusy))]
    private async Task SelectFilesAsync()
    {
        if (StorageProvider is null) return;

        var startLocation = _lastUsedFolder ?? await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures);

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "画像ファイルを選択（複数可）",
            AllowMultiple = true,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll },
            SuggestedStartLocation = startLocation
        });

        if (files.Count > 0)
        {
            _selectedFilePaths = files.Select(f => f.Path.LocalPath).ToList();
            _lastUsedFolder = await files[0].GetParentAsync();
            
            // 最初のファイルをプレビュー対象にする
            _currentInputPath = _selectedFilePaths[0];
            StatusText = $"{files.Count} 個のファイルが選択されました";
            await UpdatePreviewAsync();
            
            // コマンドの実行可否を再評価
            ProcessBatchCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanProcess))]
    private async Task ProcessBatchAsync()
    {
        if (StorageProvider is null || _selectedFilePaths.Count == 0) return;

        // 保存先フォルダの選択
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "保存先フォルダを選択",
            SuggestedStartLocation = _lastUsedFolder
        });

        if (folders.Count == 0) return;
        
        var outputDir = folders[0].Path.LocalPath;
        _lastUsedFolder = folders[0];

        IsBusy = true;
        ProgressValue = 0;
        int total = _selectedFilePaths.Count;
        int count = 0;

        try
        {
            await Task.Run(async () =>
            {
                foreach (var inputPath in _selectedFilePaths)
                {
                    string fileName = Path.GetFileName(inputPath);
                    string outputPath = Path.Combine(outputDir, "processed_" + fileName);

                    // 処理実行
                    await _imageService.RemoveExifAsync(inputPath, outputPath);
                    
                    if (!string.IsNullOrWhiteSpace(WatermarkText))
                    {
                        var sharpColor = ToSixLaborsColor(SelectedColor.Color);
                        await _imageService.AddWatermarkAsync(outputPath, outputPath, WatermarkText, sharpColor, SelectedPosition, (float)WatermarkFontSize, (float)WatermarkOpacity);
                    }

                    count++;
                    ProgressValue = (double)count / total * 100;
                    StatusText = $"処理中: {count}/{total}";
                }
            });
            StatusText = "一括処理が完了しました";
        }
        catch (Exception ex)
        {
            StatusText = $"エラー発生: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(IncludeCancelCommand = true)]
    private async Task ProcessImageAsync(CancellationToken token)
    {
        if (StorageProvider is null) return;

        // 開始フォルダの決定：記憶があればそれを使用、なければ「ピクチャー」フォルダを取得
        var startLocation = _lastUsedFolder ?? await StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Pictures);

        // ファイルを開くダイアログ
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "画像ファイルを選択",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll },
            SuggestedStartLocation = startLocation
        });

        if (files.Count >= 1)
        {
            // 選択されたファイルの親フォルダを記憶
            _lastUsedFolder = await files[0].GetParentAsync();
            var inputPath = files[0].Path.LocalPath;
            _currentInputPath = inputPath;

            // 保存先を選択するダイアログ
            var saveFile = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存先を選択",
                SuggestedFileName = "processed_" + files[0].Name,
                DefaultExtension = Path.GetExtension(inputPath).TrimStart('.'),
                FileTypeChoices = new[] { FilePickerFileTypes.ImageAll },
                SuggestedStartLocation = _lastUsedFolder
            });

            if (saveFile is not null)
            {
                // 保存先として選ばれたフォルダを次回の開始位置として記憶
                _lastUsedFolder = await saveFile.GetParentAsync();
                IsBusy = true;
                try
                {
                    var outputPath = saveFile.Path.LocalPath;
                    
                    // 入力値の簡易バリデーション
                    float fontSize = (float)Math.Max(1.0, WatermarkFontSize);
                    float opacity = (float)Math.Clamp(WatermarkOpacity, 0.0, 1.0);

                    // UIスレッドをブロックしないようにバックグラウンドで実行
                    await Task.Run(async () =>
                    {
                        // 1. EXIF除去して保存
                        await _imageService.RemoveExifAsync(inputPath, outputPath);

                        token.ThrowIfCancellationRequested();

                        // 2. 透かしテキストがある場合、作成した画像に透かしを追加して上書き保存
                        if (!string.IsNullOrWhiteSpace(WatermarkText))
                        {
                            // AvaloniaのColorをImageSharpのColorに変換
                            var sharpColor = ToSixLaborsColor(SelectedColor.Color);

                            // 入力と出力を同じパスにすることで上書き保存
                            await _imageService.AddWatermarkAsync(outputPath, outputPath, WatermarkText, sharpColor, SelectedPosition, fontSize, opacity);
                        }

                        // 処理結果をプレビュー画像として読み込む
                        PreviewImage = new Bitmap(outputPath);
                    }, token);
                }
                catch (OperationCanceledException)
                {
                    System.Diagnostics.Debug.WriteLine("画像処理がキャンセルされました。");
                }
                catch (Exception ex)
                {
                    // 実運用ではここでダイアログを表示するなどのエラー通知を行うのが理想的です
                    System.Diagnostics.Debug.WriteLine($"画像処理中にエラーが発生しました: {ex.Message}");
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }

    private bool IsNotBusy() => !IsBusy;
    private bool CanProcess() => !IsBusy && _selectedFilePaths.Count > 0;

    /// <summary>
    /// 現在の設定に基づいて右側のプレビュー画像を更新します。
    /// </summary>
    private async Task UpdatePreviewAsync()
    {
        if (string.IsNullOrEmpty(_currentInputPath) || !File.Exists(_currentInputPath) || IsBusy) return;

        // すでに更新処理が走っている場合は、この要求をスキップして競合を防ぐ
        if (!_previewSemaphore.Wait(0)) return;

        try
        {
            // プレビュー用の一時ファイルパス
            var tempPreviewPath = Path.Combine(Path.GetTempPath(), "imagefornet_preview.png");
            
            var sharpColor = ToSixLaborsColor(SelectedColor.Color);
            float fontSize = (float)Math.Max(1.0, WatermarkFontSize);
            float opacity = (float)Math.Clamp(WatermarkOpacity, 0.0, 1.0);

            await _imageService.AddWatermarkAsync(_currentInputPath, tempPreviewPath, WatermarkText, sharpColor, SelectedPosition, fontSize, opacity);

            // AvaloniaのBitmapとして読み込み（ファイルをロックしないようストリーム経由）
            using (var stream = File.OpenRead(tempPreviewPath))
            {
                var oldBitmap = PreviewImage;
                PreviewImage = new Bitmap(stream);
                oldBitmap?.Dispose(); // 古いビットマップを明示的に破棄してメモリを解放
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"プレビュー更新エラー: {ex.Message}");
        }
        finally
        {
            _previewSemaphore.Release();
        }
    }

    private static SixLaborsColor ToSixLaborsColor(Color c) => 
        SixLaborsColor.FromRgba(c.R, c.G, c.B, c.A);
}
