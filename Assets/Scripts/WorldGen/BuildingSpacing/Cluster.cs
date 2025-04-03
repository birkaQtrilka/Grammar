using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class Cluster
{
    public Dictionary<Vector2Int, BuildCell> Cells = new();
    public ReadOnlyCollection<House> Houses { get; private set; }
    public int MinimumHousePerimeter { get; set; }
    public MinMax MinMax => _minMax;
    MinMax _minMax;


    readonly Queue<Vector2Int> _toDoHouses = new();
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
        Vector2Int firstPos = Cells.Keys.OrderBy(p => p.x + p.y).FirstOrDefault();
        _toDoHouses.Enqueue(firstPos);
        House newHouse;
        int safety = 1000;
        do
        {
            newHouse = Iterate(_toDoHouses.Dequeue());
            if(newHouse == null) continue;
            AddHouse(newHouse);
            //TODO: shuffle the directions, so it's more random
            if (TryGetCellUp(newHouse, out Vector2Int pos)) _toDoHouses.Enqueue(pos);
            if (TryGetCellDown(newHouse, out pos)) _toDoHouses.Enqueue(pos);
            if (TryGetCellRight(newHouse, out pos)) _toDoHouses.Enqueue(pos);
            if (TryGetCellLeft(newHouse, out pos)) _toDoHouses.Enqueue(pos);
        } while (_toDoHouses.Count != 0 && safety-- > 0);

    }
    
    House Iterate(Vector2Int startPos)
    {
        //check if I can put the house here, if not, return null
        if (!Cells.TryGetValue(startPos, out BuildCell cell) || cell.Taken)
            return null;
        House house = new(new Rectangle(startPos.x, startPos.y, 1, 1));
        Cells[startPos] = new(startPos, true);
        int safety = 50;
        while (CanExpandRight(house) && safety-- > 0)
        {
            ExpandRight(house);
        }
        safety = 50;
        while (CanExpandLeft(house) && safety-- > 0)
        {
            ExpandLeft(house);
        }
        safety = 50;
        while (CanExpandUp(house) && safety-- > 0)
        {
            ExpandUp(house);
        }
        safety = 50;
        while (CanExpandDown(house) && safety-- > 0)
        {
            ExpandDown(house);
        }
        return house;
    }

    bool TryGetCellUp(House house, out Vector2Int pos)
    {
        int upPos = house.Rect.Y + house.Rect.Height;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            pos = new Vector2Int(house.Rect.X + x, upPos);
            if (!Cells.TryGetValue(pos, out BuildCell cell) || cell.Taken) continue;
            return true;
        }
        pos = default;
        return false;
    }

    bool TryGetCellDown(House house, out Vector2Int pos)
    {
        int upPos = house.Rect.Y -1;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            pos = new Vector2Int(house.Rect.X + x, upPos);
            if (!Cells.TryGetValue(pos, out BuildCell cell) || cell.Taken) continue;
            return true;
        }
        pos = default;
        return false;
    }

    bool TryGetCellRight(House house, out Vector2Int pos)
    {
        int rightPos = house.Rect.X + house.Rect.Width;
        for (int y = 0; y < house.Rect.Height; y++)
        {
            pos = new Vector2Int(rightPos, house.Rect.Y + y);
            if (!Cells.TryGetValue(pos, out BuildCell cell) || cell.Taken) continue;
            return true;
        }
        pos = default;
        return false;
    }

    bool TryGetCellLeft(House house, out Vector2Int pos)
    {
        int rightPos = house.Rect.X - 1;
        for (int y = 0; y < house.Rect.Height; y++)
        {
            pos = new Vector2Int(rightPos, house.Rect.Y + y);
            if (!Cells.TryGetValue(pos, out BuildCell cell) || cell.Taken) continue;
            return true;
        }
        pos = default;
        return false;
    }

    void ExpandUp(House house)
    {
        int upPos = house.Rect.Y + house.Rect.Height;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            var pos = new Vector2Int(house.Rect.X + x, upPos);
            Cells[pos] = new BuildCell(pos, true);
        }

        Rectangle expandedRect = new(house.Rect.Location, new Size(house.Rect.Width, house.Rect.Height + 1));
        house.Rect = expandedRect;
    }

    void ExpandDown(House house)
    {
        Rectangle houseR = house.Rect;
        int upPos = houseR.Y  -1;
        for (int x = 0; x < houseR.Width; x++)
        {
            var pos = new Vector2Int(houseR.X + x, upPos);
            Cells[pos] = new BuildCell(pos, true);
        }

        Rectangle expandedRect = new(houseR.X,houseR.Y-1, houseR.Width, houseR.Height + 1);
        house.Rect = expandedRect;
    }

    void ExpandRight(House house)
    {
        int rightPos = house.Rect.X + house.Rect.Width;
        for (int y = 0; y < house.Rect.Height; y++)
        {
            var pos = new Vector2Int(rightPos, house.Rect.Y + y);
            Cells[pos] = new BuildCell(pos,true);

        }

        Rectangle expandedRect = new(house.Rect.Location, new Size(house.Rect.Width + 1, house.Rect.Height));
        house.Rect = expandedRect;
    }

    void ExpandLeft(House house)
    {
        Rectangle houseR = house.Rect;
        int rightPos = houseR.X - 1;
        for (int y = 0; y < houseR.Height; y++)
        {
            var pos = new Vector2Int(rightPos, houseR.Y + y);
            Cells[pos] = new BuildCell(pos, true);
        }
        //shifting to left and expanding
        Rectangle expandedRect = new(houseR.X-1,houseR.Y, houseR.Width + 1, houseR.Height);
        house.Rect = expandedRect;
    }

    bool CanExpandRight(House house)
    {
        int rightPos = house.Rect.X + house.Rect.Width;
        if (Mathf.Abs(house.Rect.Width + 1 - house.Rect.Height) > _maxSideDifference) return false;

        for (int y = 0; y < house.Rect.Height; y++)
        {
            if(!Cells.TryGetValue(new Vector2Int(rightPos, house.Rect.Y + y), out BuildCell cell) || cell.Taken) return false;
        }

        return true;
    }

    bool CanExpandLeft(House house)
    {
        int rightPos = house.Rect.X - 1;
        if (Mathf.Abs(house.Rect.Width + 1 - house.Rect.Height) > _maxSideDifference) return false;

        for (int y = 0; y < house.Rect.Height; y++)
        {
            if (!Cells.TryGetValue(new Vector2Int(rightPos, house.Rect.Y + y), out BuildCell cell) || cell.Taken) return false;
        }

        return true;
    }

    bool CanExpandUp(House house)
    {
        int upPos = house.Rect.Y + house.Rect.Height;
        if (Mathf.Abs(house.Rect.Width - (house.Rect.Height + 1)) > _maxSideDifference) return false;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            if(!Cells.TryGetValue(new Vector2Int(house.Rect.X + x, upPos), out BuildCell cell) || cell.Taken) return false;
        }

        return true;
    }

    bool CanExpandDown(House house)
    {
        int upPos = house.Rect.Y - 1;
        if (Mathf.Abs(house.Rect.Width - (house.Rect.Height + 1)) > _maxSideDifference) return false;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            if (!Cells.TryGetValue(new Vector2Int(house.Rect.X + x, upPos), out BuildCell cell) || cell.Taken) return false;
        }

        return true;
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
        i = 0;
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
            //j++;
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
