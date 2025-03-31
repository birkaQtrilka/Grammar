using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Cluster
{
    public Dictionary<int, BuildCell> Cells { get; private set; } = new();
    public ReadOnlyCollection<House> Houses { get; private set; }
    public int MinimumHousePerimeter { get; set; }
    public MinMax MinMax => _minMax;
    MinMax _minMax;


    readonly Queue<House> _toDoHouses = new();
    readonly List<House> _houses = new();


    readonly System.Random _random;
    readonly int _minHouses;
    readonly int _maxArea;
    readonly int _maxSideDifference;
    readonly int _splitChance;

    public readonly int ID;
    UnityEngine.Color _debugClr;

    public Cluster(int minimumHousePerimeter, System.Random random, int id,  int minHouses = 2, int maxArea = 10, int maxSideDifference = 2, int splitChance = 3)
    {
        MinimumHousePerimeter = minimumHousePerimeter;
        _random = random;
        _minHouses = minHouses;
        _maxArea = maxArea;
        _maxSideDifference = maxSideDifference;
        _splitChance = splitChance;
        ID = id;
        _minMax = new MinMax(true);
        _debugClr  = UnityEngine.Random.ColorHSV(0f,1f,1f,1f,1f,1f);
        Houses = new(_houses);
    }

    public bool ContainsKey(int k)
    {
        return Cells.ContainsKey(k);
    }

    public void Add(int k, BuildCell cell)
    {
        Cells.Add(k, cell);
    }

    public void Remove(int k)
    {
        Cells.Remove(k);
    }

    public void UpdateMinMax(Vector2Int gridPos)
    {
        if (gridPos.x < _minMax.MinX) _minMax.MinX = gridPos.x;
        if (gridPos.x > _minMax.MaxX) _minMax.MaxX = gridPos.x;

        if (gridPos.y < _minMax.MinY) _minMax.MinY = gridPos.y;
        if (gridPos.y > _minMax.MaxY) _minMax.MaxY = gridPos.y;
    }

    public void UpdateMinMax(MinMax minMax)
    {
        if (minMax.MinX < _minMax.MinX) _minMax.MinX = minMax.MinX;
        if (minMax.MaxX > _minMax.MaxX) _minMax.MaxX = minMax.MaxX;

        if (minMax.MinY < _minMax.MinY) _minMax.MinY = minMax.MinY;
        if (minMax.MaxY > _minMax.MaxY) _minMax.MaxY = minMax.MaxY;
    }
    
    public void GenerateHouses()
    {
        int splits = 0;
        House startingCanvas = new
        (
            new Rectangle
            (
                x: _minMax.MinX, 
                y: _minMax.MinY, 
                width: _minMax.MaxX - _minMax.MinX + 1,
                height: _minMax.MaxY - _minMax.MinY + 1 
            )
        );
        //check the 1x1 case (because now, no house is spawned if starting canvas is 1x1)

        _toDoHouses.Enqueue( startingCanvas );
        while (_toDoHouses.Count > 0)
        {
            House house = _toDoHouses.Dequeue();
            //if everything is true, 
            bool requirementsBeforeStoppingDivision =
                splits > _minHouses &&
                house.Rect.Width * house.Rect.Height < _maxArea &&  
                Mathf.Abs(house.Rect.Width - house.Rect.Height) <= _maxSideDifference;
                
            if ( requirementsBeforeStoppingDivision && _random.Next(0, _splitChance) == 1)
            {
                AddHouse( house );
                continue;
            }

            if (house.Rect.Width < house.Rect.Height && CanDivideVertically(house, MinimumHousePerimeter))
            {
                int oneHalf = house.Rect.Height - _random.Next(MinimumHousePerimeter, house.Rect.Height - MinimumHousePerimeter);
                int otherHalf = house.Rect.Height - oneHalf;

                _toDoHouses.Enqueue(new House(new Rectangle(house.Rect.X, house.Rect.Y, house.Rect.Width, oneHalf)));
                _toDoHouses.Enqueue(new House(new Rectangle(house.Rect.X, house.Rect.Y + oneHalf , house.Rect.Width, otherHalf)));
            }
            else if (CanDivideHorizontally(house, MinimumHousePerimeter))
            {
                int oneHalf = house.Rect.Width - _random.Next(MinimumHousePerimeter, house.Rect.Width - MinimumHousePerimeter);
                int otherHalf = house.Rect.Width - oneHalf;
                _toDoHouses.Enqueue(new House(new Rectangle(house.Rect.X, house.Rect.Y, oneHalf, house.Rect.Height)));
                _toDoHouses.Enqueue(new House(new Rectangle(house.Rect.X + oneHalf , house.Rect.Y, otherHalf, house.Rect.Height)));

            }
            else
                AddHouse(house);

            splits += 2;
        }
     //   Debug.Log("Cluster: " + ID + " has " + _houses.Count + " houses");
    }

    void AddHouse(House house)
    {
        _houses.Add(house);
        bool isContained = false;
        foreach (BuildCell cell in Cells.Values.Where(c => Contains(house.Rect, c.Position)))
        {
            isContained = true;
            house.MinMax.UpdateMinMax(cell.Position);
        }
        if (!isContained) _houses.RemoveAt(_houses.Count - 1);

    }

    bool Contains(Rectangle r, Vector2Int p)
    {
        return p.x >= r.X && p.x <= r.X + r.Width &&
               p.y >= r.Y && p.y <= r.Y + r.Height ;
    }

    bool CanDivideVertically(House house, int pMinimumRoomSize)
    {
        return house.Rect.Height - pMinimumRoomSize >= pMinimumRoomSize;
    }

    bool CanDivideHorizontally(House house, int pMinimumRoomSize)
    {
        return house.Rect.Width - pMinimumRoomSize >= pMinimumRoomSize;
    }

    public void Draw(Transform prefab, Transform clusterContainer, Transform housesContainer)
    {
        var containerInst = GameObject.Instantiate(clusterContainer, housesContainer);
        containerInst.localPosition = clusterContainer.localPosition;
        foreach (House item in _houses)
        {
            var inst = GameObject.Instantiate(prefab, containerInst);
            inst.localScale = new Vector3(item.Rect.Width /3f , .1f, item.Rect.Height /3f);

            var corner_TL = new Vector3(item.Rect.X, 1, item.Rect.Y);
            //magic number 3 is the  scale of the cells
            corner_TL /= 3f;
            inst.transform.position = corner_TL;
        }
        
    }
    readonly Vector3[] arr = new Vector3[8];

    public void DrawCells(int i)
    {
        //if (ID == 26644)
        //{
        //    Debug.Log(ID);
        //}
        int j = 0;
        foreach (House house in _houses)
        {
            //float x = house.MinMax.MinX;
            //float y = house.MinMax.MinY;
            //float x1 = (house.MinMax.MaxX + 1);
            //float y1 = (house.MinMax.MaxY + 1);

            float x = house.Rect.X;
            float y = house.Rect.Y;
            float x1 = (house.Rect.X + house.Rect.Width);
            float y1 = (house.Rect.Y + house.Rect.Height);

            var corner_TL = new Vector3(x,  1 + (i + j) * .1f, y) * .3333f;
            var corner_TR = new Vector3(x1, 1 + (i + j) * .1f, y) * .3333f;
            var corner_BR = new Vector3(x1, 1 + (i + j) * .1f, y1) * .3333f;
            var corner_BL = new Vector3(x,  1 + (i + j) * .1f, y1) * .3333f;
            arr[0] = corner_TL;
            arr[1] = corner_TR;

            arr[2] = corner_TR;
            arr[3] = corner_BR;

            arr[4] = corner_BR;
            arr[5] = corner_BL;

            arr[6] = corner_BL;
            arr[7] = corner_TL;

            Gizmos.color = _debugClr;
            Gizmos.DrawLineList(arr);
            j++;
        }
    }

    public void DrawMinMax()
    {
        //if (Cells.Count == 0)
        //{
        //    return;
        //}
        var corner_TL = new Vector3(_minMax.MinX,     1, _minMax.MinY);
        var corner_TR = new Vector3(_minMax.MaxX + 1, 1, _minMax.MinY);
        var corner_BR = new Vector3(_minMax.MaxX + 1, 1, _minMax.MaxY + 1);
        var corner_BL = new Vector3(_minMax.MinX,     1, _minMax.MaxY + 1);

        corner_TL /= 3f;
        corner_TR /= 3f;
        corner_BR /= 3f;
        corner_BL /= 3f;

        //Gizmos.color = _debugClr;
        Handles.color = _debugClr;
        Handles.DrawLine(corner_TL, corner_TR, 3);
        Handles.DrawLine(corner_TR, corner_BR, 3);
        Handles.DrawLine(corner_BR, corner_BL, 3);
        Handles.DrawLine(corner_BL, corner_TL, 3);
    }
}
