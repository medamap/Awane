# Awane API設計ドキュメント

## 概要
AwaneはUnityのGetComponent<T>()パターンを分散環境に拡張した、透過的なプロセス間通信ライブラリです。

## 基本コンセプト

### 通信階層
1. **同一プロセス内** - 直接参照
2. **同一PC内** - Unix Domain Socket
3. **同一LAN内** - TCP/gRPC
4. **NAT超え（WAN）** - 事前登録されたホスト経由

### コンポーネント発見API

#### 単数形（最初に見つかったものを返す）
```csharp
IAwaneComponent GetComponent<T>();        // 同一プロセス
IAwaneComponent GetComponentInPC<T>();     // 同一PC
IAwaneComponent GetComponentInLAN<T>();    // 同一LAN
IAwaneComponent GetComponentInWAN<T>();    // NAT超え
```

#### 複数形（条件に合うすべてを返す）
```csharp
IAwaneComponent[] GetComponents<T>();      // 同一プロセス
IAwaneComponent[] GetComponentsInPC<T>();  // 同一PC
IAwaneComponent[] GetComponentsInLAN<T>(); // 同一LAN
IAwaneComponent[] GetComponentsInWAN<T>(); // NAT超え
```

## IAwaneComponentインターフェース

すべてのAwaneで管理されるコンポーネントは、`IAwaneComponent`インターフェースを実装する必要があります。

```csharp
public interface IAwaneComponent
{
    // 識別・検索用
    string AwaneId { get; }           // ユニークID
    string AwaneLocation { get; }     // 場所情報 (例: "process", "local:pid", "lan:192.168.1.10")
    
    // 型情報（GetComponent<T>で使用）
    Type[] AwaneInterfaces { get; }   // 実装しているインターフェース一覧
    
    // キャスト用
    T AwaneAs<T>() where T : class;   // 実際のインターフェースとして取得
    
    // 追加のメタ情報
    string AwaneName { get; }         // 人間が読める名前
    string AwaneVersion { get; }      // バージョン情報
    Dictionary<string, string> AwaneTags { get; }  // 検索用タグ
}
```

### プレフィクスの採用理由
- 名前衝突を避けるため、すべてのAwane関連プロパティに`Awane`プレフィクスを付与
- 利用者のコンポーネントが独自の`Id`や`Location`プロパティを持っても衝突しない

## 言語間の差異と統一API

### C#での実装
```csharp
var components = awane.GetComponentsInPC<IPoppoAcademy>();
foreach (var component in components)
{
    // 型安全なキャスト
    if (component.AwaneHasInterface("IAiri"))
    {
        var airi = Awane.GetAs<IAiri>(component, "IAiri");
        airi.Teach();
    }
}
```

### TypeScript/Node.jsでの実装
```typescript
const components = awane.getComponentsInPC("IPoppoAcademy");
for (const component of components) {
    // 文字列ベースの型チェック
    if (component.awaneHasInterface("IAiri")) {
        const airi = Awane.getAs<IAiri>(component, "IAiri");
        airi.teach();
    }
}
```

### 型情報の扱い
- **C#**: ジェネリクスの型情報を実行時に取得可能
- **TypeScript**: 型情報は実行時に消えるため、文字列で指定
- **通信層**: 両言語とも最終的に文字列として型名を送信

### TypeScriptの冗長性について
TypeScriptでは型情報が実行時に消えるため、以下のような冗長な記述が必要です：

```typescript
// 型と文字列の二重指定が必要
const airi = Awane.getAs<IAiri>(component, "IAiri");
//                      ^^^^^^              ^^^^^^
//                      コンパイル時用      実行時用
```

この冗長性は避けられませんが、両言語で統一的なAPIを提供するために受け入れています。

## Protocol Buffersでの定義

```proto
syntax = "proto3";
package awane.core.v1;

message ComponentInfo {
    string awane_id = 1;
    string awane_location = 2;
    repeated string awane_interfaces = 3;
    string awane_name = 4;
    string awane_version = 5;
    map<string, string> awane_tags = 6;
}

message GetComponentRequest {
    string interface_name = 1;
    string location_filter = 2;  // "pc", "lan", "wan"
}

message GetComponentResponse {
    ComponentInfo component = 1;
}

service ComponentService {
    rpc GetComponent(GetComponentRequest) returns (GetComponentResponse);
    rpc GetComponents(GetComponentRequest) returns (stream GetComponentResponse);
}
```

## アーキテクチャ

### レジストリ構造
```
[インターネット]
    ├─ ホストA (事前登録)
    └─ ホストB (事前登録)

[LAN]
    ├─ PC1
    │   ├─ Awane Central (PC内レジストリ)
    │   ├─ プロセスA (Airi)
    │   └─ プロセスB (Pai)
    └─ PC2
        ├─ Awane Central
        └─ プロセスC

[ローカルPC]
    ├─ Awane Central (固定ポートで待機)
    ├─ 現在のプロセス (直接参照)
    └─ 他プロセス (gRPC/UDS経由)
```

## 実装フェーズ

1. **Phase 1**: 同一プロセス内の実装（直接参照）
2. **Phase 2**: 同一PC内の実装（UDS + レジストリ）
3. **Phase 3**: 同一LAN内の実装（TCP + 自動発見）
4. **Phase 4**: NAT超えの実装（事前登録 + 認証）

## コンポーネント登録

### 登録API
```csharp
// C#
Awane.Register(component);  // component.AwaneInterfacesから自動で全インターフェース登録

// TypeScript
awane.register(component);  // component.awaneInterfacesから自動で全インターフェース登録
```

### 継承階層による実装

#### C#実装
```csharp
// 基底クラス - Awaneメタ情報の実装
public abstract class AwaneComponent : IAwaneComponent
{
    public string AwaneId { get; protected set; }
    public string AwaneLocation { get; protected set; }
    public Type[] AwaneInterfaces { get; protected set; }
    
    protected AwaneComponent()
    {
        AwaneId = Guid.NewGuid().ToString();
        AwaneLocation = "process";
        // リフレクションで実装インターフェースを自動取得
        AwaneInterfaces = GetType().GetInterfaces()
            .Where(i => !IsSystemInterface(i))  // System名前空間を除外
            .ToArray();
    }
    
    private bool IsSystemInterface(Type type)
    {
        return type.Namespace?.StartsWith("System") ?? false;
    }
    
    public T AwaneAs<T>() where T : class => this as T;
}

// 中間クラス - PoppoAcademy共通機能
public abstract class PoppoAcademy : AwaneComponent, IPoppoAcademy
{
    // PoppoAcademy共通実装
}

// 具象クラス - あいり先生
public class Airi : PoppoAcademy, IAiri
{
    // あいり先生固有の実装
}
```

#### TypeScript実装
```typescript
// 基底クラス - Awaneメタ情報の実装
abstract class AwaneComponent implements IAwaneComponent {
    awaneId: string;
    awaneLocation: string;
    awaneInterfaces: string[] = [];
    
    constructor() {
        this.awaneId = crypto.randomUUID();
        this.awaneLocation = "process";
    }
    
    protected addInterface(name: string) {
        this.awaneInterfaces.push(name);
    }
    
    awaneAs<T>(): T {
        return this as unknown as T;
    }
}

// 中間クラス - PoppoAcademy共通機能
class PoppoAcademy extends AwaneComponent implements IPoppoAcademy {
    constructor() {
        super();
        this.addInterface("IPoppoAcademy");
    }
}

// 具象クラス - あいり先生
class Airi extends PoppoAcademy implements IAiri {
    constructor() {
        super();
        this.addInterface("IAiri");
    }
    // あいり先生固有の実装
}
```

## 使用例

```csharp
// コンポーネントの作成と登録
var airi = new Airi();
Awane.Register(airi);  // IAiri, IPoppoAcademyが自動登録される

// 使用側
public class Main
{
    public void Example()
    {
        // ローカルPC内のすべてのPoppoAcademyメンバーを取得
        var members = Awane.GetComponentsInPC<IPoppoAcademy>();
        
        foreach (var member in members)
        {
            Console.WriteLine($"Found: {member.AwaneName} at {member.AwaneLocation}");
            
            // 特定のインターフェースにキャスト
            if (member.AwaneHasInterface("IAiri"))
            {
                var airi = Awane.GetAs<IAiri>(member, "IAiri");
                airi.Teach();
            }
        }
    }
}
```

## 実装済み機能（MVP）

### 1. IAwaneComponentインターフェース
- コンポーネントメタ情報の定義
- AwaneComponent基底クラス（C#/TypeScript）

### 2. コンポーネント登録
- `Awane.Register(component)` - 全インターフェース自動登録
- 完全修飾名（FQN）での内部管理

### 3. コンポーネント発見
- `GetComponent[s]<T>()` - プロセス内
- `GetComponent[s]InPC<T>()` - PC内
- `GetComponent[s]InLAN<T>()` - LAN内
- `GetComponent[s]InWAN<T>()` - WAN

### 4. 名前解決
- 段階的名前解決（最小限の指定で一意に特定）
- 曖昧な場合のエラー処理

## 今後の検討事項

1. **メソッド呼び出し**: リモートコンポーネントのメソッド実行
2. **プロキシ生成**: 透過的なリモート呼び出し
3. **エラーハンドリング**: ネットワークエラー時の振る舞い
4. **セキュリティ**: 認証・認可の仕組み
5. **パフォーマンス**: キャッシング、接続プーリング
6. **モニタリング**: メトリクス、トレーシング