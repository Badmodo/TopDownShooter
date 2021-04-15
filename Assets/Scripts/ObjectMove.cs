using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour
{
    public Transform[] target;
    public float speed;

    private int current;

    Quaternion lookAtSlowly(Transform t, Vector3 target, float turnSpeed)
    {

        //(t) is the gameobject transform
        //(target) is the location that (t) is going to look at
        //speed is the quickness of the rotation

        Vector3 relativePos = target - t.position;
        Quaternion toRotation = Quaternion.LookRotation(relativePos);
        return Quaternion.Lerp(t.rotation, toRotation, turnSpeed * Time.deltaTime);
    }

    //void FixedUpdate()
    //{
    //    transform.rotation = lookAtSlowly(transform, target[current].position, 1);
    //}

    private void LateUpdate()
    {
        if(transform.position != target[current].position)
        {
            Vector3 position = Vector3.MoveTowards(transform.position, target[current].position, speed * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(position);
            //transform.LookAt(target[current].position);
            transform.rotation = lookAtSlowly(transform, target[current].position, 2);
        }
        else
        {
            current = (current + 1) % target.Length;
        }
    }
}
