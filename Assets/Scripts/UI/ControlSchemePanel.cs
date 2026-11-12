public class ControlSchemePanel : UIPanel
{
    public void OnClickMouseAndSpace() => GameFlowManager.Instance.SelectControlScheme(ControlScheme.MouseAndSpace);
    public void OnClickZqsdAndSpace() => GameFlowManager.Instance.SelectControlScheme(ControlScheme.ZQSDAndSpace);
    public void OnClickArrowsAndRShift() => GameFlowManager.Instance.SelectControlScheme(ControlScheme.ArrowsAndRShift);
    public void OnClickBack() => GameFlowManager.Instance.BackToMenu();
}
