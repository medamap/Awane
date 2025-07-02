# ACP (Awane Communicators Protocol) 仕様書

## 概要
ACP (Awane Communicators Protocol) は、異なるプログラミング言語間でコンポーネントベースの通信を実現するプロトコルです。Unity風の`GetComponent<T>()`APIを使用して、プロセス間・言語間でのコンポーネント呼び出しを可能にします。

## 特徴
- **言語非依存**: C#、Node.js、Python等、様々な言語で実装可能
- **プロセス間通信**: Named Pipes、gRPC等による通信
- **自動探索**: レジストリによるコンポーネントの自動発見
- **Unity風API**: 直感的な`GetComponent<T>()`インターフェース
- **AIエージェント対応**: 複数のAIサービスを統合可能

## アーキテクチャ

### 1. コンポーネントレジストリ
各プロセスは起動時にレジストリに登録します。

```json
{
  "processId": 12345,
  "processName": "ProtoPai",
  "pipeName": "Awane_12345",
  "lastHeartbeat": "2024-01-01T00:00:00Z",
  "components": [
    {
      "typeName": "SharedInterfaces.IPai",
      "interfaces": ["SharedInterfaces.IPai"]
    }
  ]
}
```

### 2. 通信プロトコル

#### メッセージフォーマット
```json
{
  "MessageType": "MethodCall",
  "Payload": "..."
}
```

#### メッセージタイプ
- `Register`: プロセス登録
- `List`: アクティブプロセス一覧取得
- `Heartbeat`: 生存確認
- `MethodCall`: メソッド呼び出し
- `MethodResult`: メソッド実行結果

### 3. メソッド呼び出し

#### リクエスト (RemoteMethodCall)
```json
{
  "TypeName": "SharedInterfaces.IPai",
  "MethodName": "ExecuteAIAgent",
  "Arguments": ["\"こんにちは\""],
  "ArgumentTypes": ["System.String"]
}
```

#### レスポンス (RemoteMethodResult)
```json
{
  "Success": true,
  "ReturnValue": "{\"Success\":true,\"Message\":\"こんにちは！\"}",
  "ErrorMessage": null
}
```

## 実装例

### C# (プロバイダー側)
```csharp
// コンポーネントの実装
public class ClaudeCodePai : IPai
{
    public async Task<PaiResult> ExecuteAIAgent(string prompt)
    {
        // Claude CLIを呼び出す実装
        return new PaiResult { Success = true, Message = "応答" };
    }
}

// 登録
var awane = AwaneSystem.Instance;
awane.Register(new ClaudeCodePai());
awane.StartRemoteServer();
```

### C# (コンシューマー側)
```csharp
// 他プロセスからコンポーネントを取得
var pai = await AwaneRemote.GetComponentLocalMachine<IPai>();
if (pai != null)
{
    var result = await pai.ExecuteAIAgent("質問");
    Console.WriteLine(result.Message);
}
```

### Node.js (プロバイダー側)
```javascript
// レジストリに登録
AwaneRegistry.register({
  processId: process.pid,
  processName: 'ProtoGeminiNode',
  pipeName: `Awane_${process.pid}`,
  components: [{
    typeName: 'SharedInterfaces.IPai',
    interfaces: ['SharedInterfaces.IPai']
  }]
});

// Named Pipeサーバー起動
const server = net.createServer(socket => {
  socket.on('data', async data => {
    const message = JSON.parse(data);
    if (message.MessageType === 'MethodCall') {
      const call = JSON.parse(message.Payload);
      // Gemini APIを呼び出す等の処理
      const result = await processMethodCall(call);
      socket.write(JSON.stringify({
        MessageType: 'MethodResult',
        Payload: JSON.stringify(result)
      }));
    }
  });
});
```

### Node.js (コンシューマー側)
```javascript
// C#のコンポーネントを呼び出し
const calc = await AwaneRemote.getComponentLocalMachine('SharedInterfaces.ICalc');
if (calc) {
  const result = await calc.Calc(5, 8);
  console.log(`Result: ${result}`); // => 40
}
```

## 課題と今後の拡張

### 1. インターフェース定義の共有
現在は各言語で個別にインターフェースを定義する必要があります。将来的には以下の方法を検討：

- **IDL (Interface Definition Language)** の導入
- **TypeScript定義ファイル** の共有
- **JSONスキーマ** による定義
- **実行時の型情報** をレジストリに含める

### 2. エラーハンドリング
- タイムアウト処理
- 再接続機能
- エラーメッセージの標準化

### 3. セキュリティ
- 認証・認可機構
- 通信の暗号化
- アクセス制御

### 4. パフォーマンス最適化
- MessagePackによる高速シリアライゼーション
- 接続プーリング
- キャッシング機構

## 用語集

- **ACP**: Awane Communicators Protocol
- **コンポーネント**: インターフェースを実装したサービス
- **レジストリ**: アクティブなプロセスとコンポーネントを管理
- **プロキシ**: リモートコンポーネントへの透過的アクセスを提供

## バージョン
- v0.1.0 (プロトタイプ) - 2025年1月

## 関連情報
- [Awaneプロジェクト](https://github.com/medamap/Awane)
- MCP (Model Context Protocol) - 比較対象