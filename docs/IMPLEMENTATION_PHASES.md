# Awane - 段階的実装計画

## Phase 1: 同一プロセス内（MVP）

### 実装範囲
```csharp
// 同一プロセス内でのDI
Awane.Initialize(config =>
{
    config.Register<IPai, Pai>();
    config.Register<IPoppo, Poppo>();
});

var pai = Awane.GetComponent<IPai>();  // 同一メモリ空間から取得
```

### テスト方法
```csharp
[Test]
public void 同一プロセス内でコンポーネント取得()
{
    // すべてインメモリで完結
    var service = Awane.GetComponent<ITestService>();
    Assert.NotNull(service);
}
```

## Phase 2: 同一PC内の別プロセス

### 実装範囲
```csharp
// プロセス分離を指定
Awane.Initialize(config =>
{
    config.Register<IPai, Pai>(ProcessMode.Separate);
    config.Register<IPoppo, Poppo>(ProcessMode.InProcess);
});
```

### プロセス間通信
- **Named Pipes**（Windows）
- **Unix Domain Socket**（Mac/Linux）
- プロセス一覧は共有ファイルで管理

```csharp
// プロセス情報の保存先
~/.awane/processes.json
{
  "processes": [
    {
      "name": "Pai",
      "pid": 12345,
      "endpoint": "pipe://awane-pai",
      "interfaces": ["IPai"]
    }
  ]
}
```

## Phase 3: ローカルネットワーク（将来拡張）

### インターフェースだけ準備
```csharp
public interface IServiceDiscovery
{
    Task<ServiceEndpoint> DiscoverAsync(string hostname, Type serviceType);
    Task RegisterAsync(ServiceInfo info);
}

// 使用イメージ（未実装）
var remoteService = await Awane.Find("192.168.1.100")
    .GetComponentAsync<IRemoteService>();
```

### 課題と解決案
- **ブロードキャスト問題** → mDNS/Consulなど既存技術を活用
- **セキュリティ** → 認証機能を後から追加
- **メタデータ** → gRPCのreflectionやOpenAPIを参考に

## 実装の優先順位

### 1. コアAPI（Phase 1で実装）
```csharp
public static class Awane
{
    // 基本API
    public static void Initialize(Action<IAwaneBuilder> configure);
    public static T GetComponent<T>() where T : class;
    public static T? TryGetComponent<T>() where T : class;
    
    // 将来の拡張用（インターフェースのみ）
    public static IServiceScope LocalProcess { get; }
    public static IServiceScope LocalMachine { get; }
    public static IServiceScope Find(string path);
}
```

### 2. ローカルプロセス管理（Phase 2で実装）
```csharp
internal class ProcessManager
{
    private readonly Dictionary<Type, ProcessInfo> _processes = new();
    
    public async Task<Process> LaunchProcessAsync(Type serviceType)
    {
        // 別exeを起動
        var processPath = GetProcessPath(serviceType);
        return Process.Start(processPath);
    }
    
    public async Task<T> CreateProxyAsync<T>() where T : class
    {
        var endpoint = await DiscoverProcessEndpoint<T>();
        return ProxyGenerator.CreateInterfaceProxy<T>(
            new RemoteCallInterceptor(endpoint)
        );
    }
}
```

### 3. プロキシ生成（Phase 1から段階的に）
```csharp
// Phase 1: 同一プロセス（単純なDI）
public class InProcessInterceptor : IInterceptor
{
    private readonly object _instance;
    
    public void Intercept(IInvocation invocation)
    {
        invocation.ReturnValue = invocation.Method.Invoke(
            _instance, 
            invocation.Arguments
        );
    }
}

// Phase 2: プロセス間通信
public class NamedPipeInterceptor : IInterceptor
{
    private readonly string _pipeName;
    
    public void Intercept(IInvocation invocation)
    {
        var request = SerializeMethodCall(invocation);
        var response = SendOverNamedPipe(_pipeName, request);
        invocation.ReturnValue = DeserializeResponse(response);
    }
}
```

## 設計方針

### インターフェースファースト
```csharp
// 将来の拡張を考慮したインターフェース
public interface ITransport
{
    Task<byte[]> SendAsync(byte[] data);
}

// 実装は段階的に追加
public class InMemoryTransport : ITransport { }      // Phase 1
public class NamedPipeTransport : ITransport { }     // Phase 2
public class TcpTransport : ITransport { }           // Phase 3
public class HttpTransport : ITransport { }          // Phase 3
```

### メタデータの扱い
```csharp
[ServiceMetadata(Version = "1.0")]
public interface IPai
{
    [OperationMetadata(Timeout = 5000)]
    Task<Result> ProcessAsync(Request request);
}

// メタデータはインターフェースに付与
// プロキシ生成時に読み取って通信に活用
```

## まとめ

1. **Phase 1**: 同一プロセス内で基本API確立
2. **Phase 2**: 同一PC内のプロセス間通信
3. **Phase 3**: ネットワーク対応（インターフェースのみ準備）

シンプルに始めて、段階的に「泡」を広げていく戦略です！