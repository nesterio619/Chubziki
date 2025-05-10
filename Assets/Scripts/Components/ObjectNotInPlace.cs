using System.Collections;
using Core;
using UnityEngine;

public class ObjectNotInPlace : TriggerHandler // TODO: удаляем. Вместо данного скрипта расширяем ObjectInPlace
{
    //[SerializeField] private Transform TargetObjectPosition;
    [SerializeField] private float AllowedTimeOutOfPosition = 5f;
    [SerializeField] private Rigidbody ObjectRigidbody;

    public void StartTimer()
    {
        Player.Instance.StartCoroutine(Timer());
    }
    
    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(AllowedTimeOutOfPosition);
        ReturnObjectInPlace();
    }

    private void ReturnObjectInPlace()
    {
        if (ObjectRigidbody != null)
        {
            ObjectRigidbody.velocity = Vector3.zero;
            ObjectRigidbody.angularVelocity = Vector3.zero;
        }
        
        transform.position = transform.parent.position;
        
    }
}
