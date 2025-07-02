using AwaneCore;
using SharedInterfaces;

Console.WriteLine("=== Prototype2 - IPai Consumer ===");
Console.WriteLine($"PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
Console.WriteLine();

// Awaneシステムを起動（コンポーネントは登録しない）
var awane = AwaneSystem.Instance;
awane.StartRemoteServer();

Console.WriteLine("リモートサーバーが起動しました。");
Console.WriteLine("Prototype1で登録されたIPaiを検索します...");
await Task.Delay(1000); // サーバー起動待ち

// 他のプロセスからIPaiを探す
var remotePai = await AwaneRemote.GetComponentLocalMachine<IPai>();
if (remotePai != null)
{
    Console.WriteLine("IPaiが見つかりました！");
    
    // リモートメソッドを呼び出す
    var result = await remotePai.PaiMethodAsync(new PaiParameter 
    { 
        TaskName = "Prototype2からのタスク", 
        Priority = 5 
    });
    
    Console.WriteLine($"実行結果: {result.Message} (Success: {result.Success})");
}
else
{
    Console.WriteLine("IPaiが見つかりませんでした。");
    Console.WriteLine("Prototype1が起動していることを確認してください。");
}

Console.WriteLine();
Console.WriteLine("Enterキーで終了...");
Console.ReadLine();

awane.Dispose();
AwaneProcessDiscovery.StopRegistryServer();