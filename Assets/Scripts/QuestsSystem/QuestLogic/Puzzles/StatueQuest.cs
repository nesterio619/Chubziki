using Actors;
using Components.Mechanism;
using Core;
using QuestsSystem.Base;
using System.Collections;
using System.Linq;
using UnityEngine;


namespace QuestsSystem.QuestLogic
{
    public class StatueQuest : PuzzleQuestLogic
    {
        private const float Fall_Threshold = 0.9f;
        private const float Update_Interval = 0.5f;

        private Transform _bridgeTransform;
        private Actor _statueActor;

        private Coroutine _checkingCoroutine;

        protected internal override void OnAccept()
        {
            if(questIsCompleted) return;

            base.OnAccept();

            foreach (var actor in AllCreatedActors)
            {
                if (actor is RBLibraBridgeActor)
                    _bridgeTransform = actor.gameObject.transform;

                if (actor is RigidbodyActor)
                    _statueActor = actor;
            }

            _checkingCoroutine = Player.Instance.StartCoroutine(CheckIfStatueFell());
        }

        private IEnumerator CheckIfStatueFell()
        {
            while (!questIsCompleted)
            {
                var dotProduct = Vector3.Dot(_statueActor.transform.up, Vector3.up);
                if (dotProduct < Fall_Threshold)
                {
                    DirectionPoint.Instance.Show(_bridgeTransform.position);
                    _checkingCoroutine = null;
                    yield break;
                }

                yield return new WaitForSeconds(Update_Interval);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            DirectionPoint.Instance.Hide();
            if(_checkingCoroutine != null)
                Player.Instance.StopCoroutine(_checkingCoroutine);
        }
    }
}