# プロセス間通信テスト手順

## 1. ターミナル1でPrototype1を起動
```bash
cd Prototype1
dotnet run
```

Prototype1が起動すると、IPaiプロバイダーとして動作します。

## 2. ターミナル2でPrototype2を起動
```bash
cd Prototype2
dotnet run
```

Prototype2が起動すると、自動的にPrototype1のIPaiを検索して実行します。

## 期待される動作
- Prototype2がPrototype1で実装されているIPaiを発見
- リモートプロキシ経由でPaiMethodAsyncを呼び出し
- Prototype1側でメソッドが実行され、結果がPrototype2に返される

## アーキテクチャ
```
SharedInterfaces/
  - IPai.cs (インターフェース定義)
  - ICalc.cs

AwaneCore/
  - AwaneSystem.cs (コンポーネント管理)
  - AwaneProcessDiscovery.cs (プロセス探索)
  - AwaneRemoteProxy.cs (動的プロキシ)

Prototype1/ (IPaiプロバイダー)
  - Pai.cs (IPaiの実装)
  - Program.cs

Prototype2/ (IPaiコンシューマー)
  - Program.cs (IPaiを使用)
```