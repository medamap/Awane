using SharedInterfaces;
using ProtoPai;

Console.WriteLine("=== AI計算ノード 直接テスト ===");
Console.WriteLine();

// 直接ClaudeCodePaiインスタンスを作成
var pai = new ClaudeCodePai();

// 初期化
await pai.StartAsync(CancellationToken.None);

Console.WriteLine();
Console.WriteLine("質問: たまご＋こむぎこ＋さとう＋チョコレート＝？");
Console.WriteLine();

// Claude CLIを呼び出す
var result = await pai.ExecuteAIAgent("たまご＋こむぎこ＋さとう＋チョコレート＝？");

Console.WriteLine();
if (result.Success)
{
    Console.WriteLine("=== Claude AI の回答 ===");
    Console.WriteLine(result.Message);
    Console.WriteLine("=======================");
}
else
{
    Console.WriteLine($"エラー: {result.Message}");
}

Console.WriteLine();
Console.WriteLine("面白い回答でしたか？😊");