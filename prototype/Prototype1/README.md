# Awane Prototype1 - プロセス間通信の実装

## 概要
Unity風のGetComponent<T>()でプロセス間通信を実現するプロトタイプです。

## 実装済み機能

### 基本機能
- **Dependency Injection**: Autofacを使用したDI
- **動的プロキシ**: Castle.Coreによるインターセプター
- **ライフサイクル管理**: IAsyncStartable, ITickable, IFixedTickable
- **Dispose対応**: IDisposableコンポーネントの自動破棄

### プロセス間通信
- **プロセス探索**: Named Pipesによるレジストリサーバー
- **リモートプロキシ**: 透過的なメソッド呼び出し
- **自動登録**: プロセス起動時の自動登録とハートビート

## 使い方

### 1. 基本的な使用方法
```csharp
// コンポーネントの登録
var awane = AwaneSystem.Instance;
awane.Register(new CalcMul());

// 同一プロセス内で取得
var calc = Awane.GetComponent<ICalc>();
var result = calc.Calc(5, 3);
```

### 2. プロセス間通信
```csharp
// リモートサーバーを開始
awane.StartRemoteServer();

// 他のプロセスからコンポーネントを検索
var remoteCalc = await AwaneRemote.GetComponentLocalMachine<ICalc>();
if (remoteCalc != null)
{
    var result = remoteCalc.Calc(10, 20);
}
```

### 3. テスト方法

#### ターミナル1（メインプロセス）
```bash
dotnet run
```

#### ターミナル2（リモートプロセス）
```bash
cd TestRemote
dotnet run
```

メインプロセスでEnterキーを押すと、リモートプロセスのICalcを検索して実行します。

## アーキテクチャ

### コンポーネント構成
- **AwaneSystem**: コンポーネント管理とライフサイクル制御
- **AwaneProcessDiscovery**: プロセス探索とレジストリ管理
- **AwaneRemoteProxy**: リモートメソッド呼び出しのプロキシ
- **AwaneSystemRemote**: Named Pipesサーバー実装

### 通信フロー
1. 各プロセスがレジストリに登録
2. GetComponentLocalMachine<T>()でレジストリに問い合わせ
3. 対象プロセスが見つかったらプロキシを生成
4. プロキシ経由でNamed Pipesでメソッド呼び出し

## 今後の拡張予定
- gRPCによるネットワーク越しの通信
- MessagePackによる高速シリアライゼーション
- Unity対応のためのIL2CPP考慮