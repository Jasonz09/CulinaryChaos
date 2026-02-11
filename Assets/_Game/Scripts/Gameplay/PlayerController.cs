using UnityEngine;
using IOChef.Core;

namespace IOChef.Gameplay
{
    /// <summary>
    /// Controls player movement, interaction, and item carrying.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the kitchen grid layout.
        /// </summary>
        [Header("References")]
        [SerializeField] private KitchenGrid kitchenGrid;

        /// <summary>
        /// Animator component for player animations.
        /// </summary>
        [SerializeField] private Animator animator;

        /// <summary>
        /// Sprite renderer for the player character.
        /// </summary>
        [SerializeField] private SpriteRenderer spriteRenderer;

        /// <summary>
        /// Transform where carried items are attached.
        /// </summary>
        [SerializeField] private Transform carryPoint;

        /// <summary>
        /// Player movement speed in units per second.
        /// </summary>
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        /// <summary>
        /// Radius within which the player can interact with objects.
        /// </summary>
        [SerializeField] private float interactionRadius = 1.8f;

        /// <summary>
        /// Current facing direction.
        /// </summary>
        public FacingDirection Facing { get; private set; } = FacingDirection.Down;

        /// <summary>
        /// Currently carried ingredient.
        /// </summary>
        public Ingredient CarriedItem { get; private set; }

        /// <summary>
        /// Whether the player is carrying an item.
        /// </summary>
        public bool IsCarrying => CarriedItem != null;

        /// <summary>
        /// Current movement input vector from the player.
        /// </summary>
        private Vector2 _moveInput;

        /// <summary>
        /// Rigidbody2D used for physics-based movement.
        /// </summary>
        private Rigidbody2D _rb;

        /// <summary>
        /// Caches the Rigidbody2D and sets up initial references.
        /// </summary>
        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null)
            {
                _rb = gameObject.AddComponent<Rigidbody2D>();
            }
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Remove ALL existing BoxCollider2D (scene may have oversized 0.8 one)
            // then add our own tight collider
            foreach (var oldCol in GetComponents<BoxCollider2D>())
                DestroyImmediate(oldCol);
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.isTrigger = false;
            box.size = new Vector2(0.4f, 0.4f);

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            if (carryPoint == null)
            {
                var cp = new GameObject("CarryPoint");
                cp.transform.SetParent(transform);
                cp.transform.localPosition = new Vector3(0, 0.55f, 0);
                carryPoint = cp.transform;
            }
        }

        /// <summary>
        /// Subscribes to input manager events.
        /// </summary>
        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMovement += HandleMovement;
                InputManager.Instance.OnInteract += HandleInteract;
            }
        }

        /// <summary>
        /// Unsubscribes from input manager events.
        /// </summary>
        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnMovement -= HandleMovement;
                InputManager.Instance.OnInteract -= HandleInteract;
            }
        }

        /// <summary>
        /// Handles per-frame input polling and mouse interaction.
        /// </summary>
        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.Playing)
                return;

            if (Input.GetMouseButtonDown(0))
                HandleMouseInteract();
        }

        /// <summary>
        /// Checks for mouse click on nearby interactable objects.
        /// </summary>
        private void HandleMouseInteract()
        {
            var cam = Camera.main;
            if (cam == null) return;

            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            var hits = Physics2D.OverlapCircleAll(worldPos, 0.6f);

            InteractiveObject best = null;
            float bestDist = float.MaxValue;
            foreach (var h in hits)
            {
                var obj = h.GetComponent<InteractiveObject>();
                if (obj == null) continue;
                float d = Vector2.Distance(transform.position, h.transform.position);
                if (d < bestDist && d < interactionRadius * 2.5f)
                {
                    bestDist = d;
                    best = obj;
                }
            }

            if (best != null)
                best.OnPlayerInteract(this);
        }

        /// <summary>
        /// Stores movement direction from input.
        /// </summary>
        /// <param name="dir">The movement direction vector.</param>
        private void HandleMovement(Vector2 dir)
        {
            _moveInput = dir;
        }

        /// <summary>
        /// Applies movement velocity and updates facing direction.
        /// </summary>
        private void FixedUpdate()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.Playing)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }

            // Apply hero modifiers to speed
            float speed = moveSpeed;
            if (Heroes.HeroManager.Instance != null)
            {
                var mods = Heroes.HeroManager.Instance.GetActiveModifiers();
                speed *= mods.movementSpeedMultiplier;
            }

            _rb.linearVelocity = _moveInput * speed;

            // Update facing direction
            if (_moveInput.sqrMagnitude > 0.01f)
                UpdateFacing(_moveInput);

            // Update animator
            if (animator != null)
            {
                animator.SetFloat("MoveX", _moveInput.x);
                animator.SetFloat("MoveY", _moveInput.y);
                animator.SetBool("IsMoving", _moveInput.sqrMagnitude > 0.01f);
            }
        }

        /// <summary>
        /// Updates the player's facing direction from movement input.
        /// </summary>
        /// <param name="dir">The movement direction vector.</param>
        private void UpdateFacing(Vector2 dir)
        {
            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
                Facing = dir.x > 0 ? FacingDirection.Right : FacingDirection.Left;
            else
                Facing = dir.y > 0 ? FacingDirection.Up : FacingDirection.Down;
        }

        /// <summary>
        /// Finds and interacts with the nearest interactable object.
        /// </summary>
        private void HandleInteract()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState != GameState.Playing)
                return;

            // Find nearest interactive object
            float radius = interactionRadius;
            if (Heroes.HeroManager.Instance != null)
            {
                var mods = Heroes.HeroManager.Instance.GetActiveModifiers();
                radius *= mods.interactionRadiusMultiplier;
            }

            InteractiveObject nearest = FindNearestInteractable(radius);
            if (nearest != null)
            {
                nearest.OnPlayerInteract(this);
            }
        }

        /// <summary>
        /// Finds the closest interactable object within the given radius.
        /// </summary>
        /// <param name="radius">The search radius around the player.</param>
        /// <returns>The nearest interactable object, or null if none found.</returns>
        private InteractiveObject FindNearestInteractable(float radius)
        {
            // Direction-biased search: prefer objects in facing direction
            Vector2 facingDir = GetFacingVector();
            Vector2 checkPos = (Vector2)transform.position + facingDir * 0.5f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, radius);
            InteractiveObject best = null;
            float bestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var obj = hit.GetComponent<InteractiveObject>();
                if (obj == null) continue;

                float dist = Vector2.Distance(transform.position, hit.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = obj;
                }
            }
            return best;
        }

        /// <summary>
        /// Returns a unit vector in the player's current facing direction.
        /// </summary>
        /// <returns>A unit vector representing the facing direction.</returns>
        private Vector2 GetFacingVector()
        {
            return Facing switch
            {
                FacingDirection.Up => Vector2.up,
                FacingDirection.Down => Vector2.down,
                FacingDirection.Left => Vector2.left,
                FacingDirection.Right => Vector2.right,
                _ => Vector2.down
            };
        }

        /// <summary>
        /// Pick up an ingredient.
        /// </summary>
        /// <param name="item">Item to pick up.</param>
        public void PickupItem(Ingredient item)
        {
            if (IsCarrying) return;

            CarriedItem = item;
            item.gameObject.SetActive(true); // ensure visible (may have been deactivated)
            if (carryPoint != null)
            {
                item.transform.SetParent(carryPoint);
                item.transform.localPosition = Vector3.zero;
                item.transform.localScale = new Vector3(0.55f, 0.55f, 1f); // smaller when carried
            }

            // Ensure sorting above chef sprite
            var sr = item.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 20;

            if (animator != null)
                animator.SetTrigger("Pickup");
        }

        /// <summary>
        /// Drop the carried item.
        /// </summary>
        public void DropItem()
        {
            if (!IsCarrying) return;
            CarriedItem.transform.SetParent(null);
            CarriedItem = null;
        }

        /// <summary>
        /// Drop the carried item onto a specific interactive object's position.
        /// </summary>
        /// <returns>The released ingredient, or null.</returns>
        public Ingredient ReleaseItem()
        {
            var item = CarriedItem;
            CarriedItem = null;
            if (item != null)
            {
                item.transform.SetParent(null);
                item.transform.localScale = Vector3.one; // restore full scale
            }
            return item;
        }
    }
}
