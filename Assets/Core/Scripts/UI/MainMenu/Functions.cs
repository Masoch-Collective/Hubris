using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.MainMenu {

    public class Functions : MonoBehaviour {

        public string gameSceneName;
        public List<CanvasGroup> panels;
        public string currentPanel;

        public void Start() {
            ShowPanel(currentPanel);
        }

        public void StartGame() {
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        public void ShowPanel(string panelName) {
            if (panels.Find((panel) => panel.gameObject.name == panelName) != null) {
                foreach (var panel in panels)
                    panel.gameObject.SetActive(panel.gameObject.name == panelName);
                currentPanel = panelName;
            }
        }

        public void Quit() {
            Application.Quit();
        }

    }

}