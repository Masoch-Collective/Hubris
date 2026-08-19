using System;
using System.Collections.Generic;
using Character;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Systems {

    public class ReadyUpUtility : Singleton<ReadyUpUtility> {

        public List<CharacterCore> characters;
        public GameObject readyUpPrompt;
        public Toggle p1ReadyToggle;
        public TintElements p1ReadyTint;
        public Toggle p2ReadyToggle;
        public TintElements p2ReadyTint;
        public TextMeshProUGUI sequenceField;
        public List<string> readySequence;
        public float readySequenceStagger;
        public bool Done { get; private set; }

        private float _lastSequenceStepTime;
        private int _readySequenceProgress;

        private void Update() {

            bool ready = true;
            foreach (CharacterCore ch in characters)
                if (!ch.Ready)
                    ready = false;

            p1ReadyToggle.interactable = !(p1ReadyToggle.isOn = characters[0].Ready);
            p1ReadyTint.color =
                p1ReadyToggle.isOn ? p1ReadyToggle.colors.disabledColor : p1ReadyToggle.colors.normalColor;
            p2ReadyToggle.interactable = !(p2ReadyToggle.isOn = characters[1].Ready);
            p2ReadyTint.color =
                p2ReadyToggle.isOn ? p2ReadyToggle.colors.disabledColor : p2ReadyToggle.colors.normalColor;
            
            readyUpPrompt.SetActive(!ready);

            if (ready)
                if (Time.time > _lastSequenceStepTime + readySequenceStagger) {
                    _lastSequenceStepTime = Time.time;
                    _readySequenceProgress++;

                    if (_readySequenceProgress == readySequence.Count - 1)
                        Done = true;

                    if (_readySequenceProgress >= readySequence.Count) {
                        sequenceField.gameObject.SetActive(false);
                        enabled = false;
                        return;
                    }
                    
                } 
            sequenceField.text = readySequence[_readySequenceProgress];

        }

    }

}
