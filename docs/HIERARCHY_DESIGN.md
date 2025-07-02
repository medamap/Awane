# Awane - 階層的サービス検索設計

## Unity風の階層構造

```csharp
// Unityスタイル
GameObject.Find("Player/Hand").GetComponent<WeaponController>();

// Awaneスタイル
Awane.Find("LocalNetwork/PC-Server").GetComponent<IPaiService>();
Awane.Find("Internet/AWS-Tokyo").GetComponent<IStorageService>();
```

## 階層構造

```
World (ルート)
├── LocalProcess (同一プロセス)
│   └── InMemory Services
├── LocalMachine (同一PC)
│   ├── Process: Pai
│   ├── Process: Poppo
│   └── Process: Chinatsu
├── LocalNetwork (LAN)
│   ├── PC-Server
│   │   └── Services...
│   └── PC-Dev-01
│       └── Services...
└── Internet (WAN)
    ├── AWS-Tokyo
    │   └── Services...
    └── Azure-Japan
        └── Services...
```

## API設計

### 基本的な使い方
```csharp
// デフォルト検索（近い順に探す）
var service = Awane.GetComponent<IMyService>();

// 特定の階層から検索
var localService = Awane.LocalMachine.GetComponent<IMyService>();
var lanService = Awane.LocalNetwork.GetComponent<IMyService>();
var cloudService = Awane.Internet.GetComponent<IMyService>();

// パス指定で検索
var specific = Awane.Find("LocalNetwork/PC-Server").GetComponent<IMyService>();
```

### 実装イメージ
```csharp
public static class Awane
{
    // 階層プロパティ
    public static IServiceScope LocalProcess { get; }
    public static IServiceScope LocalMachine { get; }
    public static IServiceScope LocalNetwork { get; }
    public static IServiceScope Internet { get; }
    
    // 汎用GetComponent（最も近いものを返す）
    public static T GetComponent<T>() where T : class
    {
        // 近い順に検索
        return LocalProcess.TryGetComponent<T>()
            ?? LocalMachine.TryGetComponent<T>()
            ?? LocalNetwork.TryGetComponent<T>()
            ?? Internet.TryGetComponent<T>();
    }
    
    // パス指定検索
    public static IServiceScope Find(string path)
    {
        var parts = path.Split('/');
        return NavigateToScope(parts);
    }
}

// スコープインターフェース
public interface IServiceScope
{
    string Name { get; }
    IServiceScope Parent { get; }
    IEnumerable<IServiceScope> Children { get; }
    
    T GetComponent<T>() where T : class;
    T? TryGetComponent<T>() where T : class;
    IServiceScope Find(string name);
}
```

## 登録時の階層指定

```csharp
Awane.Initialize(config =>
{
    // ローカルプロセス内
    config.RegisterInProcess<ICache, MemoryCache>();
    
    // ローカルマシンの別プロセス
    config.RegisterInLocalMachine<IPai, Pai>("Pai");
    
    // LAN内の別PC
    config.RegisterInNetwork<IDatabase, DatabaseService>("PC-Server");
    
    // インターネット上
    config.RegisterInCloud<IStorage, S3Service>("AWS-Tokyo");
});
```

## 高度な検索

### 条件付き検索
```csharp
// 最も近いが、レイテンシ100ms以内
var fast = Awane.GetComponent<IService>(x => x.Latency < 100);

// 特定のタグを持つサービス
var tagged = Awane.GetComponent<IService>(x => x.Tags.Contains("gpu"));
```

### 複数サービス取得
```csharp
// すべての階層から取得
var allServices = Awane.GetComponents<IWorkerService>();

// 特定階層のすべて
var lanWorkers = Awane.LocalNetwork.GetComponents<IWorkerService>();
```

## サービス情報の表示

```csharp
// Unity の Hierarchy ウィンドウ的な表示
Awane.PrintHierarchy();

/*
World
├── LocalProcess
│   ├── ICache (MemoryCache)
│   └── ILogger (ConsoleLogger)
├── LocalMachine
│   ├── Process: Pai
│   │   └── IPai (PaiOrchestrator)
│   └── Process: Poppo
│       └── IPoppo (PoppoPoller)
├── LocalNetwork
│   └── PC-Server (192.168.1.100)
│       ├── IDatabase (PostgreSQL)
│       └── ICache (Redis)
└── Internet
    └── AWS-Tokyo
        └── IStorage (S3Service)
*/
```

## 自動フォールバック

```csharp
// 優先順位付き取得
var db = Awane.GetComponentWithFallback<IDatabase>(
    "LocalNetwork/PC-Server",    // 第1希望
    "LocalMachine/SQLite",       // 第2希望  
    "Internet/AWS-RDS"           // 第3希望
);
```

## 動的な階層変更

```csharp
// サービスの移動（例：ローカル→クラウド）
await Awane.MigrateService<IDataProcessor>(
    from: "LocalMachine",
    to: "Internet/AWS-Tokyo"
);
```

## これにより実現できること

1. **位置透過性**: サービスがどこにあっても同じAPIで呼び出し
2. **段階的スケール**: ローカル→LAN→クラウドへ自然に拡張
3. **障害耐性**: 自動的に別階層のサービスにフォールバック
4. **開発効率**: Unity開発者に馴染みのあるAPI

まさに「泡（プロセス）」が階層的に広がっていくイメージです！