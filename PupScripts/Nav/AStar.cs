using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AStar
{
    public int Count => graph == null ? 0 : graph.Count;
    [SerializeField]
    public List<AStarNode> graph;
    private Dictionary<Vector3, int> m_umap = new Dictionary<Vector3, int>();

    private readonly object graphLock = new object();
    private Queue<AStarRequest> requestQueue = new Queue<AStarRequest>();
    private Queue<AStarResponse> responseQueue = new Queue<AStarResponse>();
    private AutoResetEvent requestAvailable = new AutoResetEvent(false);
    private Thread workerThread;
    private bool stopWorker = false;

    public void StartWorker()
    {
        if (workerThread != null && workerThread.IsAlive)
            return;

        stopWorker = false;
        
        ThreadStart threadStart = delegate
        {
            while (stopWorker == false)
            {
                requestAvailable.WaitOne();

                while (TryDequeueRequest(out AStarRequest request))
                {
                    List<Vector3> payload = GetPath(request.idFrom, request.idTo);
                    AStarResponse response = new AStarResponse(request.callback, payload);

                    lock (responseQueue)
                    {
                        responseQueue.Enqueue(response);
                    }
                }
            }

        };

        workerThread = new Thread(threadStart)
        {
            IsBackground = true,
            Name = "AStar Worker"
        };
        workerThread.Start();
    }

    public void RequestPath(Action<List<Vector3>> _callback, int _idFrom, int _idTo)
    {
        StartWorker();

        lock (requestQueue)
        {
            requestQueue.Enqueue(new AStarRequest(_callback, _idFrom, _idTo));
        }

        requestAvailable.Set();
    }

    public void UpdateResponseQueue()
    {
        while (TryDequeueResponse(out AStarResponse response))
        {
            response.callback?.Invoke(response.parameter);
        }
    }

    public void StopWorker()
    {
        if (workerThread == null)
            return;

        stopWorker = true;
        requestAvailable.Set();

        if (workerThread.IsAlive)
        {
            workerThread.Join(100);
        }

        workerThread = null;
    }

    public void ResetGraph()
    {
        lock (graphLock)
        {
            EnsureGraph();
            graph.Clear();
            m_umap.Clear();
        }
    }
    public void RefreshCashe()
    {
        lock (graphLock)
        {
            EnsureGraph();
            RefreshCacheUnsafe();
        }
    }
    public void RefreshCache()
    {
        RefreshCashe();
    }
    public int AddPoint(Vector3 _position)
    {
        lock (graphLock)
        {
            EnsureGraph();
            int id = graph.Count;
            graph.Add(new AStarNode());
            graph[id].position = _position;
            m_umap[_position] = id;
            return id;
        }
    }
    public List<Vector3> GetPath(int _idFrom, int _idTo)
    {
        lock (graphLock)
        {
            EnsureGraph();
            return GetPathUnsafe(_idFrom, _idTo);
        }
    }
    public int GetClosestPoint(Vector3 _position)
    {
        lock (graphLock)
        {
            EnsureGraph();

            if (graph.Count == 0)
            {
                Debug.LogError("AStar::GetClosetPoint was called before a point was added to the graph.");
                return -1;
            }

            int id = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < graph.Count; i++)
            {
                float distance = (_position - graph[i].position).sqrMagnitude;

                if (minDistance > distance)
                {
                    id = i;
                    minDistance = distance;
                }
            }

            return id;
        }
    }
    public int GetPointByPosition(Vector3 _position)
    {
        lock (graphLock)
        {
            EnsureGraph();
            return m_umap.TryGetValue(_position, out int id) ? id : -1;
        }
    }
    public void RemovePoint(int _id)
    {
        RemovePoints(new HashSet<int> { _id });
    }
    public int RemovePoints(HashSet<int> _pointIds)
    {
        lock (graphLock)
        {
            EnsureGraph();

            if (_pointIds == null || _pointIds.Count == 0)
                return 0;

            bool[] keepPoint = new bool[graph.Count];
            for (int i = 0; i < keepPoint.Length; i++)
                keepPoint[i] = true;

            foreach (int id in _pointIds)
            {
                if (id >= 0 && id < keepPoint.Length)
                    keepPoint[id] = false;
            }

            return RebuildGraphUnsafe(keepPoint);
        }
    }
    public int RemoveUnreachableFrom(int _originId)
    {
        lock (graphLock)
        {
            EnsureGraph();

            if (_originId < 0 || _originId >= graph.Count)
            {
                Debug.LogError("AStar::RemoveUnreachableFrom origin id has not been added to the graph.");
                return 0;
            }

            bool[] reachable = new bool[graph.Count];
            Queue<int> queue = new Queue<int>();
            reachable[_originId] = true;
            queue.Enqueue(_originId);

            while (queue.Count > 0)
            {
                int nodeId = queue.Dequeue();
                List<int> adjacentPointIDs = graph[nodeId].adjacentPointIDs;

                for (int i = 0; i < adjacentPointIDs.Count; i++)
                {
                    int adjacentId = adjacentPointIDs[i];

                    if (adjacentId < 0 || adjacentId >= graph.Count || reachable[adjacentId])
                        continue;

                    reachable[adjacentId] = true;
                    queue.Enqueue(adjacentId);
                }
            }

            return RebuildGraphUnsafe(reachable);
        }
    }
    public void ConnectPoints(int _idFrom, int _idTo)
    {
        lock (graphLock)
        {
            EnsureGraph();

            if (graph.Count <= _idFrom || _idFrom < 0)
            {
                Debug.LogError("AStar::ConnectPoints _idFrom has not been added to the graph.");
                return;
            }

            if (graph.Count <= _idTo || _idTo < 0)
            {
                Debug.LogError("AStar::ConnectPoints _idTo has not been added to the graph.");
                return;
            }

            if (_idFrom == _idTo)
            {
                Debug.LogError("AStar::ConnectPoints _idFrom is _idTo.");
                return;
            }

            if (graph[_idFrom].adjacentPointIDs.Contains(_idTo) == false)
                graph[_idFrom].adjacentPointIDs.Add(_idTo);

            if (graph[_idTo].adjacentPointIDs.Contains(_idFrom) == false)
                graph[_idTo].adjacentPointIDs.Add(_idFrom);
        }
    }
    public void DisconnectPoints(int _idFrom, int _idTo)
    {
        lock (graphLock)
        {
            EnsureGraph();

            if (graph.Count <= _idFrom || _idFrom < 0)
            {
                Debug.LogError("AStar::DisconnectPoints _idFrom has not been added to the graph.");
                return;
            }

            if (graph.Count <= _idTo || _idTo < 0)
            {
                Debug.LogError("AStar::DisconnectPoints _idTo has not been added to the graph.");
                return;
            }

            if (_idFrom == _idTo)
            {
                Debug.LogError("AStar::DisconnectPoints _idFrom is _idTo.");
                return;
            }

            graph[_idFrom].adjacentPointIDs.Remove(_idTo);
            graph[_idTo].adjacentPointIDs.Remove(_idFrom);
        }
    }
    public bool ArePointsConnected(int _idFrom, int _idTo)
    {
        lock (graphLock)
        {
            EnsureGraph();

            if (graph.Count <= _idFrom || _idFrom < 0)
            {
                Debug.LogError("AStar::ArePointsConnected _idFrom has not been added to the graph.");
                return false;
            }

            if (graph.Count <= _idTo || _idTo < 0)
            {
                Debug.LogError("AStar::ArePointsConnected _idTo has not been added to the graph.");
                return false;
            }

            for (int i = 0; i < graph[_idFrom].adjacentPointIDs.Count; i++)
            {
                if (graph[_idFrom].adjacentPointIDs[i] == _idTo)
                {
                    return true;
                }
            }

            return false;
        }
    }
    public bool ValidPoint(Vector3 _position)
    {
        lock (graphLock)
        {
            EnsureGraph();
            return m_umap.ContainsKey(_position);
        }
    }

    private bool TryDequeueRequest(out AStarRequest _request)
    {
        lock (requestQueue)
        {
            if (requestQueue.Count > 0)
            {
                _request = requestQueue.Dequeue();
                return true;
            }
        }

        _request = default;
        return false;
    }

    private bool TryDequeueResponse(out AStarResponse _response)
    {
        lock (responseQueue)
        {
            if (responseQueue.Count > 0)
            {
                _response = responseQueue.Dequeue();
                return true;
            }
        }

        _response = default;
        return false;
    }

    private void EnsureGraph()
    {
        if (graph == null)
            graph = new List<AStarNode>();
    }

    private void RefreshCacheUnsafe()
    {
        m_umap.Clear();

        for (int i = 0; i < graph.Count; i++)
        {
            m_umap[graph[i].position] = i;
        }
    }

    private List<Vector3> GetPathUnsafe(int _idFrom, int _idTo)
    {
        List<Vector3> path = new List<Vector3>();

        if (graph.Count <= _idFrom || _idFrom < 0)
        {
            Debug.LogError("AStar::GetPath _idFrom " + _idFrom + "has not been added to the graph.");
            return path;
        }

        if (graph.Count <= _idTo || _idTo < 0)
        {
            Debug.LogError("AStar::GetPath _idTo has not been added to the graph.");
            return path;
        }

        int graphSize = graph.Count;
        List<int> searchingSet = new List<int>();
        bool[] inSearchingSet = new bool[graphSize];
        bool[] hasSearchedSet = new bool[graphSize];
        int[] predecessorIDs = new int[graphSize];
        float[] gCosts = new float[graphSize];
        float[] fCosts = new float[graphSize];

        for (int i = 0; i < graphSize; i++)
        {
            predecessorIDs[i] = -1;
            gCosts[i] = float.MaxValue;
            fCosts[i] = float.MaxValue;
        }

        searchingSet.Add(_idFrom);
        inSearchingSet[_idFrom] = true;
        gCosts[_idFrom] = 0.0f;
        fCosts[_idFrom] = Vector3.Distance(graph[_idFrom].position, graph[_idTo].position);

        while (searchingSet.Count > 0)
        {
            int lowestPath = 0;

            for (int i = 0; i < searchingSet.Count; i++)
            {
                if (fCosts[searchingSet[lowestPath]] > fCosts[searchingSet[i]])
                {
                    lowestPath = i;
                }
            }

            int graphNodeIndex = searchingSet[lowestPath];

            if (_idTo == graphNodeIndex)
            {
                return BuildPath(graphNodeIndex, predecessorIDs);
            }

            searchingSet.RemoveAt(lowestPath);
            inSearchingSet[graphNodeIndex] = false;
            hasSearchedSet[graphNodeIndex] = true;

            AStarNode node = graph[graphNodeIndex];
            List<int> neighborIDs = node.adjacentPointIDs;

            for (int i = 0; i < neighborIDs.Count; i++)
            {
                int neighborID = neighborIDs[i];

                if (neighborID < 0 || neighborID >= graphSize || hasSearchedSet[neighborID])
                    continue;

                if (graphNodeIndex == neighborID)
                {
                    continue;
                }

                AStarNode neighborNode = graph[neighborID];
                float currentG = gCosts[graphNodeIndex] + Vector3.Distance(node.position, neighborNode.position);

                if (inSearchingSet[neighborID] && currentG >= gCosts[neighborID])
                    continue;

                if (inSearchingSet[neighborID] == false)
                {
                    searchingSet.Add(neighborID);
                    inSearchingSet[neighborID] = true;
                }

                gCosts[neighborID] = currentG;
                fCosts[neighborID] = currentG + Vector3.Distance(neighborNode.position, graph[_idTo].position);
                predecessorIDs[neighborID] = graphNodeIndex;
            }
        }

        return path;
    }

    private int RebuildGraphUnsafe(bool[] _keepPoint)
    {
        int originalCount = graph.Count;

        if (_keepPoint == null || _keepPoint.Length != originalCount)
            return 0;

        int[] remap = new int[originalCount];
        int keptCount = 0;

        for (int i = 0; i < originalCount; i++)
        {
            if (_keepPoint[i])
            {
                remap[i] = keptCount;
                keptCount++;
            }
            else
            {
                remap[i] = -1;
            }
        }

        if (keptCount == originalCount)
            return 0;

        List<AStarNode> rebuiltGraph = new List<AStarNode>(keptCount);

        for (int i = 0; i < originalCount; i++)
        {
            if (remap[i] == -1)
                continue;

            rebuiltGraph.Add(new AStarNode
            {
                position = graph[i].position
            });
        }

        for (int i = 0; i < originalCount; i++)
        {
            if (remap[i] == -1)
                continue;

            AStarNode rebuiltNode = rebuiltGraph[remap[i]];
            List<int> adjacentPointIDs = graph[i].adjacentPointIDs;

            for (int a = 0; a < adjacentPointIDs.Count; a++)
            {
                int adjacentId = adjacentPointIDs[a];

                if (adjacentId < 0 || adjacentId >= originalCount || remap[adjacentId] == -1)
                    continue;

                int rebuiltAdjacentId = remap[adjacentId];

                if (rebuiltAdjacentId != remap[i] && rebuiltNode.adjacentPointIDs.Contains(rebuiltAdjacentId) == false)
                    rebuiltNode.adjacentPointIDs.Add(rebuiltAdjacentId);
            }
        }

        graph = rebuiltGraph;
        RefreshCacheUnsafe();
        return originalCount - keptCount;
    }

    private List<Vector3> BuildPath(int _endId, int[] _predecessorIDs)
    {
        List<Vector3> path = new List<Vector3>();

        int currentId = _endId;

        while (_predecessorIDs[currentId] != -1)
        {
            path.Insert(0, graph[currentId].position);
            currentId = _predecessorIDs[currentId];
        }

        return path;
    }
}

struct AStarRequest
{
    public readonly Action<List<Vector3>> callback;
    public readonly int idFrom;
    public readonly int idTo;

    public AStarRequest(Action<List<Vector3>> _callback, int _idFrom, int _idTo)
    {
        callback = _callback;
        idFrom = _idFrom;
        idTo = _idTo;
    }
}

struct AStarResponse
{
    public readonly Action<List<Vector3>> callback;
    public readonly List<Vector3> parameter;

    public AStarResponse(Action<List<Vector3>> _callback, List<Vector3> _parameter)
    {
        callback = _callback;
        parameter = _parameter;
    }
}
