using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;

namespace radiant_noether
{
    public static class SoundManager
    {
        public static string SoundsDirectory => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");

        private static MediaPlayer? _mediaPlayer;

        public static void EnsureSoundsFolder()
        {
            if (!Directory.Exists(SoundsDirectory))
            {
                Directory.CreateDirectory(SoundsDirectory);
            }

            string readmePath = Path.Combine(SoundsDirectory, "음성파일_안내.txt");
            if (!File.Exists(readmePath))
            {
                File.WriteAllText(readmePath,
@"✨ RESCENE 멤버별 음성 파일 안내 ✨

이 폴더에 멤버별 음성 파일(.mp3 또는 .wav)을 넣어주시면,
집중 모드 시작 / 휴식 모드 시작 시 해당 멤버의 목소리가 재생됩니다!

[파일명 규칙]
- 원이:   woni_focus.mp3 (집중 시작) / woni_break.mp3 (휴식 시작)
- 리브:   liv_focus.mp3 (집중 시작)  / liv_break.mp3 (휴식 시작)
- 미나미: minami_focus.mp3 (집중 시작) / minami_break.mp3 (휴식 시작)
- 메이:   may_focus.mp3 (집중 시작)   / may_break.mp3 (휴식 시작)
- 제나:   zena_focus.mp3 (집중 시작)  / zena_break.mp3 (휴식 시작)

* 확장자는 .mp3 와 .wav 둘 다 지원합니다!
* 파일이 아직 없을 때는 기본 시스템 알림음이 안전하게 재생됩니다.
");
            }
        }

        public static void PlayVoice(string memberId, bool isFocus)
        {
            EnsureSoundsFolder();

            string stateKey = isFocus ? "focus" : "break";
            string[] extensions = { ".mp3", ".wav" };
            string? foundFile = null;

            foreach (var ext in extensions)
            {
                string path = Path.Combine(SoundsDirectory, $"{memberId}_{stateKey}{ext}");
                if (File.Exists(path))
                {
                    foundFile = path;
                    break;
                }
            }

            if (foundFile != null)
            {
                try
                {
                    _mediaPlayer ??= new MediaPlayer();
                    _mediaPlayer.Stop();
                    _mediaPlayer.Open(new Uri(foundFile, UriKind.Absolute));
                    _mediaPlayer.Play();
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to play voice file: {ex.Message}");
                }
            }

            // 개별 음성 파일이 없을 경우 기본 알림음 재생
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch { }
        }

        public static void OpenSoundsFolder()
        {
            try
            {
                EnsureSoundsFolder();
                Process.Start(new ProcessStartInfo
                {
                    FileName = SoundsDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"음성 폴더를 여는 중 오류가 발생했습니다: {ex.Message}");
            }
        }
    }
}
