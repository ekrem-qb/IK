using UnityEngine;


//-------------------------------------------------------------
//--APR Player
//--Hand Contact
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
	public class HandContact : MonoBehaviour
	{
		public APRController APR_Player;

		//Is left or right hand
		public bool Left;

		//Have joint/grabbed
		public FixedJoint joint;

		WeaponManager weaponManager;

		void Awake()
		{
			weaponManager = APR_Player.COMP.GetComponent<WeaponManager>();
		}

		void Update()
		{
			if (APR_Player.useControls)
			{
				//Left Hand
				//On input release destroy joint
				if (joint && !APR_Player.IsGrabbing)
				{
					if (Left)
					{
						Destroy(joint);
					}
					else
					{
						Destroy(joint);
					}
				}
			}
		}

		//Grab on collision when input is used
		void OnCollisionEnter(Collision col)
		{
			if (APR_Player.useControls)
			{
				//Left Hand
				if (Left)
				{
					if (col.gameObject.tag == "CanBeGrabbed" && col.gameObject.layer != LayerMask.NameToLayer(APR_Player.thisPlayerLayer) && !joint)
					{
						if (APR_Player.IsGrabbing && !joint && !(weaponManager.weapon is Gun))
						{
							joint = this.gameObject.AddComponent<FixedJoint>();
							joint.breakForce = Mathf.Infinity;
							joint.connectedBody = col.gameObject.GetComponent<Rigidbody>();
						}
					}
				}
				else
				{
					if (col.gameObject.tag == "CanBeGrabbed" && col.gameObject.layer != LayerMask.NameToLayer(APR_Player.thisPlayerLayer) && !joint)
					{
						if (APR_Player.IsGrabbing && !joint && !weaponManager.weapon)
						{
							joint = this.gameObject.AddComponent<FixedJoint>();
							joint.breakForce = Mathf.Infinity;
							joint.connectedBody = col.gameObject.GetComponent<Rigidbody>();
						}
					}
				}
			}
		}
	}
}