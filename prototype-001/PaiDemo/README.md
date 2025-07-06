# Pai Demo - Claude CLI連携デモ

## 概要
「ぱいちゃん」プロトタイプのデモプログラムです。
Claude CLIを使用してAIとインタラクティブに対話できます。

## 前提条件
- Claude CLIがインストールされていること
- `claude` コマンドが使用可能なこと

## 使い方

### 1. Prototype1を起動（IPaiプロバイダー）
```bash
cd ../Prototype1
dotnet run
```

### 2. 別ターミナルでPaiDemoを起動
```bash
cd PaiDemo
dotnet run
```

### 3. プロンプトを入力
```
プロンプト（'exit'で終了）> こんにちは！元気ですか？

Claude CLIに問い合わせ中...

=== Claude応答 ===
こんにちは！私は元気です。ありがとうございます。
あなたはいかがですか？
==================
```

## 動作の流れ
1. PaiDemoがPrototype1のIPaiコンポーネントを検索
2. リモートプロキシ経由でPaiMethodAsyncを呼び出し
3. Prototype1側でClaude CLIを実行
4. 結果をPaiDemoに返却

## 注意事項
- Claude CLIのレート制限に注意してください
- プロトタイプのため、エラー処理は最小限です