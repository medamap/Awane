# Awane (泡音) - Distributed Component System

Unity風の`GetComponent<T>()`で、プロセス・言語・場所を超えてコンポーネントを繋ぐ分散システムフレームワーク

## 🌟 概要

Awaneは、異なるプロセス、異なる言語、異なるマシンで動作するコンポーネントを、まるで同一プロセス内のように簡単に呼び出せる分散コンポーネントシステムです。

```csharp
// ローカルプロセスのコンポーネント
var calc = AwaneSystem.Instance.GetComponent<ICalc>();

// 別プロセスのAIエージェント
var ai = await AwaneRemote.GetComponentLocalMachine<IPai>();

// 将来: ネットワーク上の別マシン
var remote = await AwaneRemote.GetComponentNetwork<IService>("192.168.1.100");
```

## 🎯 特徴

### シンプルなAPI
- Unity開発者に馴染み深い`GetComponent<T>()`パターン
- 同期・非同期の両方をサポート
- 型安全な呼び出し

### 言語を超えた統合 (ACP: Awane Communicators Protocol)
- C#、Node.js、Python、Go...異なる言語間での相互呼び出し
- 統一されたプロトコルで通信
- 各言語の強みを活かした実装が可能

### 自動探索機能
- プロセスの自動登録・発見
- ハートビートによる生存確認
- 動的なコンポーネント管理

### AIエージェント統合
- Claude、GPT、Gemini等のAIサービスを統合
- AIノードプログラミングの実現
- 複数AIの協調動作

## 🚀 現在の状態

### ✅ 実装済み (プロトタイプ)

#### Phase 1: ローカルプロセス内
- 基本的なコンポーネント登録・取得
- DI (Dependency Injection) 統合
- IDisposableサポート

#### Phase 2: ローカルマシン内
- Named Pipesによるプロセス間通信
- プロセスレジストリ
- リモートプロキシ (Castle.Core)
- ハートビート機能

#### Phase 3: 言語間通信 (ACP)
- C# ⇔ Node.js 相互呼び出し
- JSONベースのメッセージング
- AIエージェント実装例 (ProtoPai)

### 🔄 開発中

#### Phase 4: ネットワーク通信
- gRPCによる高速通信
- サービスメッシュ対応
- 負荷分散

#### Phase 5: エンタープライズ機能
- 認証・認可
- 暗号化通信
- 監査ログ

## 📊 ロードマップ

```
2025 Q1: プロトタイプ完成 ✅
├── ローカルプロセス通信
├── Named Pipes実装
└── ACP仕様策定

2025 Q2: 言語拡張 🔄
├── Python対応
├── Go対応
└── MessagePackシリアライゼーション

2025 Q3: ネットワーク対応
├── gRPC実装
├── サービスディスカバリ
└── 負荷分散

2025 Q4: プロダクション対応
├── セキュリティ強化
├── パフォーマンス最適化
└── Unity統合

2026: グローバル展開
├── AWS Lambda対応
├── Kubernetes対応
└── エッジコンピューティング
```

## 🛠️ アーキテクチャ

### コンポーネントの階層

```
ワールドワイド (将来)
  ↑
ネットワーク (開発中)
  ↑
ローカルマシン (実装済み)
  ↑
ローカルプロセス (実装済み)
```

### 通信プロトコル

| スコープ | プロトコル | 状態 |
|---------|-----------|------|
| プロセス内 | 直接参照 | ✅ |
| ローカルマシン | Named Pipes | ✅ |
| LAN | gRPC | 🔄 |
| WAN | gRPC + TLS | 📋 |
| Lambda | HTTP/JSON | 📋 |

## 🚦 クイックスタート

### 1. 基本的な使い方

```csharp
// コンポーネントの登録
var awane = AwaneSystem.Instance;
awane.Register<ICalc>(new Calculator());

// コンポーネントの取得
var calc = awane.GetComponent<ICalc>();
var result = calc.Add(1, 2);
```

### 2. プロセス間通信

```csharp
// サーバー側（別プロセス）
var awane = AwaneSystem.Instance;
awane.Register(new ClaudeCodePai());
await awane.StartRemoteServer();

// クライアント側
var pai = await AwaneRemote.GetComponentLocalMachine<IPai>();
var result = await pai.ExecuteAIAgent("こんにちは");
```

### 3. 言語間通信 (ACP)

```javascript
// Node.js側
const awane = new AwaneSystem();
awane.register(new GeminiAI());
await awane.startServer();
```

```csharp
// C#側から呼び出し
var gemini = await AwaneRemote.GetComponentLocalMachine<IGeminiAI>();
var result = await gemini.Generate("詩を書いて");
```

## 📚 ドキュメント

- [ACP概要](docs/ACP-README.md) - ACPプロトコルの紹介
- [ACP仕様書](docs/ACP-SPEC.md) - 技術仕様
- [Lambda統合](docs/ACP-Lambda-Integration.md) - AWS Lambda対応
- [アーキテクチャ](docs/ARCHITECTURE_DECISIONS.md) - 設計思想

## 🤝 コントリビューション

現在はプロトタイプ段階のため、フィードバックを歓迎します！

## 📄 ライセンス

MIT License

## 🎵 名前の由来

「泡音（あわね）」- 複数のプロセス（泡）が音を立てながら繋がり、調和する様子から命名。それぞれ独立したプロセスが、美しく連携するシステムを表現しています。

---

*Awane - Connecting Components Beyond Boundaries*