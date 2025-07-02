using Autofac;
using Prototype1;

// Autofac コンテナの構築
var builder = new ContainerBuilder();
builder.RegisterType<CalcMul>().As<ICalc>();
var container = builder.Build();

// GetComponent 風に取得
var calc = container.Resolve<ICalc>();

// 計算実行
var result = calc.Calc(3f, 7f);

// 結果表示
Console.WriteLine($"3 × 7 = {result}");
