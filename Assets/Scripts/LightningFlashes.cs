using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningFlashes : MonoBehaviour
{
    public GameObject lightningLight;
    public AudioSource lightning;
    public AudioSource lightning2;

    private void Start()
    {
        StartCoroutine(LightningSetup());
    }

    IEnumerator LightningSetup()
    {
        yield return new WaitForSeconds(9f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(11f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(20f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(60f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(37f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(24f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(51f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(36f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(37f);
        StartCoroutine(LightningFlashe());
        yield return new WaitForSeconds(18f);
        StartCoroutine(LightningSetup());
    }

    IEnumerator LightningFlashe()
    {
        lightning.Play();
        lightning2.Play();
        lightningLight.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        lightningLight.SetActive(false);
        yield return new WaitForSeconds(0.4f);
        lightningLight.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        lightningLight.SetActive(false);
    }
}
