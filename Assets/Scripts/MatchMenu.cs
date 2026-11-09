using UnityEngine;

[RequireComponent(typeof(MatchManager))]
public class MatchMenu : MonoBehaviour
{
    private int playerNumber = 1;
    private int aiNumber = 1;
    private MatchManager matchManager;

    void Awake()
    {
        matchManager = GetComponent<MatchManager>();
    }

    public void SetPlayerNumber(int number) => playerNumber = number;
    public void SetAiNumber(int number) => aiNumber = number;

    public void StartGame() => matchManager.StartGame(playerNumber, aiNumber);
}