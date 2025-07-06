# Awane レジストリアーキテクチャ設計

## 概要
Awaneのコンポーネント登録・発見システムのアーキテクチャ設計について記述します。

## レジストリの階層構造

### 1. ComponentRegistry（プロセスレベル）
各プロセスが持つローカルレジストリ。そのプロセス内のコンポーネントを管理します。

### 2. NodeRegistry（PCレベル）
PC内の全プロセスを統括する中央レジストリ。独立したプロセスとして動作します。

### 3. ネットワークレジストリ（LAN/WANレベル）
設定ベースで他のNodeRegistryと連携します。

## アーキテクチャ設計

### PC内アーキテクチャ
```
[PC]
├─ NodeRegistry（独立プロセス）
│   └─ 固定ポート/UDSで待機（例: /tmp/awane-node.sock）
│
├─ Process1
│   └─ ComponentRegistry
│       └─ Airi, Pai等のコンポーネント
│
└─ Process2
    └─ ComponentRegistry
        └─ Poppo等のコンポーネント
```

### NodeRegistry起動メカニズム
1. 各プロセス起動時に固定アドレスのNodeRegistryを探す
2. 見つからない場合は、NodeRegistryプロセスを起動
3. 自身のComponentRegistryをNodeRegistryに登録

### スコープ別発見戦略
- **同一プロセス**: 直接参照（ComponentRegistry）
- **同一PC**: NodeRegistry経由（UDS通信）
- **LAN**: 設定ファイルで指定されたホスト
- **WAN**: 設定ファイルで指定されたホスト（NAT考慮）

## 名前空間解決

### 完全修飾名（FQN）での登録
```
PoppoAcademy.Yamada.IAiri
PoppoAcademy.Tanaka.IAiri
PoppoAcademy.Core.IPoppoAcademy
```

### 段階的名前解決
右端から順に一意性を確認し、最小限の指定で解決：

```
登録済み: Tanaka.Cat.IAiri, Tanaka.Dog.IAiri, Hirano.Cat.IAiri

GetComponent("IAiri")        → エラー（3つ該当）
GetComponent("Cat.IAiri")    → エラー（2つ該当）
GetComponent("Dog.IAiri")    → OK（一意）
GetComponent("Hirano.Cat")   → OK（一意）
```

## Protocol Buffers定義

### レジストリサービス
```proto
syntax = "proto3";
package awane.core.v1;

// コンポーネント情報
message ComponentInfo {
    string awane_id = 1;
    string awane_location = 2;
    repeated string awane_interfaces = 3;
    string awane_name = 4;
    string awane_version = 5;
    map<string, string> awane_tags = 6;
}

// ComponentRegistry（プロセス内）
service ComponentRegistry {
    rpc RegisterComponent(RegisterRequest) returns (RegisterResponse);
    rpc GetComponent(GetComponentRequest) returns (GetComponentResponse);
    rpc GetComponents(GetComponentRequest) returns (stream ComponentInfo);
}

// NodeRegistry（PC内統括）
service NodeRegistry {
    rpc RegisterProcess(RegisterProcessRequest) returns (RegisterProcessResponse);
    rpc GetComponentInPC(GetComponentRequest) returns (GetComponentResponse);
    rpc GetComponentsInPC(GetComponentRequest) returns (stream ComponentInfo);
}
```

## 設定ファイル例

```json
{
  "awane": {
    "node_registry": {
      "address": "unix:///tmp/awane-node.sock",
      "fallback_port": 5000
    },
    "lan_nodes": [
      "192.168.1.10:5000",
      "192.168.1.11:5000"
    ],
    "wan_nodes": [
      {
        "host": "awane.example.com",
        "port": 5000,
        "auth": "token123"
      }
    ]
  }
}
```

## 実装優先順位

### Phase 1: プロセス内実装
1. IAwaneComponentインターフェース
2. AwaneComponent基底クラス
3. ComponentRegistry（プロセス内）
4. Register/GetComponent API

### Phase 2: PC内実装
1. NodeRegistry独立プロセス
2. プロセス間通信（UDS）
3. GetComponentInPC API

### Phase 3: ネットワーク実装
1. 設定ベースのノード管理
2. LAN/WAN対応
3. 認証・セキュリティ

## セキュリティ考慮事項

### PC内
- Unix Domain Socketの権限管理
- プロセス間の信頼関係

### ネットワーク
- TLS通信
- 認証トークン
- 設定ベースによる明示的な信頼関係

## 今後の拡張可能性

1. **自動発見機能**
   - mDNS/Bonjourによる LAN内自動発見
   - ただし設定で無効化可能に

2. **高度な検索**
   - タグベース検索
   - バージョン指定
   - 負荷分散

3. **監視・管理**
   - コンポーネントのヘルスチェック
   - メトリクス収集
   - 分散トレーシング