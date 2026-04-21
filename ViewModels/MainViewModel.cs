using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageForNet.Services;
using SixLabors.ImageSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ImageForNet.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ImageProcessor _processor = new();
        private readonly StorageService _storage = new();

        [ObservableProperty]
        private string _watermarkText = "Sample Text";

        [ObservableProperty]
        private float _opacity = 0.5f;

        [ObservableProperty]
        private float _fontSizeRatio = 0.05f; // デフォルトで画像高さの5%

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusText = "待機中";

        [ObservableProperty]
        private double _progressValue;

        // 選択されたファイルパスのリスト
        private List<string> _selectedFilePaths = new();

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task SelectFilesAsync()
        {
            // 本来は View から StorageProvider を介して取得
            var initialDir = _storage.GetInitialDirectory();
            StatusText = "ファイル選択中...";
            
            // 実装例：ファイルが選択されたと仮定
            // _selectedFilePaths = results;
            // _storage.SaveLastDirectory(_selectedFilePaths[0]);
        }

        [RelayCommand(CanExecute = nameof(CanProcess))]
        private async Task ProcessBatchAsync()
        {
            IsBusy = true;
            ProgressValue = 0;
            int total = _selectedFilePaths.Count;
            int count = 0;

            // 出力ディレクトリの作成
            string outputDir = Path.Combine(_storage.GetInitialDirectory(), "processed");
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            await Task.Run(() =>
            {
                Parallel.ForEach(_selectedFilePaths, (filePath) =>
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);
                        string destPath = Path.Combine(outputDir, fileName);
                        
                        // ColorはSelectedColorから取得する想定（現在は仮にBlack）
                        _processor.ProcessImage(filePath, destPath, WatermarkText, WatermarkPosition.BottomRight, Color.Black, FontSizeRatio, Opacity);
                        
                        count++;
                        ProgressValue = (double)count / total * 100;
                        StatusText = $"処理中: {count}/{total}";
                    }
                    catch { /* エラーハンドリング: ログ出力など */ }
                });
            });

            StatusText = "一括処理完了";
            IsBusy = false;
        }

        private bool IsNotBusy() => !IsBusy;
        private bool CanProcess() => !IsBusy && _selectedFilePaths.Count > 0;

        // 設定変更時にリアルタイムで呼ばれることを想定
        partial void OnWatermarkTextChanged(string value) => UpdatePreview();
        partial void OnOpacityChanged(float value) => UpdatePreview();
        partial void OnFontSizeRatioChanged(float value) => UpdatePreview();

        private void UpdatePreview()
        {
            // プレビュー用の画像生成ロジックをここに記述
        }
    }
}