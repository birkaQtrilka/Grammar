using Demo;
using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;

public class WorldSpawner : MonoBehaviour
{
    [SerializeField] float _size = 100f;
    [SerializeField] WorldGenConfig _config;
    [SerializeField] Transform _tileHolder;

    [SerializeField] bool _generateNewMap;
    [SerializeField] bool _spawnOnStart;
    public UnityEvent<Tile> TileCollapsed;
    public UnityEvent MapGenerated;

    WaitForSeconds _clusterMergingTime = new(.2f);

    [SerializeField] SimpleStock _housePrefab;
    [SerializeField] Transform _housesContainer;
    float _cellWidth;
    [SerializeField] bool _generateHousesOnStart;
    [SerializeField] bool _spawnHousesOnStart;
    void Start()
    {
        if(_spawnOnStart)
            SpawnMap(false);
        StartCoroutine(AfterClusterFormation());
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
            var inst = Instantiate(cell.tile.Prefab,
                transform.position + new Vector3(cell.X * _cellWidth + .5f * _cellWidth, 0, -cell.Y * _cellWidth - .5f * _cellWidth),
                Quaternion.Euler(90, cell.tile.Rotation, 0), _tileHolder);
            SpriteRenderer renderer = inst.GetComponent<SpriteRenderer>();
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = Vector2.one;
            inst.transform.localScale = new Vector3(_cellWidth, _cellWidth);
        }
    }

    IEnumerator AfterClusterFormation()
    {
        yield return _clusterMergingTime;
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
        foreach (Cluster cluster in BuildSpace._merger.Values)
        {
            foreach (House house in cluster.Houses)
            {
                var inst = Instantiate(_housePrefab, _housesContainer);
                Rectangle housePos = house.Rect;
                inst.Width = housePos.Width ;
                inst.Depth = housePos.Height;
                inst.Generate();
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
                inst.transform.position = corner_TL;
            }
        }
    }

    [ContextMenu("GenerateHouses")]
    public void GenerateHouses()
    {
        foreach (Cluster cluster in BuildSpace._merger.Values)
        {
            cluster.GenerateHouses();

        }
    }

    [ContextMenu("SpawnConfigMap")]
    public void SpawnConfigMap()
    {
        BuildSpace.ClearClusters();
        GameObject obj = new GameObject(_tileHolder.name);
        DestroyImmediate(_tileHolder.gameObject);
        _tileHolder = obj.transform;
        SpawnMap(false);
    }

    [ContextMenu("SpawnNewMap")]
    public void SpawnNewMap()
    {
        BuildSpace.ClearClusters();
        GameObject obj = new GameObject(_tileHolder.name);
        DestroyImmediate(_tileHolder.gameObject);
        _tileHolder = obj.transform;
        SpawnMap(true);
    }
}
