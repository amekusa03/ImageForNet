# ImageForNet

クロスプラットフォーム対応（Windows, macOS, Linux）の画像加工ツールです。
Webへのアップロード前などに、画像のEXIF情報の削除や、透かし（ウォーターマーク）の追加を簡単に行うことができます。

## Ver3.0更新内容

一括処理機能を追加しました。複数の画像ファイルをまとめて処理し、作業効率を大幅に向上させます。
また、プレビュー画面の表示ロジックも改善しました。

## ✨ 主な機能

* **EXIF情報の削除**: プライバシー保護のために撮影情報を除去します。
* **透かし（ウォーターマーク）の追加**:
  * 柔軟なカスタマイズ：テキスト、配置位置（四隅）、色（RGBA）、フォントサイズ、不透明度を自在に調整可能。
* **リアルタイムプレビュー**: 
  * 画面右側のプレビューエリアで、設定変更をリアルタイムに反映。保存前に仕上がりを正確に確認できます。
  * サンプル表示：起動時にサンプル画像が表示されるため、初めての利用でも加工イメージが即座に掴めます。
* **スマートなフォルダ選択**: 
* **一括処理機能**: 複数の画像ファイルを一度に選択し、EXIF削除や透かし追加をまとめて実行できます。進捗状況もリアルタイムで表示されます。
* **スマートなフォルダ選択**: 
  * OS最適化：初回起動時、Linuxは「ピクチャー」フォルダ、その他のOSでは「ホーム」フォルダをデフォルトで参照します。
  * 履歴記憶：前回使用した「開く」「保存」フォルダを記憶し、繰り返しの作業負荷を軽減します。
* **マルチプラットフォーム対応**: Windows, macOS, Linuxに対応。日本語フォントを含むシステムフォントの自動選択に対応しています。

## 📦 動作環境

* Windows, macOS, Linux
* .NET 8 Desktop Runtime

### .NET 8 Runtime のインストール方法

#### Windows
1. [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) にアクセス。
2. **"Download .NET Desktop Runtime 8.0"** をクリックして実行。

#### macOS
```bash
brew install dotnet
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install dotnet-runtime-8.0
```

## 🛠️ 使用技術

* **GUI Framework**: [Avalonia UI](https://avaloniaui.net/) (.NET 8)
* **Image Processing**: [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) - 高機能なマルチプラットフォーム画像処理ライブラリ
* **MVVM Pattern**: [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - .NET 8 と親和性の高い最新のMVVMツールキット

## 🚀 開発環境のセットアップと実行

* .NET 8 SDK

### ビルドと実行

```bash
# リポジトリのクローン
git clone https://github.com/amekusa03/ImageForNet.git
cd ImageForNet

dotnet restore
dotnet run --project ImageForNet
```

## 💾 配布用パッケージの作成 (Publish)

各OS向けの実行ファイルを作成するには、以下のコマンドを使用します。

### フレームワーク依存 (推奨)

ユーザー環境に .NET 8 Runtime がインストールされている前提の軽量なパッケージです。

```bash
# Windows (x64)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false

# macOS (Apple Silicon / M1, M2...)
dotnet publish -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained false

# Linux (x64)
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false
```

## ([📝 開発の背景や詳細](https://amekusa.vercel.app/app/image_for_net/image_for_net.html))

## (⭕動画(you tube))[https://youtu.be/dmGGBLUwR_o]
