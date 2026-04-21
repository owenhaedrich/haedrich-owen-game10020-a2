using UnityEngine;

// For objects that can be pushed. Used by Player and PushBlock.
public interface IPushable
{
    public PushType Push(Vector3 pushDirection, float pushSpeed);
}
