# Unity IL2CPP対応の考慮事項

## IL2CPPの制限

### 動的コード生成の制限
- `Reflection.Emit` 使用不可
- 実行時の型生成不可
- ジェネリクスの制限

### 影響を受ける機能
1. **動的プロキシ生成** - Castle.DynamicProxy使用不可
2. **リフレクションベースのシリアライザー** - 事前生成が必要
3. **Expression Trees** - 一部制限

## MessagePack-CSharpのUnity対応

### AOT Code Generation
```csharp
// mpc.exe でコード生成
mpc.exe -i MyAssembly.dll -o Generated/MessagePackGenerated.cs

// 生成されるフォーマッター
public sealed class PersonFormatter : IMessagePackFormatter<Person>
{
    public void Serialize(ref MessagePackWriter writer, Person value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(2);
        writer.Write(value.Name);
        writer.Write(value.Age);
    }
    
    public Person Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var count = reader.ReadArrayHeader();
        var name = reader.ReadString();
        var age = reader.ReadInt32();
        return new Person { Name = name, Age = age };
    }
}
```

### Unity用の設定
```csharp
// AOT用の初期化
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void Initialize()
{
    // 生成されたフォーマッターを登録
    StaticCompositeResolver.Instance.Register(
        MessagePack.Resolvers.GeneratedResolver.Instance,
        MessagePack.Resolvers.StandardResolver.Instance
    );
    
    var options = MessagePackSerializerOptions.Standard
        .WithResolver(StaticCompositeResolver.Instance);
    
    MessagePackSerializer.DefaultOptions = options;
}
```

## Awaneの対応戦略

### 1. Source Generator活用（.NET 5以降）
```csharp
// Source Generatorで事前生成
[AwaneSerializable]
public partial class MethodCallRequest
{
    public string MethodName { get; set; }
    public object[] Arguments { get; set; }
}

// 自動生成されるコード
partial class MethodCallRequest
{
    public static byte[] Serialize(MethodCallRequest value) { ... }
    public static MethodCallRequest Deserialize(byte[] bytes) { ... }
}
```

### 2. インターフェースベースのプロキシ事前生成
```csharp
// ビルド時にプロキシクラスを生成
[AwaneProxy]
public interface IPai
{
    Task<Result> ProcessAsync(Request request);
}

// 生成されるプロキシ
public class PaiProxy : IPai
{
    private readonly ITransport _transport;
    
    public async Task<Result> ProcessAsync(Request request)
    {
        var data = AwaneSerializer.Serialize(new MethodCall
        {
            Method = "ProcessAsync",
            Args = new[] { request }
        });
        
        var response = await _transport.SendAsync(data);
        return AwaneSerializer.Deserialize<Result>(response);
    }
}
```

### 3. 登録時のコード生成
```csharp
// Unity Editor拡張
[MenuItem("Awane/Generate Proxies")]
static void GenerateProxies()
{
    var interfaces = FindAllAwaneInterfaces();
    foreach (var intf in interfaces)
    {
        var code = GenerateProxyCode(intf);
        File.WriteAllText($"Generated/{intf.Name}Proxy.cs", code);
    }
}
```

## 実装フェーズ

### Phase 1: 通常の.NET（動的生成OK）
```csharp
public class DynamicProxyGenerator : IProxyGenerator
{
    public T CreateProxy<T>() where T : class
    {
        return new ProxyGenerator().CreateInterfaceProxyWithoutTarget<T>(
            new RemoteCallInterceptor()
        );
    }
}
```

### Phase 2: Unity対応（事前生成）
```csharp
public class StaticProxyGenerator : IProxyGenerator
{
    private readonly Dictionary<Type, Type> _proxyTypes = new();
    
    public T CreateProxy<T>() where T : class
    {
        // 事前生成されたプロキシを使用
        var proxyType = _proxyTypes[typeof(T)];
        return (T)Activator.CreateInstance(proxyType);
    }
}
```

### 実行環境での切り替え
```csharp
public static class Awane
{
    static Awane()
    {
        #if UNITY_2018_1_OR_NEWER && !UNITY_EDITOR
            ProxyGenerator = new StaticProxyGenerator();
            Serializer = new PrecompiledMessagePackSerializer();
        #else
            ProxyGenerator = new DynamicProxyGenerator();
            Serializer = new MessagePackSerializer();
        #endif
    }
}
```

## ベストプラクティス

### 1. インターフェース設計
```csharp
// シンプルな型を使用
public interface IMyService
{
    Task<string> GetDataAsync(int id);  // OK
    Task<MyData> GetComplexAsync(MyRequest request);  // OK（要フォーマッター）
    Task<dynamic> GetDynamicAsync();  // NG（IL2CPPで問題）
}
```

### 2. データ型の制約
```csharp
// MessagePackObject属性を付与
[MessagePackObject]
public class MyData
{
    [Key(0)]
    public int Id { get; set; }
    
    [Key(1)]
    public string Name { get; set; }
}
```

### 3. ビルドパイプライン
```yaml
# Unity Cloud Build
- step: Generate Formatters
  script: mpc.exe -i Assembly-CSharp.dll -o Assets/Generated/
  
- step: Generate Proxies
  script: awane-gen.exe -i Assembly-CSharp.dll -o Assets/Generated/
```

## まとめ

- **開発時**: 動的生成で高速開発
- **Unity向けビルド時**: 事前生成に切り替え
- **Source Generator**: 将来的な本命

これでUnityのIL2CPP環境でも動作可能です！