using UnityEngine;

namespace IOChef.Core
{
    /// <summary>
    /// Singleton manager responsible for playing music and sound effects.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        /// <summary>
        /// Global singleton instance of the AudioManager.
        /// </summary>
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        /// <summary>
        /// AudioSource used to play background music tracks.
        /// </summary>
        [SerializeField] private AudioSource musicSource;
        /// <summary>
        /// AudioSource used to play one-shot sound effects.
        /// </summary>
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume")]
        /// <summary>
        /// Current music volume, clamped between 0 and 1.
        /// </summary>
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        /// <summary>
        /// Current sound-effect volume, clamped between 0 and 1.
        /// </summary>
        [Range(0f, 1f)] public float sfxVolume = 1f;

        /// <summary>
        /// Initializes singleton and persists across scenes.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// Plays a one-shot sound effect at the current SFX volume.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        public void PlaySFX(AudioClip clip)
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip, sfxVolume);
        }

        /// <summary>
        /// Starts playing a music track, replacing any currently playing track.
        /// </summary>
        /// <param name="clip">The music clip to play.</param>
        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the currently playing music track.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Sets the music volume and applies it immediately.
        /// </summary>
        /// <param name="volume">Desired volume between 0 and 1.</param>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }

        /// <summary>
        /// Sets the sound-effect volume used for subsequent SFX playback.
        /// </summary>
        /// <param name="volume">Desired volume between 0 and 1.</param>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
    }
}
