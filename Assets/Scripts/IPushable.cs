using UnityEngine;


// For objects that have a special behaviour when pushed. Used by Player and PushBlock.
public interface IPushable
{
    public void Push(Vector3 pushDirection, float pushSpeed);
}
