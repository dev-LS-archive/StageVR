/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

See SampleFramework license.txt for license terms.  Unless required by applicable law
or agreed to in writing, the sample code is provided ?œAS IS??WITHOUT WARRANTIES OR
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific
language governing permissions and limitations under the license.

************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using System;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace OculusHand
{
    // Animated hand visuals for a user of a Touch controller.
    public class Hand : MonoBehaviour
    {
        #region Hand_variables
        private XRIDefaultInputActions _inputActions;
        public XRBaseInteractor m_BaseInteractor;
        public const string ANIM_LAYER_NAME_POINT = "Point Layer";
        public const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";
        public const string ANIM_PARAM_NAME_FLEX = "Flex";
        public const string ANIM_PARAM_NAME_POSE = "Pose";
        public const float THRESH_COLLISION_FLEX = 0.9f;

        public const float INPUT_RATE_CHANGE = 20.0f;

        public const float COLLIDER_SCALE_MIN = 0.01f;
        public const float COLLIDER_SCALE_MAX = 1.0f;
        public const float COLLIDER_SCALE_PER_SECOND = 1.0f;

        public const float TRIGGER_DEBOUNCE_TIME = 0.05f;
        public const float THUMB_DEBOUNCE_TIME = 0.15f;

        [SerializeField]
        private ActionBasedController m_controller;
        [SerializeField]
        private Animator m_animator = null;
        [SerializeField]
        private HandPose m_defaultGrabPose = null;
        [SerializeField]
        private bool HideHand_OnSelect = false;
        [SerializeField]
        private Transform HandObj;

        enum CollisionMode
        {
            CollisionOnFist,
            CollisionOnNonGrab_Obj,
            OffCollision
        }
        [SerializeField]
        private CollisionMode collisionMode = CollisionMode.OffCollision;
        private CollisionMode collisionMode_Original = CollisionMode.OffCollision;
        private Collider[] m_colliders = null;
        private bool m_collisionEnabled = false;
        //private OVRGrabber m_grabber;

        List<Renderer> m_showAfterInputFocusAcquired;

        private int m_animLayerIndexThumb = -1;
        private int m_animLayerIndexPoint = -1;
        private int m_animParamIndexFlex = -1;
        private int m_animParamIndexPose = -1;

        private bool m_isPointing = false;
        private bool m_isGivingThumbsUp = false;
        private float m_pointBlend = 0.0f;
        private float m_thumbsUpBlend = 0.0f;

        private bool m_restoreOnInputAcquired = false;
        #endregion
        
        #region XR_inputValue
        [SerializeField]
        private XRNode xRNode = XRNode.LeftHand;

        private List<InputDevice> devices = new List<InputDevice>();

        private InputDevice device;

        //[SerializeField]
        private bool triggerIsPressed;
        //[SerializeField]
        private bool gripIsPressed;
        //[SerializeField]
        private bool primaryTouched;
        //[SerializeField]
        private bool secondaryTouched;
        #endregion

        public GameObject LastSelect;
        void GetDevice()
        {
            InputDevices.GetDevicesAtXRNode(xRNode, devices);
            device = devices.FirstOrDefault();
        }

        private void Awake()
        {
            //m_BaseInteractor = GetComponent<XRBaseInteractor>();
            //m_controller = GetComponent<ActionBasedController>();
            _inputActions = new XRIDefaultInputActions();
        }
        

        private void OnEnable()
        {
            _inputActions.Enable();
            if (!device.isValid)
            {
                GetDevice();
            }
            collisionMode_Original = collisionMode;
            m_BaseInteractor.selectEntered.AddListener(GetSelectObj);
            m_BaseInteractor.selectEntered.AddListener(OffCollMode);
            m_BaseInteractor.selectExited.AddListener(OriginCollMode);
        }
        
        private void OnDisable()
        {
            _inputActions.Disable();
            m_BaseInteractor.selectEntered.RemoveListener(GetSelectObj);
            m_BaseInteractor.selectEntered.RemoveListener(OffCollMode);
            m_BaseInteractor.selectExited.RemoveListener(OriginCollMode);
        }
        public void GetSelectObj(SelectEnterEventArgs args)
        {
            LastSelect = m_BaseInteractor.selectTarget.gameObject;
        }
        
        public void OffCollMode(SelectEnterEventArgs args)
        {
            if(m_BaseInteractor.selectTarget != null)
                collisionMode = CollisionMode.OffCollision;
        }
        public void OriginCollMode(SelectExitEventArgs args)
        {
            Invoke("OrginMode", 0.5f);
        }
        void OrginMode()
        {
            collisionMode = collisionMode_Original;
        }
        private void Start()
        {
            Custiom_HandLR_Action();

            m_showAfterInputFocusAcquired = new List<Renderer>();

            // Collision starts disabled. We'll enable it for certain cases such as making a fist.
            m_colliders = HandObj.gameObject.GetComponentsInChildren<Collider>().Where(childCollider => !childCollider.isTrigger).ToArray();
            CollisionEnable(true);
            // Get animator layer indices by name, for later use switching between hand visuals
            m_animLayerIndexPoint = m_animator.GetLayerIndex(ANIM_LAYER_NAME_POINT);
            m_animLayerIndexThumb = m_animator.GetLayerIndex(ANIM_LAYER_NAME_THUMB);
            m_animParamIndexFlex = Animator.StringToHash(ANIM_PARAM_NAME_FLEX);
            m_animParamIndexPose = Animator.StringToHash(ANIM_PARAM_NAME_POSE);
        }

        private void Update()
        {
            if (!device.isValid)
            {
                GetDevice();
            }

            UpdateCapTouchStates();

            m_pointBlend = InputValueRateChange(m_isPointing, m_pointBlend);
            m_thumbsUpBlend = InputValueRateChange(m_isGivingThumbsUp, m_thumbsUpBlend);

            float flex = Capturing_grip_value();

            bool collisionEnabled = m_BaseInteractor.selectTarget == null;// && flex < THRESH_COLLISION_FLEX;
            CollisionEnable(collisionEnabled);

            UpdateAnimStates();
        }

        float Capturing_trigger_value()
        {
            // capturing grip value
            float triggerValue;
            InputFeatureUsage<float> triggerUsage = CommonUsages.trigger;

            if (device.TryGetFeatureValue(triggerUsage, out triggerValue))
            {
                //Debug.Log("trigger" + triggerValue);
                return triggerValue;
            }
            else
                return 0;
        }
        float Capturing_grip_value()
        {
            // capturing grip value
            float gripValue;
            InputFeatureUsage<float> gripUsage = CommonUsages.grip;

            if (device.TryGetFeatureValue(gripUsage, out gripValue))
            {

                //Debug.Log("grip" + gripValue);
                return gripValue;
            }
            else
                return 0;
        }
        bool Capturing_primaryTouch_value()
        {
            // capturing primary button press and release
            bool primaryTouchValue = false;
            InputFeatureUsage<bool> primaryTouchUsage = CommonUsages.primaryTouch;

            if (device.TryGetFeatureValue(primaryTouchUsage, out primaryTouchValue) && primaryTouchValue && !primaryTouched)
            {
                primaryTouched = true;
            }
            else if (!primaryTouchValue && primaryTouched)
            {
                primaryTouched = false;
            }
            return primaryTouched;
        }
        bool Capturing_secondaryTouch_value()
        {
            // capturing primary button press and release
            bool secondaryTouchValue = false;
            InputFeatureUsage<bool> secondaryTouchUsage = CommonUsages.secondaryTouch;

            if (device.TryGetFeatureValue(secondaryTouchUsage, out secondaryTouchValue) && secondaryTouchValue && !secondaryTouched)
            {
                secondaryTouched = true;
            }
            else if (!secondaryTouchValue && secondaryTouched)
            {
                secondaryTouched = false;
            }
            return secondaryTouched;
        }

        // Just checking the state of the index and thumb cap touch sensors, but with a little bit of
        // debouncing.
        private void UpdateCapTouchStates()
        {
            if (xRNode == XRNode.LeftHand)
            {
                m_isPointing = !_inputActions.XRILeftHandInteraction.Activate.IsPressed();
                m_isGivingThumbsUp = !(Capturing_primaryTouch_value() || Capturing_secondaryTouch_value());
            }
            else if (xRNode == XRNode.RightHand)
            {
                m_isPointing = !_inputActions.XRIRightHandInteraction.Activate.IsPressed();
                m_isGivingThumbsUp = !(Capturing_primaryTouch_value() || Capturing_secondaryTouch_value());
            }
        }

        private void LateUpdate()
        {
            // Hand's collision grows over a short amount of time on enable, rather than snapping to on, to help somewhat with interpenetration issues.
            if (m_collisionEnabled && m_collisionScaleCurrent + Mathf.Epsilon < COLLIDER_SCALE_MAX)
            {
                m_collisionScaleCurrent = Mathf.Min(COLLIDER_SCALE_MAX, m_collisionScaleCurrent + Time.deltaTime * COLLIDER_SCALE_PER_SECOND);
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    collider.transform.localScale = new Vector3(m_collisionScaleCurrent, m_collisionScaleCurrent, m_collisionScaleCurrent);
                }
            }
        }

        // Custom Action for Hand L&R-Trigger&Grip 
        private void Custiom_HandLR_Action()
        {
            #region ActiveAction
            if (xRNode == XRNode.LeftHand)
            {
                _inputActions.XRILeftHandInteraction.Activate.performed += ctx =>
                {
                    //Debug.Log("--- XRILeftHand.Activate.performed ---");
                    triggerIsPressed = true;
                };
                _inputActions.XRILeftHandInteraction.Activate.canceled += ctx =>
                {
                    //Debug.Log("--- XRILeftHand.Activate.canceled ---");
                    triggerIsPressed = false;
                };
            }
            else if (xRNode == XRNode.RightHand)
            {
                _inputActions.XRIRightHandInteraction.Activate.performed += ctx =>
                {
                    //Debug.Log("--- XRIRightHand.Activate.performed ---");
                    triggerIsPressed = true;
                };
                _inputActions.XRIRightHandInteraction.Activate.canceled += ctx =>
                {
                    //Debug.Log("--- XRIRIghtHand.Activate.canceled ---");
                    triggerIsPressed = false;
                };
            }
            #endregion
            #region SelectAction
            if (xRNode == XRNode.LeftHand)
            {
                _inputActions.XRILeftHandInteraction.Select.performed += ctx =>
                {
                    //Debug.Log("--- XRILeftHand.Select.performed ---");
                    gripIsPressed = true;
                };
                _inputActions.XRILeftHandInteraction.Select.canceled += ctx =>
                {
                    //Debug.Log("--- XRILeftHand.Select.canceled ---");
                    gripIsPressed = false;
                };
            }
            else if (xRNode == XRNode.RightHand)
            {
                _inputActions.XRIRightHandInteraction.Select.performed += ctx =>
                {
                    //Debug.Log("--- XRIRightHand.Select.performed ---");
                    gripIsPressed = true;
                };
                _inputActions.XRIRightHandInteraction.Select.canceled += ctx =>
                {
                    //Debug.Log("--- XRIRIghtHand.Select.canceled ---");
                    gripIsPressed = false;
                };
            }
            #endregion
        }

        // Simple Dash support. Just hide the hands.
        private void OnInputFocusLost()
        {
            if (gameObject.activeInHierarchy)
            {
                m_showAfterInputFocusAcquired.Clear();
                Renderer[] renderers = GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; ++i)
                {
                    if (renderers[i].enabled)
                    {
                        renderers[i].enabled = false;
                        m_showAfterInputFocusAcquired.Add(renderers[i]);
                    }
                }

                CollisionEnable(true);

                m_restoreOnInputAcquired = true;
            }
        }

        private void OnInputFocusAcquired()
        {
            if (m_restoreOnInputAcquired)
            {
                for (int i = 0; i < m_showAfterInputFocusAcquired.Count; ++i)
                {
                    if (m_showAfterInputFocusAcquired[i])
                    {
                        m_showAfterInputFocusAcquired[i].enabled = true;
                    }
                }
                m_showAfterInputFocusAcquired.Clear();

                // Update function will update this flag appropriately. Do not set it to a potentially incorrect value here.
                CollisionEnable(false);

                m_restoreOnInputAcquired = false;
            }
        }

        private float InputValueRateChange(bool isDown, float value)
        {
            float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
            float sign = isDown ? 1.0f : -1.0f;
            return Mathf.Clamp01(value + rateDelta * sign);
        }

        bool ActHide = false;
        private void UpdateAnimStates()
        {
            bool grabbing = m_BaseInteractor.selectTarget != null;
            HandPose grabPose = m_defaultGrabPose;
            if (grabbing)
            {
                HandPose customPose = m_BaseInteractor.selectTarget.GetComponent<HandPose>();
                if (customPose != null) grabPose = customPose;
                //Debug.Log(grabPose.PoseId);

                if (HideHand_OnSelect == true && ActHide == false)
                {
                    ActHide = true;
                    OnInputFocusLost();
                }
            }
            else
            {
                if (HideHand_OnSelect == true && ActHide == true)
                {
                    ActHide = false;
                    OnInputFocusAcquired();
                }
            }
            // Pose
            HandPoseId handPoseId = grabPose.PoseId;
            m_animator.SetInteger(m_animParamIndexPose, (int)handPoseId);

            // Flex
            // blend between open hand and fully closed fist
            float flex = Capturing_grip_value();
            m_animator.SetFloat(m_animParamIndexFlex, flex);

            // Point
            bool canPoint = !grabbing || grabPose.AllowPointing;
            //float point = canPoint ? m_pointBlend : 1 - Capturing_trigger_value();
            float point = canPoint ? 1 - Capturing_trigger_value() : 0.0f;
            m_animator.SetLayerWeight(m_animLayerIndexPoint, point);

            // Thumbs up
            bool canThumbsUp = !grabbing || grabPose.AllowThumbsUp;
            float thumbsUp = canThumbsUp ? m_thumbsUpBlend : 0.0f;
            //Debug.Log("thumbsUp: " + thumbsUp);
            //float thumbsUp = m_thumbsUpBlend;
            m_animator.SetLayerWeight(m_animLayerIndexThumb, thumbsUp);

            float pinch = Capturing_trigger_value();
            m_animator.SetFloat("Pinch", pinch);
        }

        private float m_collisionScaleCurrent = 0.0f;

        private void CollisionEnable(bool enabled)
        {
            //Debug.Log(xRNode.ToString() + "/" + collisionMode + "/" + m_collisionEnabled + "/" + enabled);
            if (m_collisionEnabled == enabled)
            {
                return;
            }
            if (collisionMode == CollisionMode.CollisionOnFist == true)
            {
                enabled = !enabled;
            }
            else if (collisionMode == CollisionMode.OffCollision)
            {
                enabled = false;
            }

            m_collisionEnabled = enabled;

            if (enabled)
            {
                m_collisionScaleCurrent = COLLIDER_SCALE_MIN;
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    collider.transform.localScale = new Vector3(COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN);
                    collider.enabled = true;
                }
            }
            else
            {
                m_collisionScaleCurrent = COLLIDER_SCALE_MAX;
                for (int i = 0; i < m_colliders.Length; ++i)
                {
                    Collider collider = m_colliders[i];
                    collider.enabled = false;
                    collider.transform.localScale = new Vector3(COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN, COLLIDER_SCALE_MIN);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (m_BaseInteractor.selectTarget == null)
            {
                if(other.TryGetComponent(out XRGrabInteractable interactable))
                {
                    collisionMode = CollisionMode.OffCollision;
                }
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (m_BaseInteractor.selectTarget == null)
            {
                if (other.TryGetComponent(out XRGrabInteractable interactable))
                {
                    collisionMode = collisionMode_Original;
                }
            }
        }
    }
}
