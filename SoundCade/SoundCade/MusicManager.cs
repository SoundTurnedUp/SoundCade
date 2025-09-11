using System.Media;

namespace SoundCade
{
    public struct BeepData
    {
        public double Frequency;
        public double Duration;
    }

    public class Beep
    {
        public static int Volume { get; set; } = 10;

        public static void Play(BeepData data)
        {
            BeepBeep(Volume, data.Frequency, data.Duration);
        }

        public static void Play(double frequency, double duration)
        {
            BeepBeep(Volume, frequency, duration);
        }

        private static void BeepBeep(double Amplitude, double Frequency, double Duration)
        {
            if (!OperatingSystem.IsWindows()) return;

            Duration += Duration * 0.1;
            double Amp = ((Amplitude * (Math.Pow(2, 15))) / 1000) - 1;
            double DeltaFT = 2 * Math.PI * Frequency / 44100.0;

            int Samples = (int)(441.0 * Duration / 10.0);
            int Bytes = Samples * sizeof(int);
            int[] Hdr = { 0X46464952, 36 + Bytes, 0X45564157, 0X20746D66, 16, 0X20001, 44100, 176400, 0X100004, 0X61746164, Bytes };

            using (MemoryStream MS = new MemoryStream(44 + Bytes))
            {
                using BinaryWriter BW = new BinaryWriter(MS);
                for (int I = 0; I < Hdr.Length; I++)
                {
                    BW.Write(Hdr[I]);
                }
                for (int T = 0; T < Samples; T++)
                {
                    short Sample = Convert.ToInt16(Amp * Math.Sin(DeltaFT * T));
                    BW.Write(Sample);
                    BW.Write(Sample);
                }

                BW.Flush();
                MS.Seek(0, SeekOrigin.Begin);
                using SoundPlayer SP = new SoundPlayer(MS);
                SP.PlaySync();
            }
            Thread.Sleep(20);
        }
    }

    public class Tones
    {
        public enum Octave
        {
            DoublePedal,
            Pedal,
            Deep,
            Low,
            Middle,
            Tenor,
            High,
            DoubleHigh,
            Size
        };

        public enum Duration
        {
            Whole = 1000,
            Half = 500,
            Quarter = 250,
            Eighth = 125,
            Sixteenth = 62
        };

        public enum Note
        {
            C, Cs, D, Ds, E, F, Fs, G, Gs, A, As, B, Size
        };

        public static int[] Frequency = {
            16,17,18,19,21,22,23,24,26,27,29,31,
            33,35,37,39,41,44,46,49,52,55,58,62,
            65,69,73,78,82,87,92,98,104,110,116,123,
            131,139,147,155,165,175,185,196,208,220,233,245,
            262,277,294,311,330,349,370,392,415,440,466,494,
            523,554,587,622,659,698,740,784,831,880,932,988,
            1046,1109,1175,1244,1328,1397,1480,1568,1661,1760,1865,1975,
            2093,2217,2349,2489,2637,2794,2960,3136,3322,3520,3729,3951
        };

        public static void PlaySong(string song)
        {
            string[] tokens = song.Split(',');
            int freq, dur;
            foreach (string token in tokens)
            {
                Parse(token, out freq, out dur);
                Beep.Play(freq, dur);
            }
        }

        public static void PlayNote(Note note)
        {
            PlayNote(note, Octave.Middle, Duration.Quarter);
        }

        public static void PlayNote(Note note, Octave oct)
        {
            PlayNote(note, oct, Duration.Quarter);
        }

        public static void PlayNote(Note note, Duration dur)
        {
            PlayNote(note, Octave.Middle, dur);
        }

        public static void PlayNote(Note note, Octave oct, Duration dur)
        {
            int freq = GetFreq(note, oct);
            Beep.Play(freq, (int)dur);
        }

        public static int GetFreq(Note note, Octave oct)
        {
            return GetFreq((int)note, (int)oct);
        }

        public static int GetFreq(Note note, int oct)
        {
            if (oct < 0 || oct >= (int)Octave.Size)
                throw new ArgumentOutOfRangeException("oct", "Octave value exceeds range.");
            return GetFreq((int)note, oct);
        }

        private static int GetFreq(int note, int oct)
        {
            return Frequency[(int)Note.Size * oct + note];
        }

        public static void Parse(string line, out int freq, out int dur)
        {
            freq = 0;
            dur = (int)Duration.Quarter;
            int octIdx = (int)Octave.Middle;

            string[] tokens = line.Split('-');
            int noteIdx = ParseNote(tokens.First().ToUpper().Trim());

            if (tokens.Length >= 3)
            {
                octIdx = ParseOctave(tokens[1]);
            }

            if (tokens.Length >= 2)
            {
                dur = ParseDuration(tokens.Last());
            }

            if (tokens.First().ToUpper().Trim() == "P")
            {
                freq = 22000;
            }
            else
            {
                freq = GetFreq(noteIdx, octIdx);
            }
        }

        private static int ParseDuration(string strValue)
        {
            float step;
            int milliseconds;

            if (!float.TryParse(strValue, out step))
                step = (float)Duration.Whole;

            switch ((int)step)
            {
                case 1: milliseconds = (int)Duration.Whole; break;
                case 2: milliseconds = (int)Duration.Half; break;
                case 4: milliseconds = (int)Duration.Quarter; break;
                case 8: milliseconds = (int)Duration.Eighth; break;
                case 16: milliseconds = (int)Duration.Sixteenth; break;
                default: milliseconds = (int)Duration.Whole; break;
            }

            if (step - (int)step > 0.0)
                milliseconds += (milliseconds / 2);

            return milliseconds;
        }

        private static int ParseOctave(string strValue)
        {
            int octIdx;
            if (!int.TryParse(strValue, out octIdx))
                octIdx = (int)Octave.Middle;
            return octIdx;
        }

        private static int ParseNote(string strValue)
        {
            int noteIdx;
            switch (strValue)
            {
                case "C": noteIdx = (int)Note.C; break;
                case "CS": noteIdx = (int)Note.Cs; break;
                case "D": noteIdx = (int)Note.D; break;
                case "DS": noteIdx = (int)Note.Ds; break;
                case "E": noteIdx = (int)Note.E; break;
                case "F": noteIdx = (int)Note.F; break;
                case "FS": noteIdx = (int)Note.Fs; break;
                case "G": noteIdx = (int)Note.G; break;
                case "GS": noteIdx = (int)Note.Gs; break;
                case "A": noteIdx = (int)Note.A; break;
                case "AS": noteIdx = (int)Note.As; break;
                case "B": noteIdx = (int)Note.B; break;
                default: noteIdx = (int)Note.C; break;
            }
            return noteIdx;
        }
    }

    public class BackgroundMusic
    {
        private static Task? musicTask;
        private static CancellationTokenSource? cancellationTokenSource;
        private static bool isPlaying = false;

        public static string GetTetrisSong()
        {
            string song = string.Empty;
            song += "E-5-4,B-4-8,C-5-8,D-5-4,C-5-8,B-4-8,";
            song += "A-4-4,A-4-8,C-5-8,E-5-4,D-5-8,C-5-8,";
            song += "B-4-4.5,C-5-8,D-5-4,E-5-4,";
            song += "C-5-4,A-4-4,A-4-8,A-4-8,B-4-8,C-5-8,";
            song += "D-5-4.5,F-5-8,A-5-4,G-5-8,F-5-8,";
            song += "E-5-4.5,C-5-8,E-5-4,D-5-8,C-5-8,";
            song += "B-4-4,B-4-8,C-5-8,D-5-4,E-5-4,";
            song += "C-5-4,A-4-4,A-4-4,P-4,";
            song += "E-5-2,C-5-2,D-5-2,B-4-2,C-5-2,A-4-2,";
            song += "GS-4-2,B-4-4,P-4,E-5-2,C-5-2,D-5-2,B-4-2,";
            song += "C-5-4,E-5-4,A-5-2,GS-5-2";
            return song;
        }

        public static void StartTetrisMusic()
        {

            if (isPlaying) return;

            cancellationTokenSource = new CancellationTokenSource();
            isPlaying = true;

            musicTask = Task.Run(async () =>
            {
                string tetrisSong = GetTetrisSong();
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Run(() => Tones.PlaySong(tetrisSong), cancellationTokenSource.Token);
                        await Task.Delay(500, cancellationTokenSource.Token); // Brief pause between loops
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }, cancellationTokenSource.Token);
        }

        public static void StopMusic()
        {
            if (isPlaying)
            {
                cancellationTokenSource?.Cancel();
                isPlaying = false;
            }
        }
    }
}