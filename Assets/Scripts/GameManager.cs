using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private BoardManager boardScript;

    public int playerTimeLeft; // seconds

    public List<Vector2> reservedPositons;

    private Text timeText;
    private Text damageText;
    private Text healText;
    private Text levelText;
    private Text gameOverText;
    private Image gameOverImage;

    public int levelNumber;
    public int realLevel = 1;

    public AudioClip damageSfx;
    public AudioClip gameOverSound;

    void Awake(){
        if(instance == null){
            instance = this;
        } else if (instance != this){
            Destroy(gameObject);
        }

        reservedPositons = new List<Vector2>();
        if (!SceneManager.GetActiveScene().name.Equals("Main")) playerTimeLeft = 50;
        
         //uncomment to use Main Scene without menu scene
        DontDestroyOnLoad(gameObject);
        SoundManager.instance.StartMusic();
        realLevel = DifficultyManager.instance.difficulty;
        
        InvokeRepeating("TimePassed", 1, 1);
    }
    
    void OnLevelWasLoaded(int index)
    {
        InitGame();
    }

    private void Update()
    {
        if (Input.GetKey("escape"))
        {
            SoundManager.instance.StopMusic();
            instance = null;
            SceneManager.LoadScene(0);
            Destroy(gameObject);
        }
    }
    void InitGame(){
        if(instance == null){
            instance = this;
        } else if (instance != this){
            Destroy(gameObject);
        }
        boardScript = GetComponent<BoardManager>();

        timeText = GameObject.Find("TimerText").GetComponent<Text>();
        damageText = GameObject.Find("DamageText").GetComponent<Text>();
        healText = GameObject.Find("HealText").GetComponent<Text>();
        levelText = GameObject.Find("LevelNumber").GetComponent<Text>();
        gameOverText = GameObject.Find("GameOverScreen").GetComponent<Text>();
        gameOverImage = GameObject.Find("GameOverImage").GetComponent<Image>();
        gameOverImage.enabled = false;

        timeText.text = "" + playerTimeLeft;
        levelText.text = "Level " + levelNumber;

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            boardScript.SetupLevel(realLevel);
        }
    }
    
    //deprecated do not use!!!!!!1
    public void Reload()
    {
        Destroy(gameObject);
        instance = null;
        instance = this;
        InitGame();
    }

    public void NextLevel()
    {
        levelNumber++;
        if (levelNumber < 7 && SceneManager.GetActiveScene().buildIndex != 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        realLevel++;
        SceneManager.LoadScene(1);
    }

    public void Damage(int damage)
    {
        SoundManager.instance.PlayerSound(damageSfx);
        playerTimeLeft -= damage;
        timeText.text = "" + playerTimeLeft;
        damageText.text = "-" + damage;
        if (playerTimeLeft <= 0) {
            CheckGameOver();
        }
        Invoke("RemoveDamageText",2);
        
    }

    public void ReloadBoard()
    {
        boardScript.Reload();
    }

    void RemoveDamageText()
    {
        damageText.text = "";
    }

    void RemoveHealText()
    {
        healText.text = "";
    }

    public void Restore(int heal)
    {
        playerTimeLeft += heal;
        timeText.text = "" + playerTimeLeft;
        healText.text = "+" + heal;
        Invoke("RemoveHealText", 2);
    }

    void TimePassed()
    {
        playerTimeLeft--;
        timeText.text = "" + playerTimeLeft;
        CheckGameOver();
    }

    public void CheckGameOver()
    {
        if (playerTimeLeft <= 0)
        {
            gameOverImage.enabled = true;
            gameOverText.text = "Game over you reached level "+levelNumber;
            SoundManager.instance.StopMusic();
            if (!SoundManager.instance.extra.isPlaying) SoundManager.instance.PlaySfx(gameOverSound);
            Invoke("GameOver",2);
        }
    }

    void GameOver()
    {
        instance = null;
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    // true == successful reserve
    public Boolean reservePosition(Vector2 position)
    {
        lock (reservedPositons)
        {
            if (!reservedPositons.Contains(position))
            {
                reservedPositons.Add(position);
                return true;
            }
            return false;
        }
    }

    // true == successful free
    public void freePosition(Vector2 position)
    {
        lock (reservedPositons)
        {
            if (reservedPositons.Contains(position))
            {
                reservedPositons.Remove(position);
            }
        }
    }

    public void freeAllPositions()
    {
        lock (reservedPositons)
        {
            reservedPositons = new List<Vector2>();
        }
    }
}
