using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxEffect;

    private float startPos;

    void Start()
    {
        startPos = transform.position.x;
    }

    void Update()
    {
        float dist = cameraTransform.position.x * parallaxEffect;
        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);
    }
}
