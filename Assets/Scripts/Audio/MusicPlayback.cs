using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayback : MonoBehaviour
{
    [ContextMenu("Play Beat")]
    public void PlayBeat()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        GetComponent<AudioSource>().Play();
    }
}