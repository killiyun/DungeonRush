using UnityEngine;

public class SpikeBehaviour : MonoBehaviour {

    public Sprite spikesOpen;
    public Sprite spikesHalf;
    public Sprite spikesClosed;
    
    public int spikeDamage;
    public float openTime;
    public float halfTime;
    public float closedTime;

    public float timeSinceChange;
    public char state; // 'o' for open, 'h' for half open, 'c' for closed

    public AudioClip spikes1;
    public AudioClip spikes2;
    public AudioClip spikes3;
    public AudioClip spikesIn;

    private SpriteRenderer renderer;
    public bool damaging;

    public bool playerInside;

    public bool damageCooldown = true;

    // Start is called before the first frame update
    void Start() {
        renderer = GetComponent<SpriteRenderer>();
        if (state == 'o')
        {
            renderer.sprite = spikesOpen;
        } else if (state == 'h')
        {
            renderer.sprite = spikesHalf;
        } else if (state == 'c')
        {
            renderer.sprite = spikesClosed;
        }
        damaging = false;
        playerInside = false;
    }

    void FixedUpdate() {
        timeSinceChange += Time.deltaTime;
        
        if (damaging && playerInside && damageCooldown)
        {
            GameManager.instance.Damage(spikeDamage);
            damageCooldown = false;
            Invoke("damageCooldownReset", 0.25f);
        }

        if (state == 'o') {
            if (timeSinceChange >= openTime)
            {
                renderer.sprite = spikesHalf;
                timeSinceChange = 0f;
                state = 'h';
            }
        }
        else if (state == 'h') {
            if (timeSinceChange >= halfTime) 
            {
                renderer.sprite = spikesClosed;
                timeSinceChange = 0f;
                state = 'c';
                damaging = true;
                SoundManager.instance.SpikeSound(spikes1, spikes2, spikes3);
            }
        }
        else if (state == 'c') {
            if (timeSinceChange >= closedTime) 
            {
                renderer.sprite = spikesOpen;
                timeSinceChange = 0f;
                state = 'o';
                damaging = false;
                SoundManager.instance.SpikeSound(spikesIn);
            }
        }
    }
    
    public void PlayerEnter()
    {
        playerInside = true;
    }

    public void PlayerExit()
    {
        playerInside = false;
    }

    void damageCooldownReset()
    {
        damageCooldown = true;
    }

}