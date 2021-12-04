using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Numerics;
using Unity.Mathematics;
using UnityEditor;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEngine.UIElements;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class BoardManager : MonoBehaviour
{

    [Serializable]
    public class Count{
        public int minimum;
        public int maximum;

        public Count (int min, int max){
            minimum = min;
            maximum = max;
        }
    }

    public int xSize;
    public int ySize;

    private int difficulty;
    private int level;

    public GameObject exit;
    public GameObject player;
    public GameObject spikes;
    public GameObject time;

    public GameObject anyFloor;
    public GameObject[] floorTiles;

    public GameObject[] tWallTiles;
    public GameObject[] bWallTiles;
    public GameObject[] lWallTiles;
    public GameObject[] rWallTiles;

    // indexes: tl, tr, bl, br
    public GameObject[] corners;

    public GameObject[] faceBlocks;
    public GameObject[] faceBlocksBig;

    public GameObject[] ExitFeatures;
    public GameObject[] TimeFeatures;
    public GameObject[] WallFeatures;

    public int spikesNum;
    public int faceBlocksNum;
    public int faceBlocksBigNum;

    private Transform boardHolder;
    private Transform wallHolder;
    public List<Vector2> gridPositions { get; private set; } // = new List<Vector3>();
    public List<Vector2> usablePositions;
    public List<Vector3> boardPositions; // isnt used, still left in so that FaceBlockManager compiles

    public Vector2 playerSpawnCoords;
    public Vector2 exitSpawnCoords;
    public GameObject chosenExit;

    public int totalLevelPositions;

    private GameObject board;
    private GameObject wall;
    
    //makes gridPositions hold all positions within walls
    void InitialiseList(){
        gridPositions = new List<Vector2>();

        for(int x=1; x<xSize-1; x++){
            for(int y=1; y<ySize-1; y++){
                gridPositions.Add(new Vector3(x,y,0f));

            }
        }

        usablePositions =
            gridPositions.FindAll(pos => pos.x >= 1 && pos.y >= 1 && pos.x <= xSize - 2 && pos.y <= ySize - 2);
        totalLevelPositions = usablePositions.Count;
    }

    void BoardSetup(){
        board  = new GameObject("Board");
        wall = new GameObject("Wall");
        
        wall.AddComponent<CompositeCollider2D>();
        Rigidbody2D rb = wall.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        boardHolder = board.transform;
        wallHolder = wall.transform;

        for(int x=0; x<xSize; x++){
            for(int y=0; y<ySize; y++){

                //default floor
                GameObject toInstantiate = anyFloor;//floorTiles[Random.Range (0, floorTiles.Length)];

                //check corners

                //bottom left
                if(x == 0 && y == 0){
                    toInstantiate = corners[2];

                //bottom right
                } else if (x == (xSize - 1) && y == 0){
                    toInstantiate = corners[3];

                //top left
                } else if (x == 0 && y == (ySize - 1)){
                    toInstantiate = corners[0];
                
                //top right
                } else if (x == ( xSize - 1 ) && y == ( ySize - 1  ) ){
                    toInstantiate = corners[1];

                // now the walls

                } else if (y == ySize - 1){
                    toInstantiate = tWallTiles[Random.Range (0, tWallTiles.Length)];
                } else if (x == xSize - 1){
                    toInstantiate = rWallTiles[Random.Range (0, rWallTiles.Length)];
                } else if (y == 0){
                    toInstantiate = bWallTiles[Random.Range (0, bWallTiles.Length)];
                } else if (x == 0){
                    toInstantiate = lWallTiles[Random.Range (0, lWallTiles.Length)];
                }

                GameObject instance = Instantiate (toInstantiate, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
                
                if ( instance.CompareTag("Wall")) { instance.transform.SetParent(wallHolder); }
                else { instance.transform.SetParent(boardHolder); }
            }
        }
        
        wall.transform.SetParent(boardHolder);
    }

    /*
    Vector3 RandomPosition()
    {
        int randomIndex = Random.Range(0, gridPositions.Count);
        Vector3 randomPosition = gridPositions[randomIndex];
        gridPositions.RemoveAt(randomIndex);
        return randomPosition;
    }
    */

    Vector2 RandomSizePosition(Vector2 bounds, GameObject obj)
    {
        Boolean found = false;
        Boolean forgivenessMode = false;
        List<Vector2> matches = new List<Vector2>();
        List<Vector2> exitMatches = new List<Vector2>();
        if(obj.CompareTag("Player") && usablePositions.Count < Math.Ceiling(bounds.x * bounds.y)){ return new Vector3(-69, -69, -69); }
        if (usablePositions.Count < (bounds.x * bounds.y)) { return new Vector3(-69, -69, -69); }
        
        foreach (Vector2 vec in usablePositions)
        {
            for (int y = 0; y < bounds.y; y++)
            {
                for (int x = 0; x < bounds.x; x++)
                {
                    if (!usablePositions.Contains(vec + new Vector2(x, y)))
                    {
                        goto unusable;
                    }
                    
                    /*
                    // ensure constant >= 0.75 board separation
                    if (obj.CompareTag("Player"))
                    {
                        //exit already spawned
                        if (!(exitSpawnCoords.x == -69 && exitSpawnCoords.y == -69) && !forgivenessMode )
                        {
                            // check too close
                            if ((vec - exitSpawnCoords).magnitude < (0.8 * (xSize - 2) * (ySize - 2)))
                            {
                                goto unusable;
                            }
                        }
                        
                        //exit has not spawned, place away from center
                        if (exitSpawnCoords.x == -69 && exitSpawnCoords.y == -69 && !forgivenessMode)
                        {
                            Boolean up = vec.y + ((ySize-2) * 0.75) < (ySize-2);
                            Boolean down = vec.y - ((ySize-2) * 0.75) > 1;
                            Boolean left = vec.x - ((xSize-2) * 0.75) > 1;
                            Boolean right = vec.x + ((xSize-2) * 0.75) < (xSize-2);
                            
                            //move player away from center
                            if (!(up && down && left && right))
                            {
                                goto unusable;
                            }
                            
                        }
                    }
                    if (obj.CompareTag("Exit"))
                    {
                        //player spawned
                        if (!(playerSpawnCoords.x == -69 && playerSpawnCoords.y == -69))
                        {
                            //feature
                            if (ExitFeatures.Contains(obj))
                            {
                                if (!forgivenessMode)
                                {
                                    if ((vec - playerSpawnCoords).magnitude < (0.8 * (xSize - 2) * (ySize - 2)))
                                    {
                                        goto unusable;
                                    }
                                }
                            }
                            
                            // not feature, check if fits
                            if ( !ExitFeatures.Contains(chosenExit) && (vec - playerSpawnCoords).magnitude < (0.8 * (xSize - 2) * (ySize - 2)) )
                            {
                                goto unusable;
                            }
                        }
                        
                        //player not spawned
                        if (playerSpawnCoords.x == -69 && playerSpawnCoords.y == -69 && !forgivenessMode)
                        {
                            // not feature, place away
                            Boolean up = vec.y + ((ySize-2) * 0.75) < (ySize-2);
                            Boolean down = vec.y - ((ySize-2) * 0.75) > 1;
                            Boolean left = vec.x - ((xSize-2) * 0.75) > 1;
                            Boolean right = vec.x + ((xSize-2) * 0.75) < (xSize-2);
                        
                            //move player away from center
                            if (!(up && down && left && right) ) //(ExitFeatures.Contains(obj) && forgivenessMode)
                            {
                                goto unusable;
                            }
                        }
                    }
                    */
                }
            }
            
            matches.Add(vec);
            
            unusable:
                continue;
        }

        if(matches.Count <= 0) { return new Vector3(-69, -69, -69); }
        
        if (obj.CompareTag("Exit"))
        {
            List<float> dists = new List<float>();
            foreach (Vector2 match in matches)
            {
                dists.Add((match - playerSpawnCoords).magnitude);
            }
            
            List<Vector2> sources = matches;

            int dist = difficulty + 1;// + (int) obj.GetComponent<Collider2D>().bounds.size.magnitude;
            if(dist > 4) { dist = 4 ;}
            
            for (int i = 0; i < dist; i++)
            {
                float currMax = dists.Max();
                int pos = dists.IndexOf(currMax);
                
                exitMatches.Add(sources[pos]);
                dists.RemoveAt(pos);
                sources.RemoveAt(pos);
            }
            
            /*
            for (int i = 0; i < difficulty; i++)
            {
                exitMatches.Add( sources [ dists.IndexOf( dists.Max() ) ] );
            }
            */

            if (exitMatches.Count > 0)
            {
                matches = exitMatches;
            }
        }

        int randomIndex = Random.Range (0, matches.Count-1);
        Vector2 randomPosition = matches[randomIndex];

        for (int y = 0; y < bounds.y; y++)
        {
            for (int x = 0; x < bounds.x; x++)
            {
                usablePositions.Remove(new Vector2(randomPosition.x + x, randomPosition.y + y));
            }
        }

        return randomPosition;
    }

    // should place on top of whatever's already there
    /*
    void RandomPlace(GameObject obj){
        Vector3 randomPosition = RandomPosition();
        Instantiate(obj, randomPosition, Quaternion.identity);
    }
    */

    Boolean RandomCarefulPlace(GameObject obj)
    {
        Vector2 bounds = obj.GetComponent<BoxCollider2D>().size;

        if (obj.CompareTag("Exit"))
        {
            chosenExit = obj;
        }
        
        Vector2 randomPosition = RandomSizePosition(bounds, obj);
        Vector2 rotatedPosition = new Vector2(-69, -69);

        /*if (randomPosition.x == -69 && randomPosition.y == -69 && bounds.x != bounds.y)
        {
            GameObject rotatedObj = obj;
            rotatedObj.transform.Rotate(new Vector3(0, 0, -90));
            rotatedObj.transform.Translate(new Vector3(0, bounds.y - 1, 0));

            Vector2 rotatedBounds = new Vector2(bounds.y, bounds.x);
            rotatedPosition = RandomSizePosition(rotatedBounds, rotatedObj);
        }*/
        
        if (randomPosition.x == -69 && randomPosition.y == -69 && rotatedPosition.x == -69 && rotatedPosition.y == -69)
        {
            Debug.LogError("Could not find position for object: " + obj.name);
            return false;
        }

        if (obj.CompareTag("Player"))
        {
            playerSpawnCoords = randomPosition;
        }
        
        Instantiate(obj, randomPosition, Quaternion.identity);
        return true;
    }

    void PopulateBoard()
    {
        List<GameObject> toSpawn = new List<GameObject>();
        toSpawn.Add(player);
        
        switch (difficulty)
        {
            case 0:
                toSpawn.Add(randomFaceorSpike());
                toSpawn.Add(maybeTime());
                toSpawn.Add(exit);
                break;
            
            case 1:
                //feature exit or no
                if (Random.Range(0, 1) == 0)
                {
                    toSpawn.Add(randomListChoice( ExitFeatures.Where( obj => obj.GetComponent<BoxCollider2D>().size.x <= (xSize - 3) && obj.GetComponent<BoxCollider2D>().size.y <= (ySize - 3) ) as List<GameObject> ));
                    toSpawn.Add(randomFaceorSpike());
                    toSpawn.Add(maybeTime());
                    break;
                }
                
                int whichFaces = Random.Range(0, 3);
                if (whichFaces <= 1)
                {
                    toSpawn.Add(randomArrayChoice(faceBlocksBig));
                    if (whichFaces == 0)
                    {
                        toSpawn.Add(randomArrayChoice(faceBlocks));
                        toSpawn.Add(maybeTime());
                        break;
                    }
                    toSpawn.Add(spikes);
                    toSpawn.Add(spikes);
                    toSpawn.Add(maybeTime());
                    break;
                }

                if (whichFaces == 2)
                {
                    toSpawn.Add(randomArrayChoice(faceBlocks));
                    toSpawn.Add(randomArrayChoice(faceBlocks));
                    toSpawn.Add(maybeTime());
                    break;
                }
                
                toSpawn.Add(randomArrayChoice(faceBlocks));
                toSpawn.Add(spikes);
                toSpawn.Add(spikes);
                toSpawn.Add(maybeTime());
                break;
        }

        foreach (GameObject spawn in toSpawn)
        {
            RandomCarefulPlace(spawn);
        }
    }

    public void SetupLevel(int lvl)
    {
        level = lvl;
        difficulty = lvl / 5;
        
        playerSpawnCoords = new Vector2(-69, -69);
        exitSpawnCoords = new Vector2(-69, -69);

        GameManager.instance.GetComponent<GameManager>().freeAllPositions();
        SetBoardSize(difficulty);
        InitialiseList(); // generates gridPositions
        BoardSetup();
        PopulateLevel();
        //testPopulate(level);
        //snapLevel();
    }

    void PopulateLevel()
    {
        int originalPos = totalLevelPositions;
        int remainingPos = totalLevelPositions;
        int stuffsNum = ((xSize - 2) / 2) + (difficulty * 2);

        Boolean exitType = Random.Range(0, 1) == 0;
        
        Boolean timeExists = Random.Range(0, difficulty + 1) == 0;
        Boolean timeType = Random.Range(0, difficulty + 1) == 0;
        
        //xSize - 2 means fits on the board, xsize - 3 means leave a gap for the player to spawn into
        List<GameObject> uFittableExits = ExitFeatures.ToList().FindAll(exit => exit.GetComponent<BoxCollider2D>().size.x < xSize - 3 && exit.GetComponent<BoxCollider2D>().size.y < ySize - 3);
        List<GameObject> fittableExits = uFittableExits.OrderBy(x => Random.value ).ToList();
        
        Boolean playerExists = RandomCarefulPlace(player);
        
        if (exitType && fittableExits.Count > 0)
        {
            GameObject exitChoice = new GameObject();
            foreach (GameObject cExit in fittableExits)
            {
                if (RandomCarefulPlace(cExit))
                {
                    exitChoice = cExit;
                    remainingPos -= (int) exitChoice.GetComponent<BoxCollider2D>().size.magnitude;
                    break;
                }
            }
        }
        else
        {
            if (RandomCarefulPlace(exit))
            {
                remainingPos--;
            }
        }

        if (timeExists)
        {
            if (timeType)
            {
                //guess what will fit
                List<GameObject> fittableTimes =
                    TimeFeatures.ToList().FindAll(time => time.GetComponent<BoxCollider2D>().size.magnitude < remainingPos);
                GameObject actualFit = null;

                foreach (GameObject toFit in fittableTimes)
                {
                    if (RandomCarefulPlace(toFit)) { actualFit = toFit; break; }
                }

                if (actualFit == null)
                {
                    Boolean succ = RandomCarefulPlace(time);
                }
            }
            else
            {
                Boolean succ = RandomCarefulPlace(time);
            }
        }

        //does not include spikes or time
        List<GameObject[]> chooseFrom = new List<GameObject[]>();
        
        if (difficulty >= 0)
        {
            chooseFrom.Add(faceBlocks);
        }
        // time == feature
        if (level % 2 == 0)
        {
            chooseFrom.Add(TimeFeatures);
        }
        // choose also from walls
        if (difficulty > 1)
        {
            chooseFrom.Add(WallFeatures);
        }
        // choose from big faces
        if (difficulty > 0)
        {
            chooseFrom.Add(faceBlocksBig);
        }
        
        List<GameObject> objs = new List<GameObject>();
        List<GameObject[]> shuffledChoices = chooseFrom.OrderBy(x => Random.value ).ToList(); 
        int timeNum = 0;
        
        for (int i=0; i<stuffsNum; i++)
        {
            bool chosen = false;
            // choice represents: time + spikes + shuffled choices
            int choice = Random.Range( 0, shuffledChoices.Count );
            
            if ( choice == 0 && Random.Range(0,timeNum+1) == 0)
            {
                GameObject mbTime = maybeTime();
                if (mbTime != null)
                {
                    objs.Add(time);
                    timeNum++;
                }
                else
                {
                    //enable at discretion
                    //choice = Random.Range( 1, shuffledChoices.Count );
                }
            }
            else if (choice == 1)
            {
                objs.Add(spikes);
            }
            else
            {
                objs.Add(randomArrayChoice(randomListListChoice(shuffledChoices)));
            }
        }

        List<GameObject> timeInObjs = new List<GameObject>();
        int timeFs = 0;
        
        foreach (GameObject time in TimeFeatures)
        {
            if (objs.Contains(time))
            {
                timeInObjs.Add(time);
                timeFs++;
            }
        }

        foreach (GameObject tim in timeInObjs)
        {
            if (remainingPos > tim.GetComponent<BoxCollider2D>().size.magnitude && timeInObjs.Count > 0)
            {
                if (RandomCarefulPlace(tim))
                {
                    remainingPos -= (int) tim.GetComponent<BoxCollider2D>().size.magnitude;
                    timeInObjs.Remove(tim);
                    objs.Remove(tim);
                }
                else
                {
                    break;
                }
            }
        }

        foreach (GameObject rest in objs)
        {
            if (remainingPos > rest.GetComponent<BoxCollider2D>().size.magnitude && objs.Count > 0)
            {
                if (RandomCarefulPlace(rest))
                {
                    remainingPos -= (int) rest.GetComponent<BoxCollider2D>().size.magnitude;
                    //timeInObjs.Remove(rest);
                    //objs.Remove(rest);
                }
                else
                {
                    break;
                }
            }
        }
    
        /*
        int[] multis = new int[] {4, 3, 2, 1};
        int moreAttempts = difficulty
        
        more:
        if ((originalPos / remainingPos) > Mathf.Pow(0.6f,(float) multis[difficulty]))
        {
            GameObject gamer = randomArrayChoice(randomListListChoice(shuffledChoices));
            if (RandomCarefulPlace(gamer))
            {
                remainingPos -= (int) gamer.GetComponent<BoxCollider2D>().size.magnitude;
            }
            else if (Random.Range(0, timeNum + 1) == 0)
            {
                if (RandomCarefulPlace(time))
                {
                    remainingPos--;
                }
            }
            else
            {
                if (RandomCarefulPlace(spikes))
                {
                    remainingPos--;
                }
            }
        }

        if ((originalPos / remainingPos) > Mathf.Pow(0.6f, (float) multis[difficulty]))
        {
            goto more;
        }
        */
    }

    GameObject randomFaceorSpike()
    {
        Boolean choice = Random.Range(0,1) == 0;
        if(choice){return spikes;}
        return randomArrayChoice(faceBlocks);
    }

    GameObject maybeTime()
    {
        Boolean odds = Random.Range(0,difficulty+1) == 0;
        if(odds){ return time; }
        return null;
    }
    
    //biased toward Time: 50% time, 50% not time
    GameObject randomFaceorSpikeorTime()
    {
        Boolean choice = Random.Range(0,1) == 0;
        if(choice){return time;}
        return randomFaceorSpike();
    }

    GameObject randomArrayChoice(GameObject[] list)
    {
        if(list.Length == 0 || list == null) { return null; }
        return list[Random.Range(0, list.Length)];
    }

    GameObject randomListChoice(List<GameObject> list)
    {
        if(list.Count == 0 || list == null) { return null; }
        return list[Random.Range(0, list.Count)];
    }

    GameObject[] randomListListChoice(List<GameObject[]> list)
    {
        if(list.Count == 0 || list == null) { return null; }
        return list[Random.Range(0, list.Count)];
    }

    void SetBoardSize(int diff)
    {
        if (diff > 4) { return; }
        xSize = 6 + (diff * 2);
        ySize = 6 + (diff * 2);
        GameObject.Find("Main Camera").transform.position = new Vector3((xSize -1)/2, (ySize -1)/2, -10f);
    }

    void snapLevel()
    {
        foreach (Transform inWall in wallHolder)
        {
            inWall.gameObject.transform.position = new Vector3(Mathf.Round(inWall.gameObject.transform.position.x), Mathf.Round(inWall.gameObject.transform.position.y), Mathf.Round(inWall.gameObject.transform.position.z));
        }

        foreach (Transform inBoard in boardHolder)
        {
            inBoard.gameObject.transform.position = new Vector3(Mathf.Round(inBoard.gameObject.transform.position.x), Mathf.Round(inBoard.gameObject.transform.position.y), Mathf.Round(inBoard.gameObject.transform.position.z));
        }
        
    }

    public void Reload()
    {
        SetupLevel(1);
    }

    void testPopulate(int level)
    {
        
        RandomCarefulPlace(randomArrayChoice(faceBlocks));
        RandomCarefulPlace(player);
        RandomCarefulPlace(exit);
        RandomCarefulPlace(randomArrayChoice(faceBlocks));
        
    }
}
