using System;
using System.Collections.Generic;
using UnityEngine;
namespace Paradox
{
    public enum Dimension { White, Black }
    public enum DimensionMembership { White, Black, Both }

    // Put on Systems. This is the only authority that changes the player's dimension.
    public sealed class DimensionManager : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private HUD hud;
        [SerializeField] private PlayerInteraction hands;
        [SerializeField] private CapsuleCollider switchProbe;
        [SerializeField] private PuzzleManager puzzle;
        [Header("White atmosphere")]
        [SerializeField] private Color whiteAmbient = new Color(0.45f, 0.48f, 0.53f);
        [SerializeField] private Color whiteFog = new Color(0.82f, 0.87f, 0.91f);
        [SerializeField] private Color whiteSun = new Color(1, 0.94f, 0.84f);
        [SerializeField] private float whiteIntensity = 0.8f;
        [SerializeField] private float whiteFogDensity = 0.008f;
        [Header("Black atmosphere")]
        [SerializeField] private Color blackAmbient = new Color(0.19f, 0.22f, 0.37f);
        [SerializeField] private Color blackFog = new Color(0.035f, 0.045f, 0.11f);
        [SerializeField] private Color blackSun = new Color(0.53f, 0.61f, 1);
        [SerializeField] private float blackIntensity = 0.8f;
        [SerializeField] private float blackFogDensity = 0.022f;
        [SerializeField, Min(0.1f)] private float switchCooldown = 0.3f;
        private readonly List<DimensionObject> objects = new List<DimensionObject>();
        private float nextSwitch;
        public Dimension Current { get; private set; } = Dimension.White;
        public event Action<Dimension> Changed;
        public void Configure(Light lightSource, Camera camera, HUD display, PlayerInteraction interaction, CapsuleCollider probe, PuzzleManager state)
        { sun = lightSource; playerCamera = camera; hud = display; hands = interaction; switchProbe = probe; puzzle = state; }
        private void Start()
        {
            if (!sun || !playerCamera || !hud || !hands || !switchProbe || !puzzle) Debug.LogWarning("Assign all DimensionManager scene references.", this);
            ApplyAtmosphere();
        }
        private void Update()
        {
            if (puzzle && puzzle.Completed) return;
            if (Cursor.lockState == CursorLockMode.Locked && PlayerInput.Switch) TrySwitch();
        }
        public void Register(DimensionObject item) { if (!objects.Contains(item)) objects.Add(item); }
        public void Unregister(DimensionObject item) { objects.Remove(item); }
        public bool TrySwitch()
        {
            if (Time.time < nextSwitch || (puzzle && puzzle.Completed)) return false;
            if (hands && hands.HeldObject) { if (hud) hud.Message("Release the object before switching"); return false; }
            Dimension target = Current == Dimension.White ? Dimension.Black : Dimension.White;
            foreach (DimensionObject item in objects)
                if (item && !item.ExistsIn(Current) && item.ExistsIn(target) && item.Overlaps(switchProbe))
                { if (hud) hud.Message("Something is in the way"); return false; }
            Current = target;
            nextSwitch = Time.time + switchCooldown;
            Changed?.Invoke(Current);
            ApplyAtmosphere();
            if (hud) hud.Flash(Current == Dimension.White ? whiteFog : new Color(0.35f, 0.2f, 0.65f));
            return true;
        }
        private void ApplyAtmosphere()
        {
            bool white = Current == Dimension.White;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = white ? whiteAmbient : blackAmbient;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = white ? whiteFog : blackFog;
            RenderSettings.fogDensity = white ? whiteFogDensity : blackFogDensity;
            if (sun) { sun.color = white ? whiteSun : blackSun; sun.intensity = white ? whiteIntensity : blackIntensity; }
            if (playerCamera) playerCamera.backgroundColor = RenderSettings.fogColor;
            if (hud) hud.SetDimension(Current);
        }
    }
}
