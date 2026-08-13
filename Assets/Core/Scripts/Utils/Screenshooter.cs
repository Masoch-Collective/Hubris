#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Utils {

    public class Screenshooter : Singleton<Screenshooter> {

        public bool editorOnly = true;
        public float interval;
        public int screenshotsTaken;
        public string path;
        public bool appendDate;
        public bool appendTime;
        public bool appendScreenshotCount;
        public string extension = ".png";
        public int disableAfterScreenshotCount = -1;

        private float _startTime;

        // Update is called once per frame
        void Update() {
            if (!Application.isEditor && editorOnly) return; // Only run in editor unless not editor only
            if (!(Mathf.Floor((Time.time - _startTime) / interval) > screenshotsTaken)) return; // Wait for next interval
            screenshotsTaken++;
            CaptureScreenshot();
            if (disableAfterScreenshotCount > 0 && screenshotsTaken >= disableAfterScreenshotCount) {
                Debug.Log($"{screenshotsTaken} screenshots reached; disabling Screenshooter.");
                enabled = false;
            }
        }

#if UNITY_EDITOR
        [MenuItem("Screenshooter/Start")]
#endif
        private static void StartScreenshooter() => Instance.enabled = true;
#if UNITY_EDITOR
        [MenuItem("Screenshooter/Pause")]
#endif
        private static void PauseScreenshooter() => Instance.enabled = false;
#if UNITY_EDITOR
        [MenuItem("Screenshooter/Stop")]
#endif
        private static void StopScreenshooter() {
            PauseScreenshooter();
            ResetCount();
        }
#if UNITY_EDITOR
        [MenuItem("Screenshooter/Reset")]
#endif
        private static void ResetCount() {
            Instance.screenshotsTaken = 0;
            Instance._startTime = Time.time; 
        }
#if UNITY_EDITOR
        [MenuItem("Screenshooter/Capture Screenshot")]
#endif
        private static void ManualCapture() => Instance.CaptureScreenshot();
        
        public void CaptureScreenshot() {
            string p = path;
            p += "_" + System.DateTime.Today;
            p += "_" + System.DateTime.Now.TimeOfDay;
            ScreenCapture.CaptureScreenshot(p + extension);
        }

    }

}
