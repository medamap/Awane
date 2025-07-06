# Awane開発ガイドライン

## 開発手法

### テスト駆動開発（TDD）の採用
本プロジェクトでは、品質保証とインクリメンタルな開発を実現するため、テスト駆動開発を採用します。

#### TDDサイクル
1. **Red**: 失敗するテストを書く
2. **Green**: テストを通す最小限の実装
3. **Refactor**: コードを改善（テストは通ったまま）

#### テスト作成の原則
- 1つのテストは1つの振る舞いを検証
- テスト名は検証内容を明確に表現
- Arrange-Act-Assert パターンを使用

## プロジェクト構造

```
Awane/prototype-002/
├── src/                        # ソースコード
│   ├── csharp/                # C#実装
│   │   ├── Awane.Core/        # コアライブラリ
│   │   │   ├── Components/    # コンポーネント関連
│   │   │   ├── Registry/      # レジストリ関連
│   │   │   └── Awane.Core.csproj
│   │   ├── Awane.Core.Tests/  # ユニットテスト
│   │   │   ├── Components/
│   │   │   ├── Registry/
│   │   │   └── Awane.Core.Tests.csproj
│   │   └── Awane.sln          # ソリューションファイル
│   │
│   └── nodejs/                # Node.js実装
│       ├── src/               # ソースコード
│       │   ├── components/
│       │   ├── registry/
│       │   └── index.ts
│       ├── test/              # テストコード
│       │   ├── components/
│       │   └── registry/
│       ├── package.json
│       └── tsconfig.json
│
├── proto/                     # Protocol Buffers定義
│   ├── awane/
│   │   └── core/
│   │       └── v1/
│   │           ├── component.proto
│   │           └── registry.proto
│   └── build.sh              # コード生成スクリプト
│
├── docs/                     # ドキュメント
│   ├── api-design.md
│   ├── architecture-overview.md
│   └── development-guidelines.md
│
├── scripts/                  # ビルド・テストスクリプト
│   ├── build.sh
│   └── test.sh
│
├── .gitignore
├── README.md
└── CLAUDE.md
```

### ディレクトリ構成の原則
- **src/とtest/の近接性**: 関連するコードとテストは近くに配置
- **言語別の独立性**: C#とNode.jsは独立したプロジェクト構造
- **proto/の中立性**: 言語に依存しない共通定義

## ファイル管理ルール

### コミット対象
以下のファイルは必ずコミットします：
- ソースコード（src/）
- テストコード（test/）
- Protocol Buffers定義（proto/）
- ドキュメント（docs/）
- 設定ファイル（*.json, *.csproj等）

### コミット対象外（.gitignore）
```gitignore
# ビルド成果物
**/bin/
**/obj/
**/dist/
**/build/
*.dll
*.exe
*.pdb

# 依存関係
**/node_modules/
**/packages/

# IDE設定
.vs/
.vscode/
.idea/
*.user
*.suo

# テスト成果物
**/TestResults/
**/.coverage/
*.trx
*.coverage

# ログファイル
*.log
**/logs/

# 一時ファイル
*.tmp
**/temp/
.DS_Store
Thumbs.db
```

## テスト成果物の管理

### 一時ファイルの配置
テスト実行時の一時ファイルは、システムの一時ディレクトリを使用します：

#### C#の例
```csharp
[TestClass]
public class ComponentRegistryTests
{
    private string testDir;

    [TestInitialize]
    public void Setup()
    {
        // 一時ディレクトリの作成
        testDir = Path.Combine(
            Path.GetTempPath(), 
            "awane-test", 
            Guid.NewGuid().ToString()
        );
        Directory.CreateDirectory(testDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // テスト後の自動削除
        if (Directory.Exists(testDir))
        {
            Directory.Delete(testDir, recursive: true);
        }
    }
}
```

#### Node.jsの例
```typescript
import { mkdtempSync, rmSync } from 'fs';
import { tmpdir } from 'os';
import { join } from 'path';

describe('ComponentRegistry', () => {
    let testDir: string;

    beforeEach(() => {
        // 一時ディレクトリの作成
        testDir = mkdtempSync(join(tmpdir(), 'awane-test-'));
    });

    afterEach(() => {
        // テスト後の自動削除
        rmSync(testDir, { recursive: true, force: true });
    });
});
```

### ログファイルの管理
- 開発時のログ: `/tmp/awane/logs/` (OS管理下)
- テスト時のログ: 各テストの一時ディレクトリ内
- 本番環境: 設定ファイルで指定

### テストデータの管理
- 小さなテストデータ: テストコード内にインライン
- 大きなテストデータ: `test/fixtures/` ディレクトリ（コミット対象）
- 生成されるデータ: 一時ディレクトリ（自動削除）

## CI/CD考慮事項

### テスト実行環境
- 一時ファイルは必ずOSの一時ディレクトリを使用
- 絶対パスの使用を避け、相対パスまたは環境変数を活用
- テスト後のクリーンアップを確実に実行

### ビルド成果物
- ビルド成果物は全て.gitignore対象
- CIでは毎回クリーンビルド
- 成果物の保存が必要な場合はCI/CDツールの機能を使用

## ベストプラクティス

1. **小さく始める**: 最小限の機能から実装とテストを開始
2. **継続的な動作確認**: 各機能が動作することを確認してから次へ
3. **ログの活用**: デバッグ情報を適切に出力
4. **クリーンな状態の維持**: 不要なファイルを残さない

## 開発フロー例

```bash
# 1. テストを書く
cd src/csharp/Awane.Core.Tests
# ComponentTests.cs を作成

# 2. テスト実行（失敗を確認）
dotnet test

# 3. 実装
cd ../Awane.Core
# Component.cs を実装

# 4. テスト実行（成功を確認）
cd ../Awane.Core.Tests
dotnet test

# 5. リファクタリング
# コードを改善し、テストが通ることを確認

# 6. コミット
git add .
git commit -m "feat: Add IAwaneComponent interface with tests"
```