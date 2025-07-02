using System.Diagnostics;
using System.IO.Pipes;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AwaneCore;

// プロセス間通信用のメッセージ
public class AwaneMessage
{
    // TODO: 本実装ではenumにすべき（タイプセーフティ、IntelliSense補完、タイポ防止のため）
    // 例: public MessageType MessageType { get; set; }
    public string MessageType { get; set; } = "";
    public string Payload { get; set; } = "";
}

// コンポーネント情報
public class ComponentInfo
{
    public string TypeName { get; set; } = "";
    public string[] Interfaces { get; set; } = Array.Empty<string>();
}

// プロセス情報
public class AwaneProcessInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = "";
    public string PipeName { get; set; } = "";
    public DateTime LastHeartbeat { get; set; }
    public List<ComponentInfo> Components { get; set; } = new();
}

// プロセス探索とレジストリ
public class AwaneProcessDiscovery
{
    private static readonly ConcurrentDictionary<string, AwaneProcessInfo> _registry = new();
    // TODO: 本実装では設定ファイルやDIで注入すべき
    private static readonly string RegistryPipeName = "AwaneRegistry";
    private static CancellationTokenSource? _registryCts;
    private static Task? _registryTask;
    
    // レジストリサーバーの開始（最初のプロセスが担当）
    public static void StartRegistryServer()
    {
        if (_registryTask != null) return;
        
        _registryCts = new CancellationTokenSource();
        _registryTask = Task.Run(async () => await RunRegistryServer(_registryCts.Token));
        Console.WriteLine("[AwaneRegistry] レジストリサーバー開始");
    }
    
    // レジストリサーバーの実行
    private static async Task RunRegistryServer(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var server = new NamedPipeServerStream(RegistryPipeName, PipeDirection.InOut);
                
                await server.WaitForConnectionAsync(cancellationToken);
                
                using var reader = new StreamReader(server);
                using var writer = new StreamWriter(server) { AutoFlush = true };
                
                var request = await reader.ReadLineAsync();
                if (request != null)
                {
                    var message = JsonSerializer.Deserialize<AwaneMessage>(request);
                    if (message != null)
                    {
                        var response = ProcessRegistryMessage(message);
                        await writer.WriteLineAsync(JsonSerializer.Serialize(response));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AwaneRegistry] エラー: {ex.Message}");
            }
        }
    }
    
    // レジストリメッセージの処理
    private static AwaneMessage ProcessRegistryMessage(AwaneMessage message)
    {
        switch (message.MessageType)
        {
            case "Register":
                var info = JsonSerializer.Deserialize<AwaneProcessInfo>(message.Payload);
                if (info != null)
                {
                    _registry[info.PipeName] = info;
                    Console.WriteLine($"[AwaneRegistry] プロセス登録: {info.ProcessName} (PID: {info.ProcessId})");
                    return new AwaneMessage { MessageType = "RegisterAck", Payload = "OK" };
                }
                break;
                
            case "List":
                // TODO: 本実装ではハートビートタイムアウト値を設定可能にすべき
                var activeProcesses = _registry.Values
                    .Where(p => (DateTime.UtcNow - p.LastHeartbeat).TotalSeconds < 30)
                    .ToList();
                return new AwaneMessage 
                { 
                    MessageType = "ListResponse", 
                    Payload = JsonSerializer.Serialize(activeProcesses) 
                };
                
            case "Heartbeat":
                var pipeName = message.Payload;
                if (_registry.ContainsKey(pipeName))
                {
                    _registry[pipeName].LastHeartbeat = DateTime.UtcNow;
                }
                return new AwaneMessage { MessageType = "HeartbeatAck", Payload = "OK" };
        }
        
        return new AwaneMessage { MessageType = "Error", Payload = "Unknown message type" };
    }
    
    // プロセスをレジストリに登録
    public static async Task<bool> RegisterProcess(AwaneProcessInfo processInfo)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", RegistryPipeName, PipeDirection.InOut);
            await client.ConnectAsync(1000);
            
            using var reader = new StreamReader(client);
            using var writer = new StreamWriter(client) { AutoFlush = true };
            
            var message = new AwaneMessage
            {
                MessageType = "Register",
                Payload = JsonSerializer.Serialize(processInfo)
            };
            
            await writer.WriteLineAsync(JsonSerializer.Serialize(message));
            var response = await reader.ReadLineAsync();
            
            if (response != null)
            {
                var ack = JsonSerializer.Deserialize<AwaneMessage>(response);
                return ack?.MessageType == "RegisterAck";
            }
        }
        catch (TimeoutException)
        {
            Console.WriteLine("[AwaneRegistry] レジストリサーバーが見つかりません。新規起動します。");
            StartRegistryServer();
            await Task.Delay(500); // サーバー起動待ち
            return await RegisterProcess(processInfo); // リトライ
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AwaneRegistry] 登録エラー: {ex.Message}");
        }
        
        return false;
    }
    
    // アクティブなプロセスのリストを取得
    public static async Task<List<AwaneProcessInfo>> GetActiveProcesses()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", RegistryPipeName, PipeDirection.InOut);
            await client.ConnectAsync(1000);
            
            using var reader = new StreamReader(client);
            using var writer = new StreamWriter(client) { AutoFlush = true };
            
            var message = new AwaneMessage { MessageType = "List" };
            await writer.WriteLineAsync(JsonSerializer.Serialize(message));
            
            var response = await reader.ReadLineAsync();
            if (response != null)
            {
                var listResponse = JsonSerializer.Deserialize<AwaneMessage>(response);
                if (listResponse?.MessageType == "ListResponse")
                {
                    return JsonSerializer.Deserialize<List<AwaneProcessInfo>>(listResponse.Payload) ?? new();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AwaneRegistry] リスト取得エラー: {ex.Message}");
        }
        
        return new List<AwaneProcessInfo>();
    }
    
    // レジストリサーバーの停止
    public static void StopRegistryServer()
    {
        _registryCts?.Cancel();
        _registryTask?.Wait(TimeSpan.FromSeconds(2));
        Console.WriteLine("[AwaneRegistry] レジストリサーバー停止");
    }
}