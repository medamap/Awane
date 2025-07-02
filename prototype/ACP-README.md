# ACP (Awane Communicators Protocol) - はじめに

## ACPとは？

ACP (Awane Communicators Protocol) は、異なるプログラミング言語で実装されたAIエージェントやコンポーネントを、Unity風の`GetComponent()`で簡単に呼び出せるようにするプロトコルです。

## なぜACPが必要？

従来のマイクロサービスやMCP (Model Context Protocol) では：
- 言語ごとに異なるクライアントライブラリが必要
- 複雑な設定やボイラープレートコード
- AIエージェント同士の連携が困難

ACPなら：
```csharp
// C#からNode.jsのAIエージェントを呼び出し
var gemini = await AwaneRemote.GetComponentLocalMachine<IGeminiAI>();
var result = await gemini.Generate("詩を書いて");

// Node.jsからC#のコンポーネントを呼び出し
const calc = await AwaneRemote.getComponent('ICalc');
const result = await calc.Calc(10, 20);
```

## 主な特徴

### 🌐 言語の壁を超える
- C#、Node.js、Python、Go...どんな言語でも実装可能
- 同じインターフェースで相互に呼び出し可能

### 🤖 AIエージェントの統合が簡単
```
ProtoPai (C#)     → Claude CLI
ProtoGemini (Node) → Gemini API  
ProtoChatGPT (Python) → OpenAI API
```

すべて同じ方法で呼び出せます！

### 🎮 Unity風の直感的API
```csharp
var component = GetComponent<T>();  // おなじみの書き方
```

### 🔍 自動探索機能
- プロセスを起動するだけで自動的に登録
- レジストリが利用可能なコンポーネントを管理

## クイックスタート

### 1. AIエージェントを起動
```bash
# ターミナル1
cd ProtoPai
dotnet run

# ターミナル2  
cd ProtoGeminiNode
npm start
```

### 2. 別のプロセスから呼び出し
```csharp
// どちらのAIエージェントも同じように使える
var claudeAI = await AwaneRemote.GetComponentLocalMachine<IPai>();
var geminiAI = await AwaneRemote.GetComponentLocalMachine<IGeminiAI>();

// 組み合わせて使う
var question = "量子力学について説明して";
var claudeAnswer = await claudeAI.ExecuteAIAgent(question);
var geminiAnswer = await geminiAI.Generate(question);
```

## 実装例

### 食材計算ノード
```csharp
var calc = await AwaneRemote.GetComponentLocalMachine<IAICalc>();
var result = await calc.Add("たまご", "こむぎこ", "さとう");
// => "パンケーキ"
```

### 複数AIの組み合わせ
```csharp
var translator = await AwaneRemote.GetComponentLocalMachine<ITranslator>();
var summarizer = await AwaneRemote.GetComponentLocalMachine<ISummarizer>();

var japanese = "長い日本語テキスト...";
var english = await translator.Translate(japanese, "en");
var summary = await summarizer.Summarize(english);
```

## なぜ「泡音」(Awane)？

複数のプロセス（泡）が音を立てながら繋がり、調和する様子から名付けられました。
それぞれ独立したプロセスが、ACPを通じて美しく連携します。

## 今後の展望

- 🌍 ネットワーク越しの通信対応（gRPC）
- 🔐 セキュリティ機能の追加
- 📊 パフォーマンスモニタリング
- 🎯 より多くの言語サポート

## 始めてみよう！

1. [インストールガイド](./docs/installation.md)
2. [チュートリアル](./docs/tutorial.md)
3. [API リファレンス](./docs/api-reference.md)
4. [サンプルプロジェクト](./examples/)

---

*ACP - 言語の壁を超えて、AIエージェントを繋ぐ新しいプロトコル*