using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class BuildSpace : MonoBehaviour
{
    readonly static Dictionary<int, Cluster> _merger = new();
    public static IEnumerable<Cluster> Clusters { get; private set; }
    public static IEnumerable<int> ClusterIDs { get; private set; }
    public static void ClearClusters() { _merger.Clear(); }
    public static Cluster GetCluster(int id) => _merger[id];
    public static bool TryGetCluster(int id, out Cluster cluster) => _merger.TryGetValue(id, out cluster);
    public static int ClusterCount() => _merger.Count;

    static event Action<int, int> Merged;
    readonly static System.Random Random = new(1);


    [SerializeField] List<BuildSpace> _links;
    int _clusterID = int.MinValue;
    public int ClusterID => _clusterID;

    [SerializeField] bool _showGizmos;
    [SerializeField] Color color = Color.red;

    bool _isFirst = true;

    void Awake()//ADD CELLS FROM LINKS
    {
        Clusters ??= _merger.Values;
        ClusterIDs ??= _merger.Keys;
        
        if (!_isFirst) return;

        _clusterID = gameObject.GetInstanceID();
        
        Cluster startCluster = new (Random, _clusterID);
        _merger.Add(_clusterID, startCluster );

        foreach (BuildSpace link in _links)
        {
            link._isFirst = false;
            link._clusterID = _clusterID;
            Vector2Int gridPos = GetGridPosition(link.transform);

            startCluster.Add(new BuildCell(gridPos));
        }

        _isFirst = false;
        
    }
    void Start()
    {
        StartCoroutine(DestroySelf());

    }
    void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        Gizmos.color = color;
        Vector3 dir = .3f * WorldSpawner.CellWidth * (transform.position - transform.parent.position).normalized;
        Gizmos.DrawRay(transform.parent.position, dir);
        Gizmos.DrawSphere(transform.parent.position+ dir, .04f);
    }

    void OnEnable()
    {
        Merged += OnMerge;
    }

    void OnDisable()
    {
        Merged -= OnMerge;
    }

    [ContextMenu("DebugGridPos")]
    public void DebugGridPos()
    {
        Debug.Log($"{GetGridPosition(transform)}");
    }

    Vector2Int GetGridPosition(Transform target)
    {
        Vector3 dir = (target.position - target.parent.position);
        if (dir.magnitude > float.Epsilon) dir = .3f * WorldSpawner.CellWidth * dir.normalized;

        Vector3 worldPos = dir + target.parent.position;

        return new Vector2Int(
            Mathf.FloorToInt(worldPos.x * (3 / WorldSpawner.CellWidth)),
            Mathf.FloorToInt(worldPos.z * (3 / WorldSpawner.CellWidth))
        );
    }

    void OnTriggerEnter(Collider other)
    {
        Vector2Int gridPos = GetGridPosition(other.transform);

        bool isInCluster = _merger[_clusterID].ContainsKey(gridPos);

        if (isInCluster) return;

        var otherBuildSpace = other.GetComponentInParent<BuildSpace>();

        if(otherBuildSpace._clusterID != _clusterID)
        {
            Merge(this, otherBuildSpace);   
        }

        bool isInSyncedCluster = _merger[_clusterID].ContainsKey(gridPos);

        if (isInSyncedCluster) return;
        
        BuildCell otherData = new(gridPos);
        Cluster syncedCluster = _merger[_clusterID];
        syncedCluster.Add(otherData);
    }

    IEnumerator DestroySelf()
    {
        yield return new WaitForSeconds(.3f);
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }

    void Merge(BuildSpace a, BuildSpace b)
    {
        Cluster clusterA = _merger[a._clusterID];
        Cluster clusterB = _merger[b._clusterID];

        foreach (var bData in clusterB.Cells)
        {
            if (clusterA.ContainsKey(bData.Key)) continue;

            clusterA.Add(bData.Value);
        }

        _merger.Remove(b._clusterID);
        Merged?.Invoke(b._clusterID, a._clusterID);
    }

    void OnMerge(int oldClusterID, int newClusterID)
    {
        if (_clusterID != oldClusterID) return;
        _clusterID = newClusterID;
    }
}
