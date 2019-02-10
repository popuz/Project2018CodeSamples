using System;
using UnityEngine;

namespace Project2018CodeSamples.Ctrl
{
    public class InputCtrl : BaseCtrl
    {        
        public event Action InteractAction; 
        public event Action MainAction; 
        public event Action<int> SetActiveSlot; 
        public event Action Escape; 
        public event Action ToggleAnimVisibility;

        public static bool IsGoingBackward => Input.GetAxis("Vertical") < 0;
        public static bool RightMouseButtonIsPressed => Input.GetAxisRaw("Fire2") > 0;
        private void Start() => On();

        public override void On()
        {
            base.On();
            GameManager.Instance.OnGamePause += HandleGamePause;
        }

        public override void Off()
        {
            base.Off();
            GameManager.Instance.OnGamePause -= HandleGamePause;
        }
        
        private void HandleGamePause(bool gameIsPaused)
        {           
            if (gameIsPaused)
                Off();
            else
                On();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                MainAction?.Invoke();
            
            if (Input.GetKeyDown(KeyCode.Escape))
                Escape?.Invoke();            

            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.KeypadEnter))
                InteractAction?.Invoke();
           
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetActiveSlot?.Invoke(1);

            if (Input.GetKeyDown(KeyCode.Alpha2))
                SetActiveSlot?.Invoke(2);

            if (Input.GetKeyDown(KeyCode.BackQuote))
                ToggleAnimVisibility?.Invoke();
        }        
    }
}
