using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using AwaneCore;
using SharedInterfaces;
using Prototype1;

Console.WriteLine("=== Prototype1 - IPai Provider ===");
Console.WriteLine();

// Autofac コンテナの構築
var builder = new ContainerBuilder();

// インターセプターを登録
builder.RegisterType<LoggingInterceptor>().AsSelf();

// Paiを登録（シングルトン）
builder.RegisterType<Pai>()
    .As<IPai>()
    .AsSelf()
    .SingleInstance();

// CalcMulも登録（前回の例）
builder.RegisterType<CalcMul>()
    .As<ICalc>()
    .EnableInterfaceInterceptors()
    .InterceptedBy(typeof(LoggingInterceptor))
    .SingleInstance();

var container = builder.Build();

// Awaneシステムの作成と初期化（シングルトンを使用）
var awane = AwaneSystem.Instance;

// コンテナからインスタンスを解決してAwaneに登録
var paiInstance = container.Resolve<Pai>(); // 具象クラスで解決
awane.Register(paiInstance);
awane.Register(container.Resolve<ICalc>());

// 初期化実行
await awane.InitializeAsync();

Console.WriteLine();

// 同一プロセス内でGetComponent
Console.WriteLine("=== GetComponent テスト ===");
var pai = Awane.GetComponent<IPai>();
Console.WriteLine($"GetComponent<IPai>: {(pai != null ? pai.GetType().Name : "null")}");

if (pai != null)
{
    var result = await pai.PaiMethodAsync(new PaiParameter 
    { 
        TaskName = "テストタスク", 
        Priority = 1 
    });
    Console.WriteLine($"結果: {result.Message}");
}
else
{
    Console.WriteLine("IPai が見つかりませんでした");
}

var calc = Awane.GetComponent<ICalc>();
Console.WriteLine($"GetComponent<ICalc>: {(calc != null ? calc.GetType().Name : "null")}");

if (calc != null)
{
    var calcResult = calc.Calc(5f, 8f);
    Console.WriteLine($"計算結果: 5 × 8 = {calcResult}");
}
else
{
    Console.WriteLine("ICalc が見つかりませんでした");
}

Console.WriteLine();
Console.WriteLine("=== メインループ開始（3秒間実行） ===");

// メインループ開始
using var cts = new CancellationTokenSource();
awane.StartMainLoopThread(cts.Token);

// 3秒間実行
await Task.Delay(3000);

// 停止
cts.Cancel();
await awane.StopMainLoopAsync();

// リモートサーバーを開始
awane.StartRemoteServer();

Console.WriteLine();
Console.WriteLine("IPaiを提供しています。");
Console.WriteLine("Prototype2からアクセスできます。");
Console.WriteLine("Enterキーで終了...");
Console.ReadLine();

awane.Dispose();
AwaneProcessDiscovery.StopRegistryServer();

Console.WriteLine();
Console.WriteLine("=== プログラム終了 ===");
