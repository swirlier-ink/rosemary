using System;
using System.Collections;
using System.Collections.Generic;

namespace Rosemary.Common.DataStructures;

public class WeakCollection<T> : ICollection<T> where T : class
{
    private readonly List<WeakReference<T>> list = [];

    public void Add(T item) => list.Add(new WeakReference<T>(item));

    public void Clear() => list.Clear();

    public int Count => list.Count;

    public bool IsReadOnly => false;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Contains(T item)
    {
        foreach (var element in this)
        {
            if (Equals(element, item))
            {
                return true;
            }
        }

        return false;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        foreach (var element in this)
        {
            array[arrayIndex++] = element;
        }
    }

    public bool Remove(T item)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (!list[i].TryGetTarget(out var target))
            {
                continue;
            }

            if (Equals(target, item))
            {
                list.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (!list[i].TryGetTarget(out var element))
            {
                list.RemoveAt(i);
                continue;
            }

            yield return element;
        }
    }
}
