using System.Collections;
using UnityEngine;


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
        //-------------------------------------------------------------
        //--Variables
        //-------------------------------------------------------------


        //Active Ragdoll Player parts
        public GameObject
            //
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

        [Header("Hand Dependancies")]
        //Hand Controller Scripts & dependancies
        public HandContact GrabRight;

        public HandContact GrabLeft;

        [Header("Input on this player")]
        //Enable controls
        public bool useControls = true;

        [Header("Player Input Axis")]
        //Player Axis controls
        public string forwardBackward = "Vertical";

        public string leftRight = "Horizontal";
        public string jump = "Jump";
        public string reachLeft = "Fire1";
        public string reachRight = "Fire2";

        [Header("Player Input KeyCodes")]
        //Player KeyCode controls
        public string punchLeft = "q";

        public string punchRight = "e";

        [Header("The Layer Only This Player Is On")]
        //Player layer name
        public string thisPlayerLayer = "Player_1";

        [Header("Movement Properties")]
        //Player properties
        public bool forwardIsCameraDirection = true;

        //Movement
        public float moveSpeed = 10f;
        public float turnSpeed = 6f;
        public float jumpForce = 18f;

        [Header("Balance Properties")]
        //Balance
        public bool autoGetUpWhenPossible = true;

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


        //Hidden variables
        private float
            timer,
            Step_R_timer,
            Step_L_timer;

        public float
            MouseYAxisArms;

        private float
            MouseXAxisArms,
            MouseYAxisBody;

        [HideInInspector] public bool
            WalkForward,
            WalkBackward,
            StepRight,
            StepLeft,
            Alert_Leg_Right,
            Alert_Leg_Left,
            balanced = true,
            GettingUp,
            isRagdoll,
            isKeyDown,
            moveAxisUsed,
            jumpAxisUsed,
            reachLeftAxisUsed,
            reachRightAxisUsed;

        [HideInInspector] public bool
            jumping,
            isJumping,
            inAir,
            punchingRight,
            punchingLeft;

        private Camera cam;
        private Vector3 Direction;
        private Vector3 CenterOfMassPoint;

        //Joint Drives on & off
        public JointDrive
            //
            BalanceOn, PoseOn, CoreStiffness, ReachStiffness, DriveOff;

        //Original pose target rotation
        [HideInInspector] public Quaternion
            //
            HeadTarget,
            BodyTarget,
            UpperRightArmTarget,
            LowerRightArmTarget,
            UpperLeftArmTarget,
            LowerLeftArmTarget,
            RightHandTarget,
            LeftHandTarget,
            UpperRightLegTarget,
            LowerRightLegTarget,
            UpperLeftLegTarget,
            LowerLeftLegTarget;

        [Header("Player Editor Debug Mode")]
        //Debug
        public bool editorDebugMode;

        WeaponManager weaponManager;

        //-------------------------------------------------------------
        //--Calling Functions
        //-------------------------------------------------------------


        //---Setup---//
        //////////////
        void Awake()
        {
            PlayerSetup();
            weaponManager = COMP.GetComponent<WeaponManager>();
        }


        //---Updates---//
        ////////////////
        void Update()
        {
            if (useControls)
            {
                if (!inAir)
                {
                    if (balanced)
                    {
                        PlayerMovement();
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
                if (balanced)
                {
                    StepPrediction();
                    CenterOfMass();
                }
            }
            else
            {
                ResetWalkCycle();
            }

            GroundCheck();
            CenterOfMass();
        }


        //---Fixed Updates---//
        //////////////////////
        void FixedUpdate()
        {
            if (!isRagdoll && balanced)
            {
                Walking();

                if (useControls)
                {
                    PlayerGetUpJumping();
                }
            }
        }


        //-------------------------------------------------------------
        //--Functions
        //-------------------------------------------------------------


        //---Player Setup--//
        ////////////////////
        void PlayerSetup()
        {
            cam = Camera.main;

            //Setup joint drives
            BalanceOn = new JointDrive();
            BalanceOn.positionSpring = balanceStrength;
            BalanceOn.positionDamper = 0;
            BalanceOn.maximumForce = Mathf.Infinity;

            PoseOn = new JointDrive();
            PoseOn.positionSpring = limbStrength;
            PoseOn.positionDamper = 0;
            PoseOn.maximumForce = Mathf.Infinity;

            CoreStiffness = new JointDrive();
            CoreStiffness.positionSpring = coreStrength;
            CoreStiffness.positionDamper = 0;
            CoreStiffness.maximumForce = Mathf.Infinity;

            ReachStiffness = new JointDrive();
            ReachStiffness.positionSpring = armReachStiffness;
            ReachStiffness.positionDamper = 0;
            ReachStiffness.maximumForce = Mathf.Infinity;

            DriveOff = new JointDrive();
            DriveOff.positionSpring = 25;
            DriveOff.positionDamper = 0;
            DriveOff.maximumForce = Mathf.Infinity;

            //Setup original pose for joint drives
            BodyTarget = Body.GetComponent<ConfigurableJoint>().targetRotation;
            HeadTarget = Head.GetComponent<ConfigurableJoint>().targetRotation;
            UpperRightArmTarget = UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation;
            LowerRightArmTarget = LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation;
            UpperLeftArmTarget = UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation;
            LowerLeftArmTarget = LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation;
            RightHandTarget = RightHand.GetComponent<ConfigurableJoint>().targetRotation;
            LeftHandTarget = LeftHand.GetComponent<ConfigurableJoint>().targetRotation;
            UpperRightLegTarget = UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation;
            LowerRightLegTarget = LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation;
            UpperLeftLegTarget = UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation;
            LowerLeftLegTarget = LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation;
        }


        //---Ground Check---//
        /////////////////////
        void GroundCheck()
        {
            Ray ray = new Ray(Root.transform.position, -Root.transform.up);
            RaycastHit hit;

            //Balance when ground is detected
            if (Physics.Raycast(ray, out hit, balanceHeight, 1 << LayerMask.NameToLayer("Ground")) && !inAir && !isJumping && !reachRightAxisUsed && !reachLeftAxisUsed)
            {
                if (!balanced && Root.GetComponent<Rigidbody>().velocity.magnitude < 1f)
                {
                    if (autoGetUpWhenPossible)
                    {
                        balanced = true;
                    }
                }
            }

            //Fall over when ground is not detected
            else if (!Physics.Raycast(ray, out hit, balanceHeight, 1 << LayerMask.NameToLayer("Ground")))
            {
                if (balanced)
                {
                    balanced = false;
                }
            }


            //Balance on/off
            if (balanced && isRagdoll)
            {
                DeactivateRagdoll();
            }
            else if (!balanced && !isRagdoll)
            {
                ActivateRagdoll();
            }
        }


        //---Step Prediction---//
        ////////////////////////
        void StepPrediction()
        {
            //Reset variables when balanced
            if (!WalkForward && !WalkBackward)
            {
                StepRight = false;
                StepLeft = false;
                Step_R_timer = 0;
                Step_L_timer = 0;
                Alert_Leg_Right = false;
                Alert_Leg_Left = false;
            }

            //Check direction to walk when off balance
            //Backwards
            if (COMP.position.z < RightFoot.transform.position.z && COMP.position.z < LeftFoot.transform.position.z)
            {
                WalkBackward = true;
            }
            else
            {
                if (!isKeyDown)
                {
                    WalkBackward = false;
                }
            }

            //Forward
            if (COMP.position.z > RightFoot.transform.position.z && COMP.position.z > LeftFoot.transform.position.z)
            {
                WalkForward = true;
            }
            else
            {
                if (!isKeyDown)
                {
                    WalkForward = false;
                }
            }
        }


        //---Reset Walk Cycle---//
        /////////////////////////
        void ResetWalkCycle()
        {
            //Reset variables when not moving
            if (!WalkForward && !WalkBackward)
            {
                StepRight = false;
                StepLeft = false;
                Step_R_timer = 0;
                Step_L_timer = 0;
                Alert_Leg_Right = false;
                Alert_Leg_Left = false;
            }
        }


        //---Player Movement---//
        ////////////////////////
        void PlayerMovement()
        {
            //Move in camera direction
            if (forwardIsCameraDirection)
            {
                Direction = new Vector3(Input.GetAxisRaw(leftRight), 0.0f, Input.GetAxisRaw(forwardBackward));
                Direction.y = 0f;
                Root.transform.GetComponent<Rigidbody>().velocity = Vector3.Lerp(Root.transform.GetComponent<Rigidbody>().velocity, (Direction * moveSpeed) + new Vector3(0, Root.transform.GetComponent<Rigidbody>().velocity.y, 0), 0.8f);

                if (Input.GetAxisRaw(leftRight) != 0 || Input.GetAxisRaw(forwardBackward) != 0 && balanced)
                {
                    if (!WalkForward && !moveAxisUsed)
                    {
                        WalkForward = true;
                        moveAxisUsed = true;
                        isKeyDown = true;
                    }
                }

                else if (Input.GetAxisRaw(leftRight) == 0 && Input.GetAxisRaw(forwardBackward) == 0)
                {
                    if (WalkForward && moveAxisUsed)
                    {
                        WalkForward = false;
                        moveAxisUsed = false;
                        isKeyDown = false;
                    }
                }
            }

            //Move in own direction
            else
            {
                if (Input.GetAxisRaw(forwardBackward) != 0)
                {
                    var v3 = Root.GetComponent<Rigidbody>().transform.forward * (Input.GetAxisRaw(forwardBackward) * moveSpeed);
                    v3.y = Root.GetComponent<Rigidbody>().velocity.y;
                    Root.GetComponent<Rigidbody>().velocity = v3;
                }


                if (Input.GetAxisRaw(forwardBackward) > 0)
                {
                    if (!WalkForward && !moveAxisUsed)
                    {
                        WalkBackward = false;
                        WalkForward = true;
                        moveAxisUsed = true;
                        isKeyDown = true;

                        if (isRagdoll)
                        {
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
                        }
                    }
                }

                else if (Input.GetAxisRaw(forwardBackward) < 0)
                {
                    if (!WalkBackward && !moveAxisUsed)
                    {
                        WalkForward = false;
                        WalkBackward = true;
                        moveAxisUsed = true;
                        isKeyDown = true;

                        if (isRagdoll)
                        {
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
                        }
                    }
                }

                else if (Input.GetAxisRaw(forwardBackward) == 0)
                {
                    if (WalkForward || WalkBackward && moveAxisUsed)
                    {
                        WalkForward = false;
                        WalkBackward = false;
                        moveAxisUsed = false;
                        isKeyDown = false;

                        if (isRagdoll)
                        {
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
                    }
                }
            }
        }


        //---Player GetUp & Jumping---//
        ///////////////////////////////
        void PlayerGetUpJumping()
        {
            if (Input.GetAxis(jump) > 0)
            {
                if (!jumpAxisUsed)
                {
                    if (balanced && !inAir)
                    {
                        jumping = true;
                    }

                    else if (!balanced)
                    {
                        DeactivateRagdoll();
                    }
                }

                jumpAxisUsed = true;
            }

            else
            {
                jumpAxisUsed = false;
            }


            if (jumping)
            {
                isJumping = true;

                var v3 = Root.GetComponent<Rigidbody>().transform.up * jumpForce;
                v3.x = Root.GetComponent<Rigidbody>().velocity.x;
                v3.z = Root.GetComponent<Rigidbody>().velocity.z;
                Root.GetComponent<Rigidbody>().velocity = v3;
            }

            if (isJumping)
            {
                timer = timer + Time.fixedDeltaTime;

                if (timer > 0.2f)
                {
                    timer = 0.0f;
                    jumping = false;
                    isJumping = false;
                    inAir = true;
                }
            }
        }


        //---Player Landed---//
        //////////////////////
        public void PlayerLanded()
        {
            if (inAir && !isJumping && !jumping)
            {
                inAir = false;
                ResetPlayerPose();
            }
        }


        //---Player Reach--//
        ////////////////////
        public void PlayerReach()
        {
            if (weaponManager)
            {
                if (!weaponManager.weaponLeft && !weaponManager.weaponRight)
                {
                    //Body Bending
                    if (MouseYAxisBody <= 0.9f && MouseYAxisBody >= -0.9f)
                    {
                        MouseYAxisBody = MouseYAxisBody + (Input.GetAxis("Mouse Y") / reachSensitivity);
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

                if (!weaponManager.weaponLeft)
                {
                    //Reach Left
                    if (Input.GetAxisRaw(reachLeft) != 0 && !punchingLeft)
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
                            MouseYAxisArms = MouseYAxisArms + (Input.GetAxis("Mouse Y") / reachSensitivity);
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

                    if (Input.GetAxisRaw(reachLeft) == 0 && !punchingLeft)
                    {
                        if (reachLeftAxisUsed)
                        {
                            if (balanced)
                            {
                                UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                                LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;

                                Body.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                Body.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            }

                            else if (!balanced)
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

                if (!weaponManager.weaponRight)
                {
                    //Reach Right
                    if (Input.GetAxisRaw(reachRight) != 0 && !punchingRight)
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
                            MouseYAxisArms = MouseYAxisArms + (Input.GetAxis("Mouse Y") / reachSensitivity);
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

                    if (Input.GetAxisRaw(reachRight) == 0 && !punchingRight)
                    {
                        if (reachRightAxisUsed)
                        {
                            if (balanced)
                            {
                                UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                                LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;

                                Body.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                                Body.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                            }

                            else if (!balanced)
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


        //---Player Punch---//
        /////////////////////
        void PlayerPunch()
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
                        UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = UpperRightArmTarget;
                        LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = LowerRightArmTarget;
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
                        UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = UpperLeftArmTarget;
                        LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = LowerLeftArmTarget;
                    }
                }
            }
        }


        //---Player Walking---//
        ///////////////////////
        void Walking()
        {
            if (!inAir)
            {
                if (WalkForward)
                {
                    //right leg
                    if (RightFoot.transform.position.z < LeftFoot.transform.position.z && !StepLeft && !Alert_Leg_Right)
                    {
                        StepRight = true;
                        Alert_Leg_Right = true;
                        Alert_Leg_Left = true;
                    }

                    //left leg
                    if (RightFoot.transform.position.z > LeftFoot.transform.position.z && !StepRight && !Alert_Leg_Left)
                    {
                        StepLeft = true;
                        Alert_Leg_Left = true;
                        Alert_Leg_Right = true;
                    }
                }

                if (WalkBackward)
                {
                    //right leg
                    if (RightFoot.transform.position.z > LeftFoot.transform.position.z && !StepLeft && !Alert_Leg_Right)
                    {
                        StepRight = true;
                        Alert_Leg_Right = true;
                        Alert_Leg_Left = true;
                    }

                    //left leg
                    if (RightFoot.transform.position.z < LeftFoot.transform.position.z && !StepRight && !Alert_Leg_Left)
                    {
                        StepLeft = true;
                        Alert_Leg_Left = true;
                        Alert_Leg_Right = true;
                    }
                }

                //Step right
                if (StepRight)
                {
                    Step_R_timer += Time.fixedDeltaTime;

                    //Right foot force down
                    RightFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

                    //walk simulation
                    if (WalkForward)
                    {
                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperLeftLeg.GetComponent<ConfigurableJoint>().GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (WalkBackward)
                    {
                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }


                    //step duration
                    if (Step_R_timer > StepDuration)
                    {
                        Step_R_timer = 0;
                        StepRight = false;

                        if (WalkForward || WalkBackward)
                        {
                            StepLeft = true;
                        }
                    }
                }
                else
                {
                    //reset to idle
                    UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation, UpperRightLegTarget, (8f) * Time.fixedDeltaTime);
                    LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(LowerRightLeg.GetComponent<ConfigurableJoint>().targetRotation, LowerRightLegTarget, (17f) * Time.fixedDeltaTime);

                    //feet force down
                    RightFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                    LeftFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                }


                //Step left
                if (StepLeft)
                {
                    Step_L_timer += Time.fixedDeltaTime;

                    //Left foot force down
                    LeftFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

                    //walk simulation
                    if (WalkForward)
                    {
                        UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.09f * StepHeight, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.09f * StepHeight * 2, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.12f * StepHeight / 2, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }

                    if (WalkBackward)
                    {
                        UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.00f * StepHeight, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                        LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.x - 0.07f * StepHeight * 2, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.y, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.z, LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation.w);

                        UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.x + 0.02f * StepHeight / 2, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.y, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.z, UpperRightLeg.GetComponent<ConfigurableJoint>().targetRotation.w);
                    }


                    //Step duration
                    if (Step_L_timer > StepDuration)
                    {
                        Step_L_timer = 0;
                        StepLeft = false;

                        if (WalkForward || WalkBackward)
                        {
                            StepRight = true;
                        }
                    }
                }
                else
                {
                    //reset to idle
                    UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(UpperLeftLeg.GetComponent<ConfigurableJoint>().targetRotation, UpperLeftLegTarget, (7f) * Time.fixedDeltaTime);
                    LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Lerp(LowerLeftLeg.GetComponent<ConfigurableJoint>().targetRotation, LowerLeftLegTarget, (18f) * Time.fixedDeltaTime);

                    //feet force down
                    RightFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                    LeftFoot.GetComponent<Rigidbody>().AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
                }
            }
        }


        //---Activate Ragdoll---//
        /////////////////////////
        public void ActivateRagdoll()
        {
            isRagdoll = true;
            balanced = false;

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


        //---Deactivate Ragdoll---//
        ///////////////////////////
        void DeactivateRagdoll()
        {
            isRagdoll = false;
            balanced = true;

            //Root
            Root.GetComponent<ConfigurableJoint>().angularXDrive = BalanceOn;
            Root.GetComponent<ConfigurableJoint>().angularYZDrive = BalanceOn;
            //head
            Head.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
            Head.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            //arms
            if (!reachRightAxisUsed)
            {
                UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
            }

            if (!reachLeftAxisUsed)
            {
                UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
                LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = PoseOn;
                LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = PoseOn;
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


        //---Reset Player Pose---//
        //////////////////////////
        public void ResetPlayerPose()
        {
            if (!jumping)
            {
                Body.GetComponent<ConfigurableJoint>().targetRotation = BodyTarget;

                if (!weaponManager || (weaponManager && !weaponManager.weaponRight))
                {
                    UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = UpperRightArmTarget;
                    LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = LowerRightArmTarget;
                }

                if (!weaponManager || (weaponManager && !weaponManager.weaponLeft))
                {
                    UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = UpperLeftArmTarget;
                    LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = LowerLeftArmTarget;
                }

                MouseYAxisArms = 0;
            }
        }


        //---Calculating Center of mass point---//
        /////////////////////////////////////////
        void CenterOfMass()
        {
            CenterOfMassPoint =
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

            COMP.position = CenterOfMassPoint;
        }


        //-------------------------------------------------------------
        //--Debug
        //-------------------------------------------------------------


        //---Editor Debug Mode---//
        //////////////////////////
        void OnDrawGizmos()
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
    }
}