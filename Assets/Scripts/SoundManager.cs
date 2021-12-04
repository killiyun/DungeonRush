using UnityEngine;
using System.Collections;

    public class SoundManager : MonoBehaviour 
    {
        public AudioSource spikes;
        public AudioSource blocks;
        public AudioSource player;
        public AudioSource music;
        public AudioSource extra;
        public static SoundManager instance = null;
        public float lowPitchRange = .95f;
        public float highPitchRange = 1.05f;

        public float sfxVolume = 1f;
        public float musicVolume = 1f;

        void Awake ()
        {
            if (instance == null)
                instance = this;
            else if (instance != this)
                Destroy (gameObject);

            DontDestroyOnLoad (gameObject);
        }

        public void RandomizeSfx (AudioSource source, params AudioClip[] clips)
        {
            int randomIndex = Random.Range(0, clips.Length);

            float randomPitch = Random.Range(lowPitchRange, highPitchRange);

            source.pitch = randomPitch;

            source.clip = clips[randomIndex];

            source.Play();
        }

        public void StartMusic()
        {
            music.Play();
            SetSFXVolume(sfxVolume);
        }

        public void StopMusic()
        {
            music.Stop();
            blocks.volume = 0f;
            player.volume = 0f;
            spikes.volume = 0f;
        }

        public void PlaySfx(AudioClip sound)
        {
            extra.clip = sound;
            extra.Play();
        }

        public void SpikeSound(params AudioClip[] clips)
        {
            RandomizeSfx(spikes, clips);
        }

        public void BlockSound(params AudioClip[] clips)
        {
            RandomizeSfx(blocks, clips);
        }

        public void PlayerSound(params AudioClip[] clips)
        {
            RandomizeSfx(player, clips);
        }

        public void SetMusicVolume(float volume)
        {
            music.volume = volume;
            musicVolume = volume;
        }

        public void SetSFXVolume(float volume)
        {
            blocks.volume = volume;
            player.volume = volume;
            spikes.volume = volume;
            extra.volume = volume;
            sfxVolume = volume;
        }
    }