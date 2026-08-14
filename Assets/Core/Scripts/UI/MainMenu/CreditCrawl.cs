using System;
using UnityEngine;

namespace UI.MainMenu {

    public class CreditCrawl : MonoBehaviour {

        private RectTransform _transform;

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
        public new RectTransform transform {
            get {
                if (!_transform)
                    _transform = GetComponent<RectTransform>();

                return _transform;
            }
        }

        private RectTransform _parentRect;
        public RectTransform ParentRect {
            get {
                if (!_parentRect)
                    _parentRect = transform.parent.GetComponent<RectTransform>();
                return _parentRect;
            }
        }

        public float crawlSpeed;

        private void Update() {
            transform.Translate(Vector2.up * crawlSpeed * Time.deltaTime);
            if (Done())
                Reset();
        }
        
        public void OnEnable() => Reset();

        public void Reset() => transform.anchoredPosition = Vector2.zero;

        private bool Done() => transform.position.y - transform.sizeDelta.y > ParentRect.rect.yMax;

    }

}