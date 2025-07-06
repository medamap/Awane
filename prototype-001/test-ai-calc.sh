#!/bin/bash

echo "=== AI計算ノードテスト ==="
echo "質問: たまご＋こむぎこ＋さとう＋チョコレート＝？"
echo ""

# Prototype1を起動
cd Prototype1
dotnet run &
PID1=$!

echo "Prototype1 (PID: $PID1) を起動しました..."
sleep 5

# 別ターミナルでテスト実行
cd ../PaiDemo
echo -e "たまご＋こむぎこ＋さとう＋チョコレート＝？\nexit" | dotnet run

# Prototype1を終了
kill $PID1 2>/dev/null

echo ""
echo "テスト完了！"