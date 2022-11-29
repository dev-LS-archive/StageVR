using UnityEngine;
using System.Collections;

public class Forklift : MonoBehaviour
{

    public AudioSource audioSrc;
    public AudioClip Forkact;
    public GameObject Dust_A;
    public GameObject Dust_B;


    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
    }

    public void Forksnd()
    {
        audioSrc.loop = false;
        audioSrc.clip = Forkact;
        audioSrc.Play();
    }

    public void dustA1()
    {
        Dust_A.SetActive(true);
    }

    public void dustA2()
    {
        Dust_A.SetActive(false);
    }

    public void dustB1()
    {
        Dust_B.SetActive(true);
    }

    public void dustB2()
    {
        Dust_B.SetActive(false);
    }

}
