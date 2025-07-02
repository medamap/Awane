namespace SharedInterfaces;

// Paiのインターフェース
public interface IPai
{
    Task<PaiResult> PaiMethodAsync(PaiParameter parameter);
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