using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ControllerVideoTrigger : MonoBehaviour
{
    [Header("Jump Settings")]
    public VideoPlayer jumpPlayer;
    public RawImage jumpUI;

    [Header("Push/Tag Settings")]
    public VideoPlayer pushPlayer;
    public RawImage pushUI;

    [Header("Movement Settings")]
    public VideoPlayer movePlayer;
    public RawImage moveUI;

    void Update()
    {
        // STEP A: The Debugger - Watch the Console window while you press buttons
        for (int i = 0; i < 20; i++)
        {
            if (Input.GetKeyDown("joystick button " + i))
            {
                Debug.Log("Unity says you pressed Button: " + i);
            }
        }

        // STEP B: The Logic
        // Based on your description, try swapping these numbers:

        // If pressing '3' does nothing, it might actually be Button 2 or 1.
        if (Input.GetKeyDown(KeyCode.JoystickButton2)) // Test with 2 if 3 is broken
        {
            PlayVideo(jumpPlayer, jumpUI);
        }

        // If pressing '4' plays Jump, then 4 is WRONG for Push. 
        // Find the correct ID from the Console and put it here:
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            PlayVideo(pushPlayer, pushUI);
        }

        // Movement
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f)
        {
            PlayVideo(movePlayer, moveUI);
        }
    }

    void PlayVideo(VideoPlayer player, RawImage ui)
    {
        // Hide all displays first
        jumpUI.gameObject.SetActive(false);
        moveUI.gameObject.SetActive(false);
        pushUI.gameObject.SetActive(false);

        // Show and play the correct one
        ui.gameObject.SetActive(true);
        player.Stop();
        player.Play();

        // Auto-hide when done
        player.loopPointReached -= (vp) => { ui.gameObject.SetActive(false); };
        player.loopPointReached += (vp) => { ui.gameObject.SetActive(false); };
    }

    // Separate function for clarity
    void HideVideo(VideoPlayer vp)
    {
        vp.targetCameraAlpha = 0; // Modern fade-out style
        vp.gameObject.SetActive(false);
    }
}