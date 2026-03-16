using UnityEngine;

public interface ISnapshottable
{
    public void Snapshot();
    public void Restore();
    public string GetName();
}
