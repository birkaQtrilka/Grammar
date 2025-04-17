using Demo;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField] ClusterDataContainer clusterDataContainer;

    [SerializeField] float _size = 100f;
    [SerializeField] WorldGenConfig _config;
    [SerializeField] Transform _tileHolder;

    [SerializeField] bool _generateNewMap;
    [SerializeField] bool _spawnOnStart;
    public UnityEvent<Tile> TileCollapsed;
    public UnityEvent MapGenerated;

    readonly WaitForSeconds _clusterMergingTime = new(.2f);
    float _cellWidth;

    [SerializeField] SimpleStock _housePrefab;
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
    public static Transform _cameraSpot { get;private set; }
    static float _lodDist;
    public static bool TooFar(Vector3 pos) => Vector3.Distance(pos, _cameraSpot.position) > _lodDist;
    public static bool LOD_Enabled { get; private set; }
    //public static GameObject GetPrefabByDistance(Vector3 pos) =
    void Start()
    {
        _cameraSpot = _cameraSpotTr;
        _lodDist = _lodDistance;
        LOD_Enabled = true;
        if(_spawnOnStart)
            SpawnMap(false);
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
                transform.position + new Vector3(
                    cell.X * _cellWidth + .5f * _cellWidth,
                    0,
                    -cell.Y * _cellWidth - .5f * _cellWidth
                    ),
                Quaternion.Euler(90, cell.tile.Rotation, 0), _tileHolder);

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
        foreach (Cluster cluster in BuildSpace.Clusters)
        {
            foreach (House house in cluster.Houses)
            {
                var inst = Instantiate(_housePrefab, _housesContainer);
                Rectangle housePos = house.Rect;
                inst.Width = housePos.Width;
                inst.Depth = housePos.Height;
                var randomScale = Random.Range(.4f, .9f);
                inst.transform.localScale = new Vector3(.33333f, .33333f, .33333f)* randomScale;

                Vector3 corner_TL = new
                (
                    housePos.X  + housePos.Width * .5f,
                    0.81f,
                    housePos.Y + housePos.Height * .5f 
                );
                //magic number 3 is the  scale of the cells
                corner_TL *= .33333f;
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
        GameObject obj = new(_tileHolder.name);
        DestroyImmediate(_tileHolder.gameObject);
        _tileHolder = obj.transform;
        SpawnMap(false);
    }

    [ContextMenu("SpawnNewMap")]
    public void SpawnNewMap()
    {
        BuildSpace.ClearClusters();
        GameObject obj = new(_tileHolder.name);
        DestroyImmediate(_tileHolder.gameObject);
        _tileHolder = obj.transform;
        SpawnMap(true);
    }
}
