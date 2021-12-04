using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Transactions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class SmartFaceBlockManager : MonoBehaviour
{ 
        public float movePeriod; // int seconds
        public int moveDistance;
        public float moveSpeed;
        
        private float startTime;
        private float distance;
        public float moveTime;

        private Vector2 bounds;
        private Vector2 movement;
        private Vector2 currentPosition;
        
        private GameManager gameManager;
        //private BoardManager BoardManager;
        private BoxCollider2D collider;
        private Animator animator;
        public List<Vector2> nextPositions;
        private Rigidbody2D body;

        private GameObject[] allWalls;
        private GameObject[] allFaces;
        private GameObject[] immovables;

        public AudioClip sound1;
        public AudioClip sound2;
        public AudioClip sound3;
        
    void Start()
    {
        Physics2D.queriesStartInColliders = false;
        gameManager = GameManager.instance;
        collider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        InvokeRepeating("Move", 2, movePeriod);
        bounds = collider.size;
        
        allWalls = GameObject.FindGameObjectsWithTag("Wall");
        allFaces = GameObject.FindGameObjectsWithTag("FaceBlock");
        
        immovables = new GameObject[allWalls.Length + allFaces.Length];
        Array.Copy(allWalls, immovables, allWalls.Length);
        Array.Copy(allFaces, 0, immovables, allWalls.Length, allFaces.Length);

        startTime = Time.time;
    }
    
    void Move()
    {
        currentPosition = transform.position;

        if (!gameManager.reservePosition(currentPosition))
        {
            Debug.LogError("Unable to reserve origin position");
            return;
        }
        
        SoundManager.instance.BlockSound(sound1, sound2, sound3);

        nextPositions = new List<Vector2>();
        Vector2 nextPosition = currentPosition;

        for (int i = 0; i < moveDistance; i++)
        {
            nextPosition = findPosition(nextPosition);
            nextPositions.Add(nextPosition);
        }
        
        Vector3 failFind = new Vector2(-69, -69);
        if (nextPositions.Contains(failFind)) {
            Debug.LogError("Could not move FaceBlock: " + this.name);
            //nextPositions = null;
            return;
        }
        
        StartCoroutine(SmoothMovement (nextPositions.FirstOrDefault()));
        //freePositions(bounds, nextPositions.FirstOrDefault());
    }

    // used for debugging
    /*
    private void FixedUpdate()
    {
        if (nextPositions.Count > 0)
        {
            transform.position = nextPositions.FirstOrDefault();
            nextPositions.Remove(nextPositions.FirstOrDefault());
        }
    }
    */

    Vector2 findPosition(Vector2 current)
    {

        Vector2 errorVec = new Vector2(-69, -69);
        List<Vector2> toTry = new List<Vector2>();

        toTry.Add(new Vector2(current.x + bounds.x, current.y));
        toTry.Add(new Vector2(current.x + bounds.x, current.y + bounds.y));
        toTry.Add(new Vector2(current.x + bounds.x, current.y - bounds.y));
        toTry.Add(new Vector2(current.x - bounds.x, current.y));
        toTry.Add(new Vector2(current.x - bounds.x, current.y + bounds.y ));
        toTry.Add(new Vector2(current.x - bounds.x, current.y - bounds.y));
        toTry.Add(new Vector2(current.x, current.y + bounds.y));
        toTry.Add(new Vector2(current.x, current.y - bounds.y));

        Vector2 result = tryDeez(current, toTry);

        if(result == errorVec)
        {
            toTry = new List<Vector2>();
            
            toTry.Add(new Vector2(current.x + 1, current.y));
            toTry.Add(new Vector2(current.x + 1, current.y + 1));
            toTry.Add(new Vector2(current.x + 1, current.y - 1));
            toTry.Add(new Vector2(current.x - 1, current.y));
            toTry.Add(new Vector2(current.x - 1, current.y + 1 ));
            toTry.Add(new Vector2(current.x - 1, current.y - 1));
            toTry.Add(new Vector2(current.x, current.y + 1));
            toTry.Add(new Vector2(current.x, current.y - 1));
            
            result = tryDeez(current, toTry);

            if (result == errorVec)
            {
                return errorVec;
            }
            else
            {
                return result;
            }
        }
        else
        {
            return result;
        }
        
        /*
        search:
        for (int i = 0; i < toTry.Count; i++)
        {
            int randomIndex = Random.Range (0, toTry.Count);
            Vector2 randomNextPosition = toTry[randomIndex];

            Vector2 direction = randomNextPosition - current;
            distance = (randomNextPosition - current).magnitude;

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(bounds.x - 0.01f, bounds.y - 0.01f), 0f, direction, distance);

            //RaycastHit2D playerCrushHit = Physics2D.BoxCast(transform.position,
                //new Vector2(bounds.x - 0.1f, bounds.y - 0.1f), 0f, direction,
                //distance + GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>().bounds.size
                    //.magnitude);


            if (hit.collider == null)
            {
                if (bounds.x > 1 && bounds.y > 1)
                {
                    if (tryReserveBigBoi(bounds, randomNextPosition))
                    {
                        return randomNextPosition;
                    }
                }
                else if (tryReserveBigBoi(bounds, randomNextPosition))
                {
                    return randomNextPosition;
                }
                toTry.Remove(randomNextPosition);
                continue;
            } // && playerCrushHit.collider == null

            if (hit.collider.gameObject.CompareTag("Spikey") || hit.collider.gameObject.CompareTag("Exit") ||
                hit.collider.gameObject.CompareTag("Time") || hit.collider.gameObject.CompareTag("Wall") ||
                hit.collider.gameObject.CompareTag("FaceBlock"))
            {
                toTry.Remove(randomNextPosition);
                continue;
            }

            Vector2 minCheck = Vector2Int.RoundToInt( direction + (Vector2) hit.collider.gameObject.transform.position ) ;
            Vector2 maxCheck = Vector2Int.RoundToInt( direction + (Vector2) hit.collider.gameObject.transform.position +
                                                                                     (Vector2) hit.collider.gameObject.GetComponent<Collider2D>().bounds.size ) ;

            if (hit.collider.gameObject.CompareTag("Player") && (isImmovableHere(minCheck) || isImmovableHere(maxCheck)) )
            {
                toTry.Remove(randomNextPosition);
                continue;
            }
            
        }
        */
    }
    
    // Must end before consecutive Move() is called !!!
    private IEnumerator SmoothMovement (Vector2 end)
    {
        float remainingDistance = ((Vector2)transform.position - end).magnitude;
        float speed = remainingDistance / moveTime;

        while(remainingDistance > 0.01)
        {
            Vector2 newPostion = Vector2.MoveTowards(transform.position, end, Time.deltaTime * speed);
            body.MovePosition(newPostion);
            remainingDistance = ((Vector2)transform.position - end).magnitude;
            yield return null;
        }
        
        freePositions(bounds, currentPosition);
        body.MovePosition(end);
        freePositions(bounds, end);
    }

    //checks for walls and faceblocks
    bool isImmovableHere(Vector2 position)
    {
        foreach (GameObject imm in immovables)
        {
            if ((Vector2)imm.transform.position == position)
            {
                return true;
            }
        }

        return false;
    }

    Boolean tryReserveBigBoi(Vector2 bounds, Vector2 position)
    {
        Boolean reserved = true;
        for (int j = 0; j < bounds.x; j++)
        {
            for (int k = 0; k < bounds.y; k++)
            {
                if (!gameManager.GetComponent<GameManager>().reservePosition(new Vector2(position.x + j,
                    position.y + k)))
                {
                    reserved = false;
                }
            }
        }
        
        if (reserved == false)
        {
            freePositions(bounds, position);
        }

        return reserved;
    }

    void freePositions(Vector2 bounds, Vector2 position)
    {
        for (int j = 0; j < bounds.x; j++)
        {
            for (int k = 0; k < bounds.y; k++)
            {
                gameManager.GetComponent<GameManager>().freePosition(new Vector2(position.x + j,
                    position.y + k));
            }
        }
    }

    Vector2 tryDeez(Vector2 current, List<Vector2> toTry)
    {
        for (int i = 0; i < toTry.Count; i++)
        {
            int randomIndex = Random.Range (0, toTry.Count);
            Vector2 randomNextPosition = toTry[randomIndex];

            Vector2 direction = randomNextPosition - current;
            distance = (randomNextPosition - current).magnitude;

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, new Vector2(bounds.x - 0.01f, bounds.y - 0.01f), 0f, direction, distance);

            //RaycastHit2D playerCrushHit = Physics2D.BoxCast(transform.position,
                //new Vector2(bounds.x - 0.1f, bounds.y - 0.1f), 0f, direction,
                //distance + GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>().bounds.size
                    //.magnitude);


            if (hit.collider == null)
            {
                if (bounds.x > 1 && bounds.y > 1)
                {
                    if (tryReserveBigBoi(bounds, randomNextPosition))
                    {
                        return randomNextPosition;
                    }
                }
                else if (tryReserveBigBoi(bounds, randomNextPosition))
                {
                    return randomNextPosition;
                }
                toTry.Remove(randomNextPosition);
                continue;
            } // && playerCrushHit.collider == null

            if (hit.collider.gameObject.CompareTag("Spikey") || hit.collider.gameObject.CompareTag("Exit") ||
                hit.collider.gameObject.CompareTag("Time") || hit.collider.gameObject.CompareTag("Wall") ||
                hit.collider.gameObject.CompareTag("FaceBlock"))
            {
                toTry.Remove(randomNextPosition);
                continue;
            }

            Vector2 minCheck = Vector2Int.RoundToInt( direction + (Vector2) hit.collider.gameObject.transform.position ) ;
            Vector2 maxCheck = Vector2Int.RoundToInt( direction + (Vector2) hit.collider.gameObject.transform.position +
                                                                                     (Vector2) hit.collider.gameObject.GetComponent<Collider2D>().bounds.size ) ;

            if (hit.collider.gameObject.CompareTag("Player") && (isImmovableHere(minCheck) || isImmovableHere(maxCheck)) )
            {
                toTry.Remove(randomNextPosition);
                continue;
            }
        }

        return new Vector2(-69, -69);
    }
    
}