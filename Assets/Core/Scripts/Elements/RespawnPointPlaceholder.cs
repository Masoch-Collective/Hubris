using System;
using UnityEditor;
using UnityEngine;
using Utils;

namespace Elements {

    [ExecuteInEditMode]
    public class RespawnPointPlaceholder : MonoBehaviour {

        private RespawnPoint TopGoalPrefab => Resources.Load<RespawnPoint>("Prefabs/RespawnPoint_TopGoal");
        private RespawnPoint BottomGoalPrefab => Resources.Load<RespawnPoint>("Prefabs/RespawnPoint_BottomGoal");
        
        public Transform parentTo;
        public RespawnPointPlaceholder selectLeft;
        public RespawnPointPlaceholder selectRight;
        public RespawnPoint Instance {
            get {
                if (_instance == null)
                    _instance = Instantiate(SpawnTopGoal ? TopGoalPrefab : BottomGoalPrefab, transform.position, transform.rotation, parentTo);
                return _instance;
            }
        }
        [NonSerialized] private RespawnPoint _instance;

        private bool SpawnTopGoal => transform.up.y < 0;

        private void Awake() {
            if (!Application.isPlaying)
                return;
            if (selectLeft)
                Instance.selectLeft = selectLeft.Instance;
            if (selectRight)
                Instance.selectRight = selectRight.Instance;
            // This is redundant but idk how to just call the getter function by itself so...
            Instance.enabled = true;
        }

        private void Start() {
            if (!Application.isPlaying)
                return;
            Destroy(gameObject);
        }

        private void OnValidate() {
            name = SpawnTopGoal ? "SpawnPointPlaceholder — TopGoal" : "SpawnPointPlaceholder — BottomGoal";
            if (selectLeft)
                selectLeft.selectRight = this;
            if (selectRight)
                selectRight.selectLeft = this;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos() {
            Miscellaneous.DrawArrowGizmo(transform.position, Color.white * 0.5f, Color.white, transform.rotation.eulerAngles.z, 0.25f, 0);
        }
        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.deepPink;
            if (selectLeft)
                Gizmos.DrawLine(transform.position, selectLeft.transform.position);
            else {
                Gizmos.DrawRay(transform.position, -transform.right);
                Gizmos.DrawLine(transform.position - transform.right + transform.up, 
                    transform.position - transform.right - transform.up);
            }
            Gizmos.color = Color.deepSkyBlue;
            if (selectRight)
                Gizmos.DrawLine(transform.position, selectRight.transform.position);
            else {
                Gizmos.DrawRay(transform.position, transform.right);

                Gizmos.DrawLine(transform.position + transform.right + transform.up,
                    transform.position + transform.right - transform.up);
            }
        }
        [MenuItem("Edit/Replace RespawnPoints in the scene with RespawnPointPlaceholder")]
        public static void ReplaceSpawnPoints() {
            RespawnPoint[] instances;
            for (int i = 0; i < (instances = FindObjectsByType<RespawnPoint>(FindObjectsSortMode.None)).Length; i++) {
                RespawnPointPlaceholder replacement = new GameObject("", typeof(RespawnPointPlaceholder))
                    .GetComponent<RespawnPointPlaceholder>();
                replacement.transform.parent = instances[i].transform.parent;
                replacement.transform.position = instances[i].transform.position;
                replacement.transform.rotation = instances[i].transform.rotation;
                replacement.parentTo = instances[i].GetComponent<ReparentOnStart>().parentTo;
                DestroyImmediate(instances[i].gameObject);
            }
        }
        #endif

    }

}
