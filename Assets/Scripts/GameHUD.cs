using UnityEngine;

public class GameHUD : MonoBehaviour
{
    private GameManager manager;
    private GUIStyle style;

    private void Start() { manager = FindFirstObjectByType<GameManager>(); }

    private void OnGUI()
    {
        if (style == null)
        {
            style = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleLeft };
            style.normal.textColor = Color.white;
        }
        int score = manager != null ? manager.score : 0;
        GUI.Box(new Rect(16, 16, 320, 92), $"PLATAFORMA 3D\nPuntaje: {score} / {(manager != null ? manager.targetScore : 16)}\nWASD mover  |  Espacio saltar  |  Shift sprint", style);
    }
}
