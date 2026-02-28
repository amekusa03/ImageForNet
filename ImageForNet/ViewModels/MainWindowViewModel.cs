using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Threading.Tasks;
using SixLaborsColor = SixLabors.ImageSharp.Color;

namespace ImageForNet.ViewModels;

public record NamedColor(string Name, Color Color);

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ImageProcessingService _imageService;

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

    public ImageProcessingService.WatermarkPosition[] Positions { get; } = 
        Enum.GetValues<ImageProcessingService.WatermarkPosition>();

    public MainWindowViewModel()
    {
        _imageService = new ImageProcessingService();
        SelectedColor = AvailableColors.First(c => c.Name == "Gray");
    }

    [RelayCommand]
    private async Task ProcessImageAsync()
    {
        if (StorageProvider is null) return;

        // ファイルを開くダイアログ
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "画像ファイルを選択",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        });

        if (files.Count >= 1)
        {
            var inputPath = files[0].Path.LocalPath;

            // 保存先を選択するダイアログ
            var saveFile = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存先を選択",
                SuggestedFileName = "processed_" + files[0].Name,
                DefaultExtension = Path.GetExtension(inputPath).TrimStart('.'),
                FileTypeChoices = new[] { FilePickerFileTypes.ImageAll }
            });

            if (saveFile is not null)
            {
                IsBusy = true;
                try
                {
                    var outputPath = saveFile.Path.LocalPath;
                    
                    // UIスレッドをブロックしないようにバックグラウンドで実行
                    await Task.Run(async () =>
                    {
                        // 1. EXIF除去して保存
                        await _imageService.RemoveExifAsync(inputPath, outputPath);

                        // 2. 透かしテキストがある場合、作成した画像に透かしを追加して上書き保存
                        if (!string.IsNullOrWhiteSpace(WatermarkText))
                        {
                            // AvaloniaのColorをImageSharpのColorに変換
                            var c = SelectedColor.Color;
                            var sharpColor = SixLaborsColor.FromRgba(c.R, c.G, c.B, c.A);

                            // 入力と出力を同じパスにすることで上書き保存
                            await _imageService.AddWatermarkAsync(outputPath, outputPath, WatermarkText, sharpColor, SelectedPosition, (float)WatermarkFontSize, (float)WatermarkOpacity);
                        }
                    });
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }
    }
}
