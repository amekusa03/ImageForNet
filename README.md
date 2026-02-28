# ImageForNet

![Status](https://img.shields.io/badge/Status-WIP-yellow)

クロスプラットフォーム対応（Windows, macOS, Linux）の画像加工ツールです。
Webへのアップロード前などに、画像のEXIF情報の削除や、透かし（ウォーターマーク）の追加を簡単に行うことができます。

> ⚠️ **Note**: このプロジェクトは現在開発中（Work In Progress）です。機能追加やデバッグ作業が進行中です。

## ✨ 主な機能

*   **EXIF情報の削除**: プライバシー保護のために撮影情報を除去します。
*   **透かし（ウォーターマーク）の追加**:
    *   テキストの設定
    *   位置の調整（四隅）
    *   色、フォントサイズ、不透明度の設定
    *   システムフォント（日本語対応）の自動選択

## 📦 動作環境

*   Windows, macOS, Linux
*   .NET 8 Runtime

## 🛠️ 使用技術

*   **Framework**: [Avalonia UI](https://avaloniaui.net/) (.NET 8)
*   **Image Processing**: [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
*   **MVVM**: CommunityToolkit.Mvvm

## 🚀 開発環境のセットアップと実行

```:
dotnet add package SixLabors.ImageSharp
```

### 必要要件

*   .NET 8 SDK

### ビルドと実行

```bash
# リポジトリのクローン
git clone https://github.com/amekusa03/ImageForNet.git
cd ImageForNet

# 依存関係の復元と実行
dotnet restore
dotnet run --project ImageForNet
```

## 💾 配布用パッケージの作成 (Publish)

各OS向けの実行ファイルを作成するには、以下のコマンドを使用します。

### フレームワーク依存 (推奨)
ファイルサイズが小さくなりますが、実行には .NET 8 Desktop Runtime が必要です。

```bash
# Windows (x64)
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false

# macOS (Apple Silicon)
dotnet publish -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained false

# Linux (x64)
dotnet publish -c Release -r linux-x64 -p:PublishSingleFile=true --self-contained false
```