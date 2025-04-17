using System.Collections.Generic;
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
    public void Init()
    {
        _clusters = BuildSpace.ClusterIDs.Select(id => new KV() { id = id }).ToList();
    }
}
