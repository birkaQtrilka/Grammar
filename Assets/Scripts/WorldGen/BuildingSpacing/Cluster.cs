using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Cluster
{
    public Dictionary<Vector2Int, BuildCell> Cells = new();
    public ReadOnlyCollection<House> Houses { get; private set; }
    public int MinimumHousePerimeter { get; set; }
    public MinMax MinMax => _minMax;
    MinMax _minMax;


    readonly Queue<House> _toDoHouses = new();
    readonly List<House> _houses = new();


    readonly System.Random _random;
    //readonly int _minHouses;
    readonly int _maxArea;
    readonly int _maxSideDifference;
    readonly int _splitChance;

    public readonly int ID;
    UnityEngine.Color _debugClr;

    public Cluster(int minimumHousePerimeter, System.Random random, int id,  /*int minHouses = 2,*/ int maxArea = 10, int maxSideDifference = 1, int splitChance = 3)
    {
        MinimumHousePerimeter = minimumHousePerimeter;
        _random = random;
        //_minHouses = minHouses;
        _maxArea = maxArea;
        _maxSideDifference = maxSideDifference;
        _splitChance = splitChance;
        ID = id;
        _minMax = new MinMax(true);
        _debugClr  = UnityEngine.Random.ColorHSV(0f,1f,1f,1f,1f,1f);
        Houses = new(_houses);
    }

    public bool ContainsKey(Vector2Int k)
    {
        return Cells.ContainsKey(k);
    }

    public void Add(BuildCell cell)
    {
        Cells.Add(cell.Position, cell);
    }

    public void Remove(BuildCell cell)
    {
        Cells.Remove(cell.Position);
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
        if(ID == 42286)
        {

        }

        Vector2Int firstPos = Cells.Keys.OrderBy(p => p.x + p.y).FirstOrDefault();

        House newHouse;
        bool result=false;
        int safety = 50;
        do
        {
            newHouse = Iterate(firstPos);

            AddHouse(newHouse);
            result = TryGetCell_Y(newHouse,true, out Vector2Int pos);
            //if (!result) result = TryGetCell_Y(newHouse,false, out pos);
            if (!result) result = TryGetCell_X(newHouse,true, out pos);
            //if (!result) result = TryGetCell_X(newHouse,false, out pos);
            firstPos = pos;
        } while (result && safety -- >0);

    }
    
    House Iterate(Vector2Int startPos)
    {
        House house = new(new Rectangle(startPos.x, startPos.y, 1, 1));
        int safety = 50;
        while (CanExpand_X(house, true) && safety-- > 0)
        {
            Expand_X(house, true);
        }
        while (CanExpand_X(house, false) && safety-- > 0)
        {
            Expand_X(house, false);
        }
        safety = 50;
        while (CanExpand_Y(house, true) && safety-- > 0)
        {
            Expand_Y(house, true);
        }
        while (CanExpand_Y(house, false) && safety-- > 0)
        {
            Expand_Y(house, false);
        }
        return house;
    }

    bool TryGetCell_Y(House house,bool up, out Vector2Int pos)
    {
        int upPos = GetYOffset(house, up);
        for (int x = 0; x < house.Rect.Width; x++)
        {
            pos = new Vector2Int(house.Rect.X + x, upPos);
            if (IsAvailableCell(pos)) return true; 
        }
        pos = default;
        return false;
    }

    bool TryGetCell_X(House house, bool right, out Vector2Int pos)
    {
        int rightPos = GetXOffset(house, right);
        for (int y = 0; y < house.Rect.Height; y++)
        {
            pos = new Vector2Int(rightPos, house.Rect.Y + y);
            if (IsAvailableCell(pos)) return true; ;
        }
        pos = default;
        return false;
    }
    
    void Expand_Y(House house, bool up)
    {
        int upPos = GetYOffset(house, up);
        for (int x = 0; x < house.Rect.Width; x++)
            Cells[new Vector2Int(house.Rect.X + x, upPos)].MarkTaken();

        Rectangle expandedRect = new(house.Rect.Location, new Size(house.Rect.Width, house.Rect.Height + 1));
        house.Rect = expandedRect;
    }

    void Expand_X(House house, bool right)
    {
        int rightPos = GetXOffset(house, right);
        for (int y = 0; y < house.Rect.Height; y++)
            Cells[new Vector2Int(rightPos, house.Rect.Y + y)].MarkTaken();
        //I need to expand this right
        Rectangle expandedRect = new(house.Rect.Location, new Size(house.Rect.Width + 1, house.Rect.Height));
        house.Rect = expandedRect;
    }

    bool CanExpand_X(House house, bool right)
    {
        int rightPos = GetXOffset(house, right);
        if (GetSideDiff(house.Rect.Width + 1, house.Rect.Height) > _maxSideDifference) return false;

        for (int y = 0; y < house.Rect.Height; y++)
        {
            if(!IsAvailableCell(new Vector2Int(rightPos, house.Rect.Y + y))) return false;
        }

        return true;
    }

    bool CanExpand_Y(House house, bool up)
    {
        int upPos = GetYOffset(house, up);
        if (GetSideDiff(house.Rect.Width, house.Rect.Height + 1) > _maxSideDifference) return false;

        for (int x = 0; x < house.Rect.Width; x++)
        {
            if(!IsAvailableCell(new Vector2Int(house.Rect.X + x, upPos))) return false;
        }

        return true;
    }

    bool IsAvailableCell(Vector2Int pos)
    {
        return Cells.TryGetValue(pos, out BuildCell cell) && !cell.Taken;
    }

    int GetSideDiff(int w, int h)
    {
        return Mathf.Abs(w - h);
    }

    int GetYOffset(House house, bool up)
    {
        return house.Rect.Y + (up ? house.Rect.Height : -1);
    }

    int GetXOffset(House house, bool right)
    {
        return house.Rect.X + (right ? house.Rect.Width : -1);
    }

    void AddHouse(House house)
    {
        _houses.Add(house);
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
