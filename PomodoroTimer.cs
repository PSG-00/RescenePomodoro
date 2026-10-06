using System;
using System.Media;
using System.Windows.Threading;

namespace radiant_noether
{
    public enum PomodoroMode
    {
        Focus,
        Break,
        LongBreak
    }

    public class PomodoroTimer
    {
        private readonly DispatcherTimer _timer;
        private readonly AppSettings _settings;

        public PomodoroMode CurrentMode { get; private set; } = PomodoroMode.Focus;
        public bool IsRunning => _timer.IsEnabled;
        public int RemainingSeconds { get; private set; }
        public int TotalSeconds { get; private set; }
        public int CompletedPomodoros { get; private set; }

        public int CurrentSessionInCycle { get; private set; } = 1;
        public int TotalSessionsInCycle => Math.Max(1, _settings.LongBreakInterval);
        public double ProgressRatio => TotalSeconds > 0 ? Math.Clamp(1.0 - ((double)RemainingSeconds / TotalSeconds), 0.0, 1.0) : 0.0;
        public int ProgressPercent => (int)(ProgressRatio * 100);

        public event Action? OnTick;
        public event Action<PomodoroMode>? OnStateChanged;

        public PomodoroTimer(AppSettings settings)
        {
            _settings = settings;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;

            ResetToCurrentMode();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (RemainingSeconds > 0)
            {
                RemainingSeconds--;
                OnTick?.Invoke();
            }

            if (RemainingSeconds <= 0)
            {
                AdvanceToNextMode();
            }
        }

        public void Start()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                OnTick?.Invoke();
            }
        }

        public void Pause()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                OnTick?.Invoke();
            }
        }

        public void Toggle()
        {
            if (IsRunning) Pause();
            else Start();
        }

        public void Reset()
        {
            Pause();
            ResetToCurrentMode();
            OnTick?.Invoke();
        }

        public void SkipNext()
        {
            Pause();
            AdvanceToNextMode();
        }

        public void SetMode(PomodoroMode newMode)
        {
            Pause();
            CurrentMode = newMode;
            int maxCycle = Math.Max(1, _settings.LongBreakInterval);
            if (newMode == PomodoroMode.LongBreak)
            {
                CurrentSessionInCycle = maxCycle;
            }
            ResetToCurrentMode();
            OnStateChanged?.Invoke(CurrentMode);
            OnTick?.Invoke();
        }

        private void AdvanceToNextMode()
        {
            int maxCycle = Math.Max(1, _settings.LongBreakInterval);

            if (CurrentMode == PomodoroMode.Focus)
            {
                CompletedPomodoros++;

                // 통계 누적
                _settings.CheckAndResetDailyStats();
                _settings.TodayCompletedSessions++;
                _settings.TotalCompletedSessions++;
                _settings.TodayFocusMinutes += _settings.FocusMinutes;
                _settings.Save();

                // 마지막 세션(예: 4회차) 집중 완료 후에는 긴 휴식으로 이어짐
                if (CurrentSessionInCycle >= maxCycle)
                {
                    CurrentMode = PomodoroMode.LongBreak;
                    // CurrentSessionInCycle은 4로 유지 (세션 4/4 긴 휴식 진행)
                }
                else
                {
                    CurrentMode = PomodoroMode.Break;
                    // CurrentSessionInCycle은 그대로 유지 (세션 1/4 집중 -> 세션 1/4 휴식)
                }
            }
            else if (CurrentMode == PomodoroMode.Break)
            {
                // 짧은 휴식 종료 -> 다음 세션 집중으로 이동
                CurrentSessionInCycle++;
                if (CurrentSessionInCycle > maxCycle)
                {
                    CurrentSessionInCycle = 1;
                }
                CurrentMode = PomodoroMode.Focus;
            }
            else // LongBreak
            {
                // 긴 휴식(마지막 세션 휴식) 종료 -> 새로운 1번째 세션 집중으로 순환
                CurrentSessionInCycle = 1;
                CurrentMode = PomodoroMode.Focus;
            }

            ResetToCurrentMode();
            PlayNotificationSound();
            OnStateChanged?.Invoke(CurrentMode);
            OnTick?.Invoke();
        }

        private void ResetToCurrentMode()
        {
            int minutes = CurrentMode switch
            {
                PomodoroMode.Focus => _settings.FocusMinutes,
                PomodoroMode.Break => _settings.BreakMinutes,
                PomodoroMode.LongBreak => _settings.LongBreakMinutes,
                _ => _settings.FocusMinutes
            };

            TotalSeconds = Math.Max(1, minutes * 60);
            RemainingSeconds = TotalSeconds;
        }

        public void RefreshSettings()
        {
            // 시간 설정이 바뀌었을 때 만약 대기 상태면 시간 갱신
            if (!IsRunning)
            {
                ResetToCurrentMode();
                OnTick?.Invoke();
            }
        }

        private void PlayNotificationSound()
        {
            if (!_settings.PlaySound) return;

            SoundManager.PlayVoice(_settings.Member, CurrentMode == PomodoroMode.Focus);
        }

        public string GetFormattedTime()
        {
            int m = RemainingSeconds / 60;
            int s = RemainingSeconds % 60;
            return $"{m:00}:{s:00}";
        }
    }
}
