using UnityEngine;


//-------------------------------------------------------------
//--APR Player
//--Impact Contact
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
    public class ImpactContact : MonoBehaviour
    {
        public APRController APR;

        //Alert APR Player when collision enters with specified force amount
        void OnCollisionEnter(Collision col)
        {

            //Knockout by impact
            if (APR.canBeKnockoutByImpact && col.relativeVelocity.magnitude > APR.requiredForceToBeKO)
            {
                APR.ActivateRagdoll();

                if (APR.SoundSource)
                {
                    if (!APR.SoundSource.isPlaying && APR.Hits.Length > 0)
                    {
                        int i = Random.Range(0, APR.Hits.Length);
                        APR.SoundSource.clip = APR.Hits[i];
                        APR.SoundSource.Play();
                    }
                }
            }

            //Sound on impact
            if (col.relativeVelocity.magnitude > APR.ImpactForce)
            {

                if (APR.SoundSource)
                {
                    if (!APR.SoundSource.isPlaying && APR.Impacts.Length > 0)
                    {
                        int i = Random.Range(0, APR.Impacts.Length);
                        APR.SoundSource.clip = APR.Impacts[i];
                        APR.SoundSource.Play();
                    }
                }
            }
        }
    }
}
