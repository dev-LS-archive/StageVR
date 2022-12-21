using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class AICarController : MonoBehaviour
{
    public WheelCollider[] wheelColliders = new WheelCollider[4];
    public GameObject[] m_WheelMeshes = new GameObject[4];
    private IEnumerator coroutine;
    public Transform jeep;
    //public Transform vtarget;
    public float maxSteerAngle;
    public Rigidbody mvjeep;
    public Transform centerofMass;
    public float TorqueSpeed;
    //public float downwardforce, height, limitpush, uppush;
    // public Vector3 raycastStart;
    // RaycastHit hit;
    //  public float raylength;
    //  public float obsSteerangle;
    // public MovingTarget mt;
   // public float Speedmai;
    public float maxSpeed;
   // public Text speedtxt;
    // public bool applysteer = false;
    //   public bool steernow;
    //  public bool shootray = true;
    void Start()
    {
       // mvjeep.AddForce(0, 0, -uppush * Time.deltaTime);
       // mvjeep.AddForce(0, 0, limitpush * Time.deltaTime);
       // mvjeep.centerOfMass = new Vector3(0, height, 0);
        //  mvjeep.centerOfMass = centerofMass.localPosition;
        //coroutine = WaitAndShoot();
        //StartCoroutine(coroutine);
    }


    void Update()
    {

        //Speedmai = mvjeep.velocity.magnitude * 3.6f;
        //speedtxt.text = Speedmai.ToString("F");

        //if (Speedmai >= maxSpeed)
        //{
            //  Debug.Log("reached");
          //  mvjeep.AddRelativeForce(0, 0, -limitpush * Time.deltaTime);
       // }

        ApplySteer();
        RotateWheels();
        wheelColliders[0].motorTorque = TorqueSpeed;
        wheelColliders[1].motorTorque = TorqueSpeed;
        wheelColliders[2].motorTorque = TorqueSpeed;
        wheelColliders[3].motorTorque = TorqueSpeed;

        //   raycastStart = transform.position + new Vector3(0,2,0);
        //  Debug.DrawLine(raycastStart, new Vector3(0,0,1));
        /*  if(Physics.Raycast(raycastStart, transform.forward, out hit, raylength))
          {

              Debug.Log("drawing");

              {
                  if (hit.collider.gameObject.tag == "moveright")
                  {
                      wheelColliders[0].steerAngle = obsSteerangle;
                      wheelColliders[1].steerAngle = obsSteerangle;
                      // steernow = true;



                      // Debug.Log("justhitmoverightobstcle");
                  }
                  // 
              }


          }*/


    }


    //private IEnumerator WaitAndShoot()
    //{

     //   yield return new WaitForSeconds(1);


       // yield return new WaitForSeconds(1);


       // yield return new WaitForSeconds(1);



   // }


    private void ApplySteer()
    {
        // if(applysteer == true)
        //  {
       // Vector3 relativeVector = jeep.transform.InverseTransformPoint(vtarget.position);

       // float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
       // wheelColliders[0].steerAngle = newSteer;
       // wheelColliders[1].steerAngle = newSteer;

       // mvjeep.AddForce(0, downwardforce * Time.deltaTime, 0);
        //  }


    }

    private void RotateWheels()
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            wheelColliders[i].GetWorldPose(out position, out quat);
            m_WheelMeshes[i].transform.position = position;
            m_WheelMeshes[i].transform.rotation = quat;
        }
    }
}
