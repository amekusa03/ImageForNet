# マルチOS（Windows, macOS, Linux）向けのインストーラーを作成方針

Avalonia（.NET）アプリケーションは、基本的に以下の2段階の手順で配布可能な形式にします。

Publish（発行）: .NETの機能を使って、各OS向けの実行ファイル（バイナリ）を作成する。
Packaging（パッケージ化）: OSごとのツールを使って、実行ファイルをインストーラー（.msi, .dmg, .debなど）に包む。

1. 実行ファイルの作成 (dotnet publish)
まず、インストーラーを作る前に、各OS単体で動作するファイルを作成する必要があります。配布方法には大きく分けて2種類あります。

## A) 自己完結型 (Self-Contained)

アプリケーションに.NETランタイムを同梱する方法です。ユーザーのPCに.NETがインストールされていなくても動作します。

Some text

**メリット**:
* ユーザーは.NETのインストールを意識する必要がない。 

**デメリット**:
* ファイルサイズが大きくなる（数十MB〜）。

bash
# Windows (x64) 用
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
# macOS (Apple Silicon / M1, M2...) 用
dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
# macOS (Intel) 用
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
# Linux (x64) 用
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true

## B) フレームワーク依存 (Framework-Dependent)　 **確認中**

> [!NOTE]
> .NET 6以降では、`-r` (ランタイムID) を指定するとデフォルトで自己完結型になります。フレームワーク依存にするには `--self-contained false` を明示的に指定する必要があります。
.NETランタイムを同梱せず、ユーザーのPCにインストールされている.NETを利用する方法です。  

**メリット**:
* ファイルサイズが非常に小さくなる（数MB）。

**デメリット**:
* ユーザーは事前に **.NET 8 Desktop Runtime** をインストールする必要がある。

bash
# Windows (x64) 用
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --no-self-contained
# (macOS, Linuxも同様に --self-contained false を追加する)

2. インストーラーの作成 (Packaging)
生成されたファイルを、各OSの作法に従ってインストーラーにします。

A. Windows (.exe / .msi)
Inno Setup: 最もポピュラーで扱いやすい無料ツールです。スクリプトを書いて、生成された .exe を指定するだけで、セットアップウィザード付きのインストーラー（setup.exe）が作れます。
Velopack: 最近のAvalonia界隈で推奨されているツールです。インストールだけでなく、自動アップデート機能も簡単に実装できます。
B. macOS (.dmg / .app)
macOSでは「インストーラー」というよりは、.app 形式のバンドルを作り、それを .dmg (ディスクイメージ) に入れて配布するのが一般的です。

手動/スクリプト: dotnet publish でできたファイルを所定のフォルダ構成 (Contents/MacOS/ 等) に配置し、hdiutil コマンドなどで .dmg 化します。
注意点: macOSで他人に配布する場合、Apple Developer Programへの登録と「コード署名（Notarization）」を行わないと、起動時に「壊れているため開けません」等の警告が出ます。
C. Linux (.deb / .rpm / AppImage)
dotnet-packaging: .NET用のツールで、.deb (Ubuntu/Debian系) や .rpm を作成できます。
AppImage: 1つのファイルでどのLinuxディストリビューションでも動く形式です。
推奨されるアプローチ
もし「自動アップデート」も含めて本格的にマルチプラットフォーム展開したい場合は、Velopack の導入を検討することをお勧めします。 単に「配布できればいい」というレベルであれば、まずは dotnet publish で生成されたフォルダをZip圧縮して配るのが一番簡単で、次のステップとして Inno Setup (Windows) などを導入するのが良いでしょう。

📚 基本概念
GitHub Releases = リポジトリの公開バージョン配布機能


メリット：✅ 認証不要で誰でもダウンロード可能✅ 配信用 URL が永続的✅ バージョン管理が楽✅ リリースノート（変更内容）も記載できる
🚀 実装ステップ
ステップ 1：Git タグを作成

git tag -a v1.0.0 -m "First Release"git push origin v1.0.0
ステップ 2：GitHub で Release ページを開く

https://github.com/amekusa03/ImageForNet/releases
ステップ 3：新しいリリースを作成
"Create a new release" をクリック
Tag version： v1.0.0 を選択
Release title： v1.0.0 など入力
Description： リリースノート記載
ステップ 4：Artifacts をアップロード
GitHub Actions から ImageForNet-Builds.zip をダウンロード
→ Release ページで "Attach binaries by dropping them here" に드래그&드롭

ステップ 5：Publish Release
"Publish release" をクリック
→ 配信用 URL が自動生成される

📂 構造イメージ

Release v1.0.0├─ ImageForNet-win-x64.zip├─ ImageForNet-osx-arm64.zip├─ ImageForNet-linux-x64.zip└─ Release Notes: 初版リリース
💡 実際にやってみますか？
今すぐ実行したければ、上記ステップ 1 から進めましょう！

どこまで説明しますか？

git add .github/workflows/build.yml && git commit -m "Remove Self-Contained publish steps to reduce artifact size" && git push

# v1.0.0 - Initial Release

## 必須要件

このアプリケーションを実行するには、**.NET 8 Desktop Runtime** のインストールが必要です。

### インストール方法

#### Windows

1. [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) にアクセス
2. **"Download .NET Desktop Runtime 8.0"** をクリック
3. インストーラーを実行

#### macOS

```bash
brew install dotnet

```

#### Linux (Ubuntu/Debian)

```bash
sudo apt-get update
sudo apt-get install dotnet-runtime-8.0
