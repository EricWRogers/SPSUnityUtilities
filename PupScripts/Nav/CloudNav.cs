using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CloudNav))]
public class CloudNavEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        CloudNav cloudNav = (CloudNav)target;

        GUILayout.Space(8.0f);

        using (new EditorGUI.DisabledScope(cloudNav.IsBuilding))
        {
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

        if (cloudNav.IsBuilding && GUILayout.Button("Cancel Build"))
        {
            cloudNav.CancelBuild();
        }
    }
}
#endif

public class CloudNav : MonoBehaviour
{
    private static readonly Vector3Int[] NeighborOffsets = BuildNeighborOffsets();

    [Header("References")]
    public GameObject cloudPointPrefab;
    public Transform checkPoint;
    public AStar aStar;

    [Header("Areas")]
    public List<BoxCollider> spawnAreas;
    public List<BoxCollider> excludeAreas;

    [Header("Grid")]
    public int xCount = 50;
    public int yCount = 10;
    public int zCount = 50;
    public float sphereRadius = 1.0f;
    public float nodeSpaceing = 1.0f;
    public LayerMask mask;

    [Header("Build UX")]
    public bool buildAsyncInPlayMode = true;
    public bool instantiateCloudPoints = true;
    [Min(1)] public int buildWorkPerFrame = 750;
    [Min(1)] public int overlapBufferSize = 100;

    public bool IsBuilding => isBuilding;
    public float BuildProgress => buildProgress;
    public string BuildStatus => buildStatus;

    private Coroutine buildRoutine;
    private bool isBuilding;
    private float buildProgress;
    private string buildStatus = string.Empty;

    void Awake()
    {
        if (aStar != null)
            aStar.RefreshCache();
    }

    void Start()
    {
        if (aStar == null)
            return;

        Debug.Log("AStar graph Count: " + aStar.Count);
        aStar.StartWorker();
    }

    void FixedUpdate()
    {
        if (aStar != null)
            aStar.UpdateResponseQueue();
    }

    void OnDestroy()
    {
        CancelBuild();

        if (aStar != null)
            aStar.StopWorker();
    }

    public void SpawnCloud(bool _collisionCheck)
    {
        if (aStar == null)
        {
            Debug.LogError("CloudNav: AStar is not assigned.");
            return;
        }

        CancelBuild();

        if (Application.isPlaying && buildAsyncInPlayMode && isActiveAndEnabled)
        {
            buildRoutine = StartCoroutine(BuildCloudRoutine(_collisionCheck, true));
            return;
        }

        IEnumerator routine = BuildCloudRoutine(_collisionCheck, false);
        while (routine.MoveNext())
        {
        }
    }

    public void SphereCheck()
    {
        if (aStar == null || aStar.Count == 0)
            return;

        SetBuildProgress("Validating reachable cloud", 0.15f);

        int originId = GetOriginId();
        if (originId > -1)
        {
            int removed = aStar.RemoveUnreachableFrom(originId);
            Debug.Log("Cloud Nav: Removed unreachable points: " + removed);
        }

        aStar.RefreshCache();
        ClearCloudObjects();
        IEnumerator instantiateRoutine = InstantiateCloudPoints(false);
        while (instantiateRoutine.MoveNext())
        {
        }

        FinishBuildProgress();
    }

    public void CancelBuild()
    {
        bool hadActiveBuild = buildRoutine != null || isBuilding;

        if (buildRoutine != null)
        {
            StopCoroutine(buildRoutine);
            buildRoutine = null;
        }

        if (isBuilding)
            isBuilding = false;

        if (hadActiveBuild && Application.isPlaying && aStar != null)
            aStar.StartWorker();

#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
    }

    private IEnumerator BuildCloudRoutine(bool collisionCheck, bool allowYield)
    {
        isBuilding = true;

        if (Application.isPlaying)
            aStar.StopWorker();

        SetBuildProgress("Clearing old cloud", 0.0f);

        Debug.Log("Cloud Nav: Removing Old Cloud");
        ClearCloudObjects();
        aStar.ResetGraph();

        if (allowYield)
            yield return null;

        Debug.Log("Cloud Nav: Spawn New Cloud");
        Vector3 center = transform.position;
        int halfXCount = xCount / 2;
        int halfYCount = yCount / 2;
        int halfZCount = zCount / 2;
        int xLength = Mathf.Max(0, halfXCount * 2);
        int yLength = Mathf.Max(0, halfYCount * 2);
        int zLength = Mathf.Max(0, halfZCount * 2);

        int[,,] gridIds = new int[xLength, yLength, zLength];
        int totalGridPoints = Mathf.Max(1, xLength * yLength * zLength);
        int processedGridPoints = 0;
        int frameWork = 0;

        for (int xi = 0; xi < xLength; xi++)
        {
            int x = xi - halfXCount;

            for (int yi = 0; yi < yLength; yi++)
            {
                int y = yi - halfYCount;

                for (int zi = 0; zi < zLength; zi++)
                {
                    int z = zi - halfZCount;
                    Vector3 targetPoint = new Vector3(x, y, z) * nodeSpaceing + center;
                    gridIds[xi, yi, zi] = -1;

                    if (IsSafePoint(targetPoint))
                        gridIds[xi, yi, zi] = aStar.AddPoint(targetPoint);

                    processedGridPoints++;

                    if (ShouldYield(allowYield, ref frameWork))
                    {
                        SetBuildProgress("Building cloud points", Mathf.Lerp(0.05f, 0.35f, processedGridPoints / (float)totalGridPoints));
                        yield return null;
                    }
                }
            }
        }

        SetBuildProgress("Connecting cloud points", 0.35f);
        Debug.Log("Cloud Nav: Connect New Points");

        processedGridPoints = 0;
        frameWork = 0;

        for (int xi = 0; xi < xLength; xi++)
        {
            for (int yi = 0; yi < yLength; yi++)
            {
                for (int zi = 0; zi < zLength; zi++)
                {
                    int id = gridIds[xi, yi, zi];
                    processedGridPoints++;

                    if (id < 0)
                        continue;

                    for (int i = 0; i < NeighborOffsets.Length; i++)
                    {
                        Vector3Int offset = NeighborOffsets[i];
                        int nx = xi + offset.x;
                        int ny = yi + offset.y;
                        int nz = zi + offset.z;

                        if (nx < 0 || nx >= xLength || ny < 0 || ny >= yLength || nz < 0 || nz >= zLength)
                            continue;

                        int neighborId = gridIds[nx, ny, nz];

                        if (neighborId > -1)
                            aStar.ConnectPoints(id, neighborId);
                    }

                    if (ShouldYield(allowYield, ref frameWork))
                    {
                        SetBuildProgress("Connecting cloud points", Mathf.Lerp(0.35f, 0.55f, processedGridPoints / (float)totalGridPoints));
                        yield return null;
                    }
                }
            }
        }

        if (collisionCheck)
        {
            IEnumerator validationRoutine = ValidateCollisions(allowYield);
            while (validationRoutine.MoveNext())
                yield return validationRoutine.Current;
        }

        SetBuildProgress("Refreshing cloud", 0.9f);
        aStar.RefreshCache();

        if (allowYield)
            yield return null;

        IEnumerator instantiateRoutine = InstantiateCloudPoints(allowYield);
        while (instantiateRoutine.MoveNext())
            yield return instantiateRoutine.Current;

        Debug.Log("Cloud Nav: Build complete. Points: " + aStar.Count);
        FinishBuildProgress();

        if (Application.isPlaying)
            aStar.StartWorker();
    }

    private IEnumerator ValidateCollisions(bool allowYield)
    {
        SetBuildProgress("Checking blocked paths", 0.55f);
        Debug.Log("Raycast per connection");

        int totalEdges = Mathf.Max(1, CountUniqueEdges());
        int checkedEdges = 0;
        int frameWork = 0;

        for (int n = 0; n < aStar.graph.Count; n++)
        {
            for (int a = 0; a < aStar.graph[n].adjacentPointIDs.Count;)
            {
                int adjacentId = aStar.graph[n].adjacentPointIDs[a];

                if (adjacentId <= n)
                {
                    a++;
                    continue;
                }

                checkedEdges++;

                if (Physics.Linecast(aStar.graph[n].position, aStar.graph[adjacentId].position))
                {
                    aStar.DisconnectPoints(n, adjacentId);
                }
                else
                {
                    a++;
                }

                if (ShouldYield(allowYield, ref frameWork))
                {
                    SetBuildProgress("Checking blocked paths", Mathf.Lerp(0.55f, 0.7f, checkedEdges / (float)totalEdges));
                    yield return null;
                }
            }
        }

        SetBuildProgress("Checking blocked points", 0.7f);
        Debug.Log("Spherecast per connection");

        Collider[] collisions = new Collider[Mathf.Max(1, overlapBufferSize)];
        HashSet<int> blockedPointIds = new HashSet<int>();
        int originId = GetOriginId();
        frameWork = 0;

        for (int n = 0; n < aStar.graph.Count; n++)
        {
            if (n == originId)
                continue;

            if (aStar.graph[n].adjacentPointIDs.Count < 26 &&
                Physics.OverlapSphereNonAlloc(aStar.graph[n].position, sphereRadius, collisions, mask, QueryTriggerInteraction.Collide) > 0)
            {
                blockedPointIds.Add(n);
            }

            if (ShouldYield(allowYield, ref frameWork))
            {
                SetBuildProgress("Checking blocked points", Mathf.Lerp(0.7f, 0.8f, n / (float)Mathf.Max(1, aStar.graph.Count)));
                yield return null;
            }
        }

        int removedBlockedPoints = aStar.RemovePoints(blockedPointIds);
        Debug.Log("Cloud Nav: Removed blocked points: " + removedBlockedPoints);

        SetBuildProgress("Validating reachable cloud", 0.8f);
        Debug.Log("Cloud Nav: Validate Points");

        originId = GetOriginId();
        if (originId > -1)
        {
            int removedUnreachablePoints = aStar.RemoveUnreachableFrom(originId);
            Debug.Log("Cloud Nav: Removed unreachable points: " + removedUnreachablePoints);
        }

        SetBuildProgress("Validating reachable cloud", 0.88f);

        if (allowYield)
            yield return null;
    }

    private IEnumerator InstantiateCloudPoints(bool allowYield)
    {
        ClearCloudObjects();

        if (instantiateCloudPoints == false || cloudPointPrefab == null)
            yield break;

        int frameWork = 0;
        int pointCount = Mathf.Max(1, aStar.Count);

        for (int g = 0; g < aStar.Count; g++)
        {
            Instantiate(cloudPointPrefab, aStar.graph[g].position, Quaternion.identity, transform);

            if (ShouldYield(allowYield, ref frameWork))
            {
                SetBuildProgress("Drawing cloud points", Mathf.Lerp(0.9f, 0.99f, g / (float)pointCount));
                yield return null;
            }
        }
    }

    private bool IsSafePoint(Vector3 targetPoint)
    {
        bool safe = false;

        if (spawnAreas != null)
        {
            for (int i = 0; i < spawnAreas.Count; i++)
            {
                if (spawnAreas[i] != null && spawnAreas[i].ClosestPoint(targetPoint) == targetPoint)
                    safe = true;
            }
        }

        if (excludeAreas != null)
        {
            for (int i = 0; i < excludeAreas.Count; i++)
            {
                if (excludeAreas[i] != null && excludeAreas[i].ClosestPoint(targetPoint) == targetPoint)
                    safe = false;
            }
        }

        return safe;
    }

    private void ClearCloudObjects()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (ShouldKeepChildWhenClearing(child))
                continue;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private bool ShouldKeepChildWhenClearing(GameObject child)
    {
        if (child == null)
            return true;

        if (checkPoint != null && child == checkPoint.gameObject)
            return true;

        return false;
    }

    private int CountUniqueEdges()
    {
        int edgeCount = 0;

        for (int n = 0; n < aStar.graph.Count; n++)
        {
            for (int a = 0; a < aStar.graph[n].adjacentPointIDs.Count; a++)
            {
                if (aStar.graph[n].adjacentPointIDs[a] > n)
                    edgeCount++;
            }
        }

        return edgeCount;
    }

    private int GetOriginId()
    {
        if (aStar == null || aStar.Count == 0)
            return -1;

        if (checkPoint == null)
        {
            Debug.LogWarning("CloudNav: Check Point is not assigned, so reachability validation was skipped.");
            return -1;
        }

        return aStar.GetClosestPoint(checkPoint.position);
    }

    private bool ShouldYield(bool allowYield, ref int frameWork)
    {
        if (allowYield == false)
            return false;

        frameWork++;

        if (frameWork < Mathf.Max(1, buildWorkPerFrame))
            return false;

        frameWork = 0;
        return true;
    }

    private void SetBuildProgress(string status, float progress)
    {
        buildStatus = status;
        buildProgress = Mathf.Clamp01(progress);

#if UNITY_EDITOR
        if (Application.isPlaying == false)
            EditorUtility.DisplayProgressBar("Cloud Nav", status, buildProgress);
#endif
    }

    private void FinishBuildProgress()
    {
        SetBuildProgress("Cloud ready", 1.0f);
        isBuilding = false;
        buildRoutine = null;

#if UNITY_EDITOR
        EditorUtility.ClearProgressBar();
#endif
    }

    private static Vector3Int[] BuildNeighborOffsets()
    {
        List<Vector3Int> offsets = new List<Vector3Int>(13);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    if (x > 0 || (x == 0 && y > 0) || (x == 0 && y == 0 && z > 0))
                        offsets.Add(new Vector3Int(x, y, z));
                }
            }
        }

        return offsets.ToArray();
    }
}
