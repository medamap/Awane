using System.Collections.Concurrent;

namespace AwaneCore;

// Awaneシステム本体
public partial class AwaneSystem
{
    private readonly ConcurrentDictionary<Type, object> _components = new();
    private readonly List<ITickable> _tickables = new();
    private readonly List<IFixedTickable> _fixedTickables = new();
    private readonly List<IAsyncStartable> _startables = new();
    
    private CancellationTokenSource? _mainLoopCts;
    private Task? _mainLoopTask;
    
    // シングルトンインスタンス（プロトタイプ用）
    // TODO: 本実装ではDIコンテナで管理し、IAwaneSystemインターフェースを介してアクセスすべき
    // Unity統合時は別途ファサードパターンで分離することを検討
    private static AwaneSystem? _instance;
    public static AwaneSystem Instance => _instance ??= new AwaneSystem();
    
    // コンポーネント登録
    public void Register<T>(T component) where T : class
    {
        var componentType = component.GetType();
        var interfaces = componentType.GetInterfaces();
        
        // 自身の型でも登録（具象クラスでの取得用）
        _components[componentType] = component;
        
        // インターフェースごとに登録
        foreach (var intf in interfaces)
        {
            if (intf != typeof(IAsyncStartable) && 
                intf != typeof(ITickable) && 
                intf != typeof(IFixedTickable))
            {
                _components[intf] = component;
                Console.WriteLine($"[AwaneSystem] 登録: {intf.Name} -> {componentType.Name}");
            }
        }
        
        // ライフサイクルインターフェースの登録
        if (component is IAsyncStartable startable)
            _startables.Add(startable);
            
        if (component is ITickable tickable)
            _tickables.Add(tickable);
            
        if (component is IFixedTickable fixedTickable)
            _fixedTickables.Add(fixedTickable);
            
        // IDisposableの場合は管理リストに追加
        if (component is IDisposable disposable)
        {
            _disposables.Add(disposable);
            Console.WriteLine($"[AwaneSystem] Disposable登録: {componentType.Name}");
        }
    }
    
    // コンポーネント取得（ローカルプロセスから）
    public T? GetComponent<T>() where T : class
    {
        if (_components.TryGetValue(typeof(T), out var component))
        {
            return component as T;
        }
        return null;
    }
    
    // 初期化
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== Awane System 初期化開始 ===");
        
        var tasks = _startables.Select(s => s.StartAsync(cancellationToken));
        await Task.WhenAll(tasks);
        
        Console.WriteLine("=== Awane System 初期化完了 ===");
    }
    
    // Tick実行（毎フレーム）
    public void ExecuteTick()
    {
        foreach (var tickable in _tickables)
        {
            tickable.Tick();
        }
    }
    
    // FixedTick実行（固定間隔）
    public void ExecuteFixedTick()
    {
        foreach (var fixedTickable in _fixedTickables)
        {
            fixedTickable.FixedTick();
        }
    }
    
    // スタンドアロン用メインループ
    public void StartMainLoopThread(CancellationToken cancellationToken = default)
    {
        _mainLoopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _mainLoopTask = Task.Run(async () => await MainLoopAsync(_mainLoopCts.Token));
    }
    
    public async Task StopMainLoopAsync()
    {
        _mainLoopCts?.Cancel();
        if (_mainLoopTask != null)
        {
            await _mainLoopTask;
        }
    }
    
    private async Task MainLoopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== Awane メインループ開始 ===");
        
        var tickInterval = TimeSpan.FromMilliseconds(16.67); // 約60fps
        var fixedTickInterval = TimeSpan.FromMilliseconds(16.67); // 固定60fps
        
        var lastTickTime = DateTime.UtcNow;
        var lastFixedTickTime = DateTime.UtcNow;
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                
                // Tick（可変フレームレート）
                ExecuteTick();
                
                // FixedTick（固定間隔）
                if (now - lastFixedTickTime >= fixedTickInterval)
                {
                    ExecuteFixedTick();
                    lastFixedTickTime = now;
                }
                
                // 次のフレームまで待機
                var elapsed = DateTime.UtcNow - now;
                var delay = tickInterval - elapsed;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // キャンセルは正常な終了
        }
        
        Console.WriteLine("=== Awane メインループ終了 ===");
    }
    
}

// Unity統合用の静的クラス
public static partial class Awane
{
    // Unity MonoBehaviourから呼び出し用
    public static void ExecuteTick()
    {
        AwaneSystem.Instance.ExecuteTick();
    }
    
    public static void ExecuteFixedTick()
    {
        AwaneSystem.Instance.ExecuteFixedTick();
    }
    
    // GetComponent風API
    public static T? GetComponent<T>() where T : class
    {
        return AwaneSystem.Instance.GetComponent<T>();
    }
}