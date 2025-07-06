# 実装ガイドライン

## プロジェクト構造

```
prototype-002/
├── proto/                  # Protocol Buffers定義
│   ├── awane.proto        # 基本的なRPC定義
│   ├── components.proto   # コンポーネント固有の定義
│   └── discovery.proto    # サービス発見用定義
├── csharp/                # C#実装
│   ├── Awane.Core/        # コアライブラリ
│   ├── Awane.Components/  # 基本コンポーネント
│   └── Awane.Examples/    # サンプル実装
├── nodejs/                # Node.js実装
│   ├── core/              # コアライブラリ
│   ├── components/        # 基本コンポーネント
│   └── examples/          # サンプル実装
└── docs/                  # ドキュメント
```

## 開発手順

### 1. Protocol Buffers定義から始める
```protobuf
// proto/components.proto
syntax = "proto3";

package awane.components;

service Airi {
    rpc CreateContext(CreateContextRequest) returns (Context);
    rpc GetCurrentQuestion(ContextId) returns (Question);
    rpc SendAnswer(AnswerRequest) returns (Response);
}

message CreateContextRequest {
    string working_directory = 1;
}

message Context {
    string session_id = 1;
    bool is_already_initialized = 2;
    bool is_locked = 3;
    string message = 4;
}
```

### 2. コード生成
```bash
# C#
protoc --csharp_out=csharp/Awane.Core/Generated \
       --grpc_out=csharp/Awane.Core/Generated \
       --plugin=protoc-gen-grpc=grpc_csharp_plugin \
       proto/*.proto

# Node.js
protoc --js_out=import_style=commonjs,binary:nodejs/core/generated \
       --grpc_out=nodejs/core/generated \
       --plugin=protoc-gen-grpc=grpc_node_plugin \
       proto/*.proto
```

### 3. Adapterパターンの実装

#### C#実装例
```csharp
// IComponentAdapter.cs
public interface IComponentAdapter
{
    Task<T> GetComponent<T>(ComponentLocation location) where T : class;
}

// DirectAdapter.cs
public class DirectAdapter : IComponentAdapter
{
    private readonly IServiceProvider _serviceProvider;
    
    public async Task<T> GetComponent<T>(ComponentLocation location) where T : class
    {
        return _serviceProvider.GetService<T>();
    }
}

// GrpcAdapter.cs
public class GrpcAdapter : IComponentAdapter
{
    public async Task<T> GetComponent<T>(ComponentLocation location) where T : class
    {
        var channel = CreateChannel(location);
        // 動的プロキシを生成してgRPCクライアントをラップ
        return ProxyGenerator.CreateProxy<T>(channel);
    }
}
```

#### Node.js実装例
```javascript
// adapters/grpcAdapter.js
class GrpcAdapter {
    async getComponent(interfaceName, location) {
        const channel = this.createChannel(location);
        const ServiceClass = this.loadServiceClass(interfaceName);
        return new ServiceClass(channel, grpc.credentials.createInsecure());
    }
    
    createChannel(location) {
        if (location.type === 'LocalMachine') {
            return `unix://${location.socketPath}`;
        }
        return `${location.host}:${location.port}`;
    }
}
```

## テスト戦略

### 1. ユニットテスト
- 各Adapterの個別テスト
- コンポーネントのビジネスロジックテスト

### 2. 統合テスト
```csharp
[Test]
public async Task CrossProcessCommunication()
{
    // プロセス1: サーバー起動
    var server = new AwaneServer();
    server.RegisterComponent<IAiri>(new AiriSensei());
    await server.StartAsync("unix:///tmp/test.sock");
    
    // プロセス2: クライアント接続
    var client = new AwaneClient();
    var airi = await client.GetComponent<IAiri>(
        Location.LocalMachine("unix:///tmp/test.sock")
    );
    
    // 動作確認
    var context = await airi.CreateContext("/test/dir");
    Assert.NotNull(context);
}
```

### 3. 言語間連携テスト
```javascript
// Node.jsクライアント → C#サーバー
describe('Cross-language communication', () => {
    it('should call C# Airi from Node.js', async () => {
        const client = new AwaneClient();
        const airi = await client.getComponent('IAiri', {
            type: 'LocalMachine',
            socketPath: '/tmp/awane/airi.sock'
        });
        
        const context = await airi.createContext('/test/dir');
        expect(context.sessionId).toBeDefined();
    });
});
```

## パフォーマンス考慮事項

### 1. 接続プーリング
```csharp
public class ChannelPool
{
    private readonly ConcurrentDictionary<string, Channel> _channels;
    
    public Channel GetOrCreate(string address)
    {
        return _channels.GetOrAdd(address, addr => 
            new Channel(addr, ChannelCredentials.Insecure));
    }
}
```

### 2. メッセージサイズの最適化
- 大きなペイロードはストリーミングを使用
- 頻繁に送信される小さなメッセージはバッチング

### 3. 非同期処理の活用
```csharp
// 複数コンポーネントの並列取得
var tasks = new[]
{
    Awane.GetComponentAsync<IAiri>(),
    Awane.GetComponentAsync<IPai>(),
    Awane.GetComponentAsync<IPoppo>()
};

var components = await Task.WhenAll(tasks);
```

## セキュリティ考慮事項

### 1. 認証・認可
- ローカル通信: ファイルシステムの権限を利用
- ネットワーク通信: TLS + トークンベース認証

### 2. 入力検証
```csharp
public async Task<AiriContext> CreateContext(string workingDirectory)
{
    // パス検証
    if (!IsValidPath(workingDirectory))
    {
        throw new ArgumentException("Invalid directory path");
    }
    
    // パストラバーサル対策
    var normalizedPath = Path.GetFullPath(workingDirectory);
    if (!normalizedPath.StartsWith(AllowedBasePath))
    {
        throw new SecurityException("Access denied");
    }
}
```

## デバッグとログ

### 1. 構造化ログ
```csharp
_logger.LogInformation("Component requested", new
{
    ComponentType = typeof(T).Name,
    Location = location,
    ProcessId = Process.GetCurrentProcess().Id,
    Timestamp = DateTime.UtcNow
});
```

### 2. 分散トレーシング
- OpenTelemetryの統合
- リクエストIDの伝播