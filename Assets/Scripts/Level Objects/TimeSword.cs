using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

// Tell ISnapshottables when to store and restore their snapshots. Send onSnapshot and onRestore events for the ISnapshottables, the SoundManager, and the HUD
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
        ISnapshottable snapshottable = other.GetComponent<ISnapshottable>();
        if (snapshottable != null)
        {
            onSnapshot.Invoke(snapshottable);
            snapshottable.Snapshot();
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
