using UnityEngine;

namespace NeonCore
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [Header("Audio Sources")]
        public AudioSource musicSource; // Nhạc nền
        public AudioSource sfxSource;   // Hiệu ứng (bắn, nổ)
        
        [Header("Music Playlist")]
        public AudioClip menuMusic;
        public AudioClip[] gameMusic; // Danh sách nhạc nền (Kéo thả game1...game9 vào đây)
        public AudioClip[] bossMusic;

        [Header("Sound Clips (Drag assets here)")]
        public AudioClip shootSound;
        public AudioClip laserSound;
        public AudioClip blastSound;     // Tiếng pháo nổ
        public AudioClip teslaSound;     // Tiếng sét
        public AudioClip explosionSound;
        public AudioClip hitSound;
        public AudioClip coreHitSound;
        public AudioClip upgradeSound;
        public AudioClip clickSound;
        public AudioClip landSound; // Âm thanh đáp đất

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Giữ âm thanh xuyên suốt các màn chơi
                
                // Tự động tạo Music Source nếu chưa có
                if (musicSource == null)
                {
                    GameObject musicObj = new GameObject("MusicSource");
                    musicObj.transform.SetParent(transform);
                    musicSource = musicObj.AddComponent<AudioSource>();
                    musicSource.loop = true; // Tự động bật lặp lại
                    musicSource.playOnAwake = true;
                }

                // Tự động tạo SFX Source nếu chưa có
                if (sfxSource == null)
                {
                    GameObject sfxObj = new GameObject("SFXSource");
                    sfxObj.transform.SetParent(transform);
                    sfxSource = sfxObj.AddComponent<AudioSource>();
                    sfxSource.loop = false; // SFX thì không lặp
                }
                
                // Bắt đầu phát nhạc ngẫu nhiên
                PlayRandomGameMusic();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayRandomGameMusic()
        {
            if (gameMusic != null && gameMusic.Length > 0 && musicSource != null)
            {
                // Chọn ngẫu nhiên 1 bài
                AudioClip randomClip = gameMusic[Random.Range(0, gameMusic.Length)];
                if (musicSource.clip != randomClip) 
                {
                    musicSource.clip = randomClip;
                    musicSource.Play();
                }
            }
        }
        
        public void PlayBossMusic()
        {
             if (bossMusic != null && bossMusic.Length > 0 && musicSource != null)
            {
                musicSource.clip = bossMusic[Random.Range(0, bossMusic.Length)];
                musicSource.Play();
            }
        }

        // --- HÀM MỚI ĐỂ FIX LỖI ---
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.clip = clip;
                musicSource.Play();
            }
        }

        public void PlayRandomMusic() 
        { 
            PlayRandomGameMusic(); 
        }

        public void PlayRelease(Vector3 pos) { PlaySFX(shootSound); } // Ví dụ helper

        // Hàm phát âm thanh cơ bản
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        // Hàm phát âm thanh ngẫu nhiên (để tiếng súng đỡ nhàm)
        public void PlayRandomSFX(AudioClip[] clips, float volume = 1f)
        {
            if (clips != null && clips.Length > 0 && sfxSource != null)
            {
                int index = Random.Range(0, clips.Length);
                sfxSource.PlayOneShot(clips[index], volume);
            }
        }
    }
}
