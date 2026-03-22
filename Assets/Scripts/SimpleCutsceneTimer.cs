using UnityEngine;
using System.Collections;

public class SimpleCutsceneTimer : MonoBehaviour
{
    public float cutsceneDuration = 4f;
    public string introAnimationTrigger = "PlayIntro"; // Change this to your Animator trigger name!

    void Start()
    {
        StartCoroutine(CutsceneRoutine());
    }

    IEnumerator CutsceneRoutine()
    {
        // 1. Wait a tiny fraction of a second to make sure all players have spawned in
        yield return new WaitForSeconds(0.1f);

        PlayerMovement[] allPlayers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        // 2. Lock their controllers and trigger the animation!
        foreach (PlayerMovement player in allPlayers)
        {
            player.isPlayingCutscene = true;

            if (player.anim != null)
            {
                player.anim.SetTrigger(introAnimationTrigger);
            }
        }

        // 3. Wait for the cutscene to finish
        yield return new WaitForSeconds(cutsceneDuration);

        // 4. Give everyone their controllers back so the match can start!
        foreach (PlayerMovement player in allPlayers)
        {
            player.isPlayingCutscene = false;
        }
    }
}