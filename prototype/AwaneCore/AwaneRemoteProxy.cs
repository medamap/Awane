using System.IO.Pipes;
using System.Text.Json;
using Castle.DynamicProxy;

namespace AwaneCore;

// リモートプロキシインターセプター
public class RemoteProxyInterceptor : IInterceptor
{
    private readonly string _pipeName;
    private readonly string _targetType;
    
    public RemoteProxyInterceptor(string pipeName, string targetType)
    {
        _pipeName = pipeName;
        _targetType = targetType;
    }
    
    public void Intercept(IInvocation invocation)
    {
        var request = new RemoteMethodCall
        {
            TypeName = _targetType,
            MethodName = invocation.Method.Name,
            Arguments = invocation.Arguments.Select(SerializeArgument).ToArray(),
            ArgumentTypes = invocation.Method.GetParameters()
                .Select(p => p.ParameterType.FullName ?? "")
                .ToArray()
        };
        
        try
        {
            var result = InvokeRemoteMethod(request).GetAwaiter().GetResult();
            
            if (result.Success)
            {
                if (result.ReturnValue != null && invocation.Method.ReturnType != typeof(void))
                {
                    invocation.ReturnValue = DeserializeReturnValue(result.ReturnValue, invocation.Method.ReturnType);
                }
            }
            else
            {
                throw new InvalidOperationException($"Remote call failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RemoteProxy] エラー: {ex.Message}");
            throw;
        }
    }
    
    private async Task<RemoteMethodResult> InvokeRemoteMethod(RemoteMethodCall call)
    {
        // TODO: 本実装では接続タイムアウトを設定可能にすべき
        // TODO: リトライ機構の実装（一時的な通信エラー対策）
        using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut);
        await client.ConnectAsync(5000);
        
        using var reader = new StreamReader(client);
        using var writer = new StreamWriter(client) { AutoFlush = true };
        
        var message = new AwaneMessage
        {
            MessageType = "MethodCall",
            Payload = JsonSerializer.Serialize(call)
        };
        
        await writer.WriteLineAsync(JsonSerializer.Serialize(message));
        var response = await reader.ReadLineAsync();
        
        if (response != null)
        {
            var responseMessage = JsonSerializer.Deserialize<AwaneMessage>(response);
            if (responseMessage?.MessageType == "MethodResult")
            {
                return JsonSerializer.Deserialize<RemoteMethodResult>(responseMessage.Payload) 
                    ?? new RemoteMethodResult { Success = false, ErrorMessage = "Deserialization failed" };
            }
        }
        
        return new RemoteMethodResult { Success = false, ErrorMessage = "No response received" };
    }
    
    private string SerializeArgument(object? arg)
    {
        if (arg == null) return "";
        // TODO: 本実装ではMessagePackを使用（高速化、バイナリサイズ削減のため）
        // Unity IL2CPPでの動作確認も必要
        return JsonSerializer.Serialize(arg, arg.GetType());
    }
    
    private object? DeserializeReturnValue(string json, Type returnType)
    {
        if (string.IsNullOrEmpty(json)) return null;
        
        // Task<T>の場合の処理
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var resultType = returnType.GetGenericArguments()[0];
            var value = JsonSerializer.Deserialize(json, resultType);
            
            // Task.FromResultを使って同期的にTaskを作成
            var fromResultMethod = typeof(Task).GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType);
            return fromResultMethod.Invoke(null, new[] { value });
        }
        
        return JsonSerializer.Deserialize(json, returnType);
    }
}

// リモートメソッド呼び出し情報
public class RemoteMethodCall
{
    public string TypeName { get; set; } = "";
    public string MethodName { get; set; } = "";
    public string[] Arguments { get; set; } = Array.Empty<string>();
    public string[] ArgumentTypes { get; set; } = Array.Empty<string>();
}

// リモートメソッド実行結果
public class RemoteMethodResult
{
    public bool Success { get; set; }
    public string? ReturnValue { get; set; }
    public string? ErrorMessage { get; set; }
}

// プロキシファクトリ
public static class AwaneProxyFactory
{
    // TODO: 本実装ではProxyGeneratorをDIで管理（シングルトンインスタンスの適切な管理）
    private static readonly ProxyGenerator _proxyGenerator = new();
    
    public static T CreateRemoteProxy<T>(string pipeName) where T : class
    {
        var interceptor = new RemoteProxyInterceptor(pipeName, typeof(T).FullName ?? typeof(T).Name);
        return _proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
    }
}