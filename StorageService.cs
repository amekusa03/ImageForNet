using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ImageForNet.Services
{
    public class StorageService
    {
        private string? _lastDirectory;

        public string GetInitialDirectory()
        {
            // 1. 最後に使用したフォルダがあればそれを優先
            if (!string.IsNullOrEmpty(_lastDirectory) && Directory.Exists(_lastDirectory))
            {
                return _lastDirectory;
            }

            // 2. OSごとのデフォルト設定
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                if (Directory.Exists(pictures)) return pictures;
            }

            // 3. Linux以外、または取得不可時はホームディレクトリ
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        public void SaveLastDirectory(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            
            var dir = Path.GetDirectoryName(filePath);
            if (Directory.Exists(dir)) _lastDirectory = dir;
        }
    }
}