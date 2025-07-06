# 実装指示フレームワーク

## 概要
PoppoAcademyプロジェクトにおいて、AI（Claude）による実装を効率的かつ確実に行うための実装指示フレームワークについて説明します。このフレームワークは、将来的なAI実装自動化への布石でもあります。

## 背景と目的

### 課題
1. **コンテキストの枯渇**: 長時間のセッションでコンテキストが満杯になる
2. **情報の混在**: 不要な情報が蓄積し、実装の精度が低下
3. **再現性の欠如**: 同じタスクでも結果が安定しない
4. **並列実装の困難**: 1つのセッションで複数タスクを扱うと混乱

### 解決策
各実装タスクごとに新規Claudeプロセスを起動し、必要最小限の情報のみを提供することで、クリーンな環境での実装を実現します。

## 実装指示の流れ

### 1. 指示ファイルの作成
実装タスクごとにJSON形式の指示ファイルを作成します。

### 2. Claudeプロセスの起動
```bash
claude --print --dangerously-skip-permissions '次のファイルの指示にしたがって作業をしてください"/path/to/instructions/awane-001.json"'
```

### 3. 実装とレポート
Claudeは指示に従って実装を行い、結果をレポートファイルに記録します。

## 指示ファイルフォーマット

### 基本構造
```json
{
  "instruction_id": "awane-001",
  "title": "タスクのタイトル",
  "objective": "達成すべき目標の明確な記述",
  "context": {
    "project_root": "/path/to/project",
    "target_language": ["csharp", "typescript"],
    "environment": {}
  },
  "requirements": [
    "必須要件1",
    "必須要件2"
  ],
  "specifications": {
    "詳細な仕様をここに記載"
  },
  "deliverables": [
    "成果物のファイルパス一覧"
  ],
  "references": {
    "primary": ["主要参照ドキュメント"],
    "secondary": ["補助参照ドキュメント"]
  },
  "report_path": "レポート出力先パス",
  "execution_notes": "実行時の注意事項"
}
```

### フィールドの説明

#### instruction_id
- 一意の識別子
- 連番形式: `001`, `002`, `003`
- 同じ機能の再実装時も新しい番号を付与

#### title
- タスクの簡潔な説明
- 実装内容が一目でわかるように

#### objective
- 何を達成すべきかの明確な記述
- 成功基準を含める

#### context
- プロジェクトのルートパス
- 対象言語
- 必要な環境情報（フレームワーク、バージョン等）

#### requirements
- 満たすべき要件のリスト
- 優先順位順に記載

#### specifications
- 詳細な技術仕様
- インターフェース定義
- テストケース
- 実装の制約事項

#### deliverables
- 作成すべきファイルの完全なパスリスト
- テストファイルも含める

#### references
- primary: 必ず参照すべきドキュメント
- secondary: 補助的な参照ドキュメント
- セクション指定可能（例: `docs/api-design.md#iawanecomponent`）

#### report_path
- 実装レポートの出力先
- 命名規則: `instructions/reports/{instruction_id}-report.md`

#### execution_notes
- 実装時の注意事項
- 手順の指定（例: "テストファーストで実装"）

## ディレクトリ構成

```
prototype-002/
├── docs/
│   ├── instructions/          # 実装指示ファイル
│   │   ├── instruction-001-awane-component-interface.json
│   │   ├── instruction-002-awane-component-base-class.json
│   │   └── instruction-003-awane-component-interface-v2.json  # 001の改善版
│   └── reports/              # 実装レポート
│       ├── report-001-awane-component-interface.md
│       ├── report-002-awane-component-base-class.md
│       └── report-003-awane-component-interface-v2.md
```

### 命名規則
- 指示ファイル: `instruction-{連番}-{機能名}.json`
- レポート: `report-{連番}-{機能名}.md`
- 連番は3桁のゼロパディング
- 機能名は指示とレポートで必ず同一にする

## 実装レポートフォーマット

実装を担当するClaudeは、以下の形式でレポートを作成します：

```markdown
# 実装レポート: {instruction_id}

## 実行日時
YYYY-MM-DD HH:MM:SS

## 実装内容
- 作成したファイル一覧
- 実装した機能の概要

## テスト結果
- 実行したテスト
- テスト結果

## 課題・注意点
- 未解決の課題
- 今後の改善点

## 次のステップの提案
- 推奨される次の実装タスク
```

## ベストプラクティス

### 1. 指示の具体性
- 曖昧な表現を避ける
- 具体的なファイルパス、メソッド名を指定
- 期待される動作を明確に記述

### 2. 参照ドキュメントの最適化
- 必要な部分のみを参照（セクション指定）
- 長大なドキュメントは specifications に要約を含める

### 3. テスト駆動の徹底
- deliverables にテストファイルを必ず含める
- execution_notes でテストファーストを明記

### 4. 段階的な実装
- 大きなタスクは複数の指示に分割
- 依存関係を明確にする

## 実装例

### 例1: IAwaneComponentインターフェース実装
```json
{
  "instruction_id": "001",
  "title": "IAwaneComponentインターフェースの実装",
  "objective": "C#とTypeScriptでIAwaneComponentインターフェースを実装し、基本的なテストを作成",
  "context": {
    "project_root": "/Volumes/PoppoSSD2T/Projects/ClaudeCodeProjects/PoppoAcademy/Awane/prototype-002",
    "target_language": ["csharp", "typescript"],
    "test_framework": {
      "csharp": "MSTest",
      "typescript": "Jest"
    }
  },
  "requirements": [
    "TDDアプローチで実装（テストを先に書く）",
    "名前衝突を避けるためAwaneプレフィクスを使用",
    "両言語で同等の機能を提供"
  ],
  "specifications": {
    "interface_members": {
      "AwaneId": "string - コンポーネントの一意識別子",
      "AwaneLocation": "string - コンポーネントの場所情報",
      "AwaneInterfaces": "string[] - 実装しているインターフェース名の配列",
      "AwaneAs<T>()": "T - 指定された型へのキャスト"
    }
  },
  "deliverables": [
    "src/csharp/Awane.Core/Components/IAwaneComponent.cs",
    "src/csharp/Awane.Core.Tests/Components/IAwaneComponentTests.cs",
    "src/nodejs/src/components/IAwaneComponent.ts",
    "src/nodejs/test/components/IAwaneComponent.test.ts"
  ],
  "references": {
    "primary": ["docs/api-design.md#iawanecomponent"],
    "secondary": ["docs/development-guidelines.md"]
  },
  "report_path": "docs/reports/report-001-awane-component-interface.md",
  "execution_notes": "テストを先に実装し、Redの状態を確認してから本実装を行ってください"
}
```

## 効果と利点

### 1. コンテキストの最適化
- 各タスクに必要な情報のみを提供
- 不要な情報による混乱を防止

### 2. 再現性の向上
- 同じ指示ファイルで同じ結果を期待できる
- 実装の品質が安定

### 3. 並列実装の実現
- 複数のタスクを同時に別プロセスで実行可能
- 開発速度の向上

### 4. 自動化への道筋
- PoppoAcademyの最終目標であるAI実装自動化の基盤
- 指示ファイルの蓄積により、パターン化が可能

## バージョン管理と再実装

### 連番管理の方針
1. **常に新しい番号**: 実装をやり直す場合も新しい連番を付与
2. **ロールバック可能**: 以前の実装は保持され、いつでも参照可能
3. **改善の追跡**: 同じ機能でも改善版は新しい番号で管理

### 例：実装の繰り返し
```
instruction-001-awane-component-interface.json  # 初回実装
instruction-003-awane-component-interface-v2.json  # 改善版
instruction-007-awane-component-interface-v3.json  # さらなる改善
```

### 実装粒度の選択

#### 粒度の種類
1. **フルサイクル型**: Red-Green-Refactorを1つの指示に含める
   - 小規模な機能に適している
   - 1つの指示で完結

2. **ステップ分割型**: Red, Green, Refactorを別々の指示に分割
   - 複雑な機能に適している
   - 各ステップを慎重に確認可能

#### 粒度選択の基準
```json
{
  "implementation_scope": {
    "type": "full-cycle",  // または "red-only", "green-only", "refactor-only"
    "reason": "シンプルなインターフェース実装のため"
  }
}
```

### 実装管理のベストプラクティス

1. **成果物の明確化**
   ```json
   "deliverables": [
     "src/csharp/Awane.Core/IAwaneComponent.cs",
     "src/csharp/Awane.Core.Tests/IAwaneComponentTests.cs"
   ],
   "expected_test_results": {
     "initial": "all tests should fail (Red)",
     "final": "all tests should pass (Green)"
   }
   ```

2. **依存関係の記録**
   ```json
   "dependencies": {
     "requires": ["instruction-001", "instruction-002"],
     "supersedes": ["instruction-001"]  // 改善版の場合
   }
   ```

3. **ロールバック手順**
   - Gitでの実装前の状態を記録
   - 失敗時は`git reset`で元に戻す
   - 新しい連番で再挑戦

## 今後の展望

1. **指示ファイルの自動生成**: 設計ドキュメントから指示ファイルを生成
2. **実装の自動検証**: レポートから次の指示を自動生成
3. **完全自動化**: PoppoAcademyによる自律的な実装サイクル
4. **学習機能**: 成功パターンの蓄積と活用

このフレームワークは、現在の手動プロセスを段階的に自動化していくための重要な第一歩となります。