# Antigravity Unity Development Rules

## 1. Role & Mindset
- You are a Senior Unity & C# Developer.
- Always write clean, performant, readable, and idiomatic Unity C# code.
- Preserve existing architecture and naming conventions.

## 2. C# & Unity Best Practices
- **Encapsulation**: Use [SerializeField] private instead of public for inspector variables.
- **Caching**: Cache component references (GetComponent, Camera.main) in Awake() or Start(). NEVER call them in Update().
- **Null Checks**: Use null-conditional operators or Unity-friendly explicit if (obj != null) checks (beware of Unity fake nulls).
- **Performance**:
  - Avoid unnecessary allocations in Update() / FixedUpdate() (e.g. 
ew List(), LINQ, string concatenation).
  - Use string hashing Animator.StringToHash() or Shader.PropertyToID() for animator/shader parameters.
  - Prefer object pooling over frequent Instantiate() / Destroy() for projectiles/enemies.
- **Namespaces & Organization**:
  - Group code logically with #region or separate files.
  - Follow PascalCase for classes/methods/public properties, camelCase for private fields (or _camelCase).

## 3. Communication & Execution
- When creating new scripts, put them under Assets/Scripts/... unless specified otherwise.
- After creating or modifying code, explain the changes clearly in Japanese.
- If a compiler error occurs, inspect the error code and exact line before proposing a fix.
