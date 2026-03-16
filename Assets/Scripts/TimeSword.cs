using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TimeSword : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<ISnapshottable> onTimeSwordSnapshot = new UnityEvent<ISnapshottable>();
    public UnityEvent onTimeSwordRestore = new UnityEvent();

    Collider weaponCollider;
    Animator animator;

    private void Awake()
    {
        weaponCollider = GetComponent<Collider>();
        animator = GetComponent<Animator>();
    }
    public void EnableHitbox(int value)
    {
        weaponCollider.enabled = value == 1 ? true : false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ISnapshottable>() != null)
        {
            onTimeSwordSnapshot.Invoke(other.GetComponent<ISnapshottable>());
            other.GetComponent<ISnapshottable>().Snapshot();
        }
    }

    public void Swing()
    {
        animator.SetTrigger("StartAttack");
    }

    public void Restore()
    {
        onTimeSwordRestore.Invoke();
    }
}
