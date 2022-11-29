using UnityEngine;
using System.Collections;

public class AudioPlay : MonoBehaviour 
{
    public void playClip()
    {
        gameObject.GetComponent<AudioSource>().Play();
    }

	// Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	    
	}
}
