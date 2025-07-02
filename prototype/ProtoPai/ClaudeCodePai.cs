using System.Diagnostics;
using AwaneCore;
using SharedInterfaces;

namespace ProtoPai;

// Claude Code実装のPaiちゃん
public class ClaudeCodePai : IPai, IAsyncStartable
{
    private readonly string _agentName = "ClaudeCodePai";
    
    // 初期化処理（必要に応じて）
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{_agentName}] 初期化開始...");
        // 必要な初期化処理があればここに記述
        await Task.Delay(100, cancellationToken);
        Console.WriteLine($"[{_agentName}] 初期化完了！");
    }
    
    // AIエージェント実行
    public async Task<PaiResult> ExecuteAIAgent(string prompt)
    {
        Console.WriteLine($"[{_agentName}] プロンプト受信: {prompt.Substring(0, Math.Min(50, prompt.Length))}...");
        
        try
        {
            // プロンプトを作成（シングルクォートのエスケープ処理）
            var escapedPrompt = prompt.Replace("'", "'\"'\"'");
            
            // Claude CLIコマンドを実行
            var processInfo = new ProcessStartInfo
            {
                FileName = "claude",
                Arguments = $"--dangerously-skip-permissions --print '{escapedPrompt}'",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            Console.WriteLine($"[{_agentName}] Claude CLIを実行中...");
            
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
            
            // TODO: Claude CLIのエラーパターンと対処法
            // 
            // 1. ログインエラー
            //    出力例: "Invalid API key"
            //    出力例: "Please run /login"
            //    出力例: "API Login Failure"
            //    対処法: ユーザーに claude login の実行を促す
            //
            // 2. レートリミットエラー
            //    出力例: "Claude AI usage limit reached|1750456800"
            //    形式: "エラーメッセージ|UNIXエポック秒"
            //    対処法: エポック秒まで待機するか、ユーザーに通知
            //           待機時間 = エポック秒 - 現在時刻 + 余裕（1分程度）
            //
            // 3. Execute error%
            //    出力例: "Execute error%"
            //    対処法: claude --continue で継続実行を試みる
            //
            // 4. その他のエラー
            //    対処法: エラーメッセージをそのまま返す
            
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"[{_agentName}] エラー発生: {error}");
                return new PaiResult
                {
                    Success = false,
                    Message = $"Claude CLIエラー: {error}"
                };
            }
            
            Console.WriteLine($"[{_agentName}] 応答取得成功");
            return new PaiResult
            {
                Success = true,
                Message = output.Trim()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{_agentName}] 例外発生: {ex.Message}");
            return new PaiResult
            {
                Success = false,
                Message = $"エラー: {ex.Message}"
            };
        }
    }
}