# Awane シリアライゼーション戦略

## MessagePackの現状

### v2.5以降の変更点
- `mpc.exe` 廃止
- Source Generator採用
- MSBuildタスクで自動生成

### Unityでの問題
```xml
<!-- Unity の .csproj は特殊で Source Generator が動かない -->
<Project>
  <!-- Unity が自動生成するため、カスタマイズ不可 -->
</Project>
```

## Awaneの対応戦略

### 1. 抽象化レイヤーを挟む

```csharp
// Awane独自のシリアライザーインターフェース
public interface IAwaneSerializer
{
    byte[] Serialize<T>(T value);
    T Deserialize<T>(byte[] data);
    
    // メソッド呼び出し専用
    byte[] SerializeMethodCall(string method, object[] args);
    (string method, object[] args) DeserializeMethodCall(byte[] data);
}
```

### 2. 複数実装を用意

```csharp
// 通常の.NET用（Source Generator使用）
public class MessagePackAwaneSerializer : IAwaneSerializer
{
    public byte[] Serialize<T>(T value)
    {
        return MessagePackSerializer.Serialize(value);
    }
}

// Unity用（手動フォーマッター）
public class UnityMessagePackSerializer : IAwaneSerializer
{
    private readonly Dictionary<Type, IMessagePackFormatter> _formatters;
    
    public UnityMessagePackSerializer()
    {
        // 手動で登録
        _formatters = new Dictionary<Type, IMessagePackFormatter>
        {
            [typeof(MethodCall)] = new MethodCallFormatter(),
            [typeof(MethodResult)] = new MethodResultFormatter()
        };
    }
}

// シンプルな独自実装
public class SimpleBinarySerializer : IAwaneSerializer
{
    // BinaryWriter/Reader使用のシンプル実装
}
```

### 3. Awane専用の型を限定

```csharp
// Awaneで必要な型は限定的
namespace Awane.Protocol
{
    [MessagePackObject]
    public class MethodCall
    {
        [Key(0)] public string ServiceName { get; set; }
        [Key(1)] public string MethodName { get; set; }
        [Key(2)] public byte[][] Arguments { get; set; }
        [Key(3)] public Dictionary<string, string> Headers { get; set; }
    }
    
    [MessagePackObject]
    public class MethodResult
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)] public byte[] Result { get; set; }
        [Key(2)] public string Error { get; set; }
    }
}
```

### 4. 手動フォーマッター生成支援

```csharp
// 開発時にコード生成
public static class FormatterGenerator
{
    public static string GenerateFormatter(Type type)
    {
        // LLMに投げやすいテンプレート
        return $@"
public sealed class {type.Name}Formatter : IMessagePackFormatter<{type.Name}>
{{
    public void Serialize(ref MessagePackWriter writer, {type.Name} value, MessagePackSerializerOptions options)
    {{
        // TODO: Implement serialization for:
{string.Join("\n", type.GetProperties().Select(p => $"        // - {p.Name} ({p.PropertyType.Name})"))}
    }}
    
    public {type.Name} Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {{
        // TODO: Implement deserialization
    }}
}}";
    }
}
```

## 実装優先順位

### Phase 1: シンプルに始める
```csharp
// 独自のバイナリ形式でもOK
public class SimpleBinarySerializer : IAwaneSerializer
{
    public byte[] SerializeMethodCall(string method, object[] args)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        writer.Write(method);
        writer.Write(args.Length);
        
        foreach (var arg in args)
        {
            // 基本型のみサポート
            switch (arg)
            {
                case int i: 
                    writer.Write("int");
                    writer.Write(i);
                    break;
                case string s:
                    writer.Write("string");
                    writer.Write(s);
                    break;
                // ...
            }
        }
        
        return ms.ToArray();
    }
}
```

### Phase 2: MessagePack統合
- 通常の.NETではSource Generator
- 必要に応じて手動フォーマッター

### Phase 3: Unity対応
- 事前生成ツール作成
- または別のシリアライザー採用

## 代替案

### MemoryPack
- より新しく、高速
- Unity対応も考慮されている
- ただし実績はMessagePackに劣る

### 独自プロトコル
- Awaneに必要な機能に特化
- 完全にコントロール可能
- 実装コストは高い

## 結論

1. **インターフェースで抽象化**してシリアライザーを差し替え可能に
2. **最初はシンプルな実装**で動作確認
3. **Unity対応は後回し**でOK
4. **手動フォーマッター**も選択肢として残す

これなら将来どんな状況にも対応できます！