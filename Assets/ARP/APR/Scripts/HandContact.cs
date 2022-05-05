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
				if (joint && !APR_Player.grabbed)
				{
					Destroy(joint);
				}
			}
		}

		//Grab on collision when input is used
		void OnCollisionEnter(Collision col)
		{
			if (APR_Player.useControls)
			{
				if (col.gameObject.CompareTag("CanBeGrabbed") && col.gameObject.layer != APR_Player.gameObject.layer && !joint)
				{
					if (APR_Player.isGrabbing && !joint && !weaponManager.weapon)
					{
						joint = this.gameObject.AddComponent<FixedJoint>();
						joint.breakForce = Mathf.Infinity;
						joint.connectedBody = col.rigidbody;
						APR_Player.grabbed = col.transform;
					}
				}
			}
		}
	}
}