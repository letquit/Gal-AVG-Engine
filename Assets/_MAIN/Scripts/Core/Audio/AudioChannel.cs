using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音频通道类，用于管理一组相关的音频轨道
/// </summary>
public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";
    public int channelIndex { get; private set; }

    public Transform trackContainer { get; private set; } = null;
    
    private List<AudioTrack> tracks = new List<AudioTrack>();

    /// <summary>
    /// 创建一个新的音频通道实例
    /// </summary>
    /// <param name="channel">通道索引号</param>
    public AudioChannel(int channel)
    {
        channelIndex = channel;

        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channel)).transform;
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    /// <summary>
    /// 播放指定的音频轨道，如果轨道已存在则直接播放，否则创建新轨道
    /// </summary>
    /// <param name="clip">要播放的音频剪辑</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="startingVolume">起始音量</param>
    /// <param name="volumeCap">音量上限</param>
    /// <param name="filePath">音频文件路径</param>
    /// <returns>播放的音频轨道实例</returns>
    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, string filePath)
    {
        // 检查是否已存在同名轨道
        if (TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            // 如果轨道存在但未在播放，则开始播放
            if (!existingTrack.isPlaying)
                existingTrack.Play();
            
            return existingTrack;
        }

        // 创建新的音频轨道并播放
        AudioTrack track = new AudioTrack(clip, loop, startingVolume, volumeCap, this, AudioManager.instance.musicMixer);
        track.Play();
        return track;
    }

    /// <summary>
    /// 尝试获取指定名称的音频轨道
    /// </summary>
    /// <param name="trackName">要查找的轨道名称</param>
    /// <param name="value">找到的音频轨道实例（如果找到）</param>
    /// <returns>如果找到轨道返回true，否则返回false</returns>
    public bool TryGetTrack(string trackName, out AudioTrack value)
    {
        trackName = trackName.ToLower();
        foreach (var track in tracks)
        {
            if (track.name.ToLower() == trackName)
            {
                value = track;
                return true;
            }
        }
        
        value = null;
        return false;
    }
}