using System.Collections.Generic;
using UnityEngine;

public class GameSession
{
    public GameMode Mode { get; }
    public Difficulty Difficulty = Difficulty.Normal;
    public ControlScheme PlayerControls = ControlScheme.MouseAndSpace;
    public SumoSetup Player;
    public SumoSetup SecondPlayer;
    public SumoSetup CurrentOpponent;
    public List<SumoSetup> FightOpponents { get; } = new List<SumoSetup>();
    public List<SumoSetup> Participants { get; private set; } = new List<SumoSetup>();
    public bool PlayerEliminated { get; private set; }

    public bool IsChampion => !PlayerEliminated && Participants.Count == 1;

    public GameSession(GameMode mode)
    {
        Mode = mode;
    }

    public void StartTournament(Color[] palette)
    {
        Participants = new List<SumoSetup> { Player };
        PlayerEliminated = false;

        List<int> numbers = new List<int>();
        for (int n = 1; n <= 9; n++)
        {
            if (n != Player.luckyNumber) numbers.Add(n);
        }
        numbers.RemoveAt(Random.Range(0, numbers.Count));

        foreach (int number in numbers)
        {
            Participants.Add(new SumoSetup(number, RandomColor(palette), true, ControlScheme.MouseAndSpace));
        }

        PrepareRound();
    }

    public void PrepareRound()
    {
        Shuffle(Participants);
        int playerIndex = Participants.IndexOf(Player);
        CurrentOpponent = Participants[playerIndex ^ 1];
    }

    public void ResolveRound(bool playerWon)
    {
        PlayerEliminated = !playerWon;

        List<SumoSetup> winners = new List<SumoSetup>();
        for (int i = 0; i < Participants.Count; i += 2)
        {
            SumoSetup a = Participants[i];
            SumoSetup b = Participants[i + 1];

            if (a == Player || b == Player) winners.Add(playerWon ? Player : CurrentOpponent);
            else winners.Add(Random.value < 0.5f ? a : b);
        }

        Participants = winners;
    }

    public void PrepareFight(Color[] palette)
    {
        FightOpponents.Clear();

        if (Mode == GameMode.LocalMulti)
        {
            FightOpponents.Add(SecondPlayer);
            return;
        }

        if (Difficulty == Difficulty.Normal)
        {
            FightOpponents.Add(CurrentOpponent);
            return;
        }

        for (int i = 0; i < Player.luckyNumber; i++)
        {
            FightOpponents.Add(new SumoSetup(CurrentOpponent.luckyNumber, RandomColor(palette), true, ControlScheme.MouseAndSpace));
        }
    }

    public static string NextRoundName(int poolSize) => poolSize switch
    {
        8 => "Quarts de finale",
        4 => "Demi-finales",
        2 => "Finale",
        _ => "Tour suivant"
    };

    private static Color RandomColor(Color[] palette) => palette[Random.Range(0, palette.Length)];

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
