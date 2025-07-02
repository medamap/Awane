using System.Collections.Concurrent;

namespace AwaneCore;

// 拡張版AwaneSystem（Dispose対応）
public partial class AwaneSystem : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed = false;
    private bool _isActive = true; // 外部からのアクセスを受け付けるか
    
    
    // システムのアクティブ状態
    public bool IsActive => _isActive && !_disposed;
    
    // 外部からのアクセスを停止
    public void Deactivate()
    {
        _isActive = false;
        Console.WriteLine("[AwaneSystem] 外部アクセスを停止しました");
    }
    
    // Dispose実装
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            Console.WriteLine("=== Awane System 終了処理開始 ===");
            
            // メインループ停止
            _mainLoopCts?.Cancel();
            _mainLoopTask?.Wait(TimeSpan.FromSeconds(5));
            
            // 登録されたDisposableを逆順で破棄
            for (int i = _disposables.Count - 1; i >= 0; i--)
            {
                try
                {
                    var disposable = _disposables[i];
                    Console.WriteLine($"[AwaneSystem] Disposing: {disposable.GetType().Name}");
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AwaneSystem] Dispose error: {ex.Message}");
                }
            }
            
            // リモートサーバーを停止
            StopRemoteServer();
            
            // コレクションをクリア
            _components.Clear();
            _tickables.Clear();
            _fixedTickables.Clear();
            _startables.Clear();
            _disposables.Clear();
            
            Console.WriteLine("=== Awane System 終了処理完了 ===");
        }
        
        _disposed = true;
        _isActive = false;
    }
}

// プロセス間通信用の拡張
public static class AwaneRemote
{
    // ローカルマシン内の全プロセスから検索
    public static async Task<T?> GetComponentLocalMachine<T>() where T : class
    {
        // まずローカルプロセスを確認
        var local = Awane.GetComponent<T>();
        if (local != null) return local;
        
        // 他のプロセスを探索
        Console.WriteLine($"[AwaneRemote] 他プロセスで {typeof(T).Name} を検索中...");
        
        var processes = await AwaneProcessDiscovery.GetActiveProcesses();
        foreach (var process in processes)
        {
            // 自プロセスはスキップ
            if (process.ProcessId == System.Diagnostics.Process.GetCurrentProcess().Id) 
                continue;
            
            // 対象の型を持つプロセスを探す
            var hasComponent = process.Components.Any(c => 
                c.TypeName == typeof(T).FullName || 
                c.Interfaces.Contains(typeof(T).FullName ?? typeof(T).Name));
            
            if (hasComponent)
            {
                Console.WriteLine($"[AwaneRemote] プロセス {process.ProcessName} (PID: {process.ProcessId}) で発見");
                return AwaneProxyFactory.CreateRemoteProxy<T>(process.PipeName);
            }
        }
        
        Console.WriteLine($"[AwaneRemote] {typeof(T).Name} が見つかりませんでした");
        return null;
    }
    
    // ネットワーク越しの検索
    public static T? GetComponentNetwork<T>(string hostname) where T : class
    {
        Console.WriteLine($"[AwaneRemote] {hostname} で {typeof(T).Name} を検索中...");
        
        // TODO: gRPC等でリモートホストに接続
        // var channel = GrpcChannel.ForAddress($"http://{hostname}:5000");
        // var client = new AwaneRemoteService.AwaneRemoteServiceClient(channel);
        // return CreateRemoteProxy<T>(client);
        
        return null;
    }
}

// Awane静的クラスの拡張
public static partial class Awane
{
    // Dispose
    public static void Dispose()
    {
        AwaneSystem.Instance.Dispose();
    }
    
    // システムの停止（外部アクセスを拒否）
    public static void Deactivate()
    {
        AwaneSystem.Instance.Deactivate();
    }
    
    // アクティブ状態の確認
    public static bool IsActive => AwaneSystem.Instance.IsActive;
}