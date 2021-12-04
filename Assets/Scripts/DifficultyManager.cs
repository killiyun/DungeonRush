using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager instance = null;
    public int difficulty = 1;
    void Awake()
    {
        if(instance == null){
            instance = this;
        } else if (instance != this){
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }

}
