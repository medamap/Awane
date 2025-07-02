using AwaneCore;
using SharedInterfaces;

Console.WriteLine("=== Pai Demo - Claude CLI連携テスト ===");
Console.WriteLine();

// ローカルでIPaiを探す
var pai = Awane.GetComponent<IPai>();
if (pai == null)
{
    // リモートプロセスから探す
    Console.WriteLine("ローカルにIPaiが見つかりません。リモートプロセスを検索中...");
    pai = await AwaneRemote.GetComponentLocalMachine<IPai>();
}

if (pai == null)
{
    Console.WriteLine("IPaiが見つかりませんでした。");
    Console.WriteLine("Prototype1を起動してください。");
    return;
}

Console.WriteLine("IPaiが見つかりました！");
Console.WriteLine();

// インタラクティブモード
while (true)
{
    Console.Write("プロンプト（'exit'で終了）> ");
    var input = Console.ReadLine();
    
    if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
    {
        break;
    }
    
    Console.WriteLine();
    Console.WriteLine("Claude CLIに問い合わせ中...");
    
    var result = await pai.PaiMethodAsync(new PaiParameter
    {
        TaskName = input,
        Priority = 1
    });
    
    Console.WriteLine();
    if (result.Success)
    {
        Console.WriteLine("=== Claude応答 ===");
        Console.WriteLine(result.Message);
        Console.WriteLine("==================");
    }
    else
    {
        Console.WriteLine($"エラー: {result.Message}");
    }
    Console.WriteLine();
}

Console.WriteLine("終了します。");