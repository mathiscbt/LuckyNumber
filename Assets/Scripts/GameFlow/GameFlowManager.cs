using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [SerializeField] private MatchManager matchManager;
    [SerializeField] private Color[] palette;
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private ControlSchemePanel controlSchemePanel;
    [SerializeField] private SetupPanel setupPanel;
    [SerializeField] private StatsPanel statsPanel;
    [SerializeField] private MatchResultPanel matchResultPanel;
    [SerializeField] private TournamentRoundPanel tournamentRoundPanel;
    [SerializeField] private TournamentEndPanel tournamentEndPanel;

    private GameSession session;
    private UIPanel[] allPanels;

    public Color[] Palette => palette;

    private void Awake()
    {
        Instance = this;
        allPanels = new UIPanel[] { mainMenuPanel, controlSchemePanel, setupPanel, statsPanel, matchResultPanel, tournamentRoundPanel, tournamentEndPanel };
        matchManager.OnFightEnded += HandleFightEnded;
    }

    private void Start()
    {
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (matchManager != null) matchManager.OnFightEnded -= HandleFightEnded;
    }

    public void ShowMainMenu()
    {
        matchManager.EndGame();
        session = null;
        ShowOnly(mainMenuPanel);
    }

    public void SelectMode(GameMode mode)
    {
        session = new GameSession(mode);

        if (mode == GameMode.VsAi)
        {
            ShowOnly(controlSchemePanel);
            return;
        }

        setupPanel.Configure(true);
        ShowOnly(setupPanel);
    }

    public void SelectControlScheme(ControlScheme controls)
    {
        if (session == null) return;
        session.PlayerControls = controls;
        setupPanel.Configure(false);
        ShowOnly(setupPanel);
    }

    public void SetDifficulty(Difficulty difficulty)
    {
        if (session != null) session.Difficulty = difficulty;
    }

    public void ConfirmSetup()
    {
        if (session == null) return;

        ControlScheme playerOneControls = session.Mode == GameMode.VsAi ? session.PlayerControls : ControlScheme.ZQSDAndSpace;
        SumoSetup playerOne = setupPanel.BuildPlayerOne(playerOneControls);
        if (playerOne == null) return;
        session.Player = playerOne;

        if (session.Mode == GameMode.LocalMulti)
        {
            SumoSetup playerTwo = setupPanel.BuildPlayerTwo(ControlScheme.ArrowsAndRShift);
            if (playerTwo == null) return;
            session.SecondPlayer = playerTwo;
        }
        else
        {
            session.StartTournament(palette);
        }

        ShowStats();
    }

    public void Replay()
    {
        matchManager.EndGame();
        ShowStats();
    }

    public void ContinueTournament()
    {
        session.PrepareRound();
        ShowStats();
    }

    public void BackToMenu()
    {
        ShowMainMenu();
    }

    private void ShowStats()
    {
        session.PrepareFight(palette);

        List<SumoData> opponentData = session.FightOpponents.ConvertAll(opponent => matchManager.GetDataFor(opponent.luckyNumber));
        SumoData playerData = matchManager.GetDataFor(session.Player.luckyNumber);

        statsPanel.Display(session.Player, playerData, session.FightOpponents, opponentData, StartFight);
        ShowOnly(statsPanel);
    }

    private void StartFight()
    {
        HideAll();
        matchManager.StartFight(new List<SumoSetup> { session.Player }, session.FightOpponents);
    }

    private void HandleFightEnded(bool playerSideWon)
    {
        if (session.Mode == GameMode.LocalMulti)
        {
            matchResultPanel.Display(playerSideWon ? "Joueur 1 gagne" : "Joueur 2 gagne");
            ShowOnly(matchResultPanel);
            return;
        }

        session.ResolveRound(playerSideWon);

        if (session.PlayerEliminated)
        {
            tournamentEndPanel.Display(false);
            ShowOnly(tournamentEndPanel);
        }
        else if (session.IsChampion)
        {
            tournamentEndPanel.Display(true);
            ShowOnly(tournamentEndPanel);
        }
        else
        {
            tournamentRoundPanel.Display(GameSession.NextRoundName(session.Participants.Count));
            ShowOnly(tournamentRoundPanel);
        }
    }

    private void ShowOnly(UIPanel panel)
    {
        foreach (UIPanel p in allPanels) p.Hide();
        panel.Show();
    }

    private void HideAll()
    {
        foreach (UIPanel p in allPanels) p.Hide();
    }
}
