using UnityEngine;

namespace Project2018CodeSamples.Ctrl
{    
    public abstract class BaseCtrl : MonoBehaviour
    {       
        public bool Enabled { get; private set; }

        protected virtual void Awake()
        {
            Enabled = true;
            enabled = true;
        }
        public virtual void On()
        {
            if (Enabled) return;
            Enabled = true;
            enabled = true;
        }

        public virtual void Off()
        {
            if (!Enabled) return;
            Enabled = false;
            enabled = false;
        }
    }
}
