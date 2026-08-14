using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace UI.MainMenu {

    public class ButtonFunctions : Singleton<ButtonFunctions> {

        // TODO: Rework to incorporate Core and Components design pattern
        public string gameSceneName;
        public List<CanvasGroup> panels;
        public CanvasGroup defaultPanel;

        private CanvasGroup _currentPanel;
        public CanvasGroup CurrentPanel { get; private set; }

        public void Start() {
            ShowPanel(defaultPanel);
        }

        public void StartGame() {
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        public void ShowPanel(CanvasGroup show) {
            foreach (var panel in panels)
                panel.gameObject.SetActive(panel == show);
            CurrentPanel = show;
        }

        public void Quit() {
            Application.Quit();
        }

    }

}