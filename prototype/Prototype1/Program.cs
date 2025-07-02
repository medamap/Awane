using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Prototype1;

// プロキシジェネレーター
var proxyGenerator = new ProxyGenerator();

// Autofac コンテナの構築
var builder = new ContainerBuilder();

// インターセプターを登録
builder.RegisterType<LoggingInterceptor>().AsSelf();

// CalcMulを登録（インターセプター付き）
builder.RegisterType<CalcMul>()
    .As<ICalc>()
    .EnableInterfaceInterceptors()
    .InterceptedBy(typeof(LoggingInterceptor));

var container = builder.Build();

Console.WriteLine("=== プロキシパターンのデモ ===");
Console.WriteLine();

// GetComponent 風に取得（実際はプロキシが返される）
var calc = container.Resolve<ICalc>();
Console.WriteLine($"取得した型: {calc.GetType().Name}");
Console.WriteLine();

// 計算実行（インターセプターが介入）
var result = calc.Calc(3f, 7f);

Console.WriteLine();
Console.WriteLine($"最終結果: 3 × 7 = {result}");
