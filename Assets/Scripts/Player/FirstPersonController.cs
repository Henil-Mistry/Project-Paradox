using UnityEngine;
namespace Paradox
{
    // Put on the Player root with a CharacterController; assign its camera and spawn.
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Transform view;
        [SerializeField] private Transform spawn;
        [SerializeField] private PlayerInteraction interaction;
        [SerializeField] private PuzzleManager puzzle;
        [Header("Movement")]
        [SerializeField, Min(1)] private float speed = 4.5f;
        [SerializeField, Min(0.1f)] private float sensitivity = 2f;
        [SerializeField, Min(0)] private float jumpHeight = 1.1f;
        [SerializeField] private float gravity = -22f;
        private CharacterController controller;
        private float verticalSpeed;
        private float pitch;
        public void Configure(Transform cameraTransform, Transform spawnPoint, PlayerInteraction hands, PuzzleManager state)
        { view = cameraTransform; spawn = spawnPoint; interaction = hands; puzzle = state; }
        private void Awake() { controller = GetComponent<CharacterController>(); }
        private void Start()
        {
            if (!view || !spawn || !interaction || !puzzle) Debug.LogWarning("Player needs camera, spawn, interaction and puzzle references.", this);
            LockCursor(true);
        }
        public static void LockCursor(bool locked)
        { Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None; Cursor.visible = !locked; }
        private void Update()
        {
            if (LevelMenus.InputBlocked) return;
            if (!view || !spawn || !puzzle || puzzle.Completed) return;
            if (PlayerInput.Escape) LockCursor(false);
            if (PlayerInput.Click) LockCursor(true);
            bool focused = Cursor.lockState == CursorLockMode.Locked;
            Vector2 look = focused ? PlayerInput.Look * sensitivity : Vector2.zero;
            transform.Rotate(0, look.x, 0);
            pitch = Mathf.Clamp(pitch - look.y, -80, 80);
            view.localRotation = Quaternion.Euler(pitch, 0, 0);
            Vector2 move = focused ? Vector2.ClampMagnitude(PlayerInput.Move, 1) : Vector2.zero;
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            if (focused && controller.isGrounded && PlayerInput.Jump) verticalSpeed = Mathf.Sqrt(jumpHeight * -2 * gravity);
            verticalSpeed += gravity * Time.deltaTime;
            controller.Move(((transform.right * move.x + transform.forward * move.y) * speed + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (transform.position.y < -8 || Mathf.Abs(transform.position.x) > 24 || Mathf.Abs(transform.position.z) > 40) Respawn();
        }
        public void Respawn()
        {
            if (!spawn) return;
            if (interaction) interaction.RecoverHeldObject();
            if (!controller) controller = GetComponent<CharacterController>();
            controller.enabled = false;
            transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            verticalSpeed = pitch = 0;
            if (view) view.localRotation = Quaternion.identity;
            controller.enabled = true;
            // Dimension and puzzle progress deliberately survive a fall.
        }
    }
}
