using System;
using UnityEditor;

namespace MasterLoader.Core
{
    public class MasterLoaderScheduler
    {
        private static MasterLoaderScheduleStatus _scheduleStatus = MasterLoaderScheduleStatus.NONE;
        public MasterLoaderScheduleStatus ScheduleStatus => _scheduleStatus;

        private static double _compileWaitTime = 0.0d;

        private static Action _onTickedAction = null;

        public void SetIsWaitingCompiled(bool isWaiting)
        {
            if (isWaiting)
            {
                _scheduleStatus = MasterLoaderScheduleStatus.WAITING_COMPILED;
            }
            else
            {
                _scheduleStatus = MasterLoaderScheduleStatus.NONE;
            }
        }

        public void OnStartWaitingTicked(Action onTicked)
        {
            _onTickedAction += onTicked;
            EditorApplication.update += OnTicked;
            _scheduleStatus = MasterLoaderScheduleStatus.WAITING_TICKED;
        }

        private static void OnTicked()
        {
            if (_compileWaitTime < 30.0d / 60.0d)
            {
                _compileWaitTime = EditorApplication.timeSinceStartup;
                return;
            }
            _onTickedAction.Invoke();

            OnEndTickAction();
        }

        public static void OnEndTickAction()
        {
            _onTickedAction = null;
            EditorApplication.update -= OnTicked;
            _compileWaitTime = 0;
            _scheduleStatus = MasterLoaderScheduleStatus.NONE;
        }
    }
}
