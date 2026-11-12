using UnityEngine;

public class SetupPanel : UIPanel
{
    [SerializeField] private SumoSetupPanel playerOne;
    [SerializeField] private SumoSetupPanel playerTwo;
    [SerializeField] private GameObject difficultyRoot;

    public void Configure(bool isLocalMulti)
    {
        playerTwo.gameObject.SetActive(isLocalMulti);
        difficultyRoot.SetActive(!isLocalMulti);
    }

    public SumoSetup BuildPlayerOne(ControlScheme controls) => playerOne.Build(controls);
    public SumoSetup BuildPlayerTwo(ControlScheme controls) => playerTwo.Build(controls);

    public void OnClickNormal() => GameFlowManager.Instance.SetDifficulty(Difficulty.Normal);
    public void OnClickHard() => GameFlowManager.Instance.SetDifficulty(Difficulty.Hard);
    public void OnClickConfirm() => GameFlowManager.Instance.ConfirmSetup();
    public void OnClickBack() => GameFlowManager.Instance.BackToMenu();
}
