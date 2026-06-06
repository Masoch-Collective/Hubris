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

            ParticlesTop.transform.localPosition    = ParticlesTop.transform.localRotation * Vector2.up * particlesHeightDisabled;
            ParticlesBottom.transform.localPosition = ParticlesBottom.transform.localRotation * Vector2.up * particlesHeightDisabled;
            
        }

        public void SetUpRole(PlayerRoles role, CharacterCore character) {
            if (role.HasFlag(PlayerRoles.Leader))
                if (Leader == null)
                    Leader = character;
                else
                    Debug.LogError($"Tried to set {character} as Leader, but {Leader} was already Leader.");
            if (role.HasFlag(PlayerRoles.Seeker))
                if (Seeker == null)
                    Seeker = character;
                else
                    Debug.LogError($"Tried to set {character} as Seeker, but {Seeker} was already Seeker.");
            if (role.HasFlag(PlayerRoles.TopGoal))
                if (TopGoal == null)
                    TopGoal = character;
                else
                    Debug.LogError($"Tried to set {character} as TopGoal, but {TopGoal} was already TopGoal.");
            if (role.HasFlag(PlayerRoles.BottomGoal))
                if (BottomGoal == null)
                    BottomGoal = character;
                else
                    Debug.LogError($"Tried to set {character} as BottomGoal, but {BottomGoal} was already BottomGoal.");
        }

        private void Update() {
            ParticlesTop.transform.localPosition =      Vector2.Lerp(ParticlesTop.transform.localPosition, 
                                                        ParticlesTop.transform.localRotation * Vector2.up *
                                                        (Leader == TopGoal
                                                            ? particlesHeightEnabled 
                                                            : particlesHeightDisabled),
                                                        Time.deltaTime * particlesHeightLerpSpeed);
            ParticlesBottom.transform.localPosition =   Vector2.Lerp(ParticlesBottom.transform.localPosition, 
                                                        ParticlesBottom.transform.localRotation * Vector2.up *
                                                        (Leader == BottomGoal
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
            // Set default top/bottom goal roles if roles haven't been assigned
            if (TopGoal == null || BottomGoal == null) {
                if (TopGoal != BottomGoal) // Print warning if somehow only one player is null
                    Debug.LogWarning("One goal was null, but the other wasn't??");
                TopGoal = Leader;
                BottomGoal = Seeker;
            }
            // Default to facing up if orientation is neutral
            if (Orientation == 0)
                Orientation = 1;
            // Flip if orientation matches whether leader is going up
            if ((Orientation == 1) == (Leader == BottomGoal))  {
                Orientation *= -1;
                OnRoleSwap?.Invoke(Orientation);
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