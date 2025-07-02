using AwaneCore;
using SharedInterfaces;

namespace Prototype1;

// Paiの実装（ライフサイクル付き）
public class Pai : IPai, IAsyncStartable, ITickable, IFixedTickable
{
    private int _tickCount = 0;
    private int _fixedTickCount = 0;
    
    // 初期化処理
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[Pai] 初期化開始...");
        await Task.Delay(500, cancellationToken); // 初期化処理のシミュレート
        Console.WriteLine("[Pai] 初期化完了！");
    }
    
    // 毎フレーム呼ばれる
    public void Tick()
    {
        _tickCount++;
        if (_tickCount % 60 == 0) // 1秒ごとに出力
        {
            Console.WriteLine($"[Pai] Tick: {_tickCount}");
        }
    }
    
    // 固定間隔で呼ばれる
    public void FixedTick()
    {
        _fixedTickCount++;
        if (_fixedTickCount % 60 == 0) // 1秒ごとに出力
        {
            Console.WriteLine($"[Pai] FixedTick: {_fixedTickCount}");
        }
    }
    
    // ビジネスロジック
    public async Task<PaiResult> PaiMethodAsync(PaiParameter parameter)
    {
        Console.WriteLine($"[Pai] タスク処理開始: {parameter.TaskName} (優先度: {parameter.Priority})");
        
        // 処理のシミュレート
        await Task.Delay(100);
        
        return new PaiResult
        {
            Success = true,
            Message = $"タスク '{parameter.TaskName}' を処理しました"
        };
    }
}