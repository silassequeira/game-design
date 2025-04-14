using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject player;
    Vector3 position;
    void Start()
    {
        position=transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        position.x=player.transform.position.x;
        transform.position=position;
    }
}
