using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频轨道类，用于管理单个音频源的播放控制
/// </summary>
public class AudioTrack
{
    private const string TRACK_NAME_FORMAT = "Track - [{0}]";
    /// <summary>
    /// 获取音频轨道的名称
    /// </summary>
    public string name { get; private set; }

    /// <summary>
    /// 获取音频轨道根游戏对象的引用
    /// </summary>
    public GameObject root => source.gameObject;
    
    private AudioChannel channel;
    private AudioSource source;
    /// <summary>
    /// 获取音频是否循环播放
    /// </summary>
    public bool loop => source.loop;
    /// <summary>
    /// 获取音量上限值
    /// </summary>
    public float volumeCap { get; private set; }
    
    /// <summary>
    /// 获取音频是否正在播放
    /// </summary>
    public bool isPlaying => source.isPlaying;
    
    /// <summary>
    /// 获取或设置音频音量
    /// </summary>
    public float volume { get { return source.volume; } set { source.volume = value; }}

    /// <summary>
    /// 创建一个新的音频轨道实例
    /// </summary>
    /// <param name="clip">要播放的音频剪辑</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="startingVolume">初始音量</param>
    /// <param name="volumeCap">音量上限</param>
    /// <param name="pitch">音频播放速率</param>
    /// <param name="channel">所属音频通道</param>
    /// <param name="mixer">音频混音器组</param>
    public AudioTrack(AudioClip clip, bool loop, float startingVolume, float volumeCap, float pitch, AudioChannel channel, AudioMixerGroup mixer)
    {
        name = clip.name;
        this.channel = channel;
        this.volumeCap = volumeCap;
        
        source = CreateSource();
        source.clip = clip;
        source.loop = loop;
        source.volume = startingVolume;
        source.pitch = pitch;
        
        source.outputAudioMixerGroup = mixer;
    }

    /// <summary>
    /// 创建音频源组件并初始化游戏对象
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