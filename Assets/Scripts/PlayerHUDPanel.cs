using UnityEngine;
using UnityEngine.UI;
using TMPro; // Make sure to include this for TextMeshPro!

public class PlayerHUDPanel : MonoBehaviour
{
    public Image backgroundImage;
    public TextMeshProUGUI scoreText;

    private Color normalColor;
    public Color itColor = Color.red;

    // The Manager calls this when the player first spawns
    public void Setup(Color playerColor)
    {
        normalColor = playerColor;
        backgroundImage.color = normalColor;
        scoreText.text = "Candies: 0";
    }

    // Call this whenever the player scores a point (like grabbing an orb)
    public void UpdateScore(int newScore)
    {
        scoreText.text = "Candies: " + newScore;
    }

    // Swaps the UI background color to red if they are IT
    public void SetItStatus(bool isIt)
    {
        backgroundImage.color = isIt ? itColor : normalColor;
    }
}