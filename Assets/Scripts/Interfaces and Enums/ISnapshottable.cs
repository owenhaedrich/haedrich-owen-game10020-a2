using UnityEngine;

// For objects that are snapshottable by the Time Sword. Used by PushBlock and Rotating Barrier.
public interface ISnapshottable
{
    // Store a snapshot
    public void Snapshot();

    // Restore a stored snapshot
    public void Restore();

    // Return the object's name for the UI
    public string GetName();
}
