using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示一个音频通道，用于管理多个音频轨道（AudioTrack）。
/// 每个通道可以播放一个音频剪辑，并支持淡入淡出过渡效果。
/// </summary>
public class AudioChannel
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";
    
    /// <summary>
    /// 获取当前音频通道的索引编号。
    /// </summary>
    public int channelIndex { get; private set; }

    /// <summary>
    /// 获取当前通道下所有音频轨道的容器 Transform。
    /// </summary>
    public Transform trackContainer { get; private set; } = null;
    
    /// <summary>
    /// 获取当前激活的音频轨道。
    /// </summary>
    public AudioTrack activeTrack { get; private set; } = null;
    
    private List<AudioTrack> tracks = new List<AudioTrack>();
    
    private bool isLevelingVolume => co_volumeLeveling != null;
    private Coroutine co_volumeLeveling = null;

    /// <summary>
    /// 初始化一个新的音频通道实例。
    /// </summary>
    /// <param name="channel">通道的索引编号。</param>
    public AudioChannel(int channel)
    {
        channelIndex = channel;

        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channel)).transform;
        trackContainer.SetParent(AudioManager.instance.transform);
    }

    /// <summary>
    /// 在当前通道中播放指定的音频剪辑。
    /// 如果该音频剪辑已经存在对应的轨道且未在播放，则恢复播放；
    /// 否则创建一个新的音频轨道并播放。
    /// </summary>
    /// <param name="clip">要播放的音频剪辑。</param>
    /// <param name="loop">是否循环播放。</param>
    /// <param name="startingVolume">起始音量。</param>
    /// <param name="volumeCap">最大音量限制。</param>
    /// <param name="pitch">音频播放速率（音调）。</param>
    /// <param name="filePath">音频文件路径（用于标识）。</param>
    /// <returns>正在播放的音频轨道对象。</returns>
    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, string filePath)
    {
        // 检查是否已存在同名轨道
        if (TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            // 如果轨道存在但未在播放，则开始播放
            if (!existingTrack.isPlaying)
                existingTrack.Play();
            
            SetAsActiveTrack(existingTrack);
            
            return existingTrack;
        }

        // 创建新的音频轨道并播放
        AudioTrack track = new AudioTrack(clip, loop, startingVolume, volumeCap, pitch, this, AudioManager.instance.musicMixer);
        track.Play();

        SetAsActiveTrack(track);
        
        return track;
    }
    
    /// <summary>
    /// 尝试根据名称获取当前通道中的音频轨道。
    /// </summary>
    /// <param name="trackName">音频轨道名称（不区分大小写）。</param>
    /// <param name="value">输出参数，返回找到的音频轨道对象。</param>
    /// <returns>如果找到匹配的音频轨道则返回 true，否则返回 false。</returns>
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

    /// <summary>
    /// 设置指定音频轨道为当前激活轨道，并将其添加到轨道列表中（如果尚未包含）。
    /// </summary>
    /// <param name="track">要设为激活状态的音频轨道。</param>
    private void SetAsActiveTrack(AudioTrack track)
    {
        if (!tracks.Contains(track))
            tracks.Add(track);
        
        activeTrack = track;
        
        TryStartVolumeLeveling();
    }
    
    /// <summary>
    /// 尝试启动音量调节协程（VolumeLeveling），如果尚未运行的话。
    /// </summary>
    private void TryStartVolumeLeveling()
    {
        if (!isLevelingVolume)
            co_volumeLeveling = AudioManager.instance.StartCoroutine(VolumeLeveling());
    }

    /// <summary>
    /// 判断是否应该继续执行音量调节逻辑。
    /// 条件包括：有激活轨道且轨道数量大于1或音量未达到上限；或者没有激活轨道但仍有其他轨道。
    /// </summary>
    /// <returns>若需要继续调节音量则返回 true，否则返回 false。</returns>
    private bool ShouldContinueVolumeLeveling()
    {
        bool hasActiveTrackCondition = activeTrack != null && (tracks.Count > 1 || activeTrack.volume != activeTrack.volumeCap);
        bool noActiveTrackCondition = activeTrack == null && tracks.Count > 0;
    
        return hasActiveTrackCondition || noActiveTrackCondition;
    }
    
    /// <summary>
    /// 音量调节协程，负责处理多个音频轨道之间的音量过渡。
    /// 激活轨道逐渐提升至最大音量，非激活轨道逐渐降低至静音并销毁。
    /// </summary>
    /// <returns>IEnumerator 接口，用于协程控制。</returns>
    private IEnumerator VolumeLeveling()
    {
        while (ShouldContinueVolumeLeveling())
        {
            for (int i = tracks.Count - 1; i >= 0; i--)
            {
                AudioTrack track = tracks[i];

                float targetVol = activeTrack == track ? track.volumeCap : 0;

                if (track == activeTrack && track.volume == targetVol)
                    continue;
                
                track.volume = Mathf.MoveTowards(track.volume, targetVol, AudioManager.TRACK_TRANSITION_SPEED * Time.deltaTime);

                if (track != activeTrack && track.volume == 0)
                {
                    DestroyTrack(track);
                }
            }
            yield return null;
        }
        
        co_volumeLeveling = null;
    }
    
    /// <summary>
    /// 销毁指定的音频轨道，从轨道列表中移除并销毁其 GameObject。
    /// </summary>
    /// <param name="track">要销毁的音频轨道。</param>
    private void DestroyTrack(AudioTrack track)
    {
        if (tracks.Contains(track))
            tracks.Remove(track);
        
        Object.Destroy(track.root);
    }

    /// <summary>
    /// 停止当前激活的音频轨道，并尝试重新启动音量调节逻辑。
    /// </summary>
    public void StopTrack()
    {
        if (activeTrack == null)
            return;

        activeTrack = null;
        TryStartVolumeLeveling();
    }
}