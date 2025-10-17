using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 音频管理器，用于控制打字机音效的播放和音频集的切换。
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const string SFX_PARENT_NAME = "SFX";
    private const string SFX_NAME_FORMAT = "SFX - [{0}]";
    public static AudioManager instance { get; private set; }
    
    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup voicesMixer;

    private Transform sfxRoot;
    
    [Header("打字机音效设置")]
    [SerializeField] private List<AudioSet> audioSets = new List<AudioSet>();
    [SerializeField] private AudioSource typingSource;
    [SerializeField] [Range(0.1f, 1.0f)] private float volumeMin = 0.5f;
    [SerializeField] [Range(0.1f, 1.0f)] private float volumeMax = 0.9f;
    [SerializeField] [Range(0.5f, 1.5f)] private float pitchMin = 0.9f;
    [SerializeField] [Range(0.5f, 1.5f)] private float pitchMax = 1.1f;

    /// <summary>
    /// 表示一个音频集，包含名称和对应的音频剪辑数组。
    /// </summary>
    [System.Serializable]
    public class AudioSet
    {
        public string setName;          // 音频集名称
        public AudioClip[] typingSounds; // 打字音效数组
    }

    private int currentAudioSetIndex = 0;

    /// <summary>
    /// 字符与音效的映射关系。
    /// </summary>
    [System.Serializable]
    public class CharacterSoundMapping
    {
        public string characters;  // 匹配的字符
        public AudioClip sound;    // 对应的音效
    }
    [SerializeField] private CharacterSoundMapping[] characterSoundMappings;

    private Dictionary<char, AudioClip> soundMappingDict = new Dictionary<char, AudioClip>();

    /// <summary>
    /// Unity生命周期函数，在对象启用时执行初始化操作
    /// 主要用于确保当前对象作为单例存在，并初始化声音映射配置
    /// </summary>
    private void Awake()
    {
        // 检查是否已存在实例，确保单例模式
        if (instance == null)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            DontDestroyOnLoad(gameObject);
            return;
        }
        
        // 创建音效根节点对象
        sfxRoot = new GameObject(SFX_PARENT_NAME).transform;
        sfxRoot.SetParent(transform);

        // 初始化字符与声音的映射关系
        InitializeSoundMappings();
    }
    
    /// <summary>
    /// 初始化字符与声音的映射字典
    /// 将字符声音映射配置转换为字典形式，便于快速查找
    /// </summary>
    private void InitializeSoundMappings()
    {
        // 构建字符到声音的映射字典
        if (characterSoundMappings != null)
        {
            foreach (var mapping in characterSoundMappings)
            {
                foreach (char c in mapping.characters)
                {
                    if (!soundMappingDict.ContainsKey(c))
                        soundMappingDict.Add(c, mapping.sound);
                }
            }
        }
    }

    /// <summary>
    /// 播放指定路径的音效文件
    /// </summary>
    /// <param name="filePath">音效文件在Resources文件夹中的路径</param>
    /// <param name="mixer">音频混音器组，默认为null时使用默认SFX混音器</param>
    /// <param name="volume">音量大小，范围0-1，默认为1</param>
    /// <param name="pitch">音调，正常为1，小于1降低音调，大于1提高音调，默认为1</param>
    /// <param name="loop">是否循环播放，默认为false</param>
    /// <returns>创建的AudioSource组件，如果加载失败则返回null</returns>
    public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
    {
        AudioClip clip = Resources.Load<AudioClip>(filePath);
        
        // 检查音频剪辑是否成功加载
        if (clip == null)
        {
            Debug.LogError(
                $"Could not load audio file '{filePath}'. Please make sure this exists in a 'Resources' folder.");
            return null;
        }

        return PlaySoundEffect(clip, mixer, volume, pitch, loop);
    }

    /// <summary>
    /// 播放指定的音频剪辑
    /// </summary>
    /// <param name="clip">要播放的AudioClip对象</param>
    /// <param name="mixer">音频混音器组，默认为null时使用默认SFX混音器</param>
    /// <param name="volume">音量大小，范围0-1，默认为1</param>
    /// <param name="pitch">音调，正常为1，小于1降低音调，大于1提高音调，默认为1</param>
    /// <param name="loop">是否循环播放，默认为false</param>
    /// <returns>创建的AudioSource组件</returns>
    public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
    {
        // 创建新的游戏对象并添加AudioSource组件用于播放音效
        AudioSource effectSource = new GameObject(string.Format(SFX_NAME_FORMAT, clip.name)).AddComponent<AudioSource>();
        effectSource.transform.SetParent(sfxRoot);
        effectSource.transform.position = sfxRoot.position;
        
        effectSource.clip = clip;
        
        // 如果未指定混音器，则使用默认的SFX混音器
        if (mixer == null)
            mixer = sfxMixer;
        
        effectSource.outputAudioMixerGroup = mixer;
        effectSource.volume = volume;
        effectSource.spatialBlend = 0;
        effectSource.pitch = pitch;
        effectSource.loop = loop;
        
        effectSource.Play();
        
        // 对于非循环音效，播放完成后自动销毁游戏对象
        if (!loop)
            Destroy(effectSource.gameObject, (clip.length / pitch) + 1);
        
        return effectSource;
    }

    /// <summary>
    /// 停止播放指定名称的音效
    /// </summary>
    /// <param name="clip">要停止播放的AudioClip对象</param>
    public void StopSoundEffect(AudioClip clip) => StopSoundEffect(clip.name);
    
    /// <summary>
    /// 停止播放指定名称的音效
    /// </summary>
    /// <param name="soundName">要停止播放的音效名称</param>
    public void StopSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();
        // 获取所有子级的AudioSource组件并查找匹配的音效
        AudioSource[] sources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach (var source in sources)
        {
            if (source.clip.name.ToLower() == soundName)
            {
                Destroy(source.gameObject);
                return;
            }
        }
    }

    /// <summary>
    /// 注册文本架构师事件监听器。
    /// </summary>
    /// <param name="architect">要注册的文本架构师实例。</param>
    public void RegisterTextArchitect(TextArchitect architect)
    {
        architect.OnCharacterTyped += PlayTypingSound;
        architect.OnMultipleCharactersTyped += PlayMultipleTypingSounds;
        architect.OnDialogueStart += PrepareDialogueAudio;
    }

    /// <summary>
    /// 取消注册文本架构师事件监听器。
    /// </summary>
    /// <param name="architect">要取消注册的文本架构师实例。</param>
    public void UnregisterTextArchitect(TextArchitect architect)
    {
        architect.OnCharacterTyped -= PlayTypingSound;
        architect.OnMultipleCharactersTyped -= PlayMultipleTypingSounds;
        architect.OnDialogueStart -= PrepareDialogueAudio;
    }

    private List<float> audioPlayTimes = new List<float>();
    private float dialogueDuration = 0;
    private int currentAudioIndex = 0;
    private Coroutine audioPlaybackRoutine = null;

    /// <summary>
    /// 准备对话音频播放计划。
    /// </summary>
    /// <param name="text">对话文本内容。</param>
    /// <param name="displayDuration">文本显示持续时间。</param>
    public void PrepareDialogueAudio(string text, float displayDuration)
    {
        if (audioPlaybackRoutine != null)
        {
            StopCoroutine(audioPlaybackRoutine);
            audioPlaybackRoutine = null;
        }

        int effectiveCharCount = CountEffectiveCharacters(text);
        
        int soundsToPlay = Mathf.Min(effectiveCharCount, 5);
        
        if (soundsToPlay <= 0) return;
        
        dialogueDuration = displayDuration;
        audioPlayTimes.Clear();
        
        if (soundsToPlay == 1)
        {
            audioPlayTimes.Add(0);
        }
        else
        {
            float interval = displayDuration / soundsToPlay;
            for (int i = 0; i < soundsToPlay; i++)
            {
                audioPlayTimes.Add(i * interval);
            }
        }
        
        currentAudioIndex = 0;
        audioPlaybackRoutine = StartCoroutine(PlayPlannedAudio());
    }

    /// <summary>
    /// 按计划播放音频的协程。
    /// </summary>
    private IEnumerator PlayPlannedAudio()
    {
        float startTime = Time.time;
        float elapsedTime = 0;
        
        while (currentAudioIndex < audioPlayTimes.Count)
        {
            elapsedTime = Time.time - startTime;
            
            if (elapsedTime >= audioPlayTimes[currentAudioIndex])
            {
                PlayRandomTypingSound();
                currentAudioIndex++;
            }
            
            yield return null;
        }
        
        audioPlaybackRoutine = null;
    }

    /// <summary>
    /// 计算文本中有效字符的数量（排除空白字符、标点符号和特殊命令）。
    /// </summary>
    /// <param name="text">要计算的文本。</param>
    /// <returns>有效字符数量。</returns>
    private int CountEffectiveCharacters(string text)
    {
        int count = 0;
        bool inSpecialCommand = false;
        
        foreach (char c in text)
        {
            if (c == '{') 
            {
                inSpecialCommand = true;
                continue;
            }
            
            if (inSpecialCommand)
            {
                if (c == '}') 
                    inSpecialCommand = false;
                continue;
            }
            
            if (char.IsWhiteSpace(c) || char.IsPunctuation(c))
                continue;
            
            count++;
        }
        
        return count;
    }

    /// <summary>
    /// 根据名称切换音频集。
    /// </summary>
    /// <param name="setName">要切换到的音频集名称。</param>
    /// <returns>切换成功返回true，否则返回false。</returns>
    public bool SwitchAudioSet(string setName)
    {
        for (int i = 0; i < audioSets.Count; i++)
        {
            if (audioSets[i].setName == setName)
            {
                currentAudioSetIndex = i;
                Debug.Log($"已切换到音频集: {setName}");
                return true;
            }
        }
        
        Debug.LogWarning($"未找到名为 {setName} 的音频集");
        return false;
    }

    /// <summary>
    /// 根据索引切换音频集。
    /// </summary>
    /// <param name="index">要切换到的音频集索引。</param>
    /// <returns>切换成功返回true，否则返回false。</returns>
    public bool SwitchAudioSet(int index)
    {
        if (index >= 0 && index < audioSets.Count)
        {
            currentAudioSetIndex = index;
            Debug.Log($"已切换到音频集: {audioSets[index].setName}");
            return true;
        }
        
        Debug.LogWarning($"音频集索引 {index} 超出范围");
        return false;
    }
    
    /// <summary>
    /// 获取当前音频集的打字音效数组。
    /// </summary>
    /// <returns>当前音频集的音频剪辑数组，如果不存在则返回null。</returns>
    public AudioClip[] GetCurrentTypingSounds()
    {
        if (audioSets.Count > 0 && currentAudioSetIndex < audioSets.Count)
            return audioSets[currentAudioSetIndex].typingSounds;
        return null;
    }

    /// <summary>
    /// 播放指定字符的打字音效。
    /// </summary>
    /// <param name="character">要播放音效的字符。</param>
    public void PlayTypingSound(char character)
    {
        if (audioPlaybackRoutine != null)
            return;
            
        if (typingSource == null) return;

        AudioClip clip = null;
        if (soundMappingDict.TryGetValue(character, out clip))
        {
            
        }
        else
        {
            AudioClip[] availableSounds = GetCurrentTypingSounds();
            if (availableSounds != null && availableSounds.Length > 0)
            {
                clip = availableSounds[Random.Range(0, availableSounds.Length)];
            }
        }

        if (clip != null)
        {
            typingSource.pitch = Random.Range(pitchMin, pitchMax);
            typingSource.volume = Random.Range(volumeMin, volumeMax);
            typingSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 播放随机的打字音效。
    /// </summary>
    private void PlayRandomTypingSound()
    {
        if (typingSource == null) return;

        AudioClip[] availableSounds = GetCurrentTypingSounds();
        if (availableSounds != null && availableSounds.Length > 0)
        {
            AudioClip clip = availableSounds[Random.Range(0, availableSounds.Length)];
            typingSource.pitch = Random.Range(pitchMin, pitchMax);
            typingSource.volume = Random.Range(volumeMin, volumeMax);
            typingSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 播放多个字符的打字音效。
    /// </summary>
    /// <param name="charCount">字符数量。</param>
    public void PlayMultipleTypingSounds(int charCount)
    {
        if (audioPlaybackRoutine != null)
            return;
            
        PlayRandomTypingSound();
    }
}
