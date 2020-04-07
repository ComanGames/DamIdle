using System;
using UnityEngine;

namespace IdleClickerKit
{
    public class CarDriving : MonoBehaviour
    {
        public float TimeCycle;
        public Transform[] Path;
        public Transform _prev;
        public Transform _next;
        public int index =0;


        private void Update()
        {
            float time = UnityEngine.Time.time%TimeCycle;
            float part = TimeCycle / Path.Length;
            int index = (int)(time / part);
            if (index < Path.Length - 1)
            {
                float f = (time % part) / part;
                transform.position = Vector3.Lerp(Path[index].position, Path[index + 1].position, f);
                transform.rotation = Quaternion.Lerp(Path[index].rotation, Path[index + 1].rotation, f);

            }

        }
    }
}