using System;
using COMMANDS;
using UnityEngine;

namespace COMMANDS
{
    /// <summary>
    /// 扩展命令数据库以支持音频播放相关指令。
    /// 包括音效、语音、背景音乐和环境音的播放与停止功能。
    /// </summary>
    public class CMD_DatabaseExtension_Audio : CMD_DatabaseExtension
    {
        // 定义各种参数标识符数组，用于解析命令中的选项
        
        private static string[] PARAM_SFX = new string[] { "-s", "-sfx" };           // 音效文件路径参数
        private static string[] PARAM_VOLUME = new string[] { "-v", "-vol", "-volume" }; // 音量参数
        private static string[] PARAM_PITCH = new string[] { "-p", "-pitch" };       // 音调参数
        private static string[] PARAM_LOOP = new string[] { "-l", "-loop" };         // 循环播放参数

        private static string[] PARAM_CHANNEL = new string[] { "-c", "-channel" };   // 播放通道参数
        private static string[] PARAM_IMMEDIATE = new string[] { "-i", "-immediate" };// 立即播放参数（未使用）
        private static string[] PARAM_START_VOLUME = new string[] { "-sv", "-startvolume" };// 起始音量参数
        private static string[] PARAM_SONG = new string[] { "-s", "-song" };         // 歌曲文件路径参数
        private static string[] PARAM_AMBIENCE = new string[] { "-a", "-ambience" }; // 环境音文件路径参数

        /// <summary>
        /// 向指定的命令数据库中注册所有音频相关的命令处理函数。
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例</param>
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("playsfx", new Action<string[]>(PlaySFX));     // 注册播放音效命令
            database.AddCommand("stopsfx", new Action<string>(StopSFX));       // 注册停止音效命令
            
            database.AddCommand("playvoice", new Action<string[]>(PlayVoice)); // 注册播放语音命令
            database.AddCommand("stopvoice", new Action<string>(StopSFX));     // 注册停止语音命令（复用音效停止方法）

            database.AddCommand("playsong", new Action<string[]>(PlaySong));   // 注册播放歌曲命令
            database.AddCommand("playambience", new Action<string[]>(PlayAmbience)); // 注册播放环境音命令
            
            database.AddCommand("stopsong", new Action<string>(StopSong));     // 注册停止歌曲命令
            database.AddCommand("stopambience", new Action<string>(StopAmbience)); // 注册停止环境音命令
        }

        /// <summary>
        /// 根据传入的数据播放一个音效。
        /// 支持设置音量、音调、是否循环等参数。
        /// </summary>
        /// <param name="data">包含音效信息及参数的字符串数组</param>
        private static void PlaySFX(string[] data)
        {
            string filepath;      // 音效资源路径
            float volume, pitch;  // 音量和音调
            bool loop;            // 是否循环播放

            var parameters = ConvertDataToParameters(data); // 将输入数据转换为参数字典

            parameters.TryGetValue(PARAM_SFX, out filepath);                    // 获取音效文件名
            parameters.TryGetValue(PARAM_VOLUME, out volume, defaultValue: 1f); // 获取音量，默认为1.0
            parameters.TryGetValue(PARAM_PITCH, out pitch, defaultValue: 1f);   // 获取音调，默认为1.0
            parameters.TryGetValue(PARAM_LOOP, out loop, defaultValue: false);  // 获取循环标志，默认不循环

            // 加载对应的AudioClip资源
            AudioClip sound = Resources.Load<AudioClip>(FilePaths.GetPathToResource(FilePaths.resources_sfx, filepath));

            if (sound == null) // 若加载失败则直接返回
                return;

            // 使用音频管理器播放该音效
            AudioManager.instance.PlaySoundEffect(sound, volume: volume, pitch: pitch, loop: loop);
        }

        /// <summary>
        /// 停止指定名称的音效播放。
        /// </summary>
        /// <param name="data">要停止的音效名称</param>
        private static void StopSFX(string data)
        {
            AudioManager.instance.StopSoundEffect(data); // 调用音频管理器停止对应音效
        }

        /// <summary>
        /// 根据传入的数据播放一段语音。
        /// 支持设置音量、音调、是否循环等参数。
        /// </summary>
        /// <param name="data">包含语音信息及参数的字符串数组</param>
        private static void PlayVoice(string[] data)
        {
            string filepath;      // 语音资源路径
            float volume, pitch;  // 音量和音调
            bool loop;            // 是否循环播放

            var parameters = ConvertDataToParameters(data); // 解析参数

            parameters.TryGetValue(PARAM_SFX, out filepath);                    // 获取语音文件名
            parameters.TryGetValue(PARAM_VOLUME, out volume, defaultValue: 1f); // 获取音量，默认为1.0
            parameters.TryGetValue(PARAM_PITCH, out pitch, defaultValue: 1f);   // 获取音调，默认为1.0
            parameters.TryGetValue(PARAM_LOOP, out loop, defaultValue: false);  // 获取循环标志，默认不循环

            // 加载语音资源
            AudioClip sound = Resources.Load<AudioClip>(FilePaths.GetPathToResource(FilePaths.resources_voices, filepath));

            if (sound == null) // 加载失败时输出日志并退出
            {
                Debug.Log($"Was not able to load voice '{filepath}'");
                return;
            }

            // 使用音频管理器播放语音
            AudioManager.instance.PlayVoice(sound, volume: volume, pitch: pitch, loop: loop);
        }

        /// <summary>
        /// 根据传入的数据播放一首歌曲。
        /// 可通过参数控制播放通道、起始音量、最大音量等属性。
        /// </summary>
        /// <param name="data">包含歌曲信息及参数的字符串数组</param>
        private static void PlaySong(string[] data)
        {
            string filepath; // 歌曲资源路径
            int channel;     // 播放通道编号

            var parameters = ConvertDataToParameters(data); // 解析参数

            parameters.TryGetValue(PARAM_SONG, out filepath);                          // 获取歌曲文件名
            filepath = FilePaths.GetPathToResource(FilePaths.resources_music, filepath); // 构造完整路径

            parameters.TryGetValue(PARAM_CHANNEL, out channel, defaultValue: 1);       // 获取播放通道，默认为1号通道

            PlayTrack(filepath, channel, parameters); // 调用通用轨道播放逻辑
        }

        /// <summary>
        /// 根据传入的数据播放一段环境音。
        /// 可通过参数控制播放通道、起始音量、最大音量等属性。
        /// </summary>
        /// <param name="data">包含环境音信息及参数的字符串数组</param>
        private static void PlayAmbience(string[] data)
        {
            string filepath; // 环境音资源路径
            int channel;     // 播放通道编号

            var parameters = ConvertDataToParameters(data); // 解析参数

            parameters.TryGetValue(PARAM_AMBIENCE, out filepath);                         // 获取环境音文件名
            filepath = FilePaths.GetPathToResource(FilePaths.resources_ambience, filepath); // 构造完整路径

            parameters.TryGetValue(PARAM_CHANNEL, out channel, defaultValue: 0);          // 获取播放通道，默认为0号通道

            PlayTrack(filepath, channel, parameters); // 调用通用轨道播放逻辑
        }

        /// <summary>
        /// 在指定通道上播放一段音频轨道（如歌曲或环境音）。
        /// 支持多种播放参数配置。
        /// </summary>
        /// <param name="filepath">音频资源的完整路径</param>
        /// <param name="channel">播放通道编号</param>
        /// <param name="parameters">其他播放参数集合</param>
        private static void PlayTrack(string filepath, int channel, CommandParameters parameters)
        {
            bool loop;              // 是否循环播放
            float volumeCap;        // 最大音量限制
            float startVolume;      // 初始音量
            float pitch;            // 音调调整
            bool immediate;         // 是否立即播放

            // 提取各项播放参数
            parameters.TryGetValue(PARAM_VOLUME, out volumeCap, defaultValue: 1f);       // 默认最大音量为1.0
            parameters.TryGetValue(PARAM_START_VOLUME, out startVolume, defaultValue: 0f); // 默认初始音量为0
            parameters.TryGetValue(PARAM_PITCH, out pitch, defaultValue: 1f);            // 默认音调为1.0
            parameters.TryGetValue(PARAM_LOOP, out loop, defaultValue: true);            // 默认开启循环播放
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false); // 默认不立即播放

            // 如果是立即播放，起始音量应等于目标音量
            if (immediate)
                startVolume = volumeCap;
            
            // 加载音频资源
            AudioClip sound = Resources.Load<AudioClip>(filepath);

            if (sound == null) // 加载失败时记录日志并返回
            {
                Debug.Log($"Was not able to load voice '{filepath}'");
                return;
            }

            // 调用音频管理器进行实际播放操作
            AudioManager.instance.PlayTrack(sound, channel, loop, startVolume, volumeCap, pitch, filepath);
        }

        /// <summary>
        /// 停止当前正在播放的歌曲。
        /// 如果没有提供具体通道，则默认停止1号通道上的播放。
        /// </summary>
        /// <param name="data">可选的通道编号字符串</param>
        private static void StopSong(string data)
        {
            if (data == string.Empty)
                StopTrack("1"); // 默认停止1号通道
            else
                StopTrack(data); // 停止指定通道
        }

        /// <summary>
        /// 停止当前正在播放的环境音。
        /// 如果没有提供具体通道，则默认停止0号通道上的播放。
        /// </summary>
        /// <param name="data">可选的通道编号字符串</param>
        private static void StopAmbience(string data)
        {
            if (data == string.Empty)
                StopTrack("0"); // 默认停止0号通道
            else
                StopTrack(data); // 停止指定通道
        }

        /// <summary>
        /// 停止指定通道或根据文件路径标识符停止某个音频轨道的播放。
        /// </summary>
        /// <param name="data">可以是数字形式的通道编号，也可以是文件路径标识符</param>
        private static void StopTrack(string data)
        {
            if (int.TryParse(data, out int channel)) // 如果是有效整数，则按通道号停止
                AudioManager.instance.StopTrack(channel);
            else                                     // 否则按照文件路径标识符停止
                AudioManager.instance.StopTrack(data);
        }
    }
}
