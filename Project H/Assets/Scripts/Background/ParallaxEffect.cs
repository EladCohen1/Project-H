using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;
    private Vector2 startingPosition;
    private float startingZ;

    // Updates every frame
    private Vector2 camMoveSinceStart => (Vector2)cam.transform.position - startingPosition;
    private float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;
    private float clippingPlane => (cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane));
    private float parallaxFactor => Mathf.Abs(zDistanceFromTarget) / clippingPlane;
    // Start is called before the first frame update
    void Start()
    {
        startingPosition = (Vector2)transform.position;
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newPos = startingPosition + camMoveSinceStart * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startingZ);
    }
}
