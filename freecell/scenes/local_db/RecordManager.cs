using Godot;
using System;
using System.Collections.Generic;

public partial class RecordManager : Node
{
    public void MakeRecord(int gameSeed, int moveCount, int undoCount, int elapsedTime, int score, FreecellGame.GameState gameState)
    {
        // 记录游戏结果
        GD.Print($"Recording game: Seed: {gameSeed}, Moves: {moveCount}, Undos: {undoCount}, Time: {elapsedTime}, Score: {score}, State: {gameState}");
    }

    public void SaveRunningGameInfo(int gameSeed, int moveCount, int undoCount, int elapsedTime, int score, FreecellGame.GameState gameState)
    {
        // 保存当前游戏状态
    }
}