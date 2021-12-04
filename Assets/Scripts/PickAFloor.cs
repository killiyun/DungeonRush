using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEngine;

public class PickAFloor : MonoBehaviour
{

    public List<GameObject> Floors;
    public GameObject chosenFloor;

    public SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        renderer.enabled = false;
        int randomIndex = Random.Range (0,Floors.Count);
        GameObject chosenFloor = Floors[randomIndex];
        
        GameObject newFloor = Instantiate (chosenFloor, new Vector3(transform.position.x, transform.position.y, 0f), quaternion.identity) as GameObject;
        newFloor.transform.parent = gameObject.transform;
    }
}
