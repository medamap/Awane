namespace SharedInterfaces;

// Paiのインターフェース - AIエージェント実行
public interface IPai
{
    Task<PaiResult> ExecuteAIAgent(string prompt);
}

// パラメータと結果の型
public class PaiParameter
{
    public string TaskName { get; set; } = "";
    public int Priority { get; set; }
}

public class PaiResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
}