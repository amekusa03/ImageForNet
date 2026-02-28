マルチOS（Windows, macOS, Linux）向けのインストーラーを作成方針

Avalonia（.NET）アプリケーションは、基本的に以下の2段階の手順で配布可能な形式にします。

Publish（発行）: .NETの機能を使って、各OS向けの実行ファイル（バイナリ）を作成する。
Packaging（パッケージ化）: OSごとのツールを使って、実行ファイルをインストーラー（.msi, .dmg, .debなど）に包む。
1. 実行ファイルの作成 (dotnet publish)
まず、インストーラーを作る前に、各OS単体で動作するファイルを作成する必要があります。配布方法には大きく分けて2種類あります。

### A) 自己完結型 (Self-Contained)

アプリケーションに.NETランタイムを同梱する方法です。ユーザーのPCに.NETがインストールされていなくても動作します。

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

### B) フレームワーク依存 (Framework-Dependent)　 **確認中**

> [!NOTE]
> .NET 6以降では、`-r` (ランタイムID) を指定するとデフォルトで自己完結型になります。フレームワーク依存にするには `--self-contained false` を明示的に指定する必要があります。
.NETランタイムを同梱せず、ユーザーのPCにインストールされている.NETを利用する方法です。  

**メリット**:
* ファイルサイズが非常に小さくなる（数MB）。

**デメリット**:
* ユーザーは事前に **.NET 8 Desktop Runtime** をインストールする必要がある。

bash
# Windows (x64) 用
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained false
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