using System;
using System.Collections.Generic;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    static GizmosDrawer _instance;

    public static GizmosDrawer Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GizmosDrawer");
                var drawer = go.AddComponent<GizmosDrawer>();
                _instance = drawer;
            }
            return _instance;
        }
    }

    readonly Stack<Action> _drawCall = new();
    public Action PersistentCall;

    void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(gameObject);
        else
            _instance = this;
    }

    void OnDrawGizmos()
    {
        while (_drawCall.Count > 0)
        {
            var call = _drawCall.Pop();
            call();
        }
        PersistentCall?.Invoke();
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            Instance.PersistentCall = null;
        }
    }

    public void PushDrawCall(Action action) => _drawCall.Push(action);
}
