using Prototype1;
using SharedInterfaces;

Console.WriteLine("=== AI計算ノード 直接テスト ===");
Console.WriteLine();

// 直接Paiインスタンスを作成
var pai = new Pai();

// 初期化
await pai.StartAsync(CancellationToken.None);

Console.WriteLine();
Console.WriteLine("質問: たまご＋こむぎこ＋さとう＋チョコレート＝？");
Console.WriteLine();

// Claude CLIを呼び出す
var result = await pai.PaiMethodAsync(new PaiParameter
{
    TaskName = "たまご＋こむぎこ＋さとう＋チョコレート＝？",
    Priority = 1
});

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