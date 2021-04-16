using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMove : MonoBehaviour
{
    public Transform[] target;
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;

    private int current;

    Quaternion lookAtSlowly(Vector3 targetPos)
    {
        //(t) is the gameobject transform
        //(target) is the location that (t) is going to look at
        //speed is the quickness of the rotation

        Vector3 relativePos = targetPos - transform.position;
        Quaternion toRotation = Quaternion.LookRotation(relativePos, Vector3.up);
        return Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if(Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            Vector3 pos = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            GetComponent<Rigidbody>().MovePosition(pos);

            transform.rotation = lookAtSlowly(targetPos);
            transform.position = pos;
        }
        else
        {
            current = (current + 1) % target.Length;
        }
    }

    Vector3 targetPos => target[current].position;

    //private void OnGUI()
    //{
    //    GUI.Label(new Rect(20, 20, 200, 20), current.ToString());
    //}
}
