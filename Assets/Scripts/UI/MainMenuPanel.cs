public class MainMenuPanel : UIPanel
{
    public void OnClickVsAi() => GameFlowManager.Instance.SelectMode(GameMode.VsAi);
    public void OnClickLocalMulti() => GameFlowManager.Instance.SelectMode(GameMode.LocalMulti);
}
