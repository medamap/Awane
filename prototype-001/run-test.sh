#!/bin/bash

echo "=== Awane プロセス間通信テスト ==="
echo ""
echo "1. Prototype1を起動（IPaiプロバイダー）"
echo "2. 別ターミナルでPrototype2を起動（IPaiコンシューマー）"
echo ""
echo "Prototype1を起動します..."

cd Prototype1
dotnet run