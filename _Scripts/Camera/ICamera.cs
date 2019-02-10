using UnityEngine;

namespace Project2018CodeSamples
{
    interface ICamera
    {
        void Init(Transform cam, Transform pivot);
        void Tick(float mouseX, float mouseY, float scrollWheel);
    }
}
