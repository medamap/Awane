# Awane アーキテクチャ概要

## 概要
Awaneは、プロセス間通信を透過的に扱うためのライブラリです。同一プロセス内から始まり、将来的にはネットワーク越しの通信まで、統一的なAPIで扱えることを目指しています。

## 設計原則
1. **透過性**: 利用者は通信方式の詳細を意識せずにコンポーネントを利用できる
2. **段階的拡張**: ローカルプロセスから始まり、徐々に通信範囲を拡大
3. **多言語対応**: C#とNode.jsを第一級サポート
4. **型安全性**: Protocol Buffersによる型定義

## レイヤー構成

```
┌─────────────────────────────────────┐
│         Application Layer           │
│  (PoppoAcademy Components)          │
├─────────────────────────────────────┤
│         Awane API Layer             │
│    (GetComponent, Context)          │
├─────────────────────────────────────┤
│         Adapter Layer               │
│  (Location-based routing)           │
├─────────────────────────────────────┤
│      Communication Layer            │
│  (Direct/UDS/gRPC)                  │
└─────────────────────────────────────┘
```

## コンポーネントモデル

### ステートレスコンポーネント
```csharp
// 単純に取得して使用
var logger = Awane.GetComponent<ILogger>();
logger.Log("Hello");
```

### ステートフルコンポーネント
```csharp
// コンテキストを作成して使用
var airi = Awane.GetComponent<IAiri>();
var context = await airi.CreateContext(workingDir);
if (context.IsAlreadyInitialized) {
    // 既に初期化済み
}
```

## 通信方式の選択

| Location | 通信方式 | 用途 |
|----------|---------|------|
| SameProcess | 直接参照 | 同一プロセス内（C#のみ） |
| LocalMachine | Unix Domain Socket | 同一マシン内の高速通信 |
| Network | gRPC/TCP | ネットワーク越しの通信 |

## 実装ロードマップ

### Phase 1: 基盤構築
- Protocol Buffers定義
- 基本的なgRPCサーバー/クライアント
- Adapterパターンの実装

### Phase 2: ローカル通信
- Unix Domain Socketサポート
- プロセス発見機構
- 基本的なコンポーネント実装

### Phase 3: ネットワーク対応
- TCP/HTTP2通信
- 認証・暗号化
- エラーハンドリング強化

### Phase 4: 運用機能
- モニタリング
- ロードバランシング
- サーキットブレーカー