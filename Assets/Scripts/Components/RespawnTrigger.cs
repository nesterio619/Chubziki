using Core;
using UnityEngine;

namespace Components
{
    public class RespawnTrigger : TriggerHandler
    {
        [SerializeField]private bool respawnOnEnter = true;
        [SerializeField]private bool doEventsBeforeRespawn = false;
    
        protected override void OnTriggerEnter(Collider other)
        {
            if(!LayerAllowed(other.gameObject))
                return;
            
            if(doEventsBeforeRespawn)
                base.OnTriggerEnter(other);

            if(respawnOnEnter)
                RespawnPlayer();
        
            if(!doEventsBeforeRespawn)
                base.OnTriggerEnter(other);
        }

        public void RespawnPlayer()
        {
            if(Player.Instance == null)
                return;
        
            Player.Instance.RespawnPlayer();
        }
    }
}
