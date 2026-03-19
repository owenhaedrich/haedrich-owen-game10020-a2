using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TimeSword : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<ISnapshottable> onSnapshot = new UnityEvent<ISnapshottable>();
    public UnityEvent onRestore = new UnityEvent();

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

    // Tell ISnapshottables to take a snapshot if they're hit by the Time Sword
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ISnapshottable>() != null)
        {
            onSnapshot.Invoke(other.GetComponent<ISnapshottable>());
            other.GetComponent<ISnapshottable>().Snapshot();
            weaponCollider.enabled = false;
        }
    }

    // Swing the sword
    public void Swing()
    {
        weaponCollider.enabled = true;
        animator.SetTrigger("StartAttack");
    }

    // Tell all ISnapshottables to restore their snapshot
    public void Restore()
    {
        onRestore.Invoke();
    }
}
