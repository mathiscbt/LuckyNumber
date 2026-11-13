using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ami.BroAudio;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [SerializeField] private MatchManager matchManager;
    [SerializeField] private SoundID victorySound;
    [SerializeField] private SoundID defeatSound;
    [SerializeField] private SoundID menuMusic;
    [SerializeField] private SoundID combatMusic;
    [SerializeField] private float musicFadeIn = 1f;
    [SerializeField] private float musicFadeOut = 1f;
    [SerializeField] private SoundID uiHoverSound;
    [SerializeField] private SoundID uiClickSound;
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
    private SoundID currentMusic;

    public Color[] Palette => palette;

    private void Awake()
    {
        Instance = this;
        allPanels = new UIPanel[] { mainMenuPanel, controlSchemePanel, setupPanel, statsPanel, matchResultPanel, tournamentRoundPanel, tournamentEndPanel };
        matchManager.OnFightEnded += HandleFightEnded;
        WireUiSounds();
    }

    private void WireUiSounds()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            button.gameObject.AddComponent<UIButtonSound>().Init(uiHoverSound, uiClickSound);
        }
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
        PlayMusic(menuMusic);
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
        ShowStats();
    }

    public void BackToMenu()
    {
        ShowMainMenu();
    }

    private void ShowStats()
    {
        session.PrepareFight(palette);

        ShowOnly(statsPanel);
        statsPanel.Display(session.Player, session.FightOpponents, session.Mode == GameMode.VsAi && session.Difficulty == Difficulty.Hard, StartFight);
    }

    private void StartFight()
    {
        HideAll();
        PlayMusic(combatMusic);
        matchManager.StartFight(new List<SumoSetup> { session.Player }, session.FightOpponents);
    }

    private void HandleFightEnded(bool playerSideWon)
    {
        PlayMusic(menuMusic);

        if (session.Mode == GameMode.LocalMulti)
        {
            BroAudio.Play(victorySound);
            matchResultPanel.Display(playerSideWon ? "Player 1 wins" : "Player 2 wins");
            ShowOnly(matchResultPanel);
            return;
        }

        BroAudio.Play(playerSideWon ? victorySound : defeatSound);
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
            tournamentRoundPanel.Display(GameSession.RoundName(session.Round));
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

    private void PlayMusic(SoundID music)
    {
        if (currentMusic.Equals(music)) return;

        BroAudio.Stop(BroAudioType.Music, musicFadeOut);
        BroAudio.Play(music, musicFadeIn);
        currentMusic = music;
    }
}
