using UnityEngine;

public class PlayerColorManager : MonoBehaviour
{
    // This makes it easy for players to find the manager
    public static PlayerColorManager Instance;

    [Header("Assign Materials in Inspector (Blue, Yellow, Purple, Orange)")]
    public Material[] colorQueue;

    // Keeps track of how many players have pressed the button
    private int playersJoined = 0;

    void Awake()
    {
        Instance = this;
    }

    // Players will call this when they press the button
    public Material GetNextColor()
    {
        if (playersJoined < colorQueue.Length)
        {
            Material assignedColor = colorQueue[playersJoined];
            playersJoined++;
            return assignedColor;
        }

        Debug.LogWarning("More than 4 players tried to get a color!");
        return null;
    }
}