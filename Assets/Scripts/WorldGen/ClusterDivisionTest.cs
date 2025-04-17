using System.Collections;
using System.Linq;
using UnityEngine;

public class ClusterDivisionTest : MonoBehaviour
{
    [SerializeField] Transform _housesContainer;
    [SerializeField] Transform _clusterContainer;
    [SerializeField] GameObject _housePrefab;
    [SerializeField] Color _minMaxClr = Color.red;
    [SerializeField] bool _showIndividualClusters;
    [SerializeField] bool _drawCells;
    [SerializeField] bool _generateOnSpawn;
    int _currentDebuggedCluster;
    void Start()
    {
        StartCoroutine(a());   
    }

    IEnumerator a()
    {
        yield return new WaitForFixedUpdate();
        if(_generateOnSpawn) GenerateHouses();
    }



    [ContextMenu("GenerateHouses")]
    public void GenerateHouses()
    {
        foreach (Cluster cluster in BuildSpace.Clusters)
        {
            cluster.GenerateHouses();
        }
    }
    

    [ContextMenu("ClusterData")]
    public void ShowClusterData()
    {
        Debug.Log("Clusters Count: " + BuildSpace.ClusterCount());
        foreach (Cluster cluster in BuildSpace.Clusters)
        {
            Debug.Log("Cluster id: " + cluster.ID);
        }
    }

    void OnDrawGizmos()
    {
        var clusters = BuildSpace.Clusters;
        if (clusters == null) return;   
        int i = 0;
        Gizmos.color = _minMaxClr;
        foreach (Cluster cluster in clusters)
        {
            if(_drawCells) 
                cluster.DrawCells(i);
            i++;

        }
        Gizmos.color = Color.white;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            _currentDebuggedCluster++;
            if (_currentDebuggedCluster >= BuildSpace.ClusterCount()) _currentDebuggedCluster = 0;
        }
    }
}
