namespace SharedInterfaces;

// あいり先生のインターフェース - 初期化・対話担当
public interface IAiri
{
    // セッション管理
    Task<string> StartInitializationSession(string sessionType);
    Task ResetSession();
    
    // 対話フロー
    Task<string> GetCurrentQuestion();
    Task<DialogResult> SendAnswer(string answer);
    Task<DialogResult> SendChoice(int choiceIndex);
    
    // 制御
    Task<bool> CanCancel();
    Task<DialogResult> CancelDialog();
    
    // 状態確認
    Task<DialogState> GetCurrentState();
}

// 対話の結果
public class DialogResult
{
    public bool IsCompleted { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = "";
    public string NextQuestion { get; set; } = "";
    public List<string> Choices { get; set; } = new();
    public QuestionType QuestionType { get; set; }
}

// 対話の状態
public class DialogState
{
    public string SessionId { get; set; } = "";
    public string CurrentStep { get; set; } = "";
    public bool IsActive { get; set; }
    public Dictionary<string, string> CollectedData { get; set; } = new();
}

// 質問の種類
public enum QuestionType
{
    FreeText,    // 自由回答
    Choice,      // 選択問題
    YesNo,       // Yes/No
    Completed    // 完了
}