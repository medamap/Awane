# Awane prototype-002

## 設計方針
gRPC + Adapter Patternで開発を進める

## ターゲット言語
- C#
- Node.js

## アーキテクチャ概要
### 通信レイヤー
- **同一プロセス**: 直接参照（C#のみ）
- **同一PC**: Unix Domain Socket
- **ネットワーク**: gRPC over TCP/HTTP2

### コンポーネント構成
- **Protocol Buffers**: インターフェース定義言語（IDL）として使用
- **Adapter Pattern**: 通信方式の違いを吸収
- **透過的なAPI**: 利用側は通信方式を意識しない設計

## 実装方針
1. .protoファイルでインターフェース定義
2. C#/Node.js両方でgRPCクライアント/サーバー実装
3. Adapterレイヤーで通信方式を抽象化
4. 段階的に通信範囲を拡張（ローカル→ネットワーク）