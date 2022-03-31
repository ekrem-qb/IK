using UnityEngine;


//-------------------------------------------------------------
//--APR Player
//--Feet Contact
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
    public class FeetContact : MonoBehaviour
    {
        public APRController APR_Player;
        private ParticleSystem _particle;

        private void Awake()
        {
            _particle = this.GetComponentInChildren<ParticleSystem>();
        }

        //Alert APR player when feet colliders enter ground object layer
        void OnCollisionEnter(Collision col)
        {
            if (!APR_Player.isJumping)
            {
                if (APR_Player.inAir)
                {
                    if (col.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        APR_Player.PlayerLanded();
                    }
                }
                else
                {
                    if (APR_Player.isRagdoll)
                    {
                        APR_Player.healthManager.health -= APR_Player.fallDamage / 2;
                    }
                    else if (_particle)
                    {
                        _particle.Play();
                    }
                }
            }
        }
    }
}