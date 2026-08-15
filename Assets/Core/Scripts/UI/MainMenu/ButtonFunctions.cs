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

        public void PagePrev(Transform holder) => PageNav(holder, -1);
        public void PageNext(Transform holder) => PageNav(holder, 1);
        public void PageNav(Transform holder, int offset) {
            bool hadIndex = int.TryParse(holder.name.Split('.')[0], out int index);
            index = Mathf.Clamp(index + offset, 0, holder.childCount - 1);
            if (hadIndex)
                holder.name = index.ToString().PadLeft(3, '0') + holder.name.Substring(3);
            else {
                Debug.LogWarning($"Paginated GameObject {holder.name} did not have a name starting with \"###.\"!");
                holder.name = index.ToString().PadLeft(3, '0') + "." + holder.name;
            }
            for (int i = 0; i < holder.childCount; i++)
                holder.GetChild(i).gameObject.SetActive(i == index);
        }

        public void Quit() {
            Application.Quit();
        }

    }

}