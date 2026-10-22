using UnityEngine;

namespace SoulsALaCarte.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource loopSource;

        private AudioClip clipClick;
        private AudioClip clipPickup;
        private AudioClip clipDrop;
        private AudioClip clipSizzle;
        private AudioClip clipBell;
        private AudioClip clipCoin;
        private AudioClip clipWash;
        private AudioClip clipSpray;
        private AudioClip clipRoar;
        private AudioClip clipBuzzer;
        private AudioClip clipBoss;
        private AudioClip clipWin;
        private AudioClip clipLose;
        private AudioClip clipLaugh;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (transform.parent != null) transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            loopSource = gameObject.AddComponent<AudioSource>();
            loopSource.playOnAwake = false;
            loopSource.loop = true;
            loopSource.volume = 0.25f;

            GenerateClips();
            StartTrainAmbient();
        }

        private void GenerateClips()
        {
            clipClick = MakeTone(0.04f, 800f, 1200f, 0.4f, WaveType.Square);
            clipPickup = MakeTone(0.08f, 400f, 750f, 0.5f, WaveType.Sine);
            clipDrop = MakeTone(0.09f, 250f, 120f, 0.5f, WaveType.Sine);
            clipSizzle = MakeNoise(0.45f, 0.35f);
            clipBell = MakeTone(0.35f, 1050f, 1050f, 0.6f, WaveType.Sine);
            clipCoin = MakeArpeggio(new[] { 987f, 1318f }, 0.14f, 0.55f);
            clipWash = MakeNoise(0.5f, 0.4f);
            clipSpray = MakeNoise(0.6f, 0.5f);
            clipRoar = MakeTone(0.55f, 140f, 70f, 0.8f, WaveType.Square);
            clipBuzzer = MakeTone(0.25f, 180f, 160f, 0.7f, WaveType.Square);
            clipBoss = MakeTone(0.8f, 120f, 60f, 0.9f, WaveType.Square);
            clipWin = MakeArpeggio(new[] { 523f, 659f, 783f, 1046f }, 0.45f, 0.7f);
            clipLose = MakeArpeggio(new[] { 440f, 415f, 370f, 293f }, 0.55f, 0.7f);
            clipLaugh = MakeArpeggio(new[] { 700f, 950f, 700f, 1050f, 750f, 1100f }, 0.45f, 0.65f);
        }

        private enum WaveType { Sine, Square, Noise }

        private AudioClip MakeTone(float duration, float startFreq, float endFreq, float volume, WaveType type)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float phase = (2f * Mathf.PI * currentFreq * i) / sampleRate;

                float sample = 0f;
                if (type == WaveType.Sine)
                    sample = Mathf.Sin(phase);
                else if (type == WaveType.Square)
                    sample = Mathf.Sin(phase) >= 0 ? 0.7f : -0.7f;

                // Envolvente simple (Fade out)
                float envelope = 1f - t;
                data[i] = sample * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("Tone_" + startFreq, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip MakeNoise(float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];
            System.Random rnd = new System.Random(42);

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float sample = (float)(rnd.NextDouble() * 2.0 - 1.0);
                float envelope = Mathf.Sin(t * Mathf.PI); // campana
                data[i] = sample * envelope * volume;
            }

            AudioClip clip = AudioClip.Create("Noise", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip MakeArpeggio(float[] notes, float duration, float volume)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            int noteCount = notes.Length;
            int samplesPerNote = sampleCount / noteCount;

            for (int n = 0; n < noteCount; n++)
            {
                float freq = notes[n];
                int start = n * samplesPerNote;
                int end = (n == noteCount - 1) ? sampleCount : (n + 1) * samplesPerNote;

                for (int i = start; i < end; i++)
                {
                    float noteT = (float)(i - start) / (end - start);
                    float phase = (2f * Mathf.PI * freq * i) / sampleRate;
                    float sample = Mathf.Sin(phase);
                    float envelope = 1f - noteT * 0.5f;
                    data[i] = sample * envelope * volume;
                }
            }

            AudioClip clip = AudioClip.Create("Arp", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void StartTrainAmbient()
        {
            // Genera bucle rítmico de vías del tren
            int sampleRate = 44100;
            float duration = 1.2f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float rhythm = (Mathf.Sin(t * 16f) > 0.8f ? 0.15f : 0.02f);
                float rumble = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.15f;
                data[i] = (rhythm + rumble) * 0.25f;
            }

            AudioClip trainLoop = AudioClip.Create("TrainLoop", sampleCount, 1, sampleRate, false);
            trainLoop.SetData(data, 0);

            loopSource.clip = trainLoop;
            loopSource.Play();
        }

        public void PlayClick() => sfxSource.PlayOneShot(clipClick);
        public void PlayPickup() => sfxSource.PlayOneShot(clipPickup);
        public void PlayDrop() => sfxSource.PlayOneShot(clipDrop);
        public void PlaySizzle() => sfxSource.PlayOneShot(clipSizzle);
        public void PlayBell() => sfxSource.PlayOneShot(clipBell);
        public void PlayCoin() => sfxSource.PlayOneShot(clipCoin);
        public void PlayWash() => sfxSource.PlayOneShot(clipWash);
        public void PlaySpray() => sfxSource.PlayOneShot(clipSpray);
        public void PlayRoar() => sfxSource.PlayOneShot(clipRoar);
        public void PlayBuzzer() => sfxSource.PlayOneShot(clipBuzzer);
        public void PlayBoss() => sfxSource.PlayOneShot(clipBoss);
        public void PlayWin() => sfxSource.PlayOneShot(clipWin);
        public void PlayLose() => sfxSource.PlayOneShot(clipLose);
        public void PlayLaugh() => sfxSource.PlayOneShot(clipLaugh);

        public void SetVolume(float vol)
        {
            AudioListener.volume = Mathf.Clamp01(vol);
        }
    }
}
