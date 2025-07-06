# コンポーネント設計

## コンポーネントの分類

### 1. ステートレスコンポーネント
単純なユーティリティや計算処理を提供するコンポーネント。

**特徴**:
- 状態を持たない
- いつでも破棄・再作成可能
- 複数プロセスから同時利用可能

**例**:
```csharp
public interface ILogger
{
    void Log(string message);
    void LogError(string error);
}

public interface ICalculator
{
    int Add(int a, int b);
    double CalculateTax(double amount);
}
```

### 2. ステートフルコンポーネント
セッションや状態管理が必要なコンポーネント。

**特徴**:
- コンテキストベースの状態管理
- 明示的なライフサイクル
- 排他制御が必要な場合あり

**例**:
```csharp
public interface IAiri
{
    Task<AiriContext> CreateContext(string workingDirectory);
}

public class AiriContext
{
    public string SessionId { get; set; }
    public bool IsAlreadyInitialized { get; set; }
    public bool IsNotGitHubRepositoryTop { get; set; }
    public bool IsLocked { get; set; }
    
    public Task<DialogState> GetCurrentQuestion();
    public Task SendAnswer(string answer);
}
```

## コンテキスト管理

### コンテキストの作成と利用
```csharp
// コンテキスト作成
var airi = Awane.GetComponent<IAiri>();
var context = await airi.CreateContext(workingDir);

// 状態チェック
if (context.IsAlreadyInitialized) 
{
    // 既に初期化済みの処理
    return;
}

// コンテキストを使った操作
var question = await context.GetCurrentQuestion();
await context.SendAnswer("はい");
```

### コンテキストの排他制御
```csharp
public class ContextManager
{
    private readonly ConcurrentDictionary<string, LockInfo> _locks;
    
    public async Task<bool> TryAcquireLock(string resourceId, TimeSpan duration)
    {
        var lockInfo = new LockInfo
        {
            AcquiredAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration),
            ProcessId = Process.GetCurrentProcess().Id
        };
        
        return _locks.TryAdd(resourceId, lockInfo);
    }
}
```

## コンポーネントのライフサイクル

### 1. 初期化フェーズ
```csharp
public interface IAsyncInitializable
{
    Task InitializeAsync();
}

public interface IRequireConfiguration
{
    void Configure(IConfiguration config);
}
```

### 2. 実行フェーズ
```csharp
public interface ITickable
{
    void Tick(float deltaTime);
}

public interface IAsyncTickable
{
    Task TickAsync(float deltaTime);
}
```

### 3. 終了フェーズ
```csharp
public interface IAsyncDisposable
{
    ValueTask DisposeAsync();
}
```

## コンポーネント登録と発見

### 登録
```csharp
// C# での登録
Awane.RegisterComponent<IAiri>(new AiriSensei());
Awane.RegisterComponent<ILogger>(new ConsoleLogger());

// メタデータ付き登録
Awane.RegisterComponent<IPai>(
    new Pai(),
    new ComponentMetadata
    {
        Version = "1.0.0",
        Tags = new[] { "assistant", "code-analysis" }
    }
);
```

### 発見
```csharp
// ローカル検索
var logger = Awane.GetComponent<ILogger>();

// 特定の場所から検索
var airi = Awane.GetComponent<IAiri>(Location.LocalMachine);

// 条件付き検索
var pai = Awane.GetComponent<IPai>(
    query => query.Version == "1.0.0" && query.HasTag("assistant")
);
```

## サンプルコンポーネント実装

### Poppo（ステートレス）
```csharp
[Component("Poppo", Version = "1.0.0")]
public class Poppo : IPoppo
{
    public async Task<BuildResult> BuildProject(string projectPath)
    {
        // ビルド処理
        return new BuildResult { Success = true };
    }
}
```

### Airi先生（ステートフル）
```csharp
[Component("AiriSensei", Version = "1.0.0")]
public class AiriSensei : IAiri, IAsyncInitializable
{
    private readonly Dictionary<string, SessionState> _sessions;
    
    public async Task InitializeAsync()
    {
        // 初期化処理
    }
    
    public async Task<AiriContext> CreateContext(string workingDirectory)
    {
        // 排他制御チェック
        if (await IsDirectoryLocked(workingDirectory))
        {
            return new AiriContext 
            { 
                IsLocked = true,
                Message = "ちょっと待ってね、まだ作業中よ"
            };
        }
        
        // コンテキスト作成
        var context = new AiriContext
        {
            SessionId = Guid.NewGuid().ToString(),
            WorkingDirectory = workingDirectory
        };
        
        // 初期化状態チェック
        context.IsAlreadyInitialized = await CheckIfInitialized(workingDirectory);
        
        return context;
    }
}