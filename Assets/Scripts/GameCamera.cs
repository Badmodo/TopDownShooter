using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour
{
    private Vector3 cameraTarget;

    public Transform target;
    //private Transform target;

    //void Start()
    //{
    //    target = GameObject.FindGameObjectWithTag("Player").transform;
    //}

    void Update()
    {
        cameraTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
        transform.position = Vector3.Lerp(transform.position, cameraTarget, Time.deltaTime * 8);
    }

    ////TODO: make it so the camera dosnt jump to the original position
    //public IEnumerator Shake(float duration, float magnitude)
    //{
    //    Vector3 originalPosition = transform.localPosition;

    //    float elapsed = 0f;

    //    while (elapsed < duration)
    //    {
    //        float x = Random.Range(-0.5f, 0.5f) * magnitude;
    //        float z = Random.Range(-0.5f, 0.5f) * magnitude;

    //        transform.localPosition = new Vector3(x, originalPosition.y, z);

    //        elapsed += Time.deltaTime;

    //        yield return null;
    //    }

    //    transform.localPosition = originalPosition;
    //}
}
