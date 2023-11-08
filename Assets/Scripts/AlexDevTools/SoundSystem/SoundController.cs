using System.Collections;
using System.Collections.Generic;
using AlexDevTools.Models;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AlexDevTools.SoundSystem
{
    public class SoundController : MonoBehaviour
    {
        [SerializeField] private KeyValue<SoundType, AudioClip[]>[] sounds;
        [SerializeField] private AudioSource audioSource;
        
        public static SoundController Instance { get; private set; }

        private Queue<AudioClip> _audioQueue;

        private void Awake()
        {
            Instance = this;
            _audioQueue = new Queue<AudioClip>();
            StartCoroutine(TryPlay());
        }

        public void Play(SoundType type)
        {
            for (int i = 0; i < sounds.Length; i++)
            {
                if (sounds[i].key != type) continue;
                
                var soundValue = sounds[i].value;
                var clip = soundValue.Length > 1 ? soundValue[Random.Range(0, soundValue.Length)] : soundValue[0];
                    
                _audioQueue.Enqueue(clip);
            }
        }

        public void Play(AudioClip clip)
        {
            _audioQueue.Enqueue(clip);
        }

        private IEnumerator TryPlay()
        {
            var wait = new WaitWhile(() => audioSource.isPlaying);
            while (true)
            {
                yield return wait;
                if (_audioQueue.Count > 0)
                {
                    audioSource.clip = _audioQueue.Dequeue();
                    audioSource.Play();
                }

                yield return null;
            }
        }
    }
}