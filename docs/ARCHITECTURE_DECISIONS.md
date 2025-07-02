# Awane アーキテクチャ決定事項

## Unity対応を見据えた設計方針

### 1. インターフェース分離の原則

```csharp
// コアインターフェース（Unity/非Unity共通）
namespace Awane.Core
{
    public interface ISerializer
    {
        byte[] Serialize<T>(T value);
        T Deserialize<T>(byte[] data);
    }
    
    public interface IProxyGenerator
    {
        T CreateProxy<T>() where T : class;
    }
    
    public interface ITransport
    {
        Task<byte[]> SendAsync(byte[] data);
    }
}

// 実装は別アセンブリ
namespace Awane.Implementations.Dynamic
{
    public class DynamicProxyGenerator : IProxyGenerator { }
}

namespace Awane.Implementations.Static  
{
    public class StaticProxyGenerator : IProxyGenerator { }
}
```

### 2. 型制約を最初から意識

```csharp
// ❌ 避けるべきパターン
public interface IBadService
{
    dynamic GetDynamic();              // IL2CPPで動作しない
    Task<object> GetBoxed(object obj); // ボクシングのオーバーヘッド
    void UseReflectionEmit();          // 動的コード生成
}

// ✅ 推奨パターン
public interface IGoodService
{
    Task<TResult> GetDataAsync<TResult>(int id) where TResult : class;
    Task<MyData> ProcessAsync(MyRequest request);
}

// データ型も最初から属性付与
[MessagePackObject]
public class MyData
{
    [Key(0)] public int Id { get; set; }
    [Key(1)] public string Name { get; set; }
}
```

### 3. 拡張ポイントの準備

```csharp
public class AwaneBuilder
{
    // 各コンポーネントを差し替え可能に
    public ISerializer Serializer { get; set; } = new MessagePackSerializer();
    public IProxyGenerator ProxyGenerator { get; set; } = new DynamicProxyGenerator();
    public ITransport Transport { get; set; } = new NamedPipeTransport();
    
    // Unity用の設定メソッド（将来用）
    public AwaneBuilder UseUnityCompatibleMode()
    {
        // 実装は後回しでOK
        throw new NotImplementedException("Unity mode will be implemented in future");
    }
}
```

### 4. アセンブリ構成

```
Awane/
├── Awane.Core/              # Unity対応可能な純粋なインターフェース
│   ├── Awane.Core.csproj    # .NET Standard 2.0
│   └── Interfaces/
│
├── Awane/                   # メイン実装
│   ├── Awane.csproj        # .NET 6.0+
│   ├── DynamicProxy/       # Castle.Core使用
│   └── Serialization/      # MessagePack使用
│
└── Awane.Unity/            # 将来のUnity実装（今は作らない）
    ├── Awane.Unity.csproj
    └── StaticProxy/
```

## 今すぐやっておくべきこと

### 1. MessagePackの属性は付けておく
```csharp
[MessagePackObject]
public class RemoteCallRequest
{
    [Key(0)] public string ServiceName { get; set; }
    [Key(1)] public string MethodName { get; set; }
    [Key(2)] public byte[][] Arguments { get; set; }
}
```

### 2. インターフェースベースの設計
```csharp
// 実装に依存しない
public static class Awane
{
    private static IAwaneCore _core;
    
    public static void Initialize(Action<IAwaneBuilder> configure)
    {
        var builder = new AwaneBuilder();
        configure(builder);
        _core = builder.Build();
    }
}
```

### 3. ジェネリクスの使用注意
```csharp
// ❌ IL2CPPで問題になる可能性
public T CreateGeneric<T>() where T : new() => new T();

// ✅ 安全な使い方
public T GetComponent<T>() where T : class => _container.Resolve<T>();
```

## 今は気にしなくて良いこと

1. **Source Generator** - 後から追加可能
2. **Unity Package** - 別リポジトリでOK  
3. **IL2CPP最適化** - 動作確認後に対応
4. **Unity固有API** - 使わない

## チェックリスト

- [ ] インターフェースと実装の分離
- [ ] MessagePack属性の事前付与
- [ ] 型制約を意識したAPI設計
- [ ] 拡張ポイントの準備
- [ ] .NET Standard 2.0互換のコアライブラリ

この設計なら、Unity対応時の変更は最小限で済みます！