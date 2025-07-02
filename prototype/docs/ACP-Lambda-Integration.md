# ACP × AWS Lambda 統合ガイド

## 概要

ACP (Awane Communicators Protocol) をAWS Lambda上で実装することで、サーバーレスアーキテクチャでの分散AIエージェントシステムを実現できます。

## なぜLambda × ACPなのか？

### 1. **コスト効率**
- AIエージェントは断続的な呼び出しパターンが多い
- 使用した分だけの課金モデルがAIワークロードに最適
- 自動スケーリングによりトラフィックスパイクに対応

### 2. **言語の多様性**
- Lambda は多言語をサポート（C#, Node.js, Python, Go, Java, Ruby）
- ACPの言語非依存性と完璧にマッチ

### 3. **グローバル展開**
- マルチリージョンデプロイが容易
- エッジロケーションでの低レイテンシー実行

## アーキテクチャ設計

### レジストリの実装

従来のプロセスレジストリの代わりに、DynamoDBを使用：

```csharp
public class AwaneLambdaRegistry : IAwaneRegistry
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private const string TableName = "AwaneComponentRegistry";
    
    public async Task RegisterLambda(LambdaRegistration registration)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["FunctionArn"] = new AttributeValue(registration.FunctionArn),
            ["Region"] = new AttributeValue(registration.Region),
            ["Interfaces"] = new AttributeValue { SS = registration.Interfaces },
            ["LastUpdated"] = new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            ["TTL"] = new AttributeValue { N = (DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()).ToString() }
        };
        
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TableName,
            Item = item
        });
    }
    
    public async Task<List<LambdaComponent>> FindComponents(string interfaceName)
    {
        var response = await _dynamoDb.ScanAsync(new ScanRequest
        {
            TableName = TableName,
            FilterExpression = "contains(Interfaces, :interface)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":interface"] = new AttributeValue(interfaceName)
            }
        });
        
        return response.Items.Select(MapToComponent).ToList();
    }
}
```

### Lambda関数の実装

#### C# Lambda（Claude AI エージェント）

```csharp
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

public class ProtoPaiLambda
{
    private static readonly AwaneSystem _awane;
    private static readonly ClaudeCodePai _pai;
    
    static ProtoPaiLambda()
    {
        // コールドスタート時の初期化
        _awane = new AwaneSystem();
        _pai = new ClaudeCodePai();
        _awane.Register(_pai);
        
        // 自身をレジストリに登録
        var registry = new AwaneLambdaRegistry();
        registry.RegisterLambda(new LambdaRegistration
        {
            FunctionArn = Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_ARN"),
            Region = Environment.GetEnvironmentVariable("AWS_REGION"),
            Interfaces = new[] { "SharedInterfaces.IPai" }
        }).Wait();
    }
    
    public async Task<APIGatewayProxyResponse> HandleRequest(
        APIGatewayProxyRequest request, 
        ILambdaContext context)
    {
        try
        {
            var acpMessage = JsonSerializer.Deserialize<AwaneMessage>(request.Body);
            
            if (acpMessage.MessageType == "MethodCall")
            {
                var methodCall = JsonSerializer.Deserialize<RemoteMethodCall>(acpMessage.Payload);
                var result = await ProcessMethodCall(methodCall);
                
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = JsonSerializer.Serialize(new AwaneMessage
                    {
                        MessageType = "MethodResult",
                        Payload = JsonSerializer.Serialize(result)
                    })
                };
            }
            
            return new APIGatewayProxyResponse
            {
                StatusCode = 400,
                Body = "Invalid message type"
            };
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error: {ex}");
            return new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Body = JsonSerializer.Serialize(new RemoteMethodResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                })
            };
        }
    }
    
    private async Task<RemoteMethodResult> ProcessMethodCall(RemoteMethodCall call)
    {
        if (call.MethodName == "ExecuteAIAgent" && call.Arguments.Length == 1)
        {
            var prompt = JsonSerializer.Deserialize<string>(call.Arguments[0]);
            var result = await _pai.ExecuteAIAgent(prompt);
            
            return new RemoteMethodResult
            {
                Success = true,
                ReturnValue = JsonSerializer.Serialize(result)
            };
        }
        
        return new RemoteMethodResult
        {
            Success = false,
            ErrorMessage = $"Method {call.MethodName} not found"
        };
    }
}
```

#### Node.js Lambda（Gemini AI エージェント）

```javascript
const { DynamoDBClient, PutItemCommand } = require('@aws-sdk/client-dynamodb');
const { GoogleGenerativeAI } = require('@google/generative-ai');

const dynamoDb = new DynamoDBClient({});
const genAI = new GoogleGenerativeAI(process.env.GEMINI_API_KEY);

// 初期化時にレジストリに登録
const registerComponent = async () => {
  const params = {
    TableName: 'AwaneComponentRegistry',
    Item: {
      FunctionArn: { S: process.env.AWS_LAMBDA_FUNCTION_ARN },
      Region: { S: process.env.AWS_REGION },
      Interfaces: { SS: ['SharedInterfaces.IGeminiAI'] },
      LastUpdated: { N: Date.now().toString() },
      TTL: { N: (Date.now() + 86400000).toString() } // 24時間後
    }
  };
  
  await dynamoDb.send(new PutItemCommand(params));
};

// コールドスタート時に実行
registerComponent().catch(console.error);

exports.handler = async (event) => {
  try {
    const acpMessage = JSON.parse(event.body);
    
    if (acpMessage.MessageType === 'MethodCall') {
      const methodCall = JSON.parse(acpMessage.Payload);
      
      if (methodCall.MethodName === 'Generate') {
        const prompt = JSON.parse(methodCall.Arguments[0]);
        const model = genAI.getGenerativeModel({ model: 'gemini-pro' });
        const result = await model.generateContent(prompt);
        
        return {
          statusCode: 200,
          body: JSON.stringify({
            MessageType: 'MethodResult',
            Payload: JSON.stringify({
              Success: true,
              ReturnValue: JSON.stringify({
                Success: true,
                Message: result.response.text()
              })
            })
          })
        };
      }
    }
    
    return {
      statusCode: 400,
      body: 'Invalid request'
    };
  } catch (error) {
    console.error('Error:', error);
    return {
      statusCode: 500,
      body: JSON.stringify({
        MessageType: 'MethodResult',
        Payload: JSON.stringify({
          Success: false,
          ErrorMessage: error.message
        })
      })
    };
  }
};
```

### Lambda呼び出しプロキシ

```csharp
public class AwaneLambdaProxy
{
    private readonly IAmazonLambda _lambdaClient;
    private readonly AwaneLambdaRegistry _registry;
    
    public async Task<T?> GetComponentLambda<T>(string? functionArn = null) where T : class
    {
        // 関数ARNが指定されていない場合はレジストリから検索
        if (string.IsNullOrEmpty(functionArn))
        {
            var components = await _registry.FindComponents(typeof(T).FullName);
            var component = SelectOptimalComponent(components);
            functionArn = component?.FunctionArn;
        }
        
        if (string.IsNullOrEmpty(functionArn))
            return null;
        
        var interceptor = new LambdaProxyInterceptor(_lambdaClient, functionArn);
        return ProxyGenerator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
    }
    
    private LambdaComponent? SelectOptimalComponent(List<LambdaComponent> components)
    {
        // リージョンやコストを考慮して最適なコンポーネントを選択
        var currentRegion = Environment.GetEnvironmentVariable("AWS_REGION");
        
        // 同一リージョンを優先
        var sameRegion = components.FirstOrDefault(c => c.Region == currentRegion);
        if (sameRegion != null) return sameRegion;
        
        // 最も新しく更新されたものを選択
        return components.OrderByDescending(c => c.LastUpdated).FirstOrDefault();
    }
}

public class LambdaProxyInterceptor : IInterceptor
{
    private readonly IAmazonLambda _lambdaClient;
    private readonly string _functionArn;
    
    public LambdaProxyInterceptor(IAmazonLambda lambdaClient, string functionArn)
    {
        _lambdaClient = lambdaClient;
        _functionArn = functionArn;
    }
    
    public void Intercept(IInvocation invocation)
    {
        var task = InterceptAsync(invocation);
        
        if (invocation.Method.ReturnType.IsGenericType &&
            invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            invocation.ReturnValue = task;
        }
        else
        {
            task.Wait();
        }
    }
    
    private async Task<object?> InterceptAsync(IInvocation invocation)
    {
        var methodCall = new RemoteMethodCall
        {
            TypeName = invocation.Method.DeclaringType.FullName,
            MethodName = invocation.Method.Name,
            Arguments = invocation.Arguments.Select(JsonSerializer.Serialize).ToArray(),
            ArgumentTypes = invocation.Method.GetParameters()
                .Select(p => p.ParameterType.FullName).ToArray()
        };
        
        var acpMessage = new AwaneMessage
        {
            MessageType = "MethodCall",
            Payload = JsonSerializer.Serialize(methodCall)
        };
        
        var invokeRequest = new InvokeRequest
        {
            FunctionName = _functionArn,
            InvocationType = InvocationType.RequestResponse,
            Payload = JsonSerializer.Serialize(new
            {
                body = JsonSerializer.Serialize(acpMessage)
            })
        };
        
        var response = await _lambdaClient.InvokeAsync(invokeRequest);
        
        // レスポンス処理...
    }
}
```

## 使用例

### 基本的な使い方

```csharp
// Lambda上のAIエージェントを呼び出し
var pai = await AwaneLambdaProxy.GetComponentLambda<IPai>();
var result = await pai.ExecuteAIAgent("こんにちは");

// 特定のLambda関数を指定
var gemini = await AwaneLambdaProxy.GetComponentLambda<IGeminiAI>(
    "arn:aws:lambda:us-east-1:123456:function:ProtoGemini"
);
```

### 複数AIの協調

```csharp
// 異なるリージョンのAIエージェントを組み合わせ
var translator = await AwaneLambdaProxy.GetComponentLambda<ITranslator>(); // us-east-1
var summarizer = await AwaneLambdaProxy.GetComponentLambda<ISummarizer>(); // ap-northeast-1

var japanese = "長い日本語のテキスト...";
var english = await translator.Translate(japanese, "en");
var summary = await summarizer.Summarize(english);
```

### Step Functionsとの統合

```json
{
  "Comment": "AI処理パイプライン",
  "StartAt": "GetUserInput",
  "States": {
    "GetUserInput": {
      "Type": "Task",
      "Resource": "arn:aws:lambda:us-east-1:123456:function:GetInput",
      "Next": "CallClaudeAI"
    },
    "CallClaudeAI": {
      "Type": "Task",
      "Resource": "arn:aws:lambda:us-east-1:123456:function:ProtoPai",
      "Parameters": {
        "body": {
          "MessageType": "MethodCall",
          "Payload.$": "$.claudePayload"
        }
      },
      "Next": "CallGeminiAI"
    },
    "CallGeminiAI": {
      "Type": "Task",
      "Resource": "arn:aws:lambda:us-east-1:123456:function:ProtoGemini",
      "End": true
    }
  }
}
```

## コスト最適化戦略

### 1. **プロビジョンドコンカレンシー**
```yaml
# SAM template
ProtoPaiFunction:
  Type: AWS::Serverless::Function
  Properties:
    ProvisionedConcurrencyConfig:
      ProvisionedConcurrentExecutions: 1  # 常に1つはウォーム状態
```

### 2. **Lambda SnapStart (Java)**
```java
@SnapStart
public class ProtoGPTLambda implements RequestHandler<APIGatewayProxyRequest, APIGatewayProxyResponse> {
    // スナップショットから高速起動
}
```

### 3. **EventBridge による非同期処理**
```csharp
// 非同期でAI処理をトリガー
await _eventBridge.PutEventsAsync(new PutEventsRequest
{
    Entries = new List<PutEventsRequestEntry>
    {
        new PutEventsRequestEntry
        {
            Source = "awane.acp",
            DetailType = "AIProcessingRequest",
            Detail = JsonSerializer.Serialize(new { Prompt = prompt })
        }
    }
});
```

## セキュリティ考慮事項

### 1. **IAMロールベースアクセス**
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": "lambda:InvokeFunction",
      "Resource": "arn:aws:lambda:*:*:function:Proto*",
      "Condition": {
        "StringEquals": {
          "aws:RequestTag/ACPComponent": "true"
        }
      }
    }
  ]
}
```

### 2. **VPCエンドポイント**
- Lambda間通信をVPC内に限定
- インターネット経由の通信を回避

### 3. **Secrets Manager統合**
```csharp
var secretsManager = new AmazonSecretsManagerClient();
var apiKey = await secretsManager.GetSecretValueAsync(new GetSecretValueRequest
{
    SecretId = "acp/claude-api-key"
});
```

## パフォーマンス考慮事項

### 1. **接続プーリング**
```csharp
// Lambda環境では静的フィールドで接続を保持
private static readonly HttpClient _httpClient = new HttpClient();
private static readonly AmazonDynamoDBClient _dynamoDb = new AmazonDynamoDBClient();
```

### 2. **バッチ処理**
```csharp
// 複数のリクエストをバッチ化
public async Task<List<PaiResult>> BatchExecuteAIAgent(List<string> prompts)
{
    var tasks = prompts.Select(p => ExecuteAIAgent(p));
    return await Task.WhenAll(tasks);
}
```

## まとめ

AWS LambdaでのACP実装により、以下が実現できます：

1. **スケーラブルなAIエージェントシステム**
   - 自動スケーリング
   - グローバル展開
   - コスト効率的な運用

2. **言語の壁を越えた統合**
   - 各言語の強みを活かしたLambda関数
   - 統一されたACPプロトコルでの通信

3. **エンタープライズ対応**
   - IAMによるアクセス制御
   - VPCによるネットワーク分離
   - CloudWatchによる監視

ACPとLambdaの組み合わせは、次世代の分散AIシステムの基盤となる可能性を秘めています。