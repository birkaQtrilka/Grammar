using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class Cluster
{
    delegate bool TryGet(House h, out Vector2Int pos);

    public Dictionary<Vector2Int, BuildCell> Cells = new();
    public ReadOnlyCollection<House> Houses { get; private set; }

    readonly Queue<Vector2Int> _toDoHouses = new();
    readonly List<House> _houses = new();

    readonly System.Random _random;
    readonly int _maxSideDifference;
    readonly int _splitChance;

    public readonly int ID;
    UnityEngine.Color _debugClr;

    //the functions that enqueue a direction of propagation of a house
    readonly TryGet[] tryGets;

    public Cluster(System.Random random, int id, int maxSideDifference = 1, int splitChance = 70)
    {
        _random = random;
        _maxSideDifference = maxSideDifference;
        _splitChance = splitChance;
        ID = id;
        _debugClr  = UnityEngine.Random.ColorHSV(0f,1f,1f,1f,1f,1f);
        Houses = new(_houses);
        tryGets = new TryGet[4] 
        { 
            TryGetCellUp,
            TryGetCellDown,
            TryGetCellRight,
            TryGetCellLeft,
        };
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
            tryGets.Shuffle();
            foreach (TryGet tryGet in tryGets)
            {
                if (tryGet(newHouse, out Vector2Int pos)) _toDoHouses.Enqueue(pos);
            }

        } while (_toDoHouses.Count != 0 && safety-- > 0);

    }

    House Iterate(Vector2Int startPos)
    {
        if (!IsAvailableCell(startPos)) return null;

        House house = new(new Rectangle(startPos.x, startPos.y, 1, 1));
        Cells[startPos] = new(startPos, true);
        
        int safety = 50;
        while (ExpandChance() && CanExpandRight(house) && safety-- > 0) ExpandRight(house);
        safety = 50;
        while (ExpandChance() && CanExpandLeft(house) && safety-- > 0)  ExpandLeft(house);
        safety = 50;
        while (ExpandChance() && CanExpandUp(house) && safety-- > 0)    ExpandUp(house);
        safety = 50;
        while (ExpandChance() && CanExpandDown(house) && safety-- > 0)  ExpandDown(house);
        return house;
    }

    bool ExpandChance()
    {
        return _random.Next(0, 100) < _splitChance;
    }

    bool TryGetCellUp(House house, out Vector2Int pos)
    {
        int upPos = house.Rect.Y + house.Rect.Height;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            pos = new Vector2Int(house.Rect.X + x, upPos);
            if (!IsAvailableCell(pos)) continue;
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
            if (!IsAvailableCell(pos)) continue;
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
            if (!IsAvailableCell(pos)) continue;
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
            if (!IsAvailableCell(pos)) continue;
            return true;
        }
        pos = default;
        return false;
    }

    void ExpandUp(House house)
    {
        int upPos = house.Rect.Y + house.Rect.Height;
        for (int x = 0; x < house.Rect.Width; x++)
            MarkCellAsTaken(new Vector2Int(house.Rect.X + x, upPos));

        Rectangle expandedRect = new(house.Rect.Location, new Size(house.Rect.Width, house.Rect.Height + 1));
        house.Rect = expandedRect;
    }

    void ExpandDown(House house)
    {
        Rectangle houseR = house.Rect;
        int upPos = houseR.Y  -1;
        for (int x = 0; x < houseR.Width; x++)
            MarkCellAsTaken(new Vector2Int(houseR.X + x, upPos));

        Rectangle expandedRect = new(houseR.X,houseR.Y-1, houseR.Width, houseR.Height + 1);
        house.Rect = expandedRect;
    }

    void ExpandRight(House house)
    {
        int rightPos = house.Rect.X + house.Rect.Width;
        for (int y = 0; y < house.Rect.Height; y++)
            MarkCellAsTaken(new Vector2Int(rightPos, house.Rect.Y + y));
        
        Rectangle expandedRect = new(house.Rect.Location, new Size(house.Rect.Width + 1, house.Rect.Height));
        house.Rect = expandedRect;
    }

    void ExpandLeft(House house)
    {
        Rectangle houseR = house.Rect;
        int rightPos = houseR.X - 1;
        for (int y = 0; y < houseR.Height; y++)
            MarkCellAsTaken(new Vector2Int(rightPos, houseR.Y + y));

        //shifting to left and expanding
        Rectangle expandedRect = new(houseR.X-1,houseR.Y, houseR.Width + 1, houseR.Height);
        house.Rect = expandedRect;
    }

    bool CanExpandRight(House house)
    {
        int rightPos = house.Rect.X + house.Rect.Width;
        if (IsSideDiferenceValid(house.Rect, upExpand: false)) return false;

        for (int y = 0; y < house.Rect.Height; y++)
            if(!IsAvailableCell(rightPos, house.Rect.Y + y)) return false;

        return true;
    }

    bool CanExpandLeft(House house)
    {
        int rightPos = house.Rect.X - 1;
        if (IsSideDiferenceValid(house.Rect, upExpand:false)) return false;

        for (int y = 0; y < house.Rect.Height; y++)
            if (!IsAvailableCell(rightPos, house.Rect.Y + y)) return false;

        return true;
    }

    bool CanExpandUp(House house)
    {
        int upPos = house.Rect.Y + house.Rect.Height;
        if (IsSideDiferenceValid(house.Rect, upExpand:true)) return false;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            if(!IsAvailableCell(house.Rect.X + x, upPos)) return false;
        }

        return true;
    }

    bool CanExpandDown(House house)
    {
        int upPos = house.Rect.Y - 1;
        if (IsSideDiferenceValid(house.Rect,upExpand:true)) return false;
        for (int x = 0; x < house.Rect.Width; x++)
        {
            if (!IsAvailableCell(house.Rect.X + x, upPos)) return false;
        }
        return true;
    }

    void MarkCellAsTaken(Vector2Int cell)
    {
        Cells[cell] = new BuildCell(cell, true);
    }

    bool IsAvailableCell(int x, int y)
    {
        return IsAvailableCell(new Vector2Int(x, y));
    }

    bool IsAvailableCell(Vector2Int pos)
    {
        return Cells.TryGetValue(pos, out BuildCell cell) && !cell.Taken;

    }

    bool IsSideDiferenceValid(Rectangle rect, bool upExpand)
    {
        return Mathf.Abs(rect.Width - rect.Height + (upExpand ? -1 : +1)) > _maxSideDifference;
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
    readonly Vector3[] drawArr = new Vector3[8];

    public void DrawCells(int i)
    {
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
            drawArr[0] = corner_TL;
            drawArr[1] = corner_TR;

            drawArr[2] = corner_TR;
            drawArr[3] = corner_BR;

            drawArr[4] = corner_BR;
            drawArr[5] = corner_BL;

            drawArr[6] = corner_BL;
            drawArr[7] = corner_TL;

            Gizmos.color = _debugClr;
            Gizmos.DrawLineList(drawArr);
            //j++;
        }
    }

}
