using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BF_RandomMovement : MonoBehaviour
{
    public CharacterController characterController;
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;
    public float raycastDistance = 10f;

    private Vector3 originPos;
    private Vector3 randomDirection;

    private void Start()
    {
        originPos = this.transform.position;
        GetRandomDirection();
        StartCoroutine(WaitMovement());
    }
    private void OnEnable()
    {
        GetRandomDirection();
        StartCoroutine(WaitMovement());
    }
    private void Update()
    {
        Vector3 randomMovement = randomDirection * moveSpeed * Time.deltaTime;
        randomMovement = new Vector3(randomMovement.x, 0, randomMovement.z);
        MoveCharacter(randomMovement);
        RotateCharacter(randomMovement);
    }

    private Vector3 GetRandomDirection()
    {
        bool isThereGround = false;
        int n = 0;
        while (!isThereGround)
        {
            randomDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            isThereGround = CheckGround(characterController.transform.position + randomDirection * moveSpeed * 3, out float yPos);
            if(!isThereGround)
            {
                randomDirection = (this.transform.position - originPos).normalized;
            }
            n++;
            if (n > 50)
                break;
        }

        return randomDirection;
    }

    private void MoveCharacter(Vector3 movement)
    {
        characterController.Move(movement);
    }

    private void RotateCharacter(Vector3 movement)
    {
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            characterController.transform.rotation = Quaternion.RotateTowards(characterController.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private bool CheckGround(Vector3 randomMovement,out float yPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(randomMovement + Vector3.up * 7f, Vector3.down, out hit, raycastDistance))
        {
            yPos = hit.point.y;
            return true;
        }
        yPos = 0;
        return false;
    }


    private IEnumerator WaitMovement()
    {
        for (; ; )
        {
            yield return new WaitForSeconds(3f);
            GetRandomDirection();
        }
    }
}