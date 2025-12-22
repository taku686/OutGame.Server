# PjOutGame.Server

PjOutGameプロジェクトのサーバー側実装です。

## 概要

このリポジトリは、.NET 9.0を使用したgRPC/MagicOnionサーバーです。

## 技術スタック

- .NET 9.0
- gRPC
- MagicOnion 7.0.8

## セットアップ

### 前提条件

- .NET 9.0 SDK
- Git

### セットアップ手順

1. リポジトリをクローン：
```powershell
git clone https://github.com/your-username/PjOutGame.Server.git
cd PjOutGame.Server
```

2. サブモジュールを初期化：
```powershell
git submodule update --init --recursive
```

3. プロジェクトをビルド：
```powershell
dotnet build
```

4. サーバーを実行：
```powershell
dotnet run
```

## サブモジュール

このプロジェクトは`PjOutGame.Shared`をGitサブモジュールとして参照しています。

### サブモジュールの更新

共有コードが更新された場合、以下のコマンドでサブモジュールを更新できます：

```powershell
git submodule update --remote src/PjOutGame.Shared
git add src/PjOutGame.Shared
git commit -m "Update shared code submodule"
```

## 開発

### プロジェクト構造

- `Program.cs` - エントリーポイント
- `Services/` - MagicOnionサービス実装
- `Protos/` - Protocol Buffers定義ファイル
- `src/PjOutGame.Shared/` - 共有コード（サブモジュール）

## 注意事項

- サブモジュールのパスは`src/PjOutGame.Shared`です
- 共有コードを変更する場合は、`PjOutGame.Shared`リポジトリで作業してください

