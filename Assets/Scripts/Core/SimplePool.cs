using System;
using System.Collections.Generic;
using UnityEngine;

// Reuses objects instead of creating/destroying them.
public class SimplePool<T> where T : Component
{
    readonly Func<T> create;
    readonly Stack<T> free = new Stack<T>();

    public SimplePool(Func<T> create) { this.create = create; }

    public T Get(Vector3 position)
    {
        T item = free.Count > 0 ? free.Pop() : create();
        item.transform.position = position; // before activating, so NavMeshAgents start on the mesh
        item.gameObject.SetActive(true);
        return item;
    }

    public void Release(T item)
    {
        item.gameObject.SetActive(false);
        free.Push(item);
    }
}
