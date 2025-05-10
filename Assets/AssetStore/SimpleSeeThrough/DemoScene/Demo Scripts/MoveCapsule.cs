using UnityEngine;

namespace SimpleSeeThroughDemo
{
    public class MoveCapsule : MonoBehaviour
    {
        [SerializeField] GameObject[] demoWalls;
        [SerializeField] Transform[] waypoints;
        [SerializeField] float speed = 1f;
        [SerializeField] float lookSpeed = 90f;
        int currentWaypoint = 0;
        int currentWall = 0;

        void FixedUpdate()
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint].position, speed * Time.fixedDeltaTime);
            
            LookAtTarget();
            if(Vector3.Distance(transform.position, waypoints[currentWaypoint].position) < 0.1f)
            {
                NextWall();
                currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
            }
        }

        void NextWall()
        {
            demoWalls[currentWall].SetActive(false);
            currentWall++;
            if(currentWall >= demoWalls.Length) currentWall = 0;
            demoWalls[currentWall].SetActive(true);
        }

        void LookAtTarget()
        {
            Vector3 direction = waypoints[currentWaypoint].position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);
        }
    }
}