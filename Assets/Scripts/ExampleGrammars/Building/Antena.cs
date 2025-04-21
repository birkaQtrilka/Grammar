using UnityEngine;
using Demo;
using System.Collections.Generic;

public class Antena : Shape
{
    [SerializeField] LodObject[] _designs;
    [SerializeField] AnimationCurve[] _shapes;
    //has the range from 0 to 1
    float _currentSize = 0;
    AnimationCurve _currentShape;

    [SerializeField]float _growthRate;
    [SerializeField]int _baseWidth;
    [SerializeField]int _baseDepth;
    [SerializeField]bool _isSameSize;
    bool _init;
    public Queue<int> PrescribedDesigns { get; private set; } = new();

    public void Init(float startSize, int width, int depth, bool isSameSize, float growthRate)
    {
        _currentSize = startSize;
        _baseWidth = width;
        _baseDepth = depth;
        _isSameSize = isSameSize;
        _growthRate = growthRate;
        _currentShape = _shapes.GetRandomItem();
        _init = true;
    }

    void Init(float startSize, int width, int depth, bool isSameSize, float growthRate, AnimationCurve currentShape)
    {
        _currentSize = startSize;
        _baseWidth = width;
        _baseDepth = depth;
        _isSameSize = isSameSize;
        _growthRate = growthRate;
        _currentShape = currentShape; 
        _init = true;
    }

    protected override void Execute()
    {
        if(!_init)
        {
            _currentSize = 1;
            _currentShape = _shapes.GetRandomItem();
        }

        bool tooFar = WorldSpawner.LOD_Enabled && WorldSpawner.TooFar(transform.position);
        LodObject block = PrescribedDesigns.Count == 0 ? _designs[RandomInt(_designs.Length)] : _designs[PrescribedDesigns.Dequeue()];
        GameObject inst = SpawnLOD(block, tooFar);

        inst.transform.localScale = GetSize();
        if(inst.transform.localScale.x <= 0.05f || inst.transform.localScale.y <= 0.05f)
        {
            DestroyImmediate(gameObject);
            return;
        }

        if (_currentSize - _growthRate <= 0.04) return;
        Antena next = CreateSymbol<Antena>("Antena",Vector3.up);
        next.Init(_currentSize - _growthRate, _baseWidth, _baseDepth, _isSameSize, _growthRate, _currentShape);
        next._designs = _designs;
        next.PrescribedDesigns = PrescribedDesigns;
        next.Generate();
    }

    Vector3 GetSize()
    {
        float currentSize = _currentShape.Evaluate(_currentSize);
        float width = _baseWidth * currentSize;
        float depth = _baseDepth * currentSize;
        if (_isSameSize)
        {
            if (width < depth) return new Vector3(width, 1, width);
            return new Vector3(depth, 1, depth);
        }

        return new Vector3(width, 1, depth);
    }
}
