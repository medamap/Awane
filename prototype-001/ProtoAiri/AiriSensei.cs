using System.Diagnostics;
using AwaneCore;
using SharedInterfaces;

namespace ProtoAiri;

// あいり先生のモック実装 - GitHub CLI初期化フロー
public class AiriSensei : IAiri, IAsyncStartable
{
    private readonly string _agentName = "あいり先生";
    private string _sessionId = "";
    private string _currentStep = "";
    private bool _isActive = false;
    private Dictionary<string, string> _collectedData = new();
    
    // 初期化フロー定義
    private readonly Dictionary<string, InitStep> _initSteps = new()
    {
        ["start"] = new InitStep
        {
            Question = "こんにちは！あいり先生です。GitHub CLIの初期化を始めましょう。\n現在のGitHub認証状態を確認しますか？",
            Type = QuestionType.YesNo,
            NextStep = "check_auth"
        },
        ["check_auth"] = new InitStep
        {
            Question = "GitHub CLIの認証方法を選択してください：",
            Type = QuestionType.Choice,
            Choices = new List<string> { "Personal Access Token", "GitHub App", "既存の認証を確認" },
            NextStep = "auth_method"
        },
        ["auth_method"] = new InitStep
        {
            Question = "Personal Access Tokenを入力してください：",
            Type = QuestionType.FreeText,
            NextStep = "token_input"
        },
        ["token_input"] = new InitStep
        {
            Question = "リポジトリのデフォルト設定を行いますか？",
            Type = QuestionType.YesNo,
            NextStep = "repo_config"
        },
        ["repo_config"] = new InitStep
        {
            Question = "設定が完了しました！無理しないでくださいね。",
            Type = QuestionType.Completed,
            NextStep = "completed"
        }
    };
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[{_agentName}] 初期化開始...");
        await Task.Delay(100, cancellationToken);
        Console.WriteLine($"[{_agentName}] 初期化完了！");
    }
    
    public async Task<string> StartInitializationSession(string sessionType)
    {
        _sessionId = Guid.NewGuid().ToString("N")[..8];
        _currentStep = "start";
        _isActive = true;
        _collectedData.Clear();
        
        Console.WriteLine($"[{_agentName}] セッション開始: {_sessionId} ({sessionType})");
        
        var step = _initSteps[_currentStep];
        return step.Question;
    }
    
    public async Task ResetSession()
    {
        _sessionId = "";
        _currentStep = "";
        _isActive = false;
        _collectedData.Clear();
        
        Console.WriteLine($"[{_agentName}] セッションリセット");
        await Task.CompletedTask;
    }
    
    public async Task<string> GetCurrentQuestion()
    {
        if (!_isActive || !_initSteps.ContainsKey(_currentStep))
        {
            return "セッションが開始されていません。";
        }
        
        var step = _initSteps[_currentStep];
        return step.Question;
    }
    
    public async Task<DialogResult> SendAnswer(string answer)
    {
        if (!_isActive)
        {
            return new DialogResult { IsSuccess = false, Message = "セッションが開始されていません。" };
        }
        
        Console.WriteLine($"[{_agentName}] 回答受信: {answer}");
        
        // 回答を保存
        _collectedData[_currentStep] = answer;
        
        // 次のステップへ
        return await MoveToNextStep();
    }
    
    public async Task<DialogResult> SendChoice(int choiceIndex)
    {
        if (!_isActive)
        {
            return new DialogResult { IsSuccess = false, Message = "セッションが開始されていません。" };
        }
        
        var step = _initSteps[_currentStep];
        if (choiceIndex < 0 || choiceIndex >= step.Choices.Count)
        {
            return new DialogResult { IsSuccess = false, Message = "無効な選択です。" };
        }
        
        var choice = step.Choices[choiceIndex];
        Console.WriteLine($"[{_agentName}] 選択受信: {choice}");
        
        // 選択を保存
        _collectedData[_currentStep] = choice;
        
        // 特別な処理（モック）
        if (_currentStep == "auth_method")
        {
            switch (choiceIndex)
            {
                case 0: // Personal Access Token
                    _currentStep = "token_input";
                    break;
                case 1: // GitHub App
                    _currentStep = "repo_config"; // スキップ
                    break;
                case 2: // 既存の認証を確認
                    await CheckExistingAuth();
                    _currentStep = "repo_config";
                    break;
            }
        }
        
        return await MoveToNextStep();
    }
    
    private async Task<DialogResult> MoveToNextStep()
    {
        if (!_initSteps.ContainsKey(_currentStep))
        {
            return new DialogResult { IsSuccess = false, Message = "無効なステップです。" };
        }
        
        var currentStep = _initSteps[_currentStep];
        
        if (currentStep.Type == QuestionType.Completed)
        {
            _isActive = false;
            return new DialogResult
            {
                IsCompleted = true,
                IsSuccess = true,
                Message = "初期化が完了しました！",
                QuestionType = QuestionType.Completed
            };
        }
        
        // 次のステップへ移動
        _currentStep = currentStep.NextStep;
        
        if (!_initSteps.ContainsKey(_currentStep))
        {
            return new DialogResult { IsSuccess = false, Message = "次のステップが見つかりません。" };
        }
        
        var nextStep = _initSteps[_currentStep];
        return new DialogResult
        {
            IsCompleted = false,
            IsSuccess = true,
            Message = "回答を受け付けました。",
            NextQuestion = nextStep.Question,
            Choices = nextStep.Choices,
            QuestionType = nextStep.Type
        };
    }
    
    private async Task CheckExistingAuth()
    {
        try
        {
            // gh auth status をモックで実行
            Console.WriteLine($"[{_agentName}] GitHub認証状態をチェック中...");
            await Task.Delay(500); // モック遅延
            
            // モック結果
            _collectedData["auth_status"] = "認証済み";
            Console.WriteLine($"[{_agentName}] 認証状態: 認証済み");
        }
        catch (Exception ex)
        {
            _collectedData["auth_status"] = $"エラー: {ex.Message}";
            Console.WriteLine($"[{_agentName}] 認証チェックエラー: {ex.Message}");
        }
    }
    
    public async Task<bool> CanCancel()
    {
        return _isActive;
    }
    
    public async Task<DialogResult> CancelDialog()
    {
        if (!_isActive)
        {
            return new DialogResult { IsSuccess = false, Message = "アクティブなセッションがありません。" };
        }
        
        await ResetSession();
        return new DialogResult
        {
            IsSuccess = true,
            Message = "対話をキャンセルしました。また何かあったら声をかけてくださいね。"
        };
    }
    
    public async Task<DialogState> GetCurrentState()
    {
        return new DialogState
        {
            SessionId = _sessionId,
            CurrentStep = _currentStep,
            IsActive = _isActive,
            CollectedData = new Dictionary<string, string>(_collectedData)
        };
    }
}

// 初期化ステップの定義
public class InitStep
{
    public string Question { get; set; } = "";
    public QuestionType Type { get; set; }
    public List<string> Choices { get; set; } = new();
    public string NextStep { get; set; } = "";
}