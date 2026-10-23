using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Paradox
{
    [Serializable]
    public struct PuzzleConfiguration
    {
        public string name;
        public Vector3 playerSpawn;
        public Vector3 objectSpawn;
        public Dimension objectDimension;
        public Dimension plateDimension;
        public Dimension gateDimension;
    }

    // One complete authored configuration is selected once, after component Awake calls.
    [DefaultExecutionOrder(-100)]
    public sealed class PuzzleConfigurationSelector : MonoBehaviour
    {
        [SerializeField] private PuzzleConfiguration[] configurations;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Transform spawn, blackHome, whiteHome;
        [SerializeField] private TransferableObject cube;
        [SerializeField] private PressurePlate plate;
        [SerializeField] private DimensionObject gate, exit;
        [SerializeField] private PuzzleManager puzzle;
        [SerializeField] private DimensionManager dimensions;
        [SerializeField] private bool useFixedSeed;
        [SerializeField] private int fixedSeed;
        public static int? DebugSeed { get; set; }
        public int SelectedIndex { get; private set; }
        public PuzzleConfiguration ActiveConfiguration => configurations[SelectedIndex];
        public int ConfigurationCount => configurations.Length;
        public void Configure(PuzzleConfiguration[] choices, FirstPersonController actor, Transform playerSpawn,
            Transform black, Transform white, TransferableObject item, PressurePlate target,
            DimensionObject door, DimensionObject finish, PuzzleManager state, DimensionManager worlds)
        {
            configurations = choices; player = actor; spawn = playerSpawn; blackHome = black; whiteHome = white;
            cube = item; plate = target; gate = door; exit = finish; puzzle = state; dimensions = worlds;
        }
        private void Start()
        {
            int? seed = DebugSeed ?? (useFixedSeed ? (int?)fixedSeed : null);
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++)
                if (args[i] == "-paradoxSeed" && int.TryParse(args[i + 1], out int parsed)) seed = parsed;
            // Fixed seeds cycle the small approved list, independently of Unity's global RNG.
            SelectedIndex = seed.HasValue ? (int)(((long)seed.Value + SceneManager.GetActiveScene().buildIndex) % configurations.Length + configurations.Length) % configurations.Length
                : UnityEngine.Random.Range(0, configurations.Length);
            PuzzleConfiguration selected = ActiveConfiguration;
            spawn.position = selected.playerSpawn;
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false; player.transform.SetPositionAndRotation(spawn.position, spawn.rotation); controller.enabled = true;
            // Both recovery homes use this safe, shared start-side floor. No recovery can skip the crossing.
            blackHome.position = whiteHome.position = selected.objectSpawn;
            cube.MoveToDimension(selected.objectDimension, selected.objectSpawn);
            plate.SetDimension(selected.plateDimension);
            gate.SetMembership(selected.gateDimension); exit.SetMembership(selected.gateDimension);
            puzzle.ConfigureGate(dimensions, selected.gateDimension);
            Physics.SyncTransforms();
        }
    }
}
