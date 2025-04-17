using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

public class WorldSpawner : MonoBehaviour
{
    public static bool TooFar(Vector3 pos) => Vector3.Distance(pos, _cameraSpot.position) > _lodDist;
    public static bool LOD_Enabled { get; private set; }
    static Transform _cameraSpot;
    static float _lodDist;

    [SerializeField] ClusterDataContainer clusterDataContainer;

    [SerializeField] float _size = 100f;
    [SerializeField] WorldGenConfig _config;
    [SerializeField] Transform _tileHolder;

    public UnityEvent<Tile> TileCollapsed;
    public UnityEvent MapGenerated;

    readonly WaitForSeconds _clusterMergingTime = new(.2f);
    float _cellWidth;

    [SerializeField] Transform _housesContainer;
    [SerializeField] bool _generateHousesOnStart;
    [SerializeField] bool _spawnHousesOnStart;
    [SerializeField] Transform _heightCenter;
    [SerializeField] int _minBuildingHeight;
    [SerializeField] int _maxBuildingHeight;
    [SerializeField] AnimationCurve _minHeightCurve;
    [SerializeField] AnimationCurve _heightIncreaseChanceCurve;
    [SerializeField] float _heightChangeRange;
    [SerializeField] Transform _cameraSpotTr;
    [SerializeField] float _lodDistance;
    static WorldSpawner _inst;
    public static float CellWidth => _inst._cellWidth;

    void Start()
    {
        _inst = this;
        _cameraSpot = _cameraSpotTr;
        _lodDist = _lodDistance;
        LOD_Enabled = true;
        StartCoroutine(AfterClusterFormation());
    }

    void OnDrawGizmos()
    {
        if (_heightCenter == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_heightCenter.position, _heightChangeRange);
    }

    void SpawnMap(bool random)
    {
        if (random)
        {
            _config.DestroyMap();
            _config.GenerateGrid(UnityEngine.Random.Range(0, 10000));

        }

        _cellWidth = _size / _config.Grid.GetHorizontalLength();

        foreach (GridCell cell in _config.Grid)
        {
            if (cell.tile.Prefab == null) continue;
            GameObject inst = Instantiate(cell.tile.Prefab,
                _tileHolder.position + new Vector3(
                    cell.X * _cellWidth + .5f * _cellWidth,
                    0,
                    -cell.Y * _cellWidth - .5f * _cellWidth
                    ),
                Quaternion.Euler(90, cell.tile.Rotation, 0), 
                _tileHolder);

            SpriteRenderer renderer = inst.GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = Vector2.one;
            inst.transform.localScale = new Vector3(_cellWidth, _cellWidth, 1);
        }
    }

    IEnumerator AfterClusterFormation()
    {
        yield return _clusterMergingTime;
        clusterDataContainer.Init();
        if(_generateHousesOnStart)
        {
            GenerateHouses();
            if(_spawnHousesOnStart)
                SpawnHouses();
        }

    }

    [ContextMenu("SpawnHouses")]
    void SpawnHouses()
    {
        foreach (KV keyVal in clusterDataContainer.Clusters)
        {
            Cluster cluster = BuildSpace.GetCluster(keyVal.id);
            HouseClusterData data = keyVal.Data as HouseClusterData; 
            if(data.BuildingPrefabs == null || data.BuildingPrefabs.Length == 0)
            {
                Debug.LogWarning("Data is empty");
                continue;
            }
            foreach (House house in cluster.Houses)
            {
                var inst = Instantiate(data.BuildingPrefabs.GetRandomItem(), _housesContainer);
                Rectangle housePos = house.Rect;
                inst.Width = housePos.Width;
                inst.Depth = housePos.Height;
                var randomScale = Random.Range(.4f, .9f);
                float scale = .33333f ;
                inst.transform.localScale = scale * randomScale * Vector3.one;

                Vector3 corner_TL = new
                (
                    housePos.X  + housePos.Width * .5f,
                    0,
                    housePos.Y + housePos.Height * .5f 
                );
                //magic number 3 is the  scale of the cells
                corner_TL *= scale;
                float dist = Vector3.Distance(corner_TL, _heightCenter.position);
                float t = Mathf.Clamp01(dist / _heightChangeRange);
                t = 1 - t;
                float buildingMinHeight = _minHeightCurve.Evaluate(t) * (_maxBuildingHeight - _minBuildingHeight) + _minBuildingHeight;
                float buildingChance = _heightIncreaseChanceCurve.Evaluate(t);

                inst.MinHeight = (int)buildingMinHeight;
                inst.stockContinueChance = buildingChance;
                inst.transform.position = corner_TL;
                inst.Generate();

            }
        }
    }

    [ContextMenu("GenerateHouses")]
    public void GenerateHouses()
    {
        foreach (Cluster cluster in BuildSpace.Clusters)
        {
            cluster.GenerateHouses();

        }
    }

    [ContextMenu("SpawnConfigMap")]
    public void SpawnConfigMap()
    {
        BuildSpace.ClearClusters();
        _tileHolder.DestroyAllChildren(true);
        SpawnMap(false);
    }

    [ContextMenu("SpawnNewMap")]
    public void SpawnNewMap()
    {
        BuildSpace.ClearClusters();
        _tileHolder.DestroyAllChildren(true);
        SpawnMap(true);
    }
}
