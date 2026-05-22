using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UI {

    public sealed class BasicGameMenu : MonoBehaviour {
        private const float PanelWidth = 520f;
        private const float ButtonHeight = 64f;
        private const float PanelPadding = 36f;
        private const int BorderSize = 4;

        private enum MenuState {
            Main,
            Playing,
            Paused,
            Options,
            Credits
        }

        private static BasicGameMenu _instance;

        private MenuState _state = MenuState.Main;
        private MenuState _returnState = MenuState.Main;
        private bool _muted;
        private float _volume = 1f;
        private GUIStyle _buttonStyle;
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _toggleStyle;
        private Texture2D _cyanTexture;
        private Texture2D _darkBlueTexture;
        private Texture2D _buttonTexture;
        private Texture2D _buttonHoverTexture;
        private Texture2D _buttonActiveTexture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap() {
            if (_instance != null)
                return;

            GameObject menuObject = new GameObject(nameof(BasicGameMenu));
            DontDestroyOnLoad(menuObject);
            _instance = menuObject.AddComponent<BasicGameMenu>();
        }

        private void Awake() {
            if (_instance != null && _instance != this) {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            ShowMainMenu();
        }

        private void Update() {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
                return;

            if (_state == MenuState.Playing)
                PauseGame();
            else if (_state == MenuState.Paused)
                ResumeGame();
            else if (_state == MenuState.Options || _state == MenuState.Credits)
                _state = _returnState;
        }

        private void OnGUI() {
            EnsureStyles();

            GUI.color = new Color(0f, 0f, 0f, 0.5f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            switch (_state) {
                case MenuState.Main:
                    DrawMainMenu();
                    break;
                case MenuState.Paused:
                    DrawPauseMenu();
                    break;
                case MenuState.Options:
                    DrawOptionsMenu();
                    break;
                case MenuState.Credits:
                    DrawCreditsMenu();
                    break;
            }
        }

        private void DrawMainMenu() {
            Rect panel = CenteredPanel(460f);
            DrawPanel(panel, "HUBRIS");

            GUILayout.BeginArea(Inner(panel));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Start", _buttonStyle, GUILayout.Height(ButtonHeight)))
                StartGame();
            if (GUILayout.Button("Options", _buttonStyle, GUILayout.Height(ButtonHeight)))
                OpenSubmenu(MenuState.Options);
            if (GUILayout.Button("Credits", _buttonStyle, GUILayout.Height(ButtonHeight)))
                OpenSubmenu(MenuState.Credits);
            if (GUILayout.Button("Exit", _buttonStyle, GUILayout.Height(ButtonHeight)))
                ExitGame();

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        private void DrawPauseMenu() {
            Rect panel = CenteredPanel(390f);
            DrawPanel(panel, "PAUSED");

            GUILayout.BeginArea(Inner(panel));
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Resume", _buttonStyle, GUILayout.Height(ButtonHeight)))
                ResumeGame();
            if (GUILayout.Button("Options", _buttonStyle, GUILayout.Height(ButtonHeight)))
                OpenSubmenu(MenuState.Options);
            if (GUILayout.Button("End Game", _buttonStyle, GUILayout.Height(ButtonHeight)))
                EndGame();
            if (GUILayout.Button("Exit", _buttonStyle, GUILayout.Height(ButtonHeight)))
                ExitGame();

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
        }

        private void DrawOptionsMenu() {
            Rect panel = CenteredPanel(380f);
            DrawPanel(panel, "OPTIONS");

            GUILayout.BeginArea(Inner(panel));
            GUILayout.Space(36f);

            _muted = GUILayout.Toggle(_muted, "Mute Audio", _toggleStyle);
            AudioListener.pause = _muted;

            GUILayout.Label($"Volume: {Mathf.RoundToInt(_volume * 100f)}%", _labelStyle);
            _volume = GUILayout.HorizontalSlider(_volume, 0f, 1f);
            AudioListener.volume = _volume;

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", _buttonStyle, GUILayout.Height(ButtonHeight)))
                _state = _returnState;

            GUILayout.EndArea();
        }

        private void DrawCreditsMenu() {
            Rect panel = CenteredPanel(340f);
            DrawPanel(panel, "CREDITS");

            GUILayout.BeginArea(Inner(panel));
            GUILayout.Space(48f);
            GUILayout.Label("Hubris", _labelStyle);
            GUILayout.Label("A student game project", _labelStyle);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Back", _buttonStyle, GUILayout.Height(ButtonHeight)))
                _state = _returnState;

            GUILayout.EndArea();
        }

        private void StartGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
        }

        private void PauseGame() {
            _state = MenuState.Paused;
            Time.timeScale = 0f;
        }

        private void ResumeGame() {
            _state = MenuState.Playing;
            Time.timeScale = 1f;
        }

        private void ShowMainMenu() {
            _state = MenuState.Main;
            _returnState = MenuState.Main;
            Time.timeScale = 0f;
        }

        private void EndGame() {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            if (!string.IsNullOrEmpty(activeScene.path))
                SceneManager.LoadScene(activeScene.path);
            else
                SceneManager.LoadScene(activeScene.buildIndex);

            ShowMainMenu();
        }

        private void OpenSubmenu(MenuState submenu) {
            _returnState = _state;
            _state = submenu;
        }

        private static void ExitGame() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private static Rect CenteredPanel(float height) {
            return new Rect(
                (Screen.width - PanelWidth) * 0.5f,
                (Screen.height - height) * 0.5f,
                PanelWidth,
                height);
        }

        private static Rect Inner(Rect rect) {
            return new Rect(
                rect.x + PanelPadding,
                rect.y + 72f,
                rect.width - PanelPadding * 2f,
                rect.height - 96f);
        }

        private void DrawPanel(Rect rect, string title) {
            GUI.Box(rect, GUIContent.none, _boxStyle);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, BorderSize), _cyanTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - BorderSize, rect.width, BorderSize), _cyanTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, BorderSize, rect.height), _cyanTexture);
            GUI.DrawTexture(new Rect(rect.xMax - BorderSize, rect.y, BorderSize, rect.height), _cyanTexture);

            Rect titleRect = new Rect(rect.x, rect.y + 16f, rect.width, 44f);
            GUI.Label(titleRect, title, _labelStyle);
        }

        private void EnsureStyles() {
            if (_buttonStyle != null)
                return;

            Color cyan = new Color32(0, 204, 204, 255);
            Color yellow = new Color32(255, 255, 64, 255);
            Color darkBlue = new Color32(0, 0, 170, 245);
            Color buttonBlue = new Color32(0, 72, 180, 255);
            Color hoverBlue = new Color32(0, 118, 220, 255);
            Color activeYellow = new Color32(204, 204, 0, 255);

            _cyanTexture = MakeTexture(cyan);
            _darkBlueTexture = MakeTexture(darkBlue);
            _buttonTexture = MakeTexture(buttonBlue);
            _buttonHoverTexture = MakeTexture(hoverBlue);
            _buttonActiveTexture = MakeTexture(activeYellow);

            _boxStyle = new GUIStyle(GUI.skin.box) {
                normal = {
                    background = _darkBlueTexture,
                    textColor = yellow
                },
                border = new RectOffset(BorderSize, BorderSize, BorderSize, BorderSize),
                padding = new RectOffset(16, 16, 16, 16)
            };

            _buttonStyle = new GUIStyle(GUI.skin.button) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = {
                    background = _buttonTexture,
                    textColor = Color.white
                },
                hover = {
                    background = _buttonHoverTexture,
                    textColor = yellow
                },
                active = {
                    background = _buttonActiveTexture,
                    textColor = Color.black
                },
                focused = {
                    background = _buttonHoverTexture,
                    textColor = yellow
                },
                margin = new RectOffset(0, 0, 8, 8)
            };

            _labelStyle = new GUIStyle(GUI.skin.label) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = {
                    textColor = yellow
                }
            };

            _toggleStyle = new GUIStyle(GUI.skin.toggle) {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = {
                    textColor = Color.white
                },
                hover = {
                    textColor = yellow
                }
            };
        }

        private static Texture2D MakeTexture(Color color) {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
