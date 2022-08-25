using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;

namespace LevelUP.Dial
{
    public class Rotator : MonoBehaviour
    {
        public Transform linkedDial;
        
        [SerializeField] private int snapRotationAmout = 25;
        private int snapRotationAmout_Origin;
        [SerializeField] private float angleTolerance;
        [SerializeField] private bool ViewCustomhandModel = false;
        [SerializeField] private GameObject RighthandModel;
        [SerializeField] private GameObject LefthandModel;

        private XRBaseInteractor interactor;
        private float startAngle;
        private bool requiresStartAngle = true;
        private bool shouldGetHandRotation = false;

        private XRBaseInteractable xRBase = null;

        public bool Reverse_rotation = false;
        private enum RotDirection { X, Y, Z };

        [SerializeField] private RotDirection rot_Direction;

        private enum RotAxis { X, Y, Z };

        [SerializeField] private RotAxis rot_Axis;

        private void Awake()
        {
            xRBase = GetComponent<XRBaseInteractable>();
        }
        protected virtual void OnEnable()
        {
            xRBase.selectEntered.AddListener(GrabbedBy);
            xRBase.selectExited.AddListener(GrabEnd);
        }

        protected virtual void OnDisable()
        {
            xRBase.selectEntered.RemoveListener(GrabbedBy);
            xRBase.selectExited.RemoveListener(GrabEnd);
        }
        private void Start()
        {
            snapRotationAmout_Origin = snapRotationAmout;
            switch (rot_Direction)
            {
                case RotDirection.X:
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ
                        | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    break;
                case RotDirection.Y:
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ
                    | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    break;
                case RotDirection.Z:
                    GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY
                    | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
                    break;
            }
        }
        public void GrabbedBy(SelectEnterEventArgs args)
        {
            interactor = GetComponent<XRGrabInteractable>().selectingInteractor;
            interactor.GetComponent<XRDirectInteractor>().hideControllerOnSelect = true;

            shouldGetHandRotation = true;
            startAngle = 0f;

            HandModelVisibility(true);
        }

        private void HandModelVisibility(bool visibilityState)
        {
            if (ViewCustomhandModel == true)
            {
                if (interactor.gameObject.GetComponent<XRController>().controllerNode == XRNode.RightHand)
                {
                    RighthandModel.SetActive(visibilityState);
                }
                else
                {
                    LefthandModel.SetActive(visibilityState);
                }
            }
        }

        public void GrabEnd(SelectExitEventArgs args)
        {
            shouldGetHandRotation = false;
            requiresStartAngle = true;
            HandModelVisibility(false);
        }

        void Update()
        {
            if (shouldGetHandRotation)
            {
                var rotationAngle = GetInteractorRotation(); //gets the current controller angle
                GetRotationDistance(rotationAngle);
            }
        }

        public float GetInteractorRotation()
        {
            var handRotation = interactor.GetComponent<Transform>().eulerAngles;
            if (rot_Axis == RotAxis.X)
                return handRotation.x;
            else if (rot_Axis == RotAxis.Y)
                return handRotation.y;
            else if (rot_Axis == RotAxis.Z)
                return handRotation.z;
            return 0;
        }

        private void GetRotationDistance(float currentAngle)
        {
            if (!requiresStartAngle)
            {
                var angleDifference = Mathf.Abs(startAngle - currentAngle);

                if (angleDifference > angleTolerance)
                {
                    if (angleDifference > 270f) //checking to see if the user has gone from 0-360 - a very tiny movement but will trigger the angletolerance
                    {
                        float angleCheck;

                        if (startAngle < currentAngle) //going anticlockwise
                        {
                            angleCheck = CheckAngle(currentAngle, startAngle);

                            if (angleCheck < angleTolerance)
                            {
                                return;
                            }
                            else
                            {
                                RotateDialAntiClockwise();
                                startAngle = currentAngle;
                            }
                        }
                        else if (startAngle > currentAngle) //going clockwise;
                        {
                            angleCheck = CheckAngle(currentAngle, startAngle);

                            if (angleCheck < angleTolerance)
                            {
                                return;
                            }
                            else
                            {
                                RotateDialClockwise();
                                startAngle = currentAngle;
                            }
                        }
                    }
                    else
                    {
                        if (startAngle < currentAngle)//clockwise
                        {
                            RotateDialClockwise();
                            startAngle = currentAngle;
                        }
                        else if (startAngle > currentAngle)
                        {
                            RotateDialAntiClockwise();
                            startAngle = currentAngle;
                        }
                    }
                }
            }
            else
            {
                requiresStartAngle = false;
                startAngle = currentAngle;
            }
        }

        private float CheckAngle(float currentAngle, float startAngle)
        {
            var checkAngleTravelled = (360f - currentAngle) + startAngle;
            return (checkAngleTravelled);
        }

        private void RotateDialClockwise()
        {
            Reverse_rot();
            switch (rot_Direction)
            {
                case RotDirection.X:
                    linkedDial.localEulerAngles = new Vector3(linkedDial.localEulerAngles.x - snapRotationAmout, linkedDial.localEulerAngles.y, linkedDial.localEulerAngles.z);
                    GetComponent<IDial>().DialChanged(linkedDial.localEulerAngles.x);
                    break;
                case RotDirection.Y:
                    linkedDial.localEulerAngles = new Vector3(linkedDial.localEulerAngles.x, linkedDial.localEulerAngles.y - snapRotationAmout, linkedDial.localEulerAngles.z);
                    GetComponent<IDial>().DialChanged(linkedDial.localEulerAngles.y);
                    break;
                case RotDirection.Z:
                    linkedDial.localEulerAngles = new Vector3(linkedDial.localEulerAngles.x, linkedDial.localEulerAngles.y, linkedDial.localEulerAngles.z - snapRotationAmout);
                    GetComponent<IDial>().DialChanged(linkedDial.localEulerAngles.z);
                    break;
            }
        }

        private void RotateDialAntiClockwise()
        {
            Reverse_rot();
            switch (rot_Direction)
            {
                case RotDirection.X:
                    linkedDial.localEulerAngles = new Vector3(linkedDial.localEulerAngles.x + snapRotationAmout, linkedDial.localEulerAngles.y, linkedDial.localEulerAngles.z);
                    GetComponent<IDial>().DialChanged(linkedDial.localEulerAngles.x);
                    break;
                case RotDirection.Y:
                    linkedDial.localEulerAngles = new Vector3(linkedDial.localEulerAngles.x, linkedDial.localEulerAngles.y + snapRotationAmout, linkedDial.localEulerAngles.z);
                    GetComponent<IDial>().DialChanged(linkedDial.localEulerAngles.y);
                    break;
                case RotDirection.Z:
                    linkedDial.localEulerAngles = new Vector3(linkedDial.localEulerAngles.x, linkedDial.localEulerAngles.y, linkedDial.localEulerAngles.z + snapRotationAmout);
                    GetComponent<IDial>().DialChanged(linkedDial.localEulerAngles.z);
                    break;
            }
        }
        private void Reverse_rot()
        {
            if (Reverse_rotation == true)
            {
                if(snapRotationAmout== snapRotationAmout_Origin)
                {
                    snapRotationAmout = -snapRotationAmout;
                }
            }
            else
            {
                if (snapRotationAmout != snapRotationAmout_Origin)
                {
                    snapRotationAmout = snapRotationAmout_Origin;
                }
            }
        }
    }
}
