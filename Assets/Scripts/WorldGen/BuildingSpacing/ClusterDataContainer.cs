using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
[System.Serializable]
public struct KV
{
    public int id;
    public ClusterData Data;
}

[System.Serializable]
public class ClusterDataContainer
{
    [SerializeField] List<KV> _clusters;
    public ReadOnlyCollection<KV> Clusters { get; private set; }
    public void Init()
    {
        ClusterData defaultData = Resources.Load<ClusterData>("Normal"); 
        _clusters = BuildSpace.ClusterIDs.Select(id => new KV() { id = id, Data = defaultData }).ToList();
        Clusters = new ReadOnlyCollection<KV>(_clusters);
    }
}
