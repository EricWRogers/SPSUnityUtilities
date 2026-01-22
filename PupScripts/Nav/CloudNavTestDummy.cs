using System.Collections.Generic;
using UnityEngine;

public class CloudNavTestDummy : MonoBehaviour
{
    public CloudNav cloudNav;
    public Transform target;
    public List<Vector3> path;
    public int startId;
    public int endId;
    public int targetIndex;
    public float speed = 100.0f;

    void Start()
    {
        GetNewPath();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (path.Count == 0)
        {
            GetNewPath();
            return;
        }

        if (transform.position == path[targetIndex])
        {
            targetIndex++;

            if (targetIndex >= path.Count)
            {
                GetNewPath();
                return;
            }
        }

        Vector3 direction = (path[targetIndex] - transform.position).normalized;
        Vector3 movePosition = transform.position + (direction * speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, path[targetIndex]) < Vector3.Distance(transform.position, movePosition))
        {
            transform.position = path[targetIndex];
            return;
        }

        transform.position = movePosition;
    }

    void GetNewPath()
    {
        if (target == null)
            return;
        
        path.Clear();

        targetIndex = 0;

        startId = cloudNav.aStar.GetClosestPoint(transform.position);
        endId = cloudNav.aStar.GetClosestPoint(target.position);

        path = cloudNav.aStar.GetPath(startId, endId);

    }
}
