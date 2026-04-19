# Image for net Ver2.0

クロスプラットフォーム対応（Windows, macOS, Linux）の画像加工ツールです。
Webへのアップロード前などに、画像のEXIF情報の削除や、透かし（ウォーターマーク）の追加を簡単に行うことができます。

## Ver2.0更新内容

プレビュー画面を追加しました。

## ✨ 主な機能

* **EXIF情報の削除**: プライバシー保護のために撮影情報を除去します。
* **透かし（ウォーターマーク）の追加**:
* テキストの設定
* 位置の調整（四隅）
* 色、フォントサイズ、不透明度の設定
* **リアルタイムプレビュー**: 画面右側のエリアで、設定変更後のイメージを即座に確認できます。
* **スマートなフォルダ選択**: 前回使用した「開く」「保存」のフォルダを記憶し、作業効率を向上させます。
* **マルチプラットフォーム対応**: 日本語フォントを含むシステムフォントの自動選択に対応。

## 📦 動作環境

* Windows, macOS, Linux
* .NET 8 Runtime

## 🛠️ 使用技術

* **Framework**: [Avalonia UI](https://avaloniaui.net/) (.NET 8)
* **Image Processing**: [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
* **MVVM**: CommunityToolkit.Mvvm

## 🚀 開発環境のセットアップと実行

```bash
dotnet add package SixLabors.ImageSharp
```

### 必要要件

* .NET 8 SDK

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

## ([📝 開発の背景や詳細](https://amekusa.vercel.app/app/image_for_net/image_for_net.html))

## (⭕動画(you tube))[https://youtu.be/dmGGBLUwR_o]
