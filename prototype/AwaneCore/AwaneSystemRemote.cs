using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;

namespace AwaneCore;

// AwaneSystemの拡張（リモート通信対応）
public partial class AwaneSystem
{
    private NamedPipeServerStream? _pipeServer;
    private Task? _pipeServerTask;
    private CancellationTokenSource? _pipeServerCts;
    private readonly string _pipeName = $"Awane_{Process.GetCurrentProcess().Id}";
    
    // リモートアクセス用のパイプサーバーを開始
    public void StartRemoteServer()
    {
        if (_pipeServerTask != null) return;
        
        _pipeServerCts = new CancellationTokenSource();
        _pipeServerTask = Task.Run(async () => await RunPipeServer(_pipeServerCts.Token));
        
        Console.WriteLine($"[AwaneSystem] リモートサーバー開始: {_pipeName}");
        
        // プロセス情報をレジストリに登録
        RegisterToDiscovery();
    }
    
    // プロセス情報をディスカバリーに登録
    private async void RegisterToDiscovery()
    {
        var processInfo = new AwaneProcessInfo
        {
            ProcessId = Process.GetCurrentProcess().Id,
            ProcessName = Process.GetCurrentProcess().ProcessName,
            PipeName = _pipeName,
            LastHeartbeat = DateTime.UtcNow,
            Components = GetRegisteredComponents()
        };
        
        await AwaneProcessDiscovery.RegisterProcess(processInfo);
    }
    
    // 登録されているコンポーネント情報を取得
    private List<ComponentInfo> GetRegisteredComponents()
    {
        var result = new List<ComponentInfo>();
        
        foreach (var kvp in _components)
        {
            if (kvp.Key == typeof(IAsyncStartable) || 
                kvp.Key == typeof(ITickable) || 
                kvp.Key == typeof(IFixedTickable))
                continue;
            
            var component = kvp.Value;
            var interfaces = component.GetType().GetInterfaces()
                .Where(i => i != typeof(IAsyncStartable) && 
                           i != typeof(ITickable) && 
                           i != typeof(IFixedTickable))
                .Select(i => i.FullName ?? i.Name)
                .ToArray();
            
            result.Add(new ComponentInfo
            {
                TypeName = kvp.Key.FullName ?? kvp.Key.Name,
                Interfaces = interfaces
            });
        }
        
        return result;
    }
    
    // パイプサーバーの実行
    private async Task RunPipeServer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && IsActive)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(_pipeName, PipeDirection.InOut);
                
                await _pipeServer.WaitForConnectionAsync(cancellationToken);
                
                // 接続ごとに新しいタスクで処理
                _ = Task.Run(async () => await HandleClient(_pipeServer, cancellationToken));
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AwaneSystem] パイプサーバーエラー: {ex.Message}");
            }
        }
    }
    
    // クライアント接続の処理
    private async Task HandleClient(NamedPipeServerStream server, CancellationToken cancellationToken)
    {
        try
        {
            using (server)
            using (var reader = new StreamReader(server))
            using (var writer = new StreamWriter(server) { AutoFlush = true })
            {
                var request = await reader.ReadLineAsync();
                if (request != null)
                {
                    var message = JsonSerializer.Deserialize<AwaneMessage>(request);
                    if (message != null)
                    {
                        var response = await ProcessRemoteMessage(message);
                        await writer.WriteLineAsync(JsonSerializer.Serialize(response));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AwaneSystem] クライアント処理エラー: {ex.Message}");
        }
    }
    
    // リモートメッセージの処理
    private async Task<AwaneMessage> ProcessRemoteMessage(AwaneMessage message)
    {
        switch (message.MessageType)
        {
            case "MethodCall":
                return await ProcessMethodCall(message.Payload);
                
            case "GetComponent":
                var typeName = message.Payload;
                var hasComponent = _components.Keys.Any(t => t.FullName == typeName || t.Name == typeName);
                return new AwaneMessage 
                { 
                    MessageType = "ComponentStatus", 
                    Payload = hasComponent.ToString() 
                };
                
            default:
                return new AwaneMessage { MessageType = "Error", Payload = "Unknown message type" };
        }
    }
    
    // メソッド呼び出しの処理
    private async Task<AwaneMessage> ProcessMethodCall(string payload)
    {
        try
        {
            var call = JsonSerializer.Deserialize<RemoteMethodCall>(payload);
            if (call == null)
            {
                return new AwaneMessage 
                { 
                    MessageType = "MethodResult", 
                    Payload = JsonSerializer.Serialize(new RemoteMethodResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Invalid method call" 
                    })
                };
            }
            
            // 型を探す
            var targetType = _components.Keys.FirstOrDefault(t => 
                t.FullName == call.TypeName || t.Name == call.TypeName);
            
            if (targetType == null || !_components.TryGetValue(targetType, out var target))
            {
                return new AwaneMessage 
                { 
                    MessageType = "MethodResult", 
                    Payload = JsonSerializer.Serialize(new RemoteMethodResult 
                    { 
                        Success = false, 
                        ErrorMessage = $"Component {call.TypeName} not found" 
                    })
                };
            }
            
            // メソッドを探す
            var method = target.GetType().GetMethod(call.MethodName);
            if (method == null)
            {
                return new AwaneMessage 
                { 
                    MessageType = "MethodResult", 
                    Payload = JsonSerializer.Serialize(new RemoteMethodResult 
                    { 
                        Success = false, 
                        ErrorMessage = $"Method {call.MethodName} not found" 
                    })
                };
            }
            
            // 引数をデシリアライズ
            var parameters = method.GetParameters();
            var arguments = new object?[call.Arguments.Length];
            for (int i = 0; i < call.Arguments.Length; i++)
            {
                if (!string.IsNullOrEmpty(call.Arguments[i]))
                {
                    arguments[i] = JsonSerializer.Deserialize(call.Arguments[i], parameters[i].ParameterType);
                }
            }
            
            // メソッド実行
            var result = method.Invoke(target, arguments);
            
            // 非同期メソッドの場合
            if (result is Task task)
            {
                await task;
                
                // Task<T>の場合は結果を取得
                if (task.GetType().IsGenericType)
                {
                    var resultProperty = task.GetType().GetProperty("Result");
                    result = resultProperty?.GetValue(task);
                }
                else
                {
                    result = null; // Task (void)
                }
            }
            
            return new AwaneMessage 
            { 
                MessageType = "MethodResult", 
                Payload = JsonSerializer.Serialize(new RemoteMethodResult 
                { 
                    Success = true, 
                    ReturnValue = result != null ? JsonSerializer.Serialize(result, result.GetType()) : null
                })
            };
        }
        catch (Exception ex)
        {
            return new AwaneMessage 
            { 
                MessageType = "MethodResult", 
                Payload = JsonSerializer.Serialize(new RemoteMethodResult 
                { 
                    Success = false, 
                    ErrorMessage = ex.Message 
                })
            };
        }
    }
    
    // リモートサーバーの停止
    public void StopRemoteServer()
    {
        _pipeServerCts?.Cancel();
        _pipeServerTask?.Wait(TimeSpan.FromSeconds(2));
        _pipeServer?.Dispose();
        Console.WriteLine($"[AwaneSystem] リモートサーバー停止: {_pipeName}");
    }
}