using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocialApp;

namespace SocialApp
{
    public class TypingAnimation : MonoBehaviour
    {
        public float Delay;
        public float Speed;
        public float MaxScaleValue;
        public float MinScaleValue;

        private float CurrentScale;

        private bool CanAnimate = false;

        private void Start()
        {
            CurrentScale = MaxScaleValue;
            transform.localScale = Vector3.zero;
            StartCoroutine(AnimateDot());
        }

        private IEnumerator AnimateDot()
        {
            yield return new WaitForSeconds(Delay);
            CanAnimate = true;
        }

        private void Update()
        {
            if (CanAnimate)
                transform.localScale = Vector3.one * Mathf.PingPong((Time.time + Delay) * Speed, MaxScaleValue);
        }

    }
}
