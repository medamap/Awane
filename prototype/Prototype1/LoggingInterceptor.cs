using Castle.DynamicProxy;

namespace Prototype1;

public class LoggingInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        Console.WriteLine($"[Interceptor] メソッド呼び出し: {invocation.Method.Name}");
        Console.WriteLine($"[Interceptor] 引数: {string.Join(", ", invocation.Arguments)}");
        
        // 実際のメソッドを呼び出す
        invocation.Proceed();
        
        Console.WriteLine($"[Interceptor] 戻り値: {invocation.ReturnValue}");
    }
}