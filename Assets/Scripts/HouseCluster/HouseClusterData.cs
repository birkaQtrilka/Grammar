using Demo;
using UnityEngine;
[CreateAssetMenu(menuName ="Stefan/ClusterData")]
public class HouseClusterData : ClusterData
{
    [field: SerializeField] public SimpleStock[] BuildingPrefabs { get; private set; }

}