using Actors;
using Components.Mechanism;
using Core.Extensions;
using Core.Utilities;
using UnityEngine;

namespace QuestsSystem.QuestLogic
{
    public class BridgeAndBallQuest : PuzzleQuestLogic
    {
        private RigidbodyActor _ballActor;
        private BridgeActor _bridgeActor;
        private PressureButtonActor _buttonActor;

        protected internal override void OnAccept()
        {
            if (questIsCompleted) return;

            base.OnAccept();

            foreach(var actor in AllCreatedActors)
            {
                if(actor is RigidbodyActor rbActor)
                    _ballActor = rbActor;

                if(actor is PressureButtonActor buttonActor)
                    _buttonActor = buttonActor;

                if (actor is BridgeActor bridgeActor)
                    _bridgeActor = bridgeActor;
            }

            _buttonActor.OnActivate += ActivateBridge;
        }

        private void ActivateBridge()
        {
            _ballActor.gameObject.SetActive(false);
            _bridgeActor.ExternalActivation(true);
        }

        public override void OnComplete(bool victory)
        {
            _buttonActor.OnActivate -= ActivateBridge;

            if(victory)
                questLocation.SaveActorPose(_ballActor.gameObject);

            base.OnComplete(victory);
        }

    }
}