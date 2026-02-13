using UnityEngine;

namespace Soyya.WaffleMonster
{
    /// <summary>
    /// BGM/SE管理 — シングルトン
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _seSource;

        [Header("BGM")]
        public AudioClip BgmTitle;
        public AudioClip BgmGame;
        public AudioClip BgmGameOver;
        public AudioClip BgmGameClear;

        [Header("SE")]
        public AudioClip SeThrow;
        public AudioClip SeHit;
        public AudioClip SeMonsterEat;
        public AudioClip SeMonsterDetect;
        public AudioClip SePickup;
        public AudioClip SeGoal;
        public AudioClip SeCaught;
        public AudioClip SeCountdown;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;
            if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }

        public void PlaySE(AudioClip clip)
        {
            if (clip == null) return;
            _seSource.PlayOneShot(clip);
        }

        public void SetBGMVolume(float volume)
        {
            _bgmSource.volume = Mathf.Clamp01(volume);
        }

        public void SetSEVolume(float volume)
        {
            _seSource.volume = Mathf.Clamp01(volume);
        }
    }
}
