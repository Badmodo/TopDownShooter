using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{
    public enum GunType { Semi, Burst, Auto};
    public GunType gunType;

    public float gunID;
    public float rpm;
    public float damage = 1;

    public int totalAmmo = 40;
    public int ammoMag = 10;

    public GameObject muzzleFlashGO;
    public Transform spawn;
    public Transform shellEjectionPort;
    public Rigidbody shell;
    public LayerMask collisionMask;
    public AudioSource GunFire;
    public AudioSource hitTarget;

    [HideInInspector]
    public GameGUI gUI;

    private float secondsBetweenShots;
    private float nextPossibleShootTime;
    [HideInInspector]
    public int currentAmmoInMag;
    private bool reloading;

    private LineRenderer tracer;

    private void Start()
    {
        secondsBetweenShots = 60 / rpm;
        if(GetComponent<LineRenderer>())
        {
            tracer = GetComponent<LineRenderer>();
        }

        currentAmmoInMag = ammoMag;

        if (gUI)
        {
            gUI.SetAmmoInfo(totalAmmo, currentAmmoInMag);
        }
    }

    public void Shoot()
    {
        if (CanShoot())
        {
            Ray ray = new Ray(spawn.position, spawn.forward);
            RaycastHit hit;

            float shotDistance = 20f;

            if (Physics.Raycast(ray, out hit, shotDistance, collisionMask))
            {
                shotDistance = hit.distance;

                if(hit.collider.GetComponent<Character>())
                {
                    hitTarget.Play();
                    hit.collider.GetComponent<Character>().TakeDamage(damage);
                }
            }

            nextPossibleShootTime = Time.time + secondsBetweenShots;

            currentAmmoInMag--;

            if(gUI)
            {
                gUI.SetAmmoInfo(totalAmmo, currentAmmoInMag);
            }

            ////test ammo going down
            //Debug.Log(currentAmmoInMag + "      " + totalAmmo);

            GunFire.Play();

            if(tracer)
            {
                StartCoroutine("RenderTracer", ray.direction * shotDistance);
            }

            Rigidbody newShell = Instantiate(shell, shellEjectionPort.position, Quaternion.identity) as Rigidbody;
            newShell.AddForce(shellEjectionPort.forward * Random.Range(150f, 200f) + spawn.forward * Random.Range(-10, 10));

            ////for testing
            //Debug.DrawRay(ray.origin, ray.direction * shotDistance, Color.red, 1);
        }
    }

    IEnumerator MuzzleFlash()
    {
        if (currentAmmoInMag != 0)
        {
            muzzleFlashGO.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            muzzleFlashGO.SetActive(false);
        }
    }

    IEnumerator RenderTracer(Vector3 hitPoint)
    {
        StartCoroutine(MuzzleFlash());

        tracer.enabled = true;
        tracer.SetPosition(0, spawn.position);
        tracer.SetPosition(1, spawn.position + hitPoint);
        yield return new WaitForSeconds(0.03f); 
        tracer.enabled = false;
    }

    public void ShootContinuous()
    {
        if(gunType == GunType.Auto)
        {
            StartCoroutine(MuzzleFlash());

            Shoot();
        }
    }

    public bool CanShoot()
    {
        bool canShoot = true;

        if (Time.time < nextPossibleShootTime)
        {
            canShoot = false;
        }

        if(currentAmmoInMag == 0)
        {
            canShoot = false;
        }

        if(reloading)
        {
            canShoot = false;
        }

        return canShoot;
    }

    //if we have ammo left and mag is not full we can reload
    public bool Reload()
    {
        if(totalAmmo != 0 && currentAmmoInMag != ammoMag)
        {
            reloading = true;
            return true;
        }
        return false;
    }

    public void FinishReload()
    {
        reloading = false;
        currentAmmoInMag = ammoMag;
        totalAmmo -= ammoMag;
        if(totalAmmo < 0)
        {
            currentAmmoInMag += totalAmmo;
            totalAmmo = 0;
        }

        if (gUI)
        {
            gUI.SetAmmoInfo(totalAmmo, currentAmmoInMag);
        }
    }
}
