using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalController : MonoBehaviour
{
    private bool player1Here = false;
    private bool player2Here = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().playerID == 1) player1Here = true;
            if (other.GetComponent<PlayerController>().playerID == 2) player2Here = true;

            CheckVictory();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().playerID == 1) player1Here = false;
            if (other.GetComponent<PlayerController>().playerID == 2) player2Here = false;
        }
    }



    private void CheckVictory()
    {
        if (player1Here && player2Here)
        {
            PlayerPrefs.SetInt("P1FinalScore", GameManager.instance.GetFinalScoreP1());
            PlayerPrefs.SetInt("P2FinalScore", GameManager.instance.GetFinalScoreP2());
            PlayerPrefs.SetInt("TotalFinalScore", GameManager.instance.GetFinalScoreP1() + GameManager.instance.GetFinalScoreP2());
            PlayerPrefs.Save();

            // Load victory scene
            SceneManager.LoadScene("VictoryScene"); 
        }
    }
}