# Developer Rules - TimeSword Project

This document outlines the coding standards and architectural rules for the TimeSword project. All developers should follow these rules to ensure consistency, modularity, and to prevent common bugs.

---

## 1. Initialization and Load Order

To prevent race conditions during scene loading, follow this lifecycle standard:

- **Awake()**: Use only for internal state initialization (fetching local components, initializing internal lists/variables).
- **Start()**: Use for cross-object references and event subscriptions (finding other objects in the scene, subscribing to singleton events).

---

## 2. Singleton Pattern

Managers should use the robust singleton pattern to allow access even before their `Awake` has finished.

```csharp
private static MyManager _instance;
public static MyManager Instance 
{ 
    get 
    {
        if (_instance == null)
        {
            _instance = FindFirstObjectByType<MyManager>();
        }
        return _instance;
    }
}

private void Awake()
{
    if (_instance != null && _instance != this)
    {
        Destroy(gameObject);
        return;
    }
    _instance = this;
}
```

---

## 3. Reference Management and Null Checks

**Do not use defensive null checks for necessary references.**

- If a script requires a manager or another object to function (e.g., `TimeSword`, `PressurePlateManager`), do not wrap its usage in `if (reference != null)`.
- We prefer a "Fail Fast" approach: if a mandatory reference is missing, the system should throw a `NullReferenceException`. This makes setup errors immediately visible in the console rather than causing silent failures that are difficult to debug.
- Only use null checks for truly optional components (e.g., an optional sound effect clip).

---

## 4. Encapsulation and Serialized Fields

**Use `[SerializeField] private` instead of bare `public` fields for inspector-exposed values.**

This keeps internal state encapsulated while still allowing the Unity Inspector to configure values. Public fields should only be used for API that other scripts explicitly need to call or read.

```csharp
// CORRECT — exposed to inspector, hidden from other scripts
[SerializeField] private float moveSpeed = 5.0f;
[SerializeField] private float gravity = -9.81f;

// CORRECT — this IS a public API other scripts call
public void EndLevel() { ... }

// WRONG — exposes internal tuning knobs to the entire codebase
public float moveSpeed = 5.0f;
```

**Exception:** `UnityEvent` fields meant to be subscribed to from other scripts in code (not the Inspector) may remain `public`. Mark them with `[HideInInspector]` if they should not appear in the Inspector.

---

## 5. Event Subscription Cleanup

**Every `AddListener` or `+=` subscription must have a corresponding `RemoveListener` or `-=` unsubscription.**

- Subscribe in `OnEnable()` or `Start()`.
- Unsubscribe in `OnDisable()` or `OnDestroy()`, matching whichever method was used to subscribe.
- This prevents `MissingReferenceException` during scene transitions and avoids memory leaks from stale delegates.

```csharp
// CORRECT
private void Start()
{
    PressurePlateManager.Instance.onColorStateChanged.AddListener(OnColorStateChanged);
}

private void OnDestroy()
{
    PressurePlateManager.Instance.onColorStateChanged.RemoveListener(OnColorStateChanged);
}

// WRONG — subscribes but never unsubscribes
private void Start()
{
    PressurePlateManager.Instance.onColorStateChanged.AddListener(OnColorStateChanged);
}
```

---

## 6. Scene-Bound vs. Persistent Objects

Clearly distinguish between objects that persist across scenes and objects that live only within a single scene.

- **Persistent (cross-scene):** Use `DontDestroyOnLoad(gameObject)` in `Awake()`. Only `GameManager` and other truly global services (e.g., a future `AudioManager`) should be persistent.
- **Scene-bound (per-level):** Everything else. Scene-bound objects are destroyed and recreated on each scene load. They must not hold long-lived references to persistent objects without proper cleanup (see Rule 5).

**If a manager needs to survive scene transitions, it must use `DontDestroyOnLoad` and the singleton pattern.** If it does not need to survive, it should be scene-bound and should not subscribe to persistent-object events without unsubscribing in `OnDestroy()`.

---

## 7. Cross-Object Discovery

**Prefer singleton accessors or inspector assignment over `FindFirstObjectByType` calls scattered across multiple scripts.**

When many scripts need to reference the same object:

1. **Best:** Make it a singleton with a static `Instance` property (see Rule 2). Consumers access it via `MyManager.Instance`.
2. **Acceptable:** Assign the reference via the Inspector using `[SerializeField]` if the relationship is one-to-one and scene-specific.
3. **Avoid:** Multiple scripts each calling `FindFirstObjectByType<T>()` independently to find the same object. This duplicates lookup logic and creates invisible coupling.

---

## 8. Single Responsibility

**Each MonoBehaviour should have one clear responsibility.**

If a script is handling multiple unrelated systems (e.g., input reading, physics movement, push interaction, knockback response), consider extracting distinct responsibilities into separate components on the same GameObject.

Guidelines:
- If a script exceeds ~150 lines, evaluate whether it can be split.
- Group related fields, state, and methods into a cohesive unit. If you can describe a group as its own "thing" (e.g., "push interaction handler"), it is a candidate for extraction.
- Extracted components communicate via direct references, interfaces, or events — not by sharing private state.

---

## 9. Don't Repeat Yourself (DRY)

**Extract duplicated logic into shared helper methods.**

If two or more methods contain the same block of code (e.g., snapping a direction to a cardinal axis and setting Rigidbody constraints), extract it into a single private helper method that both call. This ensures changes only need to be made in one place.

```csharp
// CORRECT — shared helper
private Vector3 GetCardinalDirection(Vector3 direction)
{
    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        return Vector3.right * Mathf.Sign(direction.x);
    }
    else
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
        return Vector3.forward * Mathf.Sign(direction.z);
    }
}
```

---

## 10. Naming Conventions

Follow these naming conventions consistently across all scripts:

| Element | Convention | Example |
|---|---|---|
| Private fields | `_camelCase` with underscore prefix | `_rigidbody`, `_hasSnapshot` |
| Serialized private fields | `_camelCase` with underscore prefix | `_moveSpeed` |
| Public properties / methods | `PascalCase` | `Instance`, `Push()` |
| Local variables | `camelCase` | `moveDirection`, `finalSpeed` |
| Constants | `PascalCase` | `DefaultGravity` |
| Enums and enum values | `PascalCase` | `ToggleColour.Red` |
| Interface names | `IPascalCase` | `IPushable`, `ISnapshottable` |

---

## 11. Code Style

- **One blank line** between methods. Never zero, never more than one.
- **Opening braces** on a new line for class and method declarations (Allman style).
- **Comments:** Use `//` comments above non-obvious logic. Do not use `///` XML doc comments unless building a public API or library.
- **`[Header]` and `[Tooltip]`**: Use `[Header("Section Name")]` to group related serialized fields in the Inspector. Use `[Tooltip("...")]` on fields whose purpose is not immediately obvious from the name.
- **Class-level comments:** Each MonoBehaviour should have a single `//` comment above the class declaration summarizing its purpose in one sentence.

---

## 12. Folder Organization

```
Scripts/
├── Interfaces and Enums/    # Interfaces and enum types only
├── Level Objects/            # MonoBehaviours placed on in-scene gameplay objects
│                             # (Player, PushBlock, TimeSword, PressurePlate, etc.)
├── Managers/                 # Singletons and scene-management scripts
│                             # (GameManager, LevelManager, SoundManager, etc.)
├── UI/                       # MonoBehaviours for UI canvases and elements
│                             # (HUD, MainMenu, etc.)
└── Utility/                  # Debug tools, helper classes, and extensions
                              # (InGameDebug, static utility methods, etc.)
```

- Every `.cs` file must live in the appropriate subfolder. No scripts at the `Scripts/` root.
- Non-code files (like this document) may live at the `Scripts/` root.
