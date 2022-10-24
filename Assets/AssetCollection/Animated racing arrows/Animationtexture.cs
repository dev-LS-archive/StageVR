using System;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class Animationtexture : MonoBehaviour 
{

    public Vector2 Animationspeed = new Vector2( -0.5f, 0.0f );


    Vector2 uvOffset = Vector2.zero;
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

    private void OnDisable()
    {
        print("Dis");
    }

    void LateUpdate() 
    {
		uvOffset += ( Animationspeed * Time.deltaTime );
        if( GetComponent<Renderer>().enabled )
        {
			GetComponent<Renderer>().materials[ 0 ].SetTextureOffset( BaseMap, uvOffset );
        }
    }
}
