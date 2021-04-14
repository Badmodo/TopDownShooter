using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingLights : MonoBehaviour
{
    public GameObject redLight;
    public GameObject blueLight;

    void Start()
    {
        StartCoroutine(FlashingLight());
    }

    IEnumerator FlashingLight()
    {
        redLight.SetActive(true);
        blueLight.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        redLight.SetActive(false);
        blueLight.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(FlashingLight());
    }
}
