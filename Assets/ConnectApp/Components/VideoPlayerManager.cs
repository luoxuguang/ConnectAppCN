using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace ConnectApp.components {
    public class VideoPlayerManager {
        internal VideoPlayerManager() {
        }

        public static readonly VideoPlayerManager instance = new VideoPlayerManager();

        private VideoPlayer player { get; set; }
        
        private AudioSource audioSource { get; set; }


        private GameObject gameObject { get; set; }

        public VideoPlayer getPlayer()
        {
            if (gameObject.GetComponent<VideoPlayer>())
            {
                instance.player = gameObject.GetComponent<VideoPlayer>();
            }
            else
            {
                instance.player = gameObject.AddComponent<VideoPlayer>();
            }

            return instance.player;
        }
        
        public AudioSource getAudioSource()
        {
            if (gameObject.GetComponent<AudioSource>())
            {
                instance.audioSource = gameObject.GetComponent<AudioSource>();
            }
            else
            {
                instance.audioSource = gameObject.AddComponent<AudioSource>();
            }

            return instance.audioSource;
        }

        public void destroyPlayer()
        {
            VideoPlayer.Destroy(instance.player);
        }

        public void initPlayer(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }

    }
}