
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HandPresencePhysics : MonoBehaviour
{
    public Transform target;
    private Rigidbody rb;
    public Renderer nonPhysicalHand;
    public float showNonPhysicalHandDistance = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > showNonPhysicalHandDistance)
        {
            nonPhysicalHand.enabled = true;
        }
        else
            nonPhysicalHand.enabled = false;
    }
    private void FixedUpdate()
    {
        rb.velocity = (target.position - transform.position) / Time.fixedDeltaTime;
        Quaternion rotationDifference = target.rotation * Quaternion.Inverse(transform.rotation);
        rotationDifference.ToAngleAxis(out float angelInDegree, out Vector3 rotationAxis);

        Vector3 rotationDifferenceInDegree = angelInDegree * rotationAxis;

        rb.angularVelocity = (rotationDifferenceInDegree * Mathf.Deg2Rad / Time.fixedDeltaTime);
    }
}
