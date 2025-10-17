using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频轨道类，用于管理单个音频剪辑的播放
/// </summary>
public class AudioTrack
{
    private const string TRACK_NAME_FORMAT = "Track - [{0}]";
    public string name { get; private set; }

    private AudioChannel channel;
    private AudioSource source;
    public bool loop => source.loop;
    public float volumeCap { get; private set; }
    
    public bool isPlaying => source.isPlaying;

    /// <summary>
    /// 创建一个新的音频轨道实例
    /// </summary>
    /// <param name="clip">要播放的音频剪辑</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="startingVolume">初始音量</param>
    /// <param name="volumeCap">音量上限</param>
    /// <param name="channel">音频通道</param>
    /// <param name="mixer">音频混音器组</param>
    public AudioTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, AudioChannel channel, AudioMixerGroup mixer)
    {
        name = clip.name;
        this.channel = channel;
        this.volumeCap = volumeCap;
        
        source = CreateSource();
        source.clip = clip;
        source.loop = loop;
        source.volume = startingVolume;
        
        source.outputAudioMixerGroup = mixer;
    }

    /// <summary>
    /// 创建音频源组件并将其添加到游戏对象中
    /// </summary>
    /// <returns>新创建的音频源组件</returns>
    private AudioSource CreateSource()
    {
        // 创建游戏对象并设置名称和父级容器
        GameObject go = new GameObject(string.Format(TRACK_NAME_FORMAT, name));
        go.transform.SetParent(channel.trackContainer);
        AudioSource source = go.AddComponent<AudioSource>();
        
        return source;
    }
    
    /// <summary>
    /// 开始播放音频
    /// </summary>
    public void Play()
    {
        source.Play();
    }
    
    /// <summary>
    /// 停止播放音频
    /// </summary>
    public void Stop()
    {
        source.Stop();
    }
}