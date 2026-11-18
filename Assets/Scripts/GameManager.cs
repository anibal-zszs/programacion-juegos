using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int targetScore = 16;
    public int score;
    public AudioSource pickupAudioSource;
    public AudioClip pickupClip;
    private bool restarting;

    public void Collect(Collectible item)
    {
        if (item == null || restarting) return;
        int value = item.kind == Collectible.Kind.Normal ? 1 : item.points;
        score += value;
        if (pickupAudioSource != null && pickupClip != null) pickupAudioSource.PlayOneShot(pickupClip);
        Debug.Log($"Recolectado: {(item.kind == Collectible.Kind.Normal ? "Normal" : "Especial")} ({(value >= 0 ? "+" : "")}{value}). Puntaje: {score}/{targetScore}");
        Destroy(item.gameObject);
        if (score >= targetScore)
        {
            restarting = true;
            Debug.Log("¡Meta alcanzada! Reiniciando la partida...");
            StartCoroutine(RestartRound());
        }
    }

    private IEnumerator RestartRound()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
