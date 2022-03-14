using System;
using System.Collections.Generic;

public class ObservableList<T> : List<T>
{
    public event EventHandler CountChanged;

    public new void Add(T item)
    {
        int prevCount = this.Count;
        base.Add(item);
        if (prevCount != this.Count)
        {
            CountChanged(this, null);
        }
    }

    public new void AddRange(IEnumerable<T> collection)
    {
        int prevCount = this.Count;
        base.AddRange(collection);
        if (prevCount != this.Count)
        {
            CountChanged(this, null);
        }
    }

    public new bool Remove(T item)
    {
        int prevCount = this.Count;
        base.Remove(item);
        if (prevCount != this.Count)
        {
            CountChanged(this, null);
        }

        return this.Contains(item);
    }

    public new void Clear()
    {
        int prevCount = this.Count;
        base.Clear();
        if (prevCount != this.Count)
        {
            CountChanged(this, null);
        }
    }
}