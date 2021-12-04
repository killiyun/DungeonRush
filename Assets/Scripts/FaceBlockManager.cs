using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FaceBlockManager : MonoBehaviour
{
    public float movePeriod; // int seconds
    public int moveDistance;
    public float moveSpeed;
    //public float moveDuration;

    private Vector2 bounds;
    private Vector2 movement;
    
    public GameManager GameManager;
    private BoardManager BoardManager;
    public BoxCollider2D collider;
    private Animator animator;
    public List<Vector2> nextPositions;
    private Rigidbody2D body;

    private void Awake()
    {
        
    } 
    
    void Start()
    {
        Physics2D.queriesStartInColliders = false;
        BoardManager = GameManager.instance.GetComponent<BoardManager>();
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        InvokeRepeating("Move", 4, movePeriod);
        bounds = collider.size;
    }

    void Move()
    {
        Vector2 currentPosition = transform.position;

        nextPositions = new List<Vector2>();
        Vector2 nextPosition = currentPosition;

        for (int i = 0; i < moveDistance; i++)
        {
            nextPosition = findPosition(nextPosition);
            nextPositions.Add(nextPosition);
            
        }

        // if alls well nextPositions holds the coords of where to move
        // the actual movement is then done in FixedUpdate()
        
        Vector3 failFind = new Vector3(-69, -69, -69);
        if (nextPositions.Contains(failFind)) {
            Debug.LogError("Could not move FaceBlock: " + this.name);
            nextPositions = null;
            return;
        }
    }

    Vector3 findPosition(Vector2 current)
    {
        List<Vector3> withinWalls = BoardManager.boardPositions.FindAll((pos) => 
            
            (pos.x == (current.x + bounds.x) && (current.y == pos.y || current.y == pos.y + bounds.x || current.y == pos.y - bounds.y )) || (pos.x == (current.x - bounds.x) && (current.y == pos.y || current.y == pos.y + bounds.y || current.y == pos.y - bounds.y)) || (pos.y == current.y && (current.x == pos.x + bounds.x || current.x == pos.x - bounds.x)) 
                
            );
        
        for (int i = 0; i < withinWalls.Count; i++)
        {
            int randomIndex = Random.Range (0, withinWalls.Count);
            Vector2 randomNextPosition = withinWalls[randomIndex];

            Vector2 direction = randomNextPosition - current;
            float distance = (randomNextPosition - current).magnitude;

            // doesnt account for size of face, need to do for each block within faceblock

            List<RaycastHit2D> hits = new List<RaycastHit2D>();

            for (int y=0; y < bounds.y; y++)
            {
                for (int x=0; x < bounds.x; x++)
                {
                    Vector2 position = (Vector2) transform.position + new Vector2(x, y);
                    hits.Add(Physics2D.Raycast(position, direction, distance));
                }
            }
            
            //RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, direction, distance);
            Boolean hitted = false;
            String whatWasHit = null;
            
            foreach (RaycastHit2D hit in hits)
            {
                //if (!(hit.collider == null || hit.collider.CompareTag("Player") || hit.collider.CompareTag("FaceBlock")))
                if(hit.collider == null) { continue; }
                if(hit.collider.gameObject.CompareTag("Spikey") || hit.collider.gameObject.CompareTag("Exit") || hit.collider.gameObject.CompareTag("Time"))
                {
                    hitted = true;
                    whatWasHit = hit.collider.gameObject.tag;
                }
            }
            
            //if (hit.collider == null || hit.collider.gameObject.CompareTag("Player"))
            if(hitted == false)
            {
                return randomNextPosition;
            } else if (withinWalls.Count == 0)
            {
                Debug.LogError("Could not find moveable position");
                return new Vector3(-69, -69, -69);
            }
            else
            {
                Debug.LogWarning("FaceBlock cannot hit: " + randomNextPosition.ToString() + " from " + current.ToString() + " as would hit: " + whatWasHit);// + hitCollider.gameObject.name);
                withinWalls.Remove(randomNextPosition);
            }
        }
         
        return new Vector3(-69, -69, -69);
    }

    void FixedUpdate()
    {
        if (nextPositions != null && nextPositions.Count > 0)
        {
            //Vector2 newPosition = (nextPositions[0] - (Vector2) transform.position).normalized;
            //body.MovePosition((Vector2) transform.position + newPosition * moveSpeed * Time.deltaTime);
            body.MovePosition(nextPositions[0]);
            nextPositions.RemoveAt(0);
        }
    }
}
