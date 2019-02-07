using System;
using UnityEditor;
using UnityEngine;
using XFEL.Ctrl;
using XFEL.Helpers;
using XFEL.Objects;

namespace Project2018
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

        public PlayerCtrl PlayerCtrl { get; private set; }
        public CameraCtrl CamCtrl { get; private set; }
        public BeamsCtrl BeamsCtrl { get; private set; }
        public InputCtrl InputCtrl { get; private set; }
        public SlotManager SlotManager { get; private set; }
        public CrosshairManager CrosshairManager { get; private set; }

        public BaseApparatus CurrentApparatus { get; private set; }

        private const float _camAnimTime = 2f;
        private Coroutine _currCoroute;


        protected override void Awake()
        {
            base.Awake();
            _state = GameStateTypes.FPS;

            PlayerCtrl = FindObjectOfType<PlayerCtrl>();
            CamCtrl = FindObjectOfType<CameraCtrl>();
            BeamsCtrl = FindObjectOfType<BeamsCtrl>();

            InputCtrl = gameObject.AddComponent<InputCtrl>();
            SlotManager = gameObject.AddComponent<SlotManager>();
            CrosshairManager = GetComponent<CrosshairManager>();

            InputCtrl.Escape += Pause;
        }

        private void Pause()
        {
            Time.timeScale = TimeIsNotPaused() ? 0 : 1;
            OnGamePause?.Invoke(TimeIsPaused());

            if (TimeIsPaused())
                InputCtrl.On();
        }

        private bool TimeIsPaused() => Math.Abs(Time.timeScale) < float.Epsilon;
        private bool TimeIsNotPaused() => Math.Abs(Time.timeScale - 1) < float.Epsilon;

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