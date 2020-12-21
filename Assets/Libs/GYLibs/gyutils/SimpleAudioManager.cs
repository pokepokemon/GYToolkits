using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GYLib
{
    public class SimpleAudioManager : MonoSingleton<SimpleAudioManager>
    {
        private float _totalVolumn = 1;

        /// <summary>
        /// 最大叠加音效
        /// </summary>
        public int maxAdditionCount = 15;

        /// <summary>
        /// 衰减系数
        /// </summary>
        public float additionFactor = -0.15f;

        /// <summary>
        /// 最小音效
        /// </summary>
        public float minVolumn = 0.1f;

        private List<AudioItem> _audioSouces = new List<AudioItem>();
        private Dictionary<int, bool> _audioStatus = new Dictionary<int, bool>();

        /// <summary>
        /// 一次性自动播放音效
        /// </summary>
        /// <param name="audio"></param>
        public void PlayAudio(AudioClip audio)
        {
            AudioItem item = StopAndGetLastAudio();
            item.source.clip = audio;
            item.source.loop = false;

            ReCalcVolumn();
            item.source.Play(); 
        }

        public void Update()
        {
            bool isDirty = false;
            for (int i = 0; i < _audioSouces.Count; i++)
            {
                bool status = false;
                bool isPlaying = _audioSouces[i].source.isPlaying;
                if (!isPlaying && _audioStatus.TryGetValue(i, out status) && status != isPlaying)
                {
                    StopAudio(_audioSouces[i]);
                    isDirty = true;
                }
                _audioStatus[i] = isPlaying;
            }
            if (isDirty)
            {
                ReCalcVolumn();
            }
        }

        /// <summary>
        /// 获取空闲或者强制停止音效item
        /// </summary>
        /// <returns></returns>
        private AudioItem StopAndGetLastAudio()
        {
            if (_audioSouces.Count >= maxAdditionCount)
            {
                AudioItem item;
                for (int i = 0; i < _audioSouces.Count; i++)
                {
                    item = _audioSouces[i];
                    if (!item.source.isPlaying)
                    {
                        return item;
                    }
                }
                item = _audioSouces[_audioSouces.Count - 1];
                StopAudio(item);

                return item;
            }
            else
            {
                AudioItem item = new AudioItem();
                GameObject go = new GameObject("audio");
                AudioSource source = go.AddComponent<AudioSource>();
                item.hub = go;
                item.source = source;
                _audioSouces.Add(item);
                go.transform.SetParent(this.transform);

                return item;
            }
        }

        private void StopAudio(AudioItem item)
        {
            item.source.Stop();
            if (item.source.clip != null)
            {
                AudioClip clip = item.source.clip;
                item.source.clip = null;
                for (int i = 0; i < _audioSouces.Count; i++)
                {
                    if (_audioSouces[i] != item && _audioSouces[i].source.clip == item.source.clip)
                    {
                        return;
                    }
                }
                clip.UnloadAudioData();
            }
        }

        private void ReCalcVolumn()
        {
            float resultVolumn = GetResultVolumn(true);
            for (int i = 0; i < _audioSouces.Count; i++)
            {
                _audioSouces[i].source.volume = resultVolumn * _totalVolumn;
            }
        }

        /// <summary>
        /// 获取当前音量
        /// </summary>
        /// <returns></returns>
        public float GetResultVolumn(bool withCount)
        {
            int totalCount = 0;
            if (withCount)
            {
                for (int i = 0; i < _audioSouces.Count; i++)
                {
                    totalCount += _audioSouces[i].source.isPlaying ? 1 : 0;
                }
            }
            return Mathf.Max(1 + additionFactor * totalCount, minVolumn);
        }

        public void ClearAll()
        {
            for (int i = 0; i < _audioSouces.Count; i++)
            {
                Destroy(_audioSouces[i].hub);
            }
            _audioSouces = null;
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        public float totalVolumn
        {
            get
            {
                return _totalVolumn;
            }
            set
            {
                if (value != _totalVolumn)
                {
                    _totalVolumn = value;
                    ReCalcVolumn();
                }
            }
        }

        internal class AudioItem
        {
            public GameObject hub;
            public AudioSource source;
            public float startTime;
            public float endTime;
        }
    }

}