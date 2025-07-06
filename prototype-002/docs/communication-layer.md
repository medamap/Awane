# 通信レイヤー設計

## 概要
Awaneの通信レイヤーは、異なる通信方式を統一的に扱うための抽象化層を提供します。

## 通信方式の詳細

### 1. 直接参照 (Direct Reference)
- **対象**: 同一プロセス内のコンポーネント
- **言語**: C#のみ
- **特徴**: 
  - オーバーヘッドなし
  - 型安全性が完全に保たれる
  - シリアライズ不要

### 2. Unix Domain Socket (UDS)
- **対象**: 同一マシン内の異なるプロセス
- **言語**: C#, Node.js
- **特徴**:
  - 高速（TCPより約30%高速）
  - ファイルシステムベースのアドレッシング
  - OSのセキュリティモデルを利用可能

```csharp
// C# 実装例
var channel = new Channel("unix:///tmp/awane/component.sock");
```

```javascript
// Node.js 実装例
const client = new grpc.Client(
  'unix:///tmp/awane/component.sock',
  grpc.credentials.createInsecure()
);
```

### 3. gRPC over TCP/HTTP2
- **対象**: ネットワーク越しの通信
- **言語**: C#, Node.js
- **特徴**:
  - 標準的なネットワークプロトコル
  - TLS暗号化サポート
  - ストリーミング対応

## Adapter Pattern実装

```csharp
public interface IComponentAdapter<T>
{
    Task<T> GetComponent(ComponentLocation location);
}

public class AdapterFactory
{
    public IComponentAdapter<T> CreateAdapter<T>(ComponentLocation location)
    {
        switch (location.Type)
        {
            case LocationType.SameProcess:
                return new DirectAdapter<T>();
            case LocationType.LocalMachine:
                return new UdsAdapter<T>();
            case LocationType.Network:
                return new GrpcAdapter<T>();
        }
    }
}
```

## メッセージフォーマット

### Protocol Buffers定義
```protobuf
syntax = "proto3";

package awane;

// 基本的なRPCメッセージ
message RpcRequest {
    string component_name = 1;
    string method_name = 2;
    bytes payload = 3;
    map<string, string> metadata = 4;
}

message RpcResponse {
    bytes payload = 1;
    bool success = 2;
    string error_message = 3;
}

// コンポーネント発見
message ComponentQuery {
    string interface_name = 1;
    string version = 2;
}

message ComponentInfo {
    string component_id = 1;
    string location = 2;
    repeated string interfaces = 3;
}
```

## エラーハンドリング

### 通信エラーの分類
1. **一時的エラー**: リトライ可能
   - ネットワーク一時障害
   - タイムアウト
   
2. **永続的エラー**: リトライ不可
   - コンポーネント未登録
   - バージョン不一致
   - 認証エラー

### リトライ戦略
```csharp
public class RetryPolicy
{
    public int MaxAttempts { get; set; } = 3;
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    public double BackoffMultiplier { get; set; } = 2.0;
}
```

## パフォーマンス最適化

### 接続プーリング
- gRPCチャンネルの再利用
- Unix Domain Socketの接続キャッシュ

### メッセージ圧縮
- gRPC: 組み込みの圧縮機能を利用
- UDS: MessagePackによる高速シリアライズ

### バッチング
- 複数の小さなリクエストをまとめて送信
- ストリーミングAPIの活用