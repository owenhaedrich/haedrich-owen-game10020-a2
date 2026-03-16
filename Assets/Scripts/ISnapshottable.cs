using UnityEngine;

public interface ISnapshottable
{
    public void Snapshot();
    public void Restore();
    public void Clear(ISnapshottable snapshottable);
    public string GetName();
}
