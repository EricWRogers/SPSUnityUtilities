using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine.UI;




#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CloudNav))]
public class CloudNavEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CloudNav cloudNav = (CloudNav)target;

        if (GUILayout.Button("Spawn Cloud"))
        {
            cloudNav.SpawnCloud(false);
        }

        if (GUILayout.Button("Spawn And Check"))
        {
            cloudNav.SpawnCloud(true);
        }

        if (GUILayout.Button("Sphere Check"))
        {
            cloudNav.SphereCheck();
        }
    }
}
#endif

public class CloudNav : MonoBehaviour
{
    public GameObject cloudPointPrefab;
    public Transform checkPoint;
    public AStar aStar;

    public List<BoxCollider> spawnAreas;
    public List<BoxCollider> excludeAreas;

    public int xCount = 50;
    public int yCount = 10;
    public int zCount = 50;
    public float sphereRadius = 1.0f;
    public float nodeSpacing = 1.0f;
    public LayerMask mask;

    void Awake()
    {
        aStar.RefreshCashe();
    }

    void Start()
    {
        Debug.Log("AStar graph Count: " + aStar.Count);
        aStar.StartWorker();
    }

    void FixedUpdate()
    {
        aStar.UpdateResponseQueue();
    }

    void OnDestroy()
    {
        aStar.StopWorker();
    }

    public void SpawnCloud(bool _collisionCheck)
    {
        Debug.Log("Cloud Nav: Removing Old Cloud");
        aStar.ResetGraph();
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        Debug.Log("Cloud Nav: Spawn New Cloud");
        int halfXCount = xCount / 2;
        int halfYCount = yCount / 2;
        int halfZCount = zCount / 2;

        for (int x = -halfXCount; x < halfXCount; x++)
        {
            for (int y = -halfYCount; y < halfYCount; y++)
            {
                for (int z = -halfZCount; z < halfZCount; z++)
                {
                    Vector3 targetPoint = new Vector3(x, y, z) * nodeSpacing;

                    bool safe = false;

                    for (int i = 0; i < spawnAreas.Count; i++)
                        if (spawnAreas[i].ClosestPoint(targetPoint) == targetPoint)
                            safe = true;

                    for (int i = 0; i < excludeAreas.Count; i++)
                        if (excludeAreas[i].ClosestPoint(targetPoint) != targetPoint)
                            safe = false;

                    if (safe)
                        aStar.AddPoint(targetPoint);
                }
            }
        }

        aStar.RefreshCashe();

        Debug.Log("Cloud Nav: Connect New Points");
        for (int x = -halfXCount; x < halfXCount; x++)
        {
            for (int y = -halfYCount; y < halfYCount; y++)
            {
                for (int z = -halfZCount; z < halfZCount; z++)
                {
                    int id = aStar.GetPointByPosition(new Vector3(x, y, z) * nodeSpacing);

                    if (id < 0)
                    {
                        continue;
                    }

                    if (id >= aStar.Count)
                    {
                        Debug.LogError("CloudNav: id is not valid and greater than the max");
                        continue;
                    }

                    int forward = aStar.GetPointByPosition(new Vector3(x, y, z + 1) * nodeSpacing);
                    int forwardLeft = aStar.GetPointByPosition(new Vector3(x - 1, y, z + 1) * nodeSpacing);
                    int forwardRight = aStar.GetPointByPosition(new Vector3(x + 1, y, z + 1) * nodeSpacing);
                    int left = aStar.GetPointByPosition(new Vector3(x - 1, y, z) * nodeSpacing);
                    int right = aStar.GetPointByPosition(new Vector3(x + 1, y, z) * nodeSpacing);
                    int back = aStar.GetPointByPosition(new Vector3(x, y, z - 1) * nodeSpacing);
                    int backLeft = aStar.GetPointByPosition(new Vector3(x - 1, y, z - 1) * nodeSpacing);
                    int backRight = aStar.GetPointByPosition(new Vector3(x + 1, y, z - 1) * nodeSpacing);

                    int upForward = aStar.GetPointByPosition(new Vector3(x, y + 1, z + 1) * nodeSpacing);
                    int upForwardLeft = aStar.GetPointByPosition(new Vector3(x - 1, y + 1, z + 1) * nodeSpacing);
                    int upForwardRight = aStar.GetPointByPosition(new Vector3(x + 1, y + 1, z + 1) * nodeSpacing);
                    int upLeft = aStar.GetPointByPosition(new Vector3(x - 1, y + 1, z) * nodeSpacing);
                    int up = aStar.GetPointByPosition(new Vector3(x, y + 1, z) * nodeSpacing);
                    int upRight = aStar.GetPointByPosition(new Vector3(x + 1, y + 1, z) * nodeSpacing);
                    int upBack = aStar.GetPointByPosition(new Vector3(x, y + 1, z - 1) * nodeSpacing);
                    int upBackLeft = aStar.GetPointByPosition(new Vector3(x - 1, y + 1, z - 1) * nodeSpacing);
                    int upBackRight = aStar.GetPointByPosition(new Vector3(x + 1, y + 1, z - 1) * nodeSpacing);

                    int downForward = aStar.GetPointByPosition(new Vector3(x, y - 1, z + 1) * nodeSpacing);
                    int downForwardLeft = aStar.GetPointByPosition(new Vector3(x - 1, y - 1, z + 1) * nodeSpacing);
                    int downForwardRight = aStar.GetPointByPosition(new Vector3(x + 1, y - 1, z + 1) * nodeSpacing);
                    int downLeft = aStar.GetPointByPosition(new Vector3(x - 1, y - 1, z) * nodeSpacing);
                    int down = aStar.GetPointByPosition(new Vector3(x, y - 1, z) * nodeSpacing);
                    int downRight = aStar.GetPointByPosition(new Vector3(x + 1, y - 1, z) * nodeSpacing);
                    int downBack = aStar.GetPointByPosition(new Vector3(x, y - 1, z - 1) * nodeSpacing);
                    int downBackLeft = aStar.GetPointByPosition(new Vector3(x - 1, y - 1, z - 1) * nodeSpacing);
                    int downBackRight = aStar.GetPointByPosition(new Vector3(x + 1, y - 1, z - 1) * nodeSpacing);

                    if (forward > -1) aStar.ConnectPoints(id, forward);
                    if (forwardLeft > -1) aStar.ConnectPoints(id, forwardLeft);
                    if (forwardRight > -1) aStar.ConnectPoints(id, forwardRight);
                    if (left > -1) aStar.ConnectPoints(id, left);
                    if (right > -1) aStar.ConnectPoints(id, right);
                    if (back > -1) aStar.ConnectPoints(id, back);
                    if (backLeft > -1) aStar.ConnectPoints(id, backLeft);
                    if (backRight > -1) aStar.ConnectPoints(id, backRight);

                    if (upForward > -1) aStar.ConnectPoints(id, upForward);
                    if (upForwardLeft > -1) aStar.ConnectPoints(id, upForwardLeft);
                    if (upForwardRight > -1) aStar.ConnectPoints(id, upForwardRight);
                    if (upLeft > -1) aStar.ConnectPoints(id, upLeft);
                    if (up > -1) aStar.ConnectPoints(id, up);
                    if (upRight > -1) aStar.ConnectPoints(id, upRight);
                    if (upBack > -1) aStar.ConnectPoints(id, upBack);
                    if (upBackLeft > -1) aStar.ConnectPoints(id, upBackLeft);
                    if (upBackRight > -1) aStar.ConnectPoints(id, upBackRight);

                    if (downForward > -1) aStar.ConnectPoints(id, downForward);
                    if (downForwardLeft > -1) aStar.ConnectPoints(id, downForwardLeft);
                    if (downForwardRight > -1) aStar.ConnectPoints(id, downForwardRight);
                    if (downLeft > -1) aStar.ConnectPoints(id, downLeft);
                    if (down > -1) aStar.ConnectPoints(id, down);
                    if (downRight > -1) aStar.ConnectPoints(id, downRight);
                    if (downBack > -1) aStar.ConnectPoints(id, downBack);
                    if (downBackLeft > -1) aStar.ConnectPoints(id, downBackLeft);
                    if (downBackRight > -1) aStar.ConnectPoints(id, downBackRight);
                }
            }
        }

        if (_collisionCheck)
        {
            Debug.Log("Raycast per connection");

            for (int n = 0; n < aStar.graph.Count; n++)
            {
                for (int a = 0; a < aStar.graph[n].adjacentPointIDs.Count;)
                {

                    if (Physics.Linecast(aStar.graph[n].position, aStar.graph[aStar.graph[n].adjacentPointIDs[a]].position))
                    {
                        aStar.DisconnectPoints(n, aStar.graph[n].adjacentPointIDs[a]);
                    }
                    else
                    {
                        a++;
                    }
                }
            }

            Debug.Log("Spherecast per connection");

            Collider[] collisions = new Collider[100];

            for (int n = 0; n < aStar.graph.Count; n++)
            {
                if (aStar.graph[n].position == checkPoint.position)
                    continue;

                if (aStar.graph[n].adjacentPointIDs.Count < 26)
                {
                    if (Physics.OverlapSphereNonAlloc(aStar.graph[n].position, sphereRadius, collisions, mask, QueryTriggerInteraction.Collide) > 0)//(ray, sphereRadius, out hit, sphereRadius))
                    {
                        aStar.RemovePoint(n);
                        n--;
                    }
                }

            }

            Debug.Log("Cloud Nav: Validate Points");

            //int originId = aStar.GetClosestPoint(checkPoint.position); // this can be bad

            List<Vector3> path = new List<Vector3>();
            for (int x = -halfXCount; x < halfXCount; x++)
            {
                for (int y = -halfYCount; y < halfYCount; y++)
                {
                    for (int z = -halfZCount; z < halfZCount; z++)
                    {
                        Vector3 targetPosition = new Vector3(x, y, z) * nodeSpacing;

                        if (targetPosition == checkPoint.position)
                            continue;

                        int startId = aStar.GetClosestPoint(targetPosition);

                        if (startId == -1 || aStar.graph[startId].position == checkPoint.position)
                            continue;

                        if (aStar.graph[startId].adjacentPointIDs.Count == 0)
                        {
                            aStar.RemovePoint(startId);
                            continue;
                        }

                        int originId = aStar.GetClosestPoint(checkPoint.position); // this can be bad

                        path = aStar.GetPath(startId, originId);

                        if (path.Count == 0)
                        {
                            //Debug.Log("Remove id: " + startId);
                            aStar.RemovePoint(startId);
                        }
                    }
                }
            }
        }

        aStar.RefreshCashe();

        for (int g = 0; g < aStar.Count; g++)
        {
            GameObject pointObject = Instantiate(
                        cloudPointPrefab,
                        aStar.graph[g].position,
                        Quaternion.identity,
                        transform);
        }
    }

    public void SphereCheck()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        int halfXCount = xCount / 2;
        int halfYCount = yCount / 2;
        int halfZCount = zCount / 2;



        Debug.Log("Cloud Nav: Validate Points");

        List<Vector3> path = new List<Vector3>();
        for (int x = -halfXCount; x < halfXCount; x++)
        {
            for (int y = -halfYCount; y < halfYCount; y++)
            {
                for (int z = -halfZCount; z < halfZCount; z++)
                {
                    Vector3 targetPosition = new Vector3(x, y, z) * nodeSpacing;

                    if (targetPosition == checkPoint.position)
                        continue;

                    int startId = aStar.GetClosestPoint(targetPosition);

                    if (startId == -1 || aStar.graph[startId].position == checkPoint.position)
                        continue;

                    if (aStar.graph[startId].adjacentPointIDs.Count == 0)
                    {
                        aStar.RemovePoint(startId);
                        continue;
                    }

                    int originId = aStar.GetClosestPoint(checkPoint.position);

                    //Debug.Log("count " + aStar.Count + " id " + startId + " origin " + originId);

                    path = aStar.GetPath(startId, originId);

                    if (path.Count == 0)
                    {
                        //Debug.Log("Remove id: " + startId);
                        aStar.RemovePoint(startId);
                    }
                }
            }
        }

        aStar.RefreshCashe();

        for (int g = 0; g < aStar.Count; g++)
        {
            GameObject pointObject = Instantiate(
                        cloudPointPrefab,
                        aStar.graph[g].position,
                        Quaternion.identity,
                        transform);
        }
    }
}
