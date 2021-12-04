using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    private void Awake()
    {
        GameObject.Find("MusicSlider").GetComponent<Slider>().value = SoundManager.instance.musicVolume;
        GameObject.Find("SFXSlider").GetComponent<Slider>().value = SoundManager.instance.sfxVolume;
    }

    public void SetMusic(float volume)
    {
        SoundManager.instance.SetMusicVolume(volume);
    }

    public void SetSFX(float volume)
    {
        SoundManager.instance.SetSFXVolume(volume);
    }
}
