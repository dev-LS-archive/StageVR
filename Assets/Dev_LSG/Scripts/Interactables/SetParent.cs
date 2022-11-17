using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParent : MonoBehaviour
{
    public Transform parent;

    public void SetParentFunction()
    {
        transform.SetParent(parent);
    }
}
