using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    private Canvas _gameOverCanvas;

    private void Start()
    {
        _gameOverCanvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        PlayerMovement.GameOverEvent += GameOver;
    }

    private void OnDisable()
    {
        PlayerMovement.GameOverEvent -= GameOver;
    }

    private void GameOver()
    {
        _gameOverCanvas.enabled = true;
    }
}
