using System;
using UnityEngine;

namespace IdleClickerKit
{
    public class WaterFallVisualizer : MonoBehaviour
    {
        public ClickTotal_Text Counter;
        public ParticleSystem[] Particles;
        public ParticleSystem Thin;
        public ParticleSystem Middle;

        public AudioSource Source;
        public float ShowMiddle=1f;
        public float ShowBig=2f;
        public float WaterFlowDelay = 0.2f;
        private float _waterFalltime;

        public void Awake()
        {
            Counter.onCouningUp += OnCountDown;
        }

        private void OnCountDown()
        {
            float f =  ClickManager.Instance.Clicks/ClickManager.Instance.TotalClicks;
            _waterFalltime += WaterFlowDelay;
            StartAllParticles();

        }

        private void StartAllParticles()
        {

            if (!Thin.isPlaying)
                Thin.Play();
            if(_waterFalltime>ShowMiddle&&!Middle.isPlaying)
                Middle.Play();

            if (_waterFalltime > ShowBig)
            {
                Thin.Stop();
                Middle.Stop();
                for (int i = 0; i < Particles.Length; i++)
                {
                    if (!Particles[i].isPlaying)
                        Particles[i].Play();
                }
            }
        }


        private void StopAllParticles()
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                if (Particles[i].isPlaying)
                    Particles[i].Stop();
            }
            if (Middle.isPlaying)
                Middle.Stop();

        }

        private void Update()
        {
            if (_waterFalltime > 0)
            {
                Source.volume = _waterFalltime / 10f;
                _waterFalltime -= Time.deltaTime;
            }
            else
            {
                StopAllParticles();
            }
        }
    }
}