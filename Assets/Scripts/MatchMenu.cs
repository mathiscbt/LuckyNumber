using UnityEngine;

[RequireComponent(typeof(MatchManager))]
public class MatchMenu : MonoBehaviour
{
    [SerializeField] private GameObject matchMenuCanvas;
    private int playerNumber = 1;
    private int aiNumber = 1;
    private MatchManager matchManager;

    void Awake()
    {
        matchManager = GetComponent<MatchManager>();
    }

    void Start()
    {
        matchMenuCanvas.SetActive(true);
    }

    public void SetPlayerNumber(int number) => playerNumber = number;
    public void SetAiNumber(int number) => aiNumber = number;

    public void StartGame()
    {
        matchMenuCanvas.SetActive(false);
        matchManager.StartGame(playerNumber, aiNumber);
    }
}