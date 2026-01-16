using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AStar
{
    public int Count => graph.Count;
    [SerializeField]
    public List<AStarNode> graph;
    private Dictionary<Vector3, int> m_umap = new Dictionary<Vector3, int>();

    private Queue<AStarRequest> requestQueue = new Queue<AStarRequest>();
    private Queue<AStarResponse> responseQueue = new Queue<AStarResponse>();
    private Thread workerThread;
    private bool working = false;

    // cache
    private List<int> searchingSet = new List<int>();
    private List<int> hasSearchedSet = new List<int>();

    public void StartWorker()
    {
        if (working)
            return;

        working = true;
        
        ThreadStart threadStart = delegate
        {
            while (true)
            {
                AStarRequest request;

                lock (requestQueue)
                {
                    if (requestQueue.Count == 0)
                        continue;

                    request = requestQueue.Dequeue();
                }

                List<Vector3> payload = GetPath(request.idFrom, request.idTo);

                AStarResponse response = new AStarResponse(request.callback, payload);

                lock (responseQueue)
                {
                    responseQueue.Enqueue(response);
                }
            }
        };

        workerThread = new Thread(threadStart);
        workerThread.Start();
    }

    public void RequestPath(Action<List<Vector3>> _callback, int _idFrom, int _idTo)
    {
        //AStarRequest request = new AStarRequest(_callback, _idFrom, _idTo);
        //requestQueue.Enqueue(request);

        ThreadStart threadStart = delegate
        {
            lock (requestQueue)
            {
                AStarRequest request = new AStarRequest(_callback, _idFrom, _idTo);
                requestQueue.Enqueue(request);
            }
        };

        new Thread(threadStart).Start();

        working = true;
    }

    public void UpdateResponseQueue()
    {
        for (int i = 0; i < responseQueue.Count; i++)
        {
            AStarResponse response = responseQueue.Dequeue();
            response.callback(response.parameter);
        }
    }

    public void StopWorker()
    {
        if (workerThread.IsAlive)
        {
            workerThread.Abort();
        }
    }

    public void ResetGraph()
    {
        graph.Clear();
        m_umap.Clear();
    }
    public void RefreshCashe()
    {
        m_umap.Clear();

        for (int i = 0; i < graph.Count; i++)
        {
            m_umap.Add(graph[i].position, i);
        }
    }
    public int AddPoint(Vector3 _position)
    {
        int id = graph.Count;
        graph.Add(new AStarNode());
        graph[id].position = _position;
        m_umap.Add(_position, id);
        return id;
    }
    public List<Vector3> GetPath(int _idFrom, int _idTo)
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

        {
            int graphSize = graph.Count;
            AStarNode node;
            for (int i = 0; i < graphSize; i++)
            {
                node = graph[i];
                node.f = 0.0f;
                node.g = 0.0f;
                node.h = 0.0f;
                node.predecessorID = -1;
            }
        }

        searchingSet.Clear();
        hasSearchedSet.Clear();
        int graphNodeIndex;

        searchingSet.Add(_idFrom);

        while (searchingSet.Count > 0)
        {
            int lowestPath = 0;

            for (int i = 0; i < searchingSet.Count; i++)
            {
                if (graph[searchingSet[lowestPath]].f > graph[searchingSet[i]].f)
                {
                    lowestPath = i;
                }
            }

            AStarNode node = graph[searchingSet[lowestPath]];
            graphNodeIndex = searchingSet[lowestPath];

            if (_idTo == searchingSet[lowestPath])
            {
                return BuildPath(node);
            }

            hasSearchedSet.Add(searchingSet[lowestPath]);

            searchingSet.RemoveAt(lowestPath);

            List<int> neighborIDs = node.adjacentPointIDs;

            for (int i = 0; i < neighborIDs.Count; i++)
            {
                //if (graph.Count <= neighborIDs[i])
                //    continue;

                AStarNode neighborNode = graph[neighborIDs[i]];

                if (hasSearchedSet.Contains(neighborIDs[i]) == false)
                {
                    if (graphNodeIndex == neighborIDs[i])
                    {
                        continue;
                    }

                    float currentG = node.g + Vector3.Distance(node.position, neighborNode.position);

                    bool isNewPath = false;

                    if (searchingSet.Contains(neighborIDs[i]))
                    {
                        if (currentG < neighborNode.g)
                        {
                            isNewPath = true;
                            neighborNode.g = currentG;
                        }
                    }
                    else
                    {
                        isNewPath = true;
                        neighborNode.g = currentG;
                        searchingSet.Add(neighborIDs[i]);
                    }

                    if (isNewPath)
                    {
                        neighborNode.h = Vector3.Distance(neighborNode.position, graph[_idTo].position);
                        neighborNode.f = neighborNode.g + neighborNode.h;
                        neighborNode.predecessorID = graphNodeIndex;
                    }
                }
            }
        }

        return path;
    }
    public int GetClosestPoint(Vector3 _position)
    {
        if (graph.Count == 0)
        {
            Debug.LogError("AStar::GetClosetPoint was called before a point was added to the graph.");
            return -1;
        }

        int id = 0;
        float distance;
        float minDistance = float.MaxValue;

        for (int i = 1; i < graph.Count; i++)
        {
            distance = (_position - graph[i].position).sqrMagnitude;

            if (minDistance > distance)
            {
                id = i;
                minDistance = distance;
            }
        }

        return id;
    }
    public int GetPointByPosition(Vector3 _position)
    {
        for (int i = 0; i < graph.Count; i++)
            if (graph[i].position == _position)
                return i;

        return -1;
        //if (m_umap.ContainsKey(_position))
        //    return m_umap[_position];
        //else
        //    return -1;
    }
    public void RemovePoint(int _id)
    {
        if (graph.Count <= _id && _id < 0)
        {
            Debug.LogError("AStar::RemovePoint id has not been added to the graph.");
            return;
        }

        Vector3 position = graph[_id].position;

        // unconnect
        for (int i = 0; i < graph[_id].adjacentPointIDs.Count; i++)
        {
            int adjacentPointID = graph[_id].adjacentPointIDs[i];
            graph[adjacentPointID].adjacentPointIDs.Remove(_id);
        }

        graph.RemoveAt(_id);
        //RefreshCashe();

        //m_umap.Remove(position);

        for (int n = 0; n < graph.Count; n++)
        {
            for (int a = 0; a < graph[n].adjacentPointIDs.Count; a++)
            {
                if (graph[n].adjacentPointIDs[a] > _id)
                    graph[n].adjacentPointIDs[a]--;
            }
        }
    }
    public void ConnectPoints(int _idFrom, int _idTo)
    {
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
    public void DisconnectPoints(int _idFrom, int _idTo)
    {
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
    public bool ArePointsConnected(int _idFrom, int _idTo)
    {
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
    public bool ValidPoint(Vector3 _position)
    {
        return m_umap.ContainsKey(_position);
    }
    private List<Vector3> BuildPath(AStarNode _node)
    {
        List<Vector3> path = new List<Vector3>();

        AStarNode currentNode = _node;

        while (currentNode.predecessorID != -1)
        {
            path.Insert(0, currentNode.position);
            currentNode = graph[currentNode.predecessorID];
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
