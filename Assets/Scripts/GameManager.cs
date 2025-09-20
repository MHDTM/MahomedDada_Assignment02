using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI References")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Slider player1HealthSlider;
    public Slider player2HealthSlider;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip jumpClip;
    public AudioClip collectClip;
    public AudioClip enemyDeathClip;
    public AudioClip throwClip;

    [Header("Gameplay")]
    public GameObject projectilePrefab;
    public Transform[] projectileSpawnPoints;

    [Header("Respawn Settings")]
    public int respawnDelay = 10;
    public int deathPenalty = 5;
    public Text respawnTextP1;
    public Text respawnTextP2;

    public PlayerController player1;
    public PlayerController player2;

    private void Start()
    {

    }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(PlayerController player, int id)
    {
        if (id == 1) player1 = player;
        else if (id == 2) player2 = player;
    }

    public void UpdateScoreUI(int id, int score)
    {
        if (id == 1) player1ScoreText.text = "P1 Score: " + score;
        else if (id == 2) player2ScoreText.text = "P2 Score: " + score;
    }

    public void SetMaxHealth(int id, int maxHealth)
    {
        if (id == 1)
        {
            player1HealthSlider.maxValue = maxHealth;
            player1HealthSlider.value = maxHealth;   // initialize to full
        }
        else if (id == 2)
        {
            player2HealthSlider.maxValue = maxHealth;
            player2HealthSlider.value = maxHealth;   // initialize to full
        }
    }

    public void UpdateHealthUI(int id, int currentHealth)
    {
        if (id == 1) player1HealthSlider.value = currentHealth;
        else if (id == 2) player2HealthSlider.value = currentHealth;
    }

    public void PlayerDied(int id)
    {
        PlayerController p = (id == 1) ? player1 : player2;
        if (p == null) return;

        // Subtract score
        p.AddScore(-deathPenalty);

        // Start respawn countdown
        StartCoroutine(RespawnCountdown(p, id));

        // Check if BOTH dead at same time
        bool p1Dead = (player1 == null || player1.GetHealth() <= 0);
        bool p2Dead = (player2 == null || player2.GetHealth() <= 0);

        if (p1Dead && p2Dead)
        {
            PlayerPrefs.SetInt("P1FinalScore", GetFinalScoreP1());
            PlayerPrefs.SetInt("P2FinalScore", GetFinalScoreP2());
            PlayerPrefs.SetInt("TotalFinalScore", GetFinalScoreP1() + GetFinalScoreP2());
            PlayerPrefs.Save();

            SceneManager.LoadScene("Game Over");
        }
    }

    public void SpawnProjectile(Vector3 pos, int playerID)
    {
        Transform spawnPoint = projectileSpawnPoints[playerID - 1];
        Instantiate(projectilePrefab, spawnPoint.position, Quaternion.identity);
        PlaySound(throwClip);
    }

    public void PlayJumpSound() => PlaySound(jumpClip);
    public void PlayCollectSound() => PlaySound(collectClip);
    public void PlayEnemyDeathSound() => PlaySound(enemyDeathClip);

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource && clip) sfxSource.PlayOneShot(clip);
    }

    public int GetFinalScoreP1() => player1 != null ? player1.GetScore() : 0;
    public int GetFinalScoreP2() => player2 != null ? player2.GetScore() : 0;

    private IEnumerator RespawnCountdown(PlayerController player, int id)
    {
        player.gameObject.SetActive(false); // temporarily disable player

        Text countdownText = (id == 1) ? respawnTextP1 : respawnTextP2;
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        int timeLeft = respawnDelay;
        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = "P" + id + " Respawning in " + timeLeft + "...";
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        // Respawn
        player.Respawn();
        player.gameObject.SetActive(true);
    }
}