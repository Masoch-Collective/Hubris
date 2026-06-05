using System;
using Character;
using UnityEngine;
using Utils;

namespace Systems {
    
    [Flags]
    public enum PlayerRoles {
        Leader      = 1 << 0,
        Seeker      = 1 << 1,
        TopGoal     = 1 << 2,
        BottomGoal  = 1 << 3
    }

    public class CombatLoopManager : Singleton<CombatLoopManager> {

        public string particlesNameTop =    "SeekerParticles_GodRays_P1";
        public string particlesNameBottom = "SeekerParticles_GodRays_P2";
        public float particlesHeightEnabled = 6;
        public float particlesHeightDisabled = 8;
        public float particlesHeightLerpSpeed = 5;
        public ParticleSystem ParticlesTop {
            get {
                if (_particlesTop == null)
                    _particlesTop = GameObject.Find(particlesNameTop).GetComponent<ParticleSystem>();
                return _particlesTop;
            }
        }
        [NonSerialized] private ParticleSystem _particlesTop;
        public ParticleSystem ParticlesBottom {
            get {
                if (_particlesBottom == null)
                    _particlesBottom = GameObject.Find(particlesNameBottom).GetComponent<ParticleSystem>();
                return _particlesBottom;
            }
        }
        [NonSerialized] private ParticleSystem _particlesBottom;
        [field:SerializeField] public CharacterCore Leader      { get; private set; }
        [field:SerializeField] public CharacterCore Seeker      { get; private set; }
        [field:SerializeField] public CharacterCore TopGoal     { get; private set; }
        [field:SerializeField] public CharacterCore BottomGoal  { get; private set; }
        [field:SerializeField] public int Orientation           { get; private set; }
        public event Action<int> OnRoleSwap;

        private void Awake() {
            OnRoleSwap += _ => MapVerticalFlipper.Instance.Flip();
        }

        private void Update() {
            ParticlesTop.transform.localPosition =      Vector2.Lerp(ParticlesTop.transform.localPosition, 
                                                        ParticlesTop.transform.localRotation * Vector2.up *
                                                        (ParticlesTop.isPlaying 
                                                            ? particlesHeightEnabled 
                                                            : particlesHeightDisabled),
                                                        Time.deltaTime * particlesHeightLerpSpeed);
            ParticlesBottom.transform.localPosition =   Vector2.Lerp(ParticlesBottom.transform.localPosition, 
                                                        ParticlesBottom.transform.localRotation * Vector2.up *
                                                        (ParticlesBottom.isPlaying 
                                                            ? particlesHeightEnabled 
                                                            : particlesHeightDisabled),
                                                        Time.deltaTime * particlesHeightLerpSpeed);
        }

        public void CharacterEliminated(CharacterCore killer, CharacterCore killed) {
            if (killer == null || killed == null) // TODO: Clear leader status if leader was killed but not by opponent
                return;
            if (killer == Leader) return; // Ignore elimination if Leader eliminated Seeker
            Leader = killer;
            Seeker = killed;
            if (Orientation == 0)
                Orientation = 1;
            else {
                Orientation *= -1;
                OnRoleSwap?.Invoke(Orientation);
            }
            if (TopGoal == null || BottomGoal == null) { // If either player is null, this was the first elimination of the match; assign top/bottom
                if (TopGoal != BottomGoal) // Print warning if somehow only one player is null
                    Debug.LogWarning("One goal was null, but the other wasn't??");
                TopGoal = Leader;
                BottomGoal = Seeker;
            }
            
            if (Leader == TopGoal)
                ParticlesTop.Play();
            else 
                ParticlesTop.Stop();
            
            if (Leader == BottomGoal)
                ParticlesBottom.Play();
            else 
                ParticlesBottom.Stop();
        }

        public static bool EvaluateRole(CharacterCore character, PlayerRoles role) {
            if (character == Instance.Leader && role.HasFlag(PlayerRoles.Leader)) return true;
            if (character == Instance.Seeker && role.HasFlag(PlayerRoles.Seeker)) return true;
            if (character == Instance.TopGoal && role.HasFlag(PlayerRoles.TopGoal)) return true;
            if (character == Instance.BottomGoal && role.HasFlag(PlayerRoles.BottomGoal)) return true;
            return false;
        }

    }

}