using System.Diagnostics;
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
    
    // ビジネスロジック - Claude CLIを呼び出す
    public async Task<PaiResult> PaiMethodAsync(PaiParameter parameter)
    {
        Console.WriteLine($"[Pai] タスク処理開始: {parameter.TaskName} (優先度: {parameter.Priority})");
        
        try
        {
            // プロンプトを作成（シングルクォートのエスケープ処理）
            var prompt = parameter.TaskName.Replace("'", "'\"'\"'");
            
            // Claude CLIコマンドを実行
            var processInfo = new ProcessStartInfo
            {
                FileName = "claude",
                Arguments = $"--dangerously-skip-permissions --print '{prompt}'",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            Console.WriteLine($"[Pai] Claude CLIを実行: {processInfo.Arguments}");
            
            using var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException("プロセスの起動に失敗しました");
            }
            
            // 標準出力を非同期で読み取る
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            
            // プロセスの終了を待つ
            await process.WaitForExitAsync();
            
            var output = await outputTask;
            var error = await errorTask;
            
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"[Pai] エラー発生: {error}");
                return new PaiResult
                {
                    Success = false,
                    Message = $"Claude CLIエラー: {error}"
                };
            }
            
            Console.WriteLine($"[Pai] Claude応答取得成功");
            return new PaiResult
            {
                Success = true,
                Message = output.Trim()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pai] 例外発生: {ex.Message}");
            return new PaiResult
            {
                Success = false,
                Message = $"エラー: {ex.Message}"
            };
        }
    }
}