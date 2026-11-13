using System.Collections.Generic;
using UnityEngine;

public class GameSession
{
    public const int RoundCount = 5;

    public GameMode Mode { get; }
    public Difficulty Difficulty = Difficulty.Normal;
    public ControlScheme PlayerControls = ControlScheme.MouseAndSpace;
    public SumoSetup Player;
    public SumoSetup SecondPlayer;
    public SumoSetup CurrentOpponent;
    public List<SumoSetup> FightOpponents { get; } = new List<SumoSetup>();
    public int Round { get; private set; }
    public bool PlayerEliminated { get; private set; }
    public bool IsChampion => !PlayerEliminated && Round == RoundCount;

    private readonly List<SumoSetup> tournamentOpponents = new List<SumoSetup>();

    public GameSession(GameMode mode)
    {
        Mode = mode;
    }

    public void StartTournament(Color[] palette)
    {
        List<int> numbers = new List<int>();
        for (int n = 1; n <= 9; n++)
        {
            if (n != Player.luckyNumber) numbers.Add(n);
        }
        Shuffle(numbers);

        tournamentOpponents.Clear();
        for (int i = 0; i < RoundCount; i++)
        {
            tournamentOpponents.Add(new SumoSetup(numbers[i], RandomAiColor(palette), true, ControlScheme.MouseAndSpace));
        }

        Round = 0;
        PlayerEliminated = false;
        PrepareRound();
    }

    public void PrepareRound()
    {
        CurrentOpponent = tournamentOpponents[Round];
    }

    public void ResolveRound(bool playerWon)
    {
        PlayerEliminated = !playerWon;
        if (!playerWon) return;

        Round++;
        if (Round < RoundCount) PrepareRound();
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

        Color groupColor = RandomAiColor(palette);
        for (int i = 0; i < Player.luckyNumber; i++)
        {
            FightOpponents.Add(new SumoSetup(CurrentOpponent.luckyNumber, groupColor, true, ControlScheme.MouseAndSpace));
        }
    }

    public static string RoundName(int round) => round switch
    {
        0 => "1/16 finals",
        1 => "1/8 finals",
        2 => "1/4 finals",
        3 => "Semi-finals",
        _ => "Final"
    };

    private Color RandomAiColor(Color[] palette)
    {
        List<Color> options = new List<Color>();
        foreach (Color color in palette)
        {
            if (color != Player.color) options.Add(color);
        }

        return options.Count == 0 ? palette[0] : options[Random.Range(0, options.Count)];
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
