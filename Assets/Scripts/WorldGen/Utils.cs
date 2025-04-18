using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    readonly static System.Random rng = new();

    public static void DestroyAllChildren(this GameObject obj)
    {
        DestroyAllChildren(obj.transform);
    }

    public static void DestroyAllChildren(this Transform transform, bool immediate = false)
    {
        if(immediate)
        {
            for (int i = transform.childCount - 1; i >= 0; --i)
                GameObject.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        else
        {
            for (int i = transform.childCount - 1; i >= 0; --i)
                GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static T GetRandomItem<T>(this List<T> list, System.Random generator = null)
    {
        generator ??= rng;
        return list[generator.Next(0, list.Count)];
    }
    public static T GetRandomItem<T>(this T[] arr, System.Random generator = null)
    {
        generator ??= rng;
        return arr[generator.Next(0, arr.Length)];
    }
    public static List<GameObject> FindGameObjectInChildWithTag(this GameObject parent, string tag)
    {
        Transform t = parent.transform;
        List<GameObject> objs = new();
        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).gameObject.CompareTag(tag))
            {
                objs.Add( t.GetChild(i).gameObject );
            }

        }

        return objs;
    }

    public static void FindGameObjectInChildWithTag(this GameObject parent, string tag, List<GameObject> results)
    {
        Transform t = parent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).gameObject.CompareTag(tag))
            {
                results.Add(t.GetChild(i).gameObject);
            }

        }

    }

    public static float GetManhattanDistance(this Vector2 a, Vector2 b)
    {
        Vector2 manhatanVector = a - b;
        return Mathf.Abs(manhatanVector.x) + Mathf.Abs(manhatanVector.y);
    }

    public static int GetManhattanDistance(this Vector2Int a, Vector2Int b)
    {
        Vector2Int manhatanVector = a - b;
        return Mathf.Abs(manhatanVector.x) + Mathf.Abs(manhatanVector.y);
    }

    public static IEnumerator DoFadeOut(GameObject panel, float fadeTime, float bufferTime, Action onComplete = null)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();

        yield return DoFadeOut(group, fadeTime, bufferTime, onComplete);
    }

    public static IEnumerator DoFadeOut(CanvasGroup group, float fadeTime, float bufferTime,Action onComplete = null)
    {
        yield return new WaitForSeconds(bufferTime);
        float currTime = fadeTime;
        while (currTime > 0)
        {
            group.alpha = currTime / fadeTime;

            currTime -= Time.deltaTime;
            yield return null;
        }
        group.alpha = 0;
        group.gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    public static IEnumerator DoFadeIn(GameObject panel, float fadeTime, float bufferTime, Action onComplete = null)
    {
        CanvasGroup group = panel.GetComponent<CanvasGroup>();

        yield return DoFadeIn(group, fadeTime, bufferTime, onComplete);
    }

    public static IEnumerator DoFadeIn(CanvasGroup group, float fadeTime, float bufferTime, Action onComplete = null)
    {
        yield return new WaitForSeconds(bufferTime);
        float currTime = fadeTime;
        while (currTime > 0)
        {
            group.alpha = 1 - (currTime / fadeTime);

            currTime -= Time.deltaTime;
            yield return null;
        }
        group.alpha = 1;

        onComplete?.Invoke();
    }

}
