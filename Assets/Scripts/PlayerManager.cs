using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public float moveSpeed;
    public Rigidbody2D rb;
    private Vector2 movement;
    
    public Animator animator;
    public GameManager GameManager;

    public AudioClip exit1;
    public AudioClip exit2;
    public AudioClip exit3;
    public AudioClip pickup;


    public float restartLevelDelay = 1f;

    public int pickupHeal;
    
    // Update is called once per frame
    void Update()
    {
        ProcessInputs();
    }

    void FixedUpdate(){
        rb.velocity = new Vector2(movement.x * moveSpeed, movement.y * moveSpeed);
    }

    void ProcessInputs(){
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }
    
    /*
    void Move(){
        rb.velocity = new Vector2(movement.x * moveSpeed, movement.y * moveSpeed);
        //rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
    */

    private void OnTriggerEnter2D (Collider2D other)
    {
        if (other.CompareTag("Exit"))
        {
           NextLevel();
           SoundManager.instance.PlayerSound(exit1, exit2, exit3);
        } else if (other.CompareTag("Spikey"))
        {
            other.gameObject.GetComponent<SpikeBehaviour>().PlayerEnter();
        } else if (other.CompareTag("Time"))
        {
            GameManager.instance.Restore(pickupHeal);
            other.gameObject.SetActive(false);
            SoundManager.instance.PlayerSound(pickup);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Spikey"))
        {
            other.gameObject.GetComponent<SpikeBehaviour>().PlayerExit();
        }
    }
    
    private void NextLevel(){
        GameManager.instance.NextLevel();
        //GameManager.instance.reloadBoard();
        //SceneManager.LoadScene( SceneManager.GetActiveScene().name );
    }
}
