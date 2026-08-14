using System;
using System.Collections.Generic;
using Character;
using TMPro;
using UnityEngine;
using Utils;

namespace Systems {

    public class ReadyUpUtility : Singleton<ReadyUpUtility> {

        public List<CharacterCore> characters;
        public GameObject readyUpPrompt;
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
