using System;
using System.Collections.Generic;

public class ObservableList<T> : List<T>
{
    public Action<int> CountChanged = i => { };

    public new void Add(T item)
    {
        int prevCount = this.Count;
        base.Add(item);
        if (prevCount != this.Count)
        {
            CountChanged(this.Count);
        }
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        int prevCount = this.Count;
        base.AddRange(collection);
        if (prevCount != this.Count)
        {
            CountChanged(this.Count);
        }
    }

    public new bool Remove(T item)
    {
        int prevCount = this.Count;
        bool isRemoved = base.Remove(item);
        if (prevCount != this.Count)
        {
            CountChanged(this.Count);
        }

        return isRemoved;
    }

    public new void Clear()
    {
        int prevCount = this.Count;
        base.Clear();
        if (prevCount != this.Count)
        {
            CountChanged(this.Count);
        }
    }
}