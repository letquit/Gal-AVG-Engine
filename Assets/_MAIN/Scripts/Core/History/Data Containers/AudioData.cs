using System;
using System.Collections.Generic;
using UnityEngine;

namespace History
{
    /// <summary>
    /// 音频数据类，用于序列化和存储音频通道的状态信息
    /// </summary>
    [Serializable]
    public class AudioData
    {
        public int channel = 0;
        public string trackName;
        public string trackPath;
        public float trackVolume;
        public float trackPitch;
        public bool loop;

        /// <summary>
        /// 构造函数，从AudioChannel对象初始化AudioData
        /// </summary>
        /// <param name="channel">音频通道对象，用于提取当前播放轨道的信息</param>
        public AudioData(AudioChannel channel)
        {
            this.channel = channel.channelIndex;
            
            // 如果通道中没有激活的轨道，则直接返回
            if(channel.activeTrack == null)
                return;
            
            var track = channel.activeTrack;
            trackName = track.name;
            trackPath = track.path;
            trackVolume = track.volumeCap;
            trackPitch = track.pitch;
            loop = track.loop;
        }

        /// <summary>
        /// 捕获当前所有音频通道的数据快照
        /// </summary>
        /// <returns>包含所有活动音频通道数据的列表</returns>
        public static List<AudioData> Capture()
        {
            List<AudioData> audioChannels = new List<AudioData>();

            // 遍历所有音频通道，收集有活动轨道的通道数据
            foreach (var channel in AudioManager.instance.channels)
            {
                if(channel.Value.activeTrack == null)
                    continue;
                
                AudioData data = new AudioData(channel.Value);
                audioChannels.Add(data);
            }
            
            return audioChannels;
        }
        
        /// <summary>
        /// 应用音频数据列表，更新音频通道的状态
        /// </summary>
        /// <param name="data">音频数据列表，包含各个通道的音频配置信息</param>
        public static void Apply(List<AudioData> data)
        {
            // 缓存已处理的通道索引
            List<int> cache = new List<int>();
            foreach (var channelData in data)
            {
                // 获取或创建指定索引的音频通道
                AudioChannel channel =
                    AudioManager.instance.TryGetChannel(channelData.channel, createIfDoesNotExist: true);
                
                // 检查当前活动轨道是否与目标轨道一致，不一致则加载并播放新轨道
                if (channel.activeTrack == null || channel.activeTrack.name != channelData.trackName)
                {
                    AudioClip clip = HistoryCache.LoadAudio(channelData.trackPath);
                    if (clip != null)
                    {
                        channel.StopTrack(immediate: true);
                        channel.PlayTrack(clip, channelData.loop, channelData.trackVolume, channelData.trackVolume,
                            channelData.trackPitch, channelData.trackPath);
                    }
                    else
                        Debug.LogWarning($"History State: Could not load audio track '{channelData.trackPath}'");
                }
                
                cache.Add(channelData.channel);
            }

            // 停止未在数据列表中指定的所有其他通道
            foreach (var channel in AudioManager.instance.channels)
            {
                if (!cache.Contains(channel.Value.channelIndex))
                    channel.Value.StopTrack(immediate: true);
            }
        }
    }
}