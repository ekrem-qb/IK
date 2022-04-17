using System.Collections;
using EpicToonFX;
using UnityEngine;
using UnityEngine.UI;

//-------------------------------------------------------------
//--APR Player
//--APRController (Main Player Controller)
//
//--Unity Asset Store - Version 1.0
//
//--By The Famous Mouse
//
//--Twitter @FamousMouse_Dev
//--Youtube TheFamouseMouse
//-------------------------------------------------------------

namespace ARP.APR.Scripts
{
    public class APRController : MonoBehaviour
    {
        //Active Ragdoll Player parts
        public GameObject
            Root,
            Body,
            Head,
            UpperRightArm,
            LowerRightArm,
            UpperLeftArm,
            LowerLeftArm,
            UpperRightLeg,
            LowerRightLeg,
            UpperLeftLeg,
            LowerLeftLeg,
            RightFoot,
            LeftFoot;

        //Rigidbody Hands
        public Rigidbody RightHand, LeftHand;

        //Center of mass point
        public Transform COMP;

        [Header("Input on this player")]
        //Enable controls
        public bool useControls = true;

        public Joystick joystick;

        [Header("Player Input Axis")]
        //Player Axis controls
        public string forwardBackward = "Vertical";

        public string leftRight = "Horizontal";
        public KeyCode keyJump = KeyCode.Space;
        public KeyCode reachLeft = KeyCode.Keypad1;
        public KeyCode reachRight = KeyCode.Keypad3;

        [Header("Player Input KeyCodes")]
        //Player KeyCode controls
        public string punchLeft = "q";

        public string punchRight = "e";

        [Header("The Layer Only This Player Is On")]
        //Player layer name
        public string thisPlayerLayer = "Player_1";

        //Movement
        public float moveSpeed = 10;
        public float maxJumpStrength = 18;
        public float maxJumpRequiredSeconds = 2;
        public HoldButton jumpButton;
        public Slider jumpStrengthIndicator;

        [Header("Balance Properties")]
        //Balance
        public bool autoGetUpWhenPossible = true;

        public ParticleSystem stunningParticle;
        public float getUpDelay = 2;
        public float fallDamage = 5;

        public bool useStepPrediction = true;
        public float balanceHeight = 2.5f;
        public float balanceStrength = 5000f;
        public float coreStrength = 1500f;

        public float limbStrength = 500f;

        //Walking
        public float StepDuration = 0.2f;
        public float StepHeight = 1.7f;
        public float FeetMountForce = 25f;

        [Header("Reach Properties")]
        //Reach
        public float reachSensitivity = 25f;

        public float armReachStiffness = 2000f;

        [Header("Actions")]
        //Punch
        public bool canBeKnockoutByImpact = true;

        public float requiredForceToBeKO = 20f;
        public bool canPunch = true;
        public float punchForce = 15f;

        [Header("Audio")]
        //Impact sounds
        public float ImpactForce = 10f;

        public AudioClip[] Impacts;
        public AudioClip[] Hits;
        public AudioSource SoundSource;

        public float
            MouseYAxisArms,
            MouseYAxisBody;

        [ReadOnly] public bool
            walkForward,
            walkBackward,
            stepRight,
            stepLeft,
            alertLegRight,
            alertLegLeft,
            isBalanced = true,
            isGettingUp,
            isRagdoll,
            isKeyDown,
            moveAxisUsed,
            reachLeftAxisUsed,
            reachRightAxisUsed;

        [ReadOnly] public bool
            isInAir,
            punchingRight,
            punchingLeft;

        [HideInInspector] public Quaternion
            upperRightArmTarget,
            lowerRightArmTarget,
            upperLeftArmTarget,
            lowerLeftArmTarget,
            rightHandTarget,
            leftHandTarget;

        [Header("Player Editor Debug Mode")] public bool editorDebugMode;

        [HideInInspector] public HealthManager healthManager;

        //Joint Drives on & off
        private JointDrive
            _balanceOn;

        private Camera _cam;
        private Vector3 _centerOfMassPoint;
        private Vector3 _direction;

        //Original pose target rotation
        private Quaternion
            _headTarget,
            _bodyTarget,
            _upperRightLegTarget,
            _lowerRightLegTarget,
            _upperLeftLegTarget,
            _lowerLeftLegTarget;

        private float
            _mouseXAxisArms;

        private ETFXRotation[] _stunningParticleRotations;

        //Hidden variables
        private float
            _timer,
            _stepRTimer,
            _stepLTimer;

        private WeaponManager _weaponManager;

        public JointDrive
            PoseOn,
            CoreStiffness,
            ReachStiffness,
            DriveOff;

        private void Awake()
        {
            PlayerSetup();
            DeactivateRagdoll();
            _weaponManager = COMP.GetComponent<WeaponManager>();
            healthManager = this.GetComponent<HealthManager>();
            if (stunningParticle)
            {
                _stunningParticleRotations = stunningParticle.GetComponentsInChildren<ETFXRotation>();
            }

            if (jumpStrengthIndicator)
            {
                jumpStrengthIndicator.minValue = 0;
                jumpStrengthIndicator.maxValue = maxJumpStrength;
            }

            if (jumpButton)
            {
                jumpButton.onPress.AddListener(JumpPress);
                jumpButton.onHold.AddListener(JumpHold);
                jumpButton.onRelease.AddListener(JumpRelease);
            }

            // Root.GetComponent<Rigidbody>().maxAngularVelocity = Mathf.Infinity;
        }

        private void Update()
        {
            if (useControls)
            {
                if (!isInAir)
                {
                    if (isBalanced)
                    {
                        PlayerMovement();
                        Jumping();
                    }

                    if (canPunch)
                    {
                        PlayerPunch();
                    }
                }

                PlayerReach();
            }

            if (useStepPrediction)
            {
                if (isBalanced)
                {
                    StepPrediction();
                    CenterOfMass();
                }
            }
            else
            {
                ResetWalkCycle();
            }
        }

        private void FixedUpdate()
        {
            if (!isRagdoll && isBalanced)
            {
                Walking();
            }

            GroundCheck();
            CenterOfMass();
        }

        private void OnDrawGizmos()
        {
            if (editorDebugMode)
            {
                Debug.DrawRay(Root.transform.position, -Root.transform.up * balanceHeight, Color.green);

                if (useStepPrediction)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(COMP.position, 0.3f);
                }
            }
        }

        private void PlayerSetup()
        {
            _cam = Camera.main;

            //Setup joint drives
            _balanceOn = new JointDrive
            {
                positionSpring = balanceStrength,
                positionDamper = 0,
                maximumForce = Mathf.Infinity
            };

            PoseOn = new JointDrive
            {
                positionSpring = limbStrength,
                positionDamper = 0,
                maximumForce = Mathf.Infinity
            };

            CoreStiffness = new JointDrive
            {
                positionSpring = coreStrength,
                positionDamper = 0,
                maximumForce = Mathf.Infinity
            };

            ReachStiffness = new JointDrive
            {
                positionSpring = armReachStiffness,
                positionDamper = 0,
                maximumForce = Mathf.Infinity
            };

            DriveOff = new JointDrive
            {
                positionSpring = 25,
                positionDamper = 0,
                maximumForce = Mathf.Infinity
            };

            //Setup original pose for joint drives
            _bodyTarget = Body.GetComponent<ConfigurableJoint>().targetRotation;
            _headTarget = Head.GetComponent<ConfigurableJoint>().targetRotation;
            upperRightArmTarget = UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation;
            lowerRightArmTarget = LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation;
            upperLeftArmTarget = UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation;
            lowerLeftArmTarget = LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation;
            rightHandTarget = RightHand.GetComponent<ConfigurableJoint>().targetRotation;
            leftHandTarget = LeftHand.GetComponent<ConfigurableJoint>().targetRotation;
            _upperRightLegTarget = UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation;
            _lowerRightLegTarget = LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation;
            _upperLeftLegTarget = UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation;
            _lowerLeftLegTarget = LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation;
        }

        private void GroundCheck()
        {
            Ray ray = new Ray(Root.transform.position, -Root.transform.up);

            //Balance when ground is detected
            if (Physics.Raycast(ray, out RaycastHit hit, balanceHeight))
            {
                if (hit.transform.root != this.transform.root)
                {
                    if (!isBalanced && Root.GetComponent<Rigidbody>().velocity.magnitude < 1f)
                    {
                        if (!hit.transform.GetComponent<Trampoline>())
                        {
                            StartCoroutine(GetUp());
                        }
                    }
                }
            }
            else
            {
                isBalanced = false;
                isInAir = true;
            }

            //Balance on/off
            if (isBalanced && isRagdoll)
            {
                DeactivateRagdoll();
            }
            else if (!isBalanced && !isRagdoll)
            {
                ActivateRagdoll();
            }
        }

        private IEnumerator GetUp()
        {
            if (autoGetUpWhenPossible)
            {
                if (!isGettingUp)
                {
                    isGettingUp = true;
                    if (stunningParticle)
                    {
                        stunningParticle.Play();
                        for (int i = 0; i < _stunningParticleRotations.Length; i++)
                        {
                            _stunningParticleRotations[i].enabled = true;
                        }
                    }

                    yield return new WaitForSeconds(getUpDelay);

                    isBalanced = true;
                    DeactivateRagdoll();

                    isGettingUp = false;
                    if (stunningParticle)
                    {
                        stunningParticle.Stop();
                        for (int i = 0; i < _stunningParticleRotations.Length; i++)
                        {
                            _stunningParticleRotations[i].enabled = false;
                        }
                    }
                }
            }
        }

        private void StepPrediction()
        {
            //Reset variables when balanced
            if (!walkForward && !walkBackward)
            {
                stepRight = false;
                stepLeft = false;
                _stepRTimer = 0;
                _stepLTimer = 0;
                alertLegRight = false;
                alertLegLeft = false;
            }

            //Check direction to walk when off balance
            //Backwards
            if (COMP.position.z < RightFoot.transform.position.z && COMP.position.z < LeftFoot.transform.position.z)
            {
                walkBackward = true;
            }
            else
            {
                if (!isKeyDown)
                {
                    walkBackward = false;
                }
            }

            //Forward
            if (COMP.position.z > RightFoot.transform.position.z && COMP.position.z > LeftFoot.transform.position.z)
            {
                walkForward = true;
            }
            else
            {
                if (!isKeyDown)
                {
                    walkForward = false;
                }
            }
        }

        private void ResetWalkCycle()
        {
            //Reset variables when not moving
            if (!walkForward && !walkBackward)
            {
                stepRight = false;
                stepLeft = false;
                _stepRTimer = 0;
                _stepLTimer = 0;
                alertLegRight = false;
                alertLegLeft = false;
            }
        }

        private void PlayerMovement()
        {
            _direction = new Vector3(Input.GetAxisRaw(leftRight), 0.0f, Input.GetAxisRaw(forwardBackward));

            if (joystick)
            {
                _direction.x += joystick.Horizontal;
                _direction.z += joystick.Vertical;
            }

            if (_direction != Vector3.zero && isBalanced)
            {
                if (!walkForward && !moveAxisUsed)
                {
                    walkForward = true;
                    moveAxisUsed = true;
                    isKeyDown = true;
                }
            }
            else
            {
                if (walkForward && moveAxisUsed)
                {
                    walkForward = false;
                    moveAxisUsed = false;
                    isKeyDown = false;
                }
            }

            Root.transform.GetComponent<Rigidbody>().velocity = Vector3.Lerp(Root.transform.GetComponent<Rigidbody>().velocity, (_direction * moveSpeed) + new Vector3(0, Root.transform.GetComponent<Rigidbody>().velocity.y, 0), 0.8f);
        }

        private void Jumping()
        {
            if (Input.GetKeyDown(keyJump))
            {
                JumpPress();
            }
            else
            {
                if (Input.GetKey(keyJump))
                {
                    JumpHold();
                }
                else if (Input.GetKeyUp(keyJump))
                {
                    JumpRelease();
                }
            }
        }

        private void JumpPress()
        {
            if (isBalanced && !isInAir)
            {
                _timer = Time.time;

                if (jumpStrengthIndicator)
                {
                    jumpStrengthIndicator.gameObject.SetActive(true);
                }
            }
        }

        private void JumpHold()
        {
            if (isBalanced && !isInAir)
            {
                if (jumpStrengthIndicator)
                {
                    jumpStrengthIndicator.value = (Mathf.Clamp(Time.time - _timer, 0, maxJumpRequiredSeconds) / maxJumpRequiredSeconds) * maxJumpStrength;
                }
            }
        }

        private void JumpRelease()
        {
            if (isBalanced && !isInAir)
            {
                isInAir = true;

                float strength = (Mathf.Clamp(Time.time - _timer, 0, maxJumpRequiredSeconds) / maxJumpRequiredSeconds);

                if (strength > 0.75f)
                {
                    ActivateRagdoll();
                }

                Root.GetComponent<Rigidbody>().AddForce(Root.transform.up * strength * maxJumpStrength * 20, ForceMode.Impulse);

                if (jumpStrengthIndicator)
                {
                    jumpStrengthIndicator.value = 0;
                    jumpStrengthIndicator.gameObject.SetActive(false);
                }
            }
        }

        public void PlayerLanded()
        {
            if (isInAir)
            {
                isInAir = false;
                ResetPlayerPose();
            }
        }

        private void PlayerReach()
        {
            if (_weaponManager)
            {
                if (!_weaponManager.weaponLeft && !_weaponManager.weaponRight)
                {
                    //Body Bending
                    if (MouseYAxisBody <= 0.9f && MouseYAxisBody >= -0.9f)
                    {
                        MouseYAxisBody += (Input.GetAxis("Mouse Y") / reachSensitivity);
                    }
                    else if (MouseYAxisBody > 0.9f)
                    {
                        MouseYAxisBody = 0.9f;
                    }
                    else if (MouseYAxisBody < -0.9f)
                    {
                        MouseYAxisBody = -0.9f;
                    }

                    Body.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(MouseYAxisBody, 0, 0, 1);
                }

                if (!_weaponManager.weaponLeft)
                {
                    //Reach Left
                    if (Input.GetKey(reachLeft) && !punchingLeft)
                    {
                        if (!reachLeftAxisUsed)
                        {
                            //Adjust Left Arm joint strength
                            UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = ReachStiffness;
                            UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = ReachStiffness;
                            LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = ReachStiffness;
                            LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = ReachStiffness;

                            //Adjust body joint strength
                            Body.GetComponent<ConfigurableJoint>().angularXDrive = CoreStiffness;
                            Body.GetComponent<ConfigurableJoint>().angularYZDrive = CoreStiffness;

                            reachLeftAxisUsed = true;
                        }

                        if (MouseYAxisArms <= 1.2f && MouseYAxisArms >= -1.2f)
                        {
                            MouseYAxisArms += (Input.GetAxis("Mouse Y") / reachSensitivity);
                        }

                        else if (MouseYAxisArms > 1.2f)
                        {
                            MouseYAxisArms = 1.2f;
                        }

                        else if (MouseYAxisArms < -1.2f)
                        {
                            MouseYAxisArms = -1.2f;
                        }

                        //upper  left arm pose
                        UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.88f - MouseYAxisArms, 0.58f + MouseYAxisArms, -0.8f, 1);
                    }

                    if (!Input.GetKey(reachLeft) && !punchingLeft)
                    {
                        if (reachLeftAxisUsed)
                        {
                            if (isBalanced)
                            {
                                UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                                LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;

                                Body.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                Body.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            }

                            else if (!isBalanced)
                            {
                                UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                                LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            }

                            ResetPlayerPose();
                            reachLeftAxisUsed = false;
                        }
                    }
                }

                if (!_weaponManager.weaponRight)
                {
                    //Reach Right
                    if (Input.GetKey(reachRight) && !punchingRight)
                    {
                        if (!reachRightAxisUsed)
                        {
                            //Adjust Right Arm joint strength
                            UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = ReachStiffness;
                            UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = ReachStiffness;
                            LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = ReachStiffness;
                            LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = ReachStiffness;

                            //Adjust body joint strength
                            Body.GetComponent<ConfigurableJoint>().angularXDrive = CoreStiffness;
                            Body.GetComponent<ConfigurableJoint>().angularYZDrive = CoreStiffness;

                            reachRightAxisUsed = true;
                        }

                        if (MouseYAxisArms <= 1.2f && MouseYAxisArms >= -1.2f)
                        {
                            MouseYAxisArms += (Input.GetAxis("Mouse Y") / reachSensitivity);
                        }

                        else if (MouseYAxisArms > 1.2f)
                        {
                            MouseYAxisArms = 1.2f;
                        }

                        else if (MouseYAxisArms < -1.2f)
                        {
                            MouseYAxisArms = -1.2f;
                        }

                        //upper right arm pose
                        UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Inverse(new Quaternion(0.88f + MouseYAxisArms, 0.58f + MouseYAxisArms, -0.8f, 1));
                    }

                    if (!Input.GetKey(reachRight) && !punchingRight)
                    {
                        if (reachRightAxisUsed)
                        {
                            if (isBalanced)
                            {
                                UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                                LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;

                                Body.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                Body.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            }

                            else if (!isBalanced)
                            {
                                UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                                LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                                LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                            }

                            ResetPlayerPose();
                            reachRightAxisUsed = false;
                        }
                    }
                }
            }
        }

        private void PlayerPunch()
        {
            //punch right
            if (!punchingRight && Input.GetKey(punchRight))
            {
                punchingRight = true;

                //Right hand punch pull back pose
                Body.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.15f, -0.15f, 0, 1);
                UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.62f, -0.51f, 0.02f, 1);
                LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(1.31f, 0.5f, -0.5f, 1);
            }

            if (punchingRight && !Input.GetKey(punchRight))
            {
                punchingRight = false;

                //Right hand punch release pose
                Body.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.15f, 0.15f, 0, 1);
                UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0.74f, 0.04f, 0f, 1);
                LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0.2f, 0, 0, 1);

                //Right hand punch force
                RightHand.AddForce(Root.transform.forward * punchForce, ForceMode.Impulse);

                Body.GetComponent<Rigidbody>().AddForce(Root.transform.forward * punchForce, ForceMode.Impulse);

                StartCoroutine(DelayCoroutine());

                IEnumerator DelayCoroutine()
                {
                    yield return new WaitForSeconds(0.3f);
                    if (!Input.GetKey(punchRight))
                    {
                        UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = upperRightArmTarget;
                        LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = lowerRightArmTarget;
                    }
                }
            }


            //punch left
            if (!punchingLeft && Input.GetKey(punchLeft))
            {
                punchingLeft = true;

                //Left hand punch pull back pose
                Body.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.15f, 0.15f, 0, 1);
                UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(0.62f, -0.51f, 0.02f, 1);
                LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-1.31f, 0.5f, 0.5f, 1);
            }

            if (punchingLeft && !Input.GetKey(punchLeft))
            {
                punchingLeft = false;

                //Left hand punch release pose
                Body.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.15f, -0.15f, 0, 1);
                UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.74f, 0.04f, 0f, 1);
                LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.2f, 0, 0, 1);

                //Left hand punch force
                LeftHand.AddForce(Root.transform.forward * punchForce, ForceMode.Impulse);

                Body.GetComponent<Rigidbody>().AddForce(Root.transform.forward * punchForce, ForceMode.Impulse);

                StartCoroutine(DelayCoroutine());

                IEnumerator DelayCoroutine()
                {
                    yield return new WaitForSeconds(0.3f);
                    if (!Input.GetKey(punchLeft))
                    {
                        UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = upperLeftArmTarget;
                        LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = lowerLeftArmTarget;
                    }
                }
            }
        }

        private void Walking()
        {
            if (!isInAir)
            {
                if (walkForward)
                {
                    //right leg
                    if (RightFoot.transform.position.z < LeftFoot.transform.position.z && !stepLeft && !alertLegRight)
                    {
                        stepRight = true;
                        alertLegRight = true;
                        alertLegLeft = true;
                    }

                    //left leg
                    if (RightFoot.transform.position.z > LeftFoot.transform.position.z && !stepRight && !alertLegLeft)
                    {
                        stepLeft = true;
                        alertLegLeft = true;
                        alertLegRight = true;
                    }
                }

                if (walkBackward)
                {
                    //right leg
                    if (RightFoot.transform.position.z > LeftFoot.transform.position.z && !stepLeft && !alertLegRight)
                    {
                        stepRight = true;
                        alertLegRight = true;
                        alertLegLeft = true;
                    }

                    //left leg
                    if (RightFoot.transform.position.z < LeftFoot.transform.position.z && !stepRight && !alertLegLeft)
                    {
                        stepLeft = true;
                        alertLegLeft = true;
                        alertLegRight = true;
                    }
                }

                //Step right
                if (stepRight)
                {
                    _stepRTimer += Time.fixedDeltaTime;

                    //Right foot force down
                    RightFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

                    //walk simulation
                    if (walkForward)
                    {
                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperLeftLeg.GetComponent<ConfigurableJoint>().GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (walkBackward)
                    {
                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }


                    //step duration
                    if (_stepRTimer > StepDuration)
                    {
                        _stepRTimer = 0;
                        stepRight = false;

                        if (walkForward || walkBackward)
                        {
                            stepLeft = true;
                        }
                    }
                }
                else
                {
                    //reset to idle
                    UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation, _upperRightLegTarget, (8f) * Time.fixedDeltaTime);
                    LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation, _lowerRightLegTarget, (17f) * Time.fixedDeltaTime);

                    //feet force down
                    RightFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                    LeftFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                }


                //Step left
                if (stepLeft)
                {
                    _stepLTimer += Time.fixedDeltaTime;

                    //Left foot force down
                    LeftFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

                    //walk simulation
                    if (walkForward)
                    {
                        UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (walkBackward)
                    {
                        UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }


                    //Step duration
                    if (_stepLTimer > StepDuration)
                    {
                        _stepLTimer = 0;
                        stepLeft = false;

                        if (walkForward || walkBackward)
                        {
                            stepRight = true;
                        }
                    }
                }
                else
                {
                    //reset to idle
                    UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation, _upperLeftLegTarget, (7f) * Time.fixedDeltaTime);
                    LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation, _lowerLeftLegTarget, (18f) * Time.fixedDeltaTime);

                    //feet force down
                    RightFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                    LeftFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                }
            }
        }

        public void ActivateRagdoll()
        {
            isRagdoll = true;
            isBalanced = false;

            //Root
            Root.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            Root.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            //head
            Head.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            Head.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            //arms
            if (!reachRightAxisUsed)
            {
                UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            }

            if (!reachLeftAxisUsed)
            {
                UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            }

            //legs
            UpperRightLeg.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            UpperRightLeg.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            LowerRightLeg.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            LowerRightLeg.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            UpperLeftLeg.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            UpperLeftLeg.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            LowerLeftLeg.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            LowerLeftLeg.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            RightFoot.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            RightFoot.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            LeftFoot.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
            LeftFoot.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
        }

        private void DeactivateRagdoll()
        {
            isRagdoll = false;
            isBalanced = true;

            //Root
            Root.GetComponent<ConfigurableJoint>().angularXDrive = _balanceOn;
            Root.GetComponent<ConfigurableJoint>().angularYZDrive = _balanceOn;
            //head
            Head.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            Head.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            //arms
            if (!reachRightAxisUsed)
            {
                UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            }

            if (!reachLeftAxisUsed)
            {
                UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
                LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = DriveOff;
                LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = DriveOff;
            }

            //legs
            UpperRightLeg.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            UpperRightLeg.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            LowerRightLeg.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            LowerRightLeg.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            UpperLeftLeg.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            UpperLeftLeg.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            LowerLeftLeg.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            LowerLeftLeg.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            RightFoot.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            RightFoot.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            LeftFoot.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            LeftFoot.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;

            ResetPlayerPose();
        }

        public void ResetPlayerPose()
        {
            if (!isInAir)
            {
                Body.GetComponent<ConfigurableJoint>().targetRotation = _bodyTarget;

                if (!_weaponManager || (_weaponManager && (!_weaponManager.weaponRight || !_weaponManager.weaponRight.gameObject.activeSelf)))
                {
                    UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = upperRightArmTarget;
                    LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = lowerRightArmTarget;
                }

                if (!_weaponManager || (_weaponManager && (!_weaponManager.weaponLeft || !_weaponManager.weaponLeft.gameObject.activeSelf)))
                {
                    UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = upperLeftArmTarget;
                    LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = lowerLeftArmTarget;
                }

                MouseYAxisArms = 0;
            }
        }

        private void CenterOfMass()
        {
            _centerOfMassPoint =
                (Root.GetComponent<Rigidbody>().mass * Root.transform.position +
                 Body.GetComponent<Rigidbody>().mass * Body.transform.position +
                 Head.GetComponent<Rigidbody>().mass * Head.transform.position +
                 UpperRightArm.GetComponent<Rigidbody>().mass * UpperRightArm.transform.position +
                 LowerRightArm.GetComponent<Rigidbody>().mass * LowerRightArm.transform.position +
                 UpperLeftArm.GetComponent<Rigidbody>().mass * UpperLeftArm.transform.position +
                 LowerLeftArm.GetComponent<Rigidbody>().mass * LowerLeftArm.transform.position +
                 UpperRightLeg.GetComponent<Rigidbody>().mass * UpperRightLeg.transform.position +
                 LowerRightLeg.GetComponent<Rigidbody>().mass * LowerRightLeg.transform.position +
                 UpperLeftLeg.GetComponent<Rigidbody>().mass * UpperLeftLeg.transform.position +
                 LowerLeftLeg.GetComponent<Rigidbody>().mass * LowerLeftLeg.transform.position +
                 RightFoot.GetComponent<Rigidbody>().mass * RightFoot.transform.position +
                 LeftFoot.GetComponent<Rigidbody>().mass * LeftFoot.transform.position)
                /
                (Root.GetComponent<Rigidbody>().mass + Body.GetComponent<Rigidbody>().mass +
                 Head.GetComponent<Rigidbody>().mass + UpperRightArm.GetComponent<Rigidbody>().mass +
                 LowerRightArm.GetComponent<Rigidbody>().mass + UpperLeftArm.GetComponent<Rigidbody>().mass +
                 LowerLeftArm.GetComponent<Rigidbody>().mass + UpperRightLeg.GetComponent<Rigidbody>().mass +
                 LowerRightLeg.GetComponent<Rigidbody>().mass + UpperLeftLeg.GetComponent<Rigidbody>().mass +
                 LowerLeftLeg.GetComponent<Rigidbody>().mass + RightFoot.GetComponent<Rigidbody>().mass +
                 LeftFoot.GetComponent<Rigidbody>().mass);

            COMP.position = _centerOfMassPoint;
        }
    }
}