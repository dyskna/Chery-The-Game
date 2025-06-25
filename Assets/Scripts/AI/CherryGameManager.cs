using UnityEngine;
using System;
using WorldTime;

public class CherryGameManager : MonoBehaviour
{
    public int playerScore = 0;
    public int npcScore = 0;
    public int matchLengthInMinutes = 8;

    public WorldTime.WorldTime worldTime;
    private int matchStartMinute = 0;
    private bool gameEnded = false;

    private void Start()
    {
        if (worldTime == null)
            worldTime = FindObjectOfType<WorldTime.WorldTime>();

        matchStartMinute = worldTime._currentTime.Minutes;
        worldTime.WorldTimeChanged += OnTimeUpdated;
    }

    private void OnTimeUpdated(object sender, TimeSpan currentTime)
    {
        int elapsed = currentTime.Minutes - matchStartMinute;

        if (!gameEnded && elapsed >= matchLengthInMinutes)
        {
            EndGame();
        }
    }

    public void AddScore(bool isPlayer, int amount)
    {
        if (isPlayer)
            playerScore += amount;
        else
            npcScore += amount;
    }

    private void EndGame()
    {
        gameEnded = true;

        string winner;
        if (playerScore > npcScore)
            winner = "Player wins!";
        else if (npcScore > playerScore)
            winner = "AI wins!";
        else
            winner = "It's a tie!";

        Debug.Log($"Game Over! {winner} Player: {playerScore} vs AI: {npcScore}");
    }
}
