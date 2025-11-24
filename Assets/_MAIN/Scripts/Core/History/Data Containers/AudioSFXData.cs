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
                
                // 从游戏对象名称中提取资源路径（格式：SFX - [路径]）
                string resourcesPath = sound.gameObject.name.Split(AudioManager.SFX_NAME_FORMAT_CONTAINERS)[1];
                data.filePath = resourcesPath;
                
                //正确设置 fileName（用于后续检查音效是否在播放）
                data.fileName = sound.clip.name;
                
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
                // 构造完整的音效名称用于检查（格式："SFX - [filePath]"）
                string effectName = string.Format(AudioManager.SFX_NAME_FORMAT, sound.filePath);
                
                if (!AudioManager.instance.IsPlayingSoundEffect(sound.fileName))
                {
                    // sound.filePath 已经包含完整路径（例如 "Audio/SFX/ChurchBellsFar"） 直接使用 Resources.Load
                    AudioClip clip = Resources.Load<AudioClip>(sound.filePath);
                    
                    if (clip != null)
                    {
                        AudioManager.instance.PlaySoundEffect(clip, volume: sound.volume, pitch: sound.pitch,
                            loop: true, filePath: sound.filePath);
                    }
                    else
                    {
                        Debug.LogWarning($"[AudioSFXData] 无法加载音效资源：{sound.filePath}");
                    }
                }
                
                // 缓存的是完整的音效名称
                cache.Add(effectName);
            }

            // 停止不需要继续播放的音效
            foreach (var source in AudioManager.instance.allSFX)
            {
                if (!cache.Contains(source.gameObject.name))
                    AudioManager.instance.StopSoundEffect(source.clip);
            }
        }
    }
}