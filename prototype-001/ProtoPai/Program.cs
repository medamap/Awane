using Autofac;
using AwaneCore;
using SharedInterfaces;
using ProtoPai;

Console.WriteLine("=== ProtoPai - AIエージェントサービス ===");
Console.WriteLine($"PID: {System.Diagnostics.Process.GetCurrentProcess().Id}");
Console.WriteLine();

// Autofac コンテナの構築
var builder = new ContainerBuilder();

// AIエージェントの登録（将来的に複数のAIエージェントを切り替え可能）
builder.RegisterType<ClaudeCodePai>()
    .As<IPai>()
    .SingleInstance();

// TODO: 将来的な拡張例
// builder.RegisterType<ChatGPTPai>().As<IPai>().Named<IPai>("chatgpt");
// builder.RegisterType<GeminiPai>().As<IPai>().Named<IPai>("gemini");

var container = builder.Build();

// Awaneシステムに登録
var awane = AwaneSystem.Instance;
var pai = container.Resolve<IPai>();
awane.Register(pai);

// 初期化
await awane.InitializeAsync();

// リモートサーバーを開始
awane.StartRemoteServer();

Console.WriteLine("AIエージェントサービスが起動しました。");
Console.WriteLine("他のプロセスからIPaiを利用できます。");
Console.WriteLine();
Console.WriteLine("Enterキーで終了...");
Console.ReadLine();

// 終了処理
awane.Dispose();
AwaneProcessDiscovery.StopRegistryServer();

Console.WriteLine("=== ProtoPai 終了 ===");