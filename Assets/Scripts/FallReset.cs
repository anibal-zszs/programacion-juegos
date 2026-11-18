using UnityEngine;

public class FallReset : MonoBehaviour
{
    public float resetHeight = -2f;
    public Vector3 startPosition = new Vector3(-7f, 0.85f, 0f);
    public AudioSource fallAudioSource;
    public AudioClip fallClip;
    private PlayerController player;
    private bool fallSoundPlayed;

    private void Start() { player = GetComponent<PlayerController>(); }

    private void Update()
    {
        if (transform.position.y < resetHeight && player != null)
        {
            if (!fallSoundPlayed && fallAudioSource != null && fallClip != null)
            {
                fallAudioSource.PlayOneShot(fallClip);
                fallSoundPlayed = true;
            }
            player.ResetMotion(startPosition);
            Debug.Log("Caída detectada: jugador reposicionado en el inicio.");
        }
        else if (transform.position.y > resetHeight + 0.5f)
        {
            fallSoundPlayed = false;
        }
    }
}
