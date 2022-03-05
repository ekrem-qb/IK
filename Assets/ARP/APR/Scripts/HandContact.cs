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
        public APRController APR;

        //Is left or right hand
        public bool Left;

        //Have joint/grabbed
        public FixedJoint joint;

        GunManager gunManager;

        void Awake()
        {
            gunManager = APR.COMP.GetComponent<GunManager>();
        }

        void Update()
        {
            if (APR.useControls)
            {
                //Left Hand
                //On input release destroy joint
                if (Left)
                {
                    if (joint && Input.GetAxisRaw(APR.reachLeft) == 0)
                    {
                        Destroy(joint);
                    }
                }

                //Right Hand
                //On input release destroy joint
                if (!Left)
                {
                    if (joint && Input.GetAxisRaw(APR.reachRight) == 0)
                    {
                        Destroy(joint);
                    }
                }
            }
        }

        //Grab on collision when input is used
        void OnCollisionEnter(Collision col)
        {
            if (APR.useControls)
            {
                //Left Hand
                if (Left)
                {
                    if (col.gameObject.tag == "CanBeGrabbed" && col.gameObject.layer != LayerMask.NameToLayer(APR.thisPlayerLayer) && !joint)
                    {
                        if (Input.GetAxisRaw(APR.reachLeft) != 0 && !joint && !APR.punchingLeft && !gunManager.gunLeft)
                        {
                            joint = this.gameObject.AddComponent<FixedJoint>();
                            joint.breakForce = Mathf.Infinity;
                            joint.connectedBody = col.gameObject.GetComponent<Rigidbody>();
                        }
                    }
                }

                //Right Hand
                if (!Left)
                {
                    if (col.gameObject.tag == "CanBeGrabbed" && col.gameObject.layer != LayerMask.NameToLayer(APR.thisPlayerLayer) && !joint)
                    {
                        if (Input.GetAxisRaw(APR.reachRight) != 0 && !joint && !APR.punchingRight && !gunManager.gunRight)
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
