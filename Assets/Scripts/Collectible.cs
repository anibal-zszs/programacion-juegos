using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum Kind { Normal, Especial }
    public Kind kind = Kind.Normal;
    public int points = 1;
    public float rotationSpeed = 90f;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        GameManager manager = FindFirstObjectByType<GameManager>();
        if (manager != null) manager.Collect(this);
    }
}
