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
		public enum Arms
		{
			Left,
			Right,
			Both
		}

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

		[Header("Player Input")]
		//Player Axis controls
		public string forwardBackward = "Vertical";

		public string leftRight = "Horizontal";
		public Slider bendingSlider;

		public InputControl jump = new InputControl()
		{
			key = KeyCode.Space
		};

		public InputControl drop = new InputControl()
		{
			key = KeyCode.Q
		};

		//Movement
		public float moveSpeed = 10;
		public float maxJumpStrength = 18;
		public float maxJumpRequiredSeconds = 2;
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

		public float armReachStiffness = 2000f;

		[Header("Actions")]
		//Punch
		public bool canBeKnockoutByImpact = true;

		public float requiredForceToBeKO = 20f;

		[Header("Audio")]
		//Impact sounds
		public float ImpactForce = 10f;

		public AudioClip[] Impacts;
		public AudioClip[] Hits;
		public AudioSource SoundSource;

		public float
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
			moveAxisUsed;

		[SerializeField] [ReadOnly] private bool _isGrabbing;
		[SerializeField] [ReadOnly] private Transform _grabbed;

		[ReadOnly] public bool
			isInAir;

		[Header("Player Editor Debug Mode")] public bool editorDebugMode;

		[HideInInspector] public HealthManager healthManager;

		//Joint Drives on & off
		private JointDrive
			_balanceOn;

		private Camera _cam;
		private Vector3 _centerOfMassPoint;
		private Vector3 _direction;
		private HoldTrigger _grabbingToggleButton;

		private float
			_mouseXAxisArms;

		private Player _player;

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

		public BodyPart
			root,
			body,
			head,
			armRight,
			armRightLow,
			handRight,
			armLeft,
			armLeftLow,
			handLeft,
			legRight,
			legRightLow,
			legLeft,
			legLeftLow,
			footRight,
			footLeft;

		public bool isGrabbing
		{
			get => _isGrabbing;
			set
			{
				_isGrabbing = value;

				if (value)
				{
					//Adjust Left Arm joint strength
					armLeft.joint.angularXDrive = ReachStiffness;
					armLeft.joint.angularYZDrive = ReachStiffness;
					armLeftLow.joint.angularXDrive = ReachStiffness;
					armLeftLow.joint.angularYZDrive = ReachStiffness;

					armRight.joint.angularXDrive = ReachStiffness;
					armRight.joint.angularYZDrive = ReachStiffness;
					armRightLow.joint.angularXDrive = ReachStiffness;
					armRightLow.joint.angularYZDrive = ReachStiffness;

					//Adjust body joint strength
					body.joint.angularXDrive = CoreStiffness;
					body.joint.angularYZDrive = CoreStiffness;
				}
				else
				{
					armLeft.joint.angularXDrive = DriveOff;
					armLeft.joint.angularYZDrive = DriveOff;
					armLeftLow.joint.angularXDrive = DriveOff;
					armLeftLow.joint.angularYZDrive = DriveOff;

					armRight.joint.angularXDrive = DriveOff;
					armRight.joint.angularYZDrive = DriveOff;
					armRightLow.joint.angularXDrive = DriveOff;
					armRightLow.joint.angularYZDrive = DriveOff;

					if (isBalanced)
					{
						body.joint.angularXDrive = PoseOn;
						body.joint.angularYZDrive = PoseOn;
					}

					ResetPlayerPose();

					grabbed = null;
				}
			}
		}

		public Transform grabbed
		{
			get => _grabbed;
			set
			{
				if (drop.button)
				{
					drop.button.gameObject.SetActive(value);
				}

				if (value)
				{
					if (value.GetComponent<Box>())
					{
						foreach (Mover mover in _player.nearTargets.FindAll(enemy => enemy is Mover))
						{
							mover.Annoy();
						}
					}
				}
				else
				{
					if (_grabbed)
					{
						if (_grabbed.GetComponent<Box>())
						{
							foreach (Mover mover in _player.nearTargets.FindAll(enemy => enemy is Mover))
							{
								mover.Calm();
							}
						}
					}
				}

				_grabbed = value;
			}
		}

		private void Awake()
		{
			PlayerSetup();
			DeactivateRagdoll();
			_weaponManager = COMP.GetComponent<WeaponManager>();
			_player = root.transform.GetComponent<Player>();
			healthManager = this.GetComponent<HealthManager>();
			if (stunningParticle)
			{
				_stunningParticleRotations = stunningParticle.GetComponentsInChildren<ETFXRotation>();
			}

			if (jumpStrengthIndicator)
			{
				jumpStrengthIndicator.gameObject.SetActive(false);
				jumpStrengthIndicator.minValue = 0;
				jumpStrengthIndicator.maxValue = maxJumpStrength;
			}

			if (jump.holdButton)
			{
				jump.holdButton.onPress.AddListener(JumpPress);
				jump.holdButton.onHold.AddListener(JumpHold);
				jump.holdButton.onRelease.AddListener(JumpRelease);
			}

			if (bendingSlider)
			{
				_grabbingToggleButton = bendingSlider.GetComponent<HoldTrigger>();

				if (_weaponManager)
				{
					_weaponManager.WeaponChanged += () => bendingSlider.gameObject.SetActive(!_weaponManager.weapon);
				}
			}

			if (_grabbingToggleButton)
			{
				_grabbingToggleButton.onPress.AddListener(() =>
				{
					if (!grabbed && isBalanced)
					{
						isGrabbing = true;
					}
				});
				_grabbingToggleButton.onRelease.AddListener(() =>
				{
					if (!grabbed)
					{
						isGrabbing = false;
					}
				});
			}

			if (drop.button)
			{
				drop.button.onClick.AddListener(() => isGrabbing = false);
			}
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
						PlayerReach();
					}
				}
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
				Debug.DrawRay(Root.transform.position, Vector3.down * balanceHeight, Color.green);

				if (useStepPrediction)
				{
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireSphere(COMP.position, 0.3f);
				}
			}
		}

		public void PlayerSetup()
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

			root.transform = Root.transform;
			root.rigidbody = root.transform.GetComponent<Rigidbody>();
			root.joint = root.transform.GetComponent<ConfigurableJoint>();
			root.originalRotation = root.joint.targetRotation;

			body.transform = Body.transform;
			body.rigidbody = body.transform.GetComponent<Rigidbody>();
			body.joint = body.transform.GetComponent<ConfigurableJoint>();
			body.originalRotation = body.joint.targetRotation;

			head.transform = Head.transform;
			head.rigidbody = head.transform.GetComponent<Rigidbody>();
			head.joint = head.transform.GetComponent<ConfigurableJoint>();
			head.originalRotation = head.joint.targetRotation;

			armRight.transform = UpperRightArm.transform;
			armRight.rigidbody = armRight.transform.GetComponent<Rigidbody>();
			armRight.joint = armRight.transform.GetComponent<ConfigurableJoint>();
			armRight.originalRotation = armRight.joint.targetRotation;

			armRightLow.transform = LowerRightArm.transform;
			armRightLow.rigidbody = armRightLow.transform.GetComponent<Rigidbody>();
			armRightLow.joint = armRightLow.transform.GetComponent<ConfigurableJoint>();
			armRightLow.originalRotation = armRightLow.joint.targetRotation;

			handRight.transform = RightHand.transform;
			handRight.rigidbody = handRight.transform.GetComponent<Rigidbody>();
			handRight.joint = handRight.transform.GetComponent<ConfigurableJoint>();
			handRight.originalRotation = handRight.joint.targetRotation;

			armLeft.transform = UpperLeftArm.transform;
			armLeft.rigidbody = armLeft.transform.GetComponent<Rigidbody>();
			armLeft.joint = armLeft.transform.GetComponent<ConfigurableJoint>();
			armLeft.originalRotation = armLeft.joint.targetRotation;

			armLeftLow.transform = LowerLeftArm.transform;
			armLeftLow.rigidbody = armLeftLow.transform.GetComponent<Rigidbody>();
			armLeftLow.joint = armLeftLow.transform.GetComponent<ConfigurableJoint>();
			armLeftLow.originalRotation = armLeftLow.joint.targetRotation;

			handLeft.transform = LeftHand.transform;
			handLeft.rigidbody = handLeft.transform.GetComponent<Rigidbody>();
			handLeft.joint = handLeft.transform.GetComponent<ConfigurableJoint>();
			handLeft.originalRotation = handLeft.joint.targetRotation;

			legRight.transform = UpperRightLeg.transform;
			legRight.rigidbody = legRight.transform.GetComponent<Rigidbody>();
			legRight.joint = legRight.transform.GetComponent<ConfigurableJoint>();
			legRight.originalRotation = legRight.joint.targetRotation;

			legRightLow.transform = LowerRightLeg.transform;
			legRightLow.rigidbody = legRightLow.transform.GetComponent<Rigidbody>();
			legRightLow.joint = legRightLow.transform.GetComponent<ConfigurableJoint>();
			legRightLow.originalRotation = legRightLow.joint.targetRotation;

			legLeft.transform = UpperLeftLeg.transform;
			legLeft.rigidbody = legLeft.transform.GetComponent<Rigidbody>();
			legLeft.joint = legLeft.transform.GetComponent<ConfigurableJoint>();
			legLeft.originalRotation = legLeft.joint.targetRotation;

			legLeftLow.transform = LowerLeftLeg.transform;
			legLeftLow.rigidbody = legLeftLow.transform.GetComponent<Rigidbody>();
			legLeftLow.joint = legLeftLow.transform.GetComponent<ConfigurableJoint>();
			legLeftLow.originalRotation = legLeftLow.joint.targetRotation;

			footRight.transform = RightFoot.transform;
			footRight.rigidbody = footRight.transform.GetComponent<Rigidbody>();
			footRight.joint = footRight.transform.GetComponent<ConfigurableJoint>();
			footRight.originalRotation = footRight.joint.targetRotation;

			footLeft.transform = LeftFoot.transform;
			footLeft.rigidbody = footLeft.transform.GetComponent<Rigidbody>();
			footLeft.joint = footLeft.transform.GetComponent<ConfigurableJoint>();
			footLeft.originalRotation = footLeft.joint.targetRotation;
		}

		private void GroundCheck()
		{
			Ray ray = new Ray(root.transform.position, Vector3.down);

			//Balance when ground is detected
			if (Physics.Raycast(ray, out RaycastHit hit, balanceHeight, ~(1 << this.gameObject.layer)))
			{
				if (!isBalanced && root.rigidbody.velocity.magnitude < 1f)
				{
					if (!hit.transform.GetComponent<Trampoline>())
					{
						StartCoroutine(GetUp());
					}
				}
			}
			else
			{
				isBalanced = false;
				if (bendingSlider)
				{
					bendingSlider.gameObject.SetActive(false);
				}

				if (jump.holdButton)
				{
					jump.holdButton.gameObject.SetActive(false);
				}

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
			if (COMP.position.z < footRight.transform.position.z && COMP.position.z < footLeft.transform.position.z)
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
			if (COMP.position.z > footRight.transform.position.z && COMP.position.z > footLeft.transform.position.z)
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

			root.rigidbody.velocity = Vector3.Lerp(root.rigidbody.velocity, (_direction * moveSpeed) + new Vector3(0, root.rigidbody.velocity.y, 0), 0.8f);
		}

		private void Jumping()
		{
			if (Input.GetKeyDown(jump.key))
			{
				JumpPress();
			}
			else
			{
				if (Input.GetKey(jump.key))
				{
					JumpHold();
				}
				else if (Input.GetKeyUp(jump.key))
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

				root.rigidbody.AddForce(root.transform.up * strength * maxJumpStrength * 20, ForceMode.Impulse);

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
				if (Input.GetKeyDown(drop.key))
				{
					isGrabbing = false;
				}

				if (!_weaponManager.weapon)
				{
					if (bendingSlider)
					{
						body.joint.targetRotation = new Quaternion(Mathf.Clamp(bendingSlider.value, -1, 0.25f), 0, 0, 1);
					}
				}

				if (isGrabbing)
				{
					if (!_weaponManager.weapon)
					{
						float clampedValue = Mathf.Clamp(bendingSlider.value, -0.5f, 1);
						Quaternion targetRotation = new Quaternion(-0.88f - clampedValue, 0.58f + clampedValue, -0.8f, 1);

						//Reach Left
						//upper  left arm pose
						armLeft.joint.targetRotation = targetRotation;

						//Reach Right
						//upper right arm pose
						targetRotation.x = -targetRotation.x;
						armRight.joint.targetRotation = Quaternion.Inverse(targetRotation);
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
					if (footRight.transform.position.z < footLeft.transform.position.z && !stepLeft && !alertLegRight)
					{
						stepRight = true;
						alertLegRight = true;
						alertLegLeft = true;
					}

					//left leg
					if (footRight.transform.position.z > footLeft.transform.position.z && !stepRight && !alertLegLeft)
					{
						stepLeft = true;
						alertLegLeft = true;
						alertLegRight = true;
					}
				}

				if (walkBackward)
				{
					//right leg
					if (footRight.transform.position.z > footLeft.transform.position.z && !stepLeft && !alertLegRight)
					{
						stepRight = true;
						alertLegRight = true;
						alertLegLeft = true;
					}

					//left leg
					if (footRight.transform.position.z < footLeft.transform.position.z && !stepRight && !alertLegLeft)
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
					footRight.rigidbody.AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

					//walk simulation
					if (walkForward)
					{
						legRight.joint.targetRotation = new Quaternion(legRight.joint.targetRotation.x + 0.09f * StepHeight, legRight.joint.targetRotation.y, legRight.joint.targetRotation.z, legRight.joint.targetRotation.w);
						legRightLow.joint.targetRotation = new Quaternion(legRightLow.joint.targetRotation.x - 0.09f * StepHeight * 2, legRightLow.joint.targetRotation.y, legRightLow.joint.targetRotation.z, legRightLow.joint.targetRotation.w);

						legLeft.joint.targetRotation = new Quaternion(legLeft.joint.targetRotation.x - 0.12f * StepHeight / 2, legLeft.joint.targetRotation.y, legLeft.joint.targetRotation.z, legLeft.joint.targetRotation.w);
					}

					if (walkBackward)
					{
						legRight.joint.targetRotation = new Quaternion(legRight.joint.targetRotation.x - 0.00f * StepHeight, legRight.joint.targetRotation.y, legRight.joint.targetRotation.z, legRight.joint.targetRotation.w);
						legRightLow.joint.targetRotation = new Quaternion(legRightLow.joint.targetRotation.x - 0.07f * StepHeight * 2, legRightLow.joint.targetRotation.y, legRightLow.joint.targetRotation.z, legRightLow.joint.targetRotation.w);

						legLeft.joint.targetRotation = new Quaternion(legLeft.joint.targetRotation.x + 0.02f * StepHeight / 2, legLeft.joint.targetRotation.y, legLeft.joint.targetRotation.z, legLeft.joint.targetRotation.w);
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
					legRight.joint.targetRotation = Quaternion.Lerp(legRight.joint.targetRotation, legRight.originalRotation, (8f) * Time.fixedDeltaTime);
					legRightLow.joint.targetRotation = Quaternion.Lerp(legRightLow.joint.targetRotation, legRightLow.originalRotation, (17f) * Time.fixedDeltaTime);

					//feet force down
					footRight.rigidbody.AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
					footLeft.rigidbody.AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
				}


				//Step left
				if (stepLeft)
				{
					_stepLTimer += Time.fixedDeltaTime;

					//Left foot force down
					footLeft.rigidbody.AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);

					//walk simulation
					if (walkForward)
					{
						legLeft.joint.targetRotation = new Quaternion(legLeft.joint.targetRotation.x + 0.09f * StepHeight, legLeft.joint.targetRotation.y, legLeft.joint.targetRotation.z, legLeft.joint.targetRotation.w);
						legLeftLow.joint.targetRotation = new Quaternion(legLeftLow.joint.targetRotation.x - 0.09f * StepHeight * 2, legLeftLow.joint.targetRotation.y, legLeftLow.joint.targetRotation.z, legLeftLow.joint.targetRotation.w);

						legRight.joint.targetRotation = new Quaternion(legRight.joint.targetRotation.x - 0.12f * StepHeight / 2, legRight.joint.targetRotation.y, legRight.joint.targetRotation.z, legRight.joint.targetRotation.w);
					}

					if (walkBackward)
					{
						legLeft.joint.targetRotation = new Quaternion(legLeft.joint.targetRotation.x - 0.00f * StepHeight, legLeft.joint.targetRotation.y, legLeft.joint.targetRotation.z, legLeft.joint.targetRotation.w);
						legLeftLow.joint.targetRotation = new Quaternion(legLeftLow.joint.targetRotation.x - 0.07f * StepHeight * 2, legLeftLow.joint.targetRotation.y, legLeftLow.joint.targetRotation.z, legLeftLow.joint.targetRotation.w);

						legRight.joint.targetRotation = new Quaternion(legRight.joint.targetRotation.x + 0.02f * StepHeight / 2, legRight.joint.targetRotation.y, legRight.joint.targetRotation.z, legRight.joint.targetRotation.w);
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
					legLeft.joint.targetRotation = Quaternion.Lerp(legLeft.joint.targetRotation, legLeft.originalRotation, (7f) * Time.fixedDeltaTime);
					legLeftLow.joint.targetRotation = Quaternion.Lerp(legLeftLow.joint.targetRotation, legLeftLow.originalRotation, (18f) * Time.fixedDeltaTime);

					//feet force down
					footRight.rigidbody.AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
					footLeft.rigidbody.AddForce(-Vector3.up * FeetMountForce * Time.deltaTime, ForceMode.Impulse);
				}
			}
		}

		public void ActivateRagdoll()
		{
			isRagdoll = true;
			isBalanced = false;
			if (bendingSlider)
			{
				bendingSlider.gameObject.SetActive(false);
			}

			if (jump.holdButton)
			{
				jump.holdButton.gameObject.SetActive(false);
			}

			//Root
			root.joint.angularXDrive = DriveOff;
			root.joint.angularYZDrive = DriveOff;
			//head
			head.joint.angularXDrive = DriveOff;
			head.joint.angularYZDrive = DriveOff;
			//arms
			if (!isGrabbing)
			{
				armRight.joint.angularXDrive = DriveOff;
				armRight.joint.angularYZDrive = DriveOff;
				armRightLow.joint.angularXDrive = DriveOff;
				armRightLow.joint.angularYZDrive = DriveOff;

				armLeft.joint.angularXDrive = DriveOff;
				armLeft.joint.angularYZDrive = DriveOff;
				armLeftLow.joint.angularXDrive = DriveOff;
				armLeftLow.joint.angularYZDrive = DriveOff;
			}

			//legs
			legRight.joint.angularXDrive = DriveOff;
			legRight.joint.angularYZDrive = DriveOff;
			legRightLow.joint.angularXDrive = DriveOff;
			legRightLow.joint.angularYZDrive = DriveOff;
			legLeft.joint.angularXDrive = DriveOff;
			legLeft.joint.angularYZDrive = DriveOff;
			legLeftLow.joint.angularXDrive = DriveOff;
			legLeftLow.joint.angularYZDrive = DriveOff;
			footRight.joint.angularXDrive = DriveOff;
			footRight.joint.angularYZDrive = DriveOff;
			footLeft.joint.angularXDrive = DriveOff;
			footLeft.joint.angularYZDrive = DriveOff;
		}

		private void DeactivateRagdoll()
		{
			isRagdoll = false;
			isBalanced = true;
			if (bendingSlider)
			{
				bendingSlider.gameObject.SetActive(true);
			}

			if (jump.holdButton)
			{
				jump.holdButton.gameObject.SetActive(true);
			}

			//Root
			root.joint.angularXDrive = _balanceOn;
			root.joint.angularYZDrive = _balanceOn;
			//head
			head.joint.angularXDrive = PoseOn;
			head.joint.angularYZDrive = PoseOn;
			//arms
			if (!isGrabbing)
			{
				armRight.joint.angularXDrive = DriveOff;
				armRight.joint.angularYZDrive = DriveOff;
				armRightLow.joint.angularXDrive = DriveOff;
				armRightLow.joint.angularYZDrive = DriveOff;

				armLeft.joint.angularXDrive = DriveOff;
				armLeft.joint.angularYZDrive = DriveOff;
				armLeftLow.joint.angularXDrive = DriveOff;
				armLeftLow.joint.angularYZDrive = DriveOff;
			}

			//legs
			legRight.joint.angularXDrive = PoseOn;
			legRight.joint.angularYZDrive = PoseOn;
			legRightLow.joint.angularXDrive = PoseOn;
			legRightLow.joint.angularYZDrive = PoseOn;
			legLeft.joint.angularXDrive = PoseOn;
			legLeft.joint.angularYZDrive = PoseOn;
			legLeftLow.joint.angularXDrive = PoseOn;
			legLeftLow.joint.angularYZDrive = PoseOn;
			footRight.joint.angularXDrive = PoseOn;
			footRight.joint.angularYZDrive = PoseOn;
			footLeft.joint.angularXDrive = PoseOn;
			footLeft.joint.angularYZDrive = PoseOn;

			ResetPlayerPose();
		}

		public void ResetPlayerPose()
		{
			if (!isInAir)
			{
				body.joint.targetRotation = body.originalRotation;

				if (!_weaponManager || (_weaponManager && (!_weaponManager.weapon || !_weaponManager.weapon.gameObject.activeSelf)))
				{
					armRight.joint.targetRotation = armRight.originalRotation;
					armRightLow.joint.targetRotation = armRightLow.originalRotation;
				}

				if (!_weaponManager || (_weaponManager && (!(_weaponManager.weapon is Gun))))
				{
					armLeft.joint.targetRotation = armLeft.originalRotation;
					armLeftLow.joint.targetRotation = armLeftLow.originalRotation;
				}

				if (bendingSlider)
				{
					bendingSlider.value = 0;
				}
			}
		}

		private void CenterOfMass()
		{
			_centerOfMassPoint =
				(root.rigidbody.mass * root.transform.position +
				 body.rigidbody.mass * body.transform.position +
				 head.rigidbody.mass * head.transform.position +
				 armRight.rigidbody.mass * armRight.transform.position +
				 armRightLow.rigidbody.mass * armRightLow.transform.position +
				 armLeft.rigidbody.mass * armLeft.transform.position +
				 armLeftLow.rigidbody.mass * armLeftLow.transform.position +
				 legRight.rigidbody.mass * legRight.transform.position +
				 legRightLow.rigidbody.mass * legRightLow.transform.position +
				 legLeft.rigidbody.mass * legLeft.transform.position +
				 legLeftLow.rigidbody.mass * legLeftLow.transform.position +
				 footRight.rigidbody.mass * footRight.transform.position +
				 footLeft.rigidbody.mass * footLeft.transform.position)
				/
				(root.rigidbody.mass + body.rigidbody.mass +
				 head.rigidbody.mass + armRight.rigidbody.mass +
				 armRightLow.rigidbody.mass + armLeft.rigidbody.mass +
				 armLeftLow.rigidbody.mass + legRight.rigidbody.mass +
				 legRightLow.rigidbody.mass + legLeft.rigidbody.mass +
				 legLeftLow.rigidbody.mass + footRight.rigidbody.mass +
				 footLeft.rigidbody.mass);

			COMP.position = _centerOfMassPoint;
		}

		public void StrainArms(Arms arms)
		{
			if (arms == Arms.Left || arms == Arms.Both)
			{
				armLeft.joint.angularXDrive = ReachStiffness;
				armLeft.joint.angularYZDrive = ReachStiffness;
				armLeftLow.joint.angularXDrive = ReachStiffness;
				armLeftLow.joint.angularYZDrive = ReachStiffness;
				armLeftLow.joint.targetRotation = Quaternion.identity;
			}

			if (arms == Arms.Right || arms == Arms.Both)
			{
				armRight.joint.angularXDrive = ReachStiffness;
				armRight.joint.angularYZDrive = ReachStiffness;
				armRightLow.joint.angularXDrive = ReachStiffness;
				armRightLow.joint.angularYZDrive = ReachStiffness;
				armRightLow.joint.targetRotation = Quaternion.identity;
			}
		}

		public void RelaxArms(Arms arms)
		{
			if (arms == Arms.Left || arms == Arms.Both)
			{
				armLeft.joint.angularXDrive = DriveOff;
				armLeft.joint.angularYZDrive = DriveOff;
				armLeft.joint.targetRotation = armLeft.originalRotation;
				armLeftLow.joint.angularXDrive = DriveOff;
				armLeftLow.joint.angularYZDrive = DriveOff;
				armLeftLow.joint.targetRotation = armLeftLow.originalRotation;
			}

			if (arms == Arms.Right || arms == Arms.Both)
			{
				armRight.joint.angularXDrive = DriveOff;
				armRight.joint.angularYZDrive = DriveOff;
				armRight.joint.targetRotation = armRight.originalRotation;
				armRightLow.joint.angularXDrive = DriveOff;
				armRightLow.joint.angularYZDrive = DriveOff;
				armRightLow.joint.targetRotation = armRightLow.originalRotation;
			}
		}
	}
}