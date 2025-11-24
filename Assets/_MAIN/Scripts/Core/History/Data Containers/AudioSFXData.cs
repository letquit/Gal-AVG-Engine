using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 音频特效数据类，用于序列化和反序列化音频特效的相关信息
    /// </summary>
    [Serializable]
    public class AudioSFXData
    {
        public string filePath;
        public string fileName;
        public float volume;
        public float pitch;

        /// <summary>
        /// 捕获当前正在播放的循环音效数据
        /// </summary>
        /// <returns>包含所有循环音效数据的列表</returns>
        public static List<AudioSFXData> Capture()
        {
            List<AudioSFXData> audioList = new List<AudioSFXData>(); 
            AudioSource[] sfx = AudioManager.instance.allSFX;

            // 遍历所有音效源，收集循环播放的音效数据
            foreach (var sound in sfx)
            {
                // 只处理循环播放的音效
                if (!sound.loop)
                    continue;
                
                AudioSFXData data = new AudioSFXData();
                data.volume = sound.volume;
                data.pitch = sound.pitch;
                data.filePath = sound.clip.name;

                // 从游戏对象名称中提取资源路径
                string resourcesPath = sound.gameObject.name.Split(AudioManager.SFX_NAME_FORMAT_CONTAINERS)[1];
                
                data.filePath = resourcesPath;
                
                audioList.Add(data);
            }
            
            return audioList;
        }

        /// <summary>
        /// 应用音效数据列表，恢复对应的音效播放状态
        /// </summary>
        /// <param name="sfx">要应用的音效数据列表</param>
        public static void Apply(List<AudioSFXData> sfx)
        {
            List<string> cache = new List<string>();
            
            // 播放需要恢复的音效
            foreach (var sound in sfx)
            {
                if (!AudioManager.instance.IsPlayingSoundEffect(sound.fileName))
                    AudioManager.instance.PlaySoundEffect(sound.filePath, volume: sound.volume, pitch: sound.pitch,
                        loop: true);
                cache.Add(sound.fileName);
            }

            // 停止不需要继续播放的音效
            foreach (var source in AudioManager.instance.allSFX)
            {
                if (!cache.Contains(source.name))
                    AudioManager.instance.StopSoundEffect(source.clip);
            }
        }
    }
}