using System;
using UnityEditor;
using UnityEngine;
using XFEL.Ctrl;
using XFEL.Helpers;
using XFEL.Objects;

namespace Project2018CodeSamples
{
    public enum GameStateTypes
    {
        FPS,
        QuestToFPS,
        Quest
    }

    public class GameManager : Singleton<GameManager>
    {
        public event Action OnGameStateChanged;
        public event Action<bool> OnGamePause;
        public event Action<BaseApparatus> OnFocusToApparatus;

        private const float _camAnimTime = 2f;
        private Coroutine _currCoroute;        
        [SerializeField] private GameStateTypes _state;
        public GameStateTypes State
        {
            get { return _state; }
            private set
            {
                if (_state != value)
                {
                    _state = value;
                    OnGameStateChanged?.Invoke();
                }
            }
        }       
        
        public InputCtrl InputCtrl { get; private set; }
        public CameraCtrl CamCtrl { get; private set; }
        public BeamsCtrl BeamsCtrl { get; private set; }
        
        public PlayerCtrl PlayerCtrl { get; private set; }
        public SlotManager SlotManager { get; private set; }
        public CrosshairManager CrosshairManager { get; private set; }
        public BaseApparatus CurrentApparatus { get; private set; }
        
        protected override void Awake()
        {
            base.Awake();
            _state = GameStateTypes.FPS;

            InputCtrl = gameObject.AddComponent<InputCtrl>();                        
            CamCtrl = FindObjectOfType<CameraCtrl>();
            BeamsCtrl = FindObjectOfType<BeamsCtrl>();

            PlayerCtrl = FindObjectOfType<PlayerCtrl>();
            SlotManager = gameObject.AddComponent<SlotManager>();
            CrosshairManager = GetComponent<CrosshairManager>();            
        }

        private void Start() => InputCtrl.Escape += Pause;

        private void Pause()
        {            
            Time.timeScale = TimeIsPaused() ? 1 : 0;
            OnGamePause?.Invoke(TimeIsPaused());

            if (TimeIsPaused())
                InputCtrl.On();
        }
        
        private bool TimeIsPaused() => Math.Abs(Time.timeScale) < float.Epsilon;

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                Pause();
        }

        private void Update()
        {
            if (_state == GameStateTypes.Quest && InputCtrl.IsGoingBackward)
                ReturnToFps();
        }

        public void ReturnToFps()
        {
            if (State == GameStateTypes.QuestToFPS) return;

            State = GameStateTypes.QuestToFPS;
            CamCtrl.SwitchToFpsCam(_camAnimTime, onComplete: () =>
            {
                State = GameStateTypes.FPS;
                OnFocusToApparatus?.Invoke(null);
            });
            CurrentApparatus = null;
            BeamsCtrl.SwitchMainBeamLoop(MainBeamState.NONE);
        }

        public void FocusOnApparatus(BaseApparatus apparatus, Transform targetPivot, Transform targetCamTrsf,
            float animTime)
        {
            CurrentApparatus = apparatus;           
            State = GameStateTypes.Quest;

            CamCtrl.SwitchToOrbitCam(targetPivot, targetCamTrsf, animTime);
            OnFocusToApparatus?.Invoke(apparatus);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}