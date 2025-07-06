using AwaneCore;
using SharedInterfaces;
using ProtoAiri;

namespace ProtoAiri;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== あいり先生 GitHub CLI初期化フロー テスト ===");
        
        // Awaneシステム初期化
        using var awane = new AwaneSystem();
        
        // あいり先生を登録
        awane.Register<IAiri>(new AiriSensei());
        
        // 初期化
        await awane.InitializeAsync();
        
        // テストシナリオ実行
        await RunTestScenarios(awane);
        
        Console.WriteLine("\n=== テスト完了 ===");
    }
    
    static async Task RunTestScenarios(AwaneSystem awane)
    {
        var airi = awane.GetComponent<IAiri>();
        
        // シナリオ1: 正常フロー
        Console.WriteLine("\n--- シナリオ1: 正常フロー ---");
        await TestNormalFlow(airi);
        
        // シナリオ2: キャンセル
        Console.WriteLine("\n--- シナリオ2: キャンセル ---");
        await TestCancelFlow(airi);
        
        // シナリオ3: 既存認証確認
        Console.WriteLine("\n--- シナリオ3: 既存認証確認 ---");
        await TestExistingAuthFlow(airi);
    }
    
    static async Task TestNormalFlow(IAiri airi)
    {
        try
        {
            // セッション開始
            var question = await airi.StartInitializationSession("github-cli");
            Console.WriteLine($"Q: {question}");
            
            // Yes/No質問に回答
            var result = await airi.SendAnswer("yes");
            Console.WriteLine($"A: yes -> {result.Message}");
            
            if (!result.IsCompleted)
            {
                Console.WriteLine($"Q: {result.NextQuestion}");
                foreach (var (choice, index) in result.Choices.Select((c, i) => (c, i)))
                {
                    Console.WriteLine($"  {index}: {choice}");
                }
                
                // 選択肢0を選択
                result = await airi.SendChoice(0);
                Console.WriteLine($"A: 0 -> {result.Message}");
                
                if (!result.IsCompleted)
                {
                    Console.WriteLine($"Q: {result.NextQuestion}");
                    
                    // トークン入力
                    result = await airi.SendAnswer("ghp_mocktoken123456789");
                    Console.WriteLine($"A: ghp_mocktoken123456789 -> {result.Message}");
                    
                    if (!result.IsCompleted)
                    {
                        Console.WriteLine($"Q: {result.NextQuestion}");
                        
                        // 最終確認
                        result = await airi.SendAnswer("yes");
                        Console.WriteLine($"A: yes -> {result.Message}");
                    }
                }
            }
            
            // 最終結果
            if (result.IsCompleted)
            {
                Console.WriteLine($"✅ 完了: {result.Message}");
            }
            
            // セッション状態確認
            var state = await airi.GetCurrentState();
            Console.WriteLine($"セッション状態: {state.CurrentStep}, アクティブ: {state.IsActive}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ エラー: {ex.Message}");
        }
    }
    
    static async Task TestCancelFlow(IAiri airi)
    {
        try
        {
            // セッション開始
            var question = await airi.StartInitializationSession("github-cli");
            Console.WriteLine($"Q: {question}");
            
            // キャンセル
            var result = await airi.CancelDialog();
            Console.WriteLine($"キャンセル結果: {result.Message}");
            
            // セッション状態確認
            var state = await airi.GetCurrentState();
            Console.WriteLine($"セッション状態: {state.CurrentStep}, アクティブ: {state.IsActive}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ エラー: {ex.Message}");
        }
    }
    
    static async Task TestExistingAuthFlow(IAiri airi)
    {
        try
        {
            // セッション開始
            var question = await airi.StartInitializationSession("github-cli");
            Console.WriteLine($"Q: {question}");
            
            // Yes/No質問に回答
            var result = await airi.SendAnswer("yes");
            Console.WriteLine($"A: yes -> {result.Message}");
            
            if (!result.IsCompleted)
            {
                Console.WriteLine($"Q: {result.NextQuestion}");
                foreach (var (choice, index) in result.Choices.Select((c, i) => (c, i)))
                {
                    Console.WriteLine($"  {index}: {choice}");
                }
                
                // 選択肢2（既存認証確認）を選択
                result = await airi.SendChoice(2);
                Console.WriteLine($"A: 2 -> {result.Message}");
                
                if (!result.IsCompleted)
                {
                    Console.WriteLine($"Q: {result.NextQuestion}");
                    
                    // 最終確認
                    result = await airi.SendAnswer("no");
                    Console.WriteLine($"A: no -> {result.Message}");
                }
            }
            
            // 最終結果
            if (result.IsCompleted)
            {
                Console.WriteLine($"✅ 完了: {result.Message}");
            }
            
            // セッション状態確認
            var state = await airi.GetCurrentState();
            Console.WriteLine($"セッション状態: {state.CurrentStep}, アクティブ: {state.IsActive}");
            Console.WriteLine("収集データ:");
            foreach (var (key, value) in state.CollectedData)
            {
                Console.WriteLine($"  {key}: {value}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ エラー: {ex.Message}");
        }
    }
}