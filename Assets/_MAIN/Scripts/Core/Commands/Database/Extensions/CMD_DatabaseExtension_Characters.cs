using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CHARACTERS;
using UnityEngine;

namespace COMMANDS
{
    /// <summary>
    /// 扩展命令数据库以支持角色相关的指令。
    /// 包括创建、移动、显示和隐藏角色等功能。
    /// </summary>
    public class CMD_DatabaseExtension_Characters : CMD_DatabaseExtension
    {
        // 参数定义：启用角色
        private static string[] PARAM_ENABLE => new string[] { "-e", "-enabled" };
        // 参数定义：立即执行（无动画）
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate" };
        // 参数定义：速度
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed" };
        // 参数定义：是否平滑移动
        private static string[] PARAM_SMOOTH => new string[] { "-s", "-smooth" };
        // 参数定义：X坐标位置
        private static string PARAM_XPOS => "-x";
        // 参数定义：Y坐标位置
        private static string PARAM_YPOS => "-y";
        
        /// <summary>
        /// 向指定的命令数据库中注册与角色操作相关的新命令。
        /// 注册的命令包括：createcharacter, movecharacter, show, hide。
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例。</param>
        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("createcharacter", new Action<string[]>(CreateCharacter));
            database.AddCommand("movecharacter", new Func<string[], IEnumerator>(MoveCharacter));
            database.AddCommand("show", new Func<string[], IEnumerator>(ShowAll));
            database.AddCommand("hide", new Func<string[], IEnumerator>(HideAll));
        }

        /// <summary>
        /// 创建一个新角色，并根据参数决定是否立即显示该角色。
        /// </summary>
        /// <param name="data">
        /// 命令数据数组。第一个元素是角色名称，
        /// 可选参数包括 -e/-enabled 表示启用角色，
        /// -i/-immediate 表示立即显示而不播放动画。
        /// </param>
        public static void CreateCharacter(string[] data)
        {
            string characterName = data[0];
            bool enable = false;
            bool immediate = false;
            
            var parameters = ConvertDataToParameters(data);
            
            parameters.TryGetValue(PARAM_ENABLE, out enable, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            Character character = CharacterManager.instance.CreateCharacter(characterName);

            if (!enable)
                return;
            
            if (immediate)
                character.isVisible = true;
            else
                character.Show();
        }

        /// <summary>
        /// 移动指定的角色到目标位置。
        /// 支持设置移动速度和平滑选项。
        /// </summary>
        /// <param name="data">
        /// 命令数据数组。第一个元素为目标角色名，
        /// 其他可选参数包括：
        /// -x 指定 X 轴坐标，
        /// -y 指定 Y 轴坐标，
        /// -spd/-speed 设置移动速度，默认为 1，
        /// -s/-smooth 是否使用平滑移动方式，默认为 false，
        /// -i/-immediate 是否立即移动而无需过渡动画，默认为 false。
        /// </param>
        /// <returns>协程迭代器对象，用于异步处理移动过程。</returns>
        private static IEnumerator MoveCharacter(string[] data)
        {
            // 获取目标角色名称并查找对应的角色对象
            string characterName = data[0];
            Character character = CharacterManager.instance.GetCharacter(characterName);
            
            // 如果未找到对应角色，则直接结束协程
            if (character == null)
                yield break;
            
            // 初始化移动参数默认值
            float x = 0, y = 0;
            float speed = 1;
            bool smooth = false;
            bool immediate = false;

            // 解析命令参数
            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_XPOS, out x);
            parameters.TryGetValue(PARAM_YPOS, out y);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1);
            parameters.TryGetValue(PARAM_SMOOTH, out smooth, defaultValue: false);
            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            
            Vector2 position = new Vector2(x, y);

            // 根据是否立即移动执行不同的移动逻辑
            if (immediate)
                character.SetPosition(position);
            else
            {
                // 添加终止动作以确保在命令被中断时仍能设置最终位置
                CommandManager.instance.AddTerminationActionToCurrentProcess(() => { character?.SetPosition(position); });
                // 执行带过渡效果的移动并等待完成
                yield return character.MoveToPosition(position, speed, smooth);
            }
        }
        
        /// <summary>
        /// 显示一组指定的角色。
        /// </summary>
        /// <param name="data">
        /// 命令数据数组。每个字符串代表一个角色名称。
        /// 可选参数 -i/-immediate 控制是否跳过淡入动画直接显示。
        /// </param>
        /// <returns>协程迭代器对象，等待所有角色完成显示动作后结束。</returns>
        public static IEnumerator ShowAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            float speed = 1f;
            
            // 获取所有有效的角色引用并加入列表
            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }
            
            if (characters.Count == 0)
                yield break;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // 根据参数选择立即或渐进式显示角色
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = true;
                else
                    character.Show(speed);
            }

            // 如果不是立即模式，则等待所有角色完成显示动画
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = true;
                });
                
                while (characters.Any(c => c.isRevealing))
                    yield return null;
            }
        }

        /// <summary>
        /// 隐藏一组指定的角色。
        /// </summary>
        /// <param name="data">
        /// 命令数据数组。每个字符串代表一个角色名称。
        /// 可选参数 -i/-immediate 控制是否跳过淡出动画直接隐藏。
        /// </param>
        /// <returns>协程迭代器对象，等待所有角色完成隐藏动作后结束。</returns>
        public static IEnumerator HideAll(string[] data)
        {
            List<Character> characters = new List<Character>();
            bool immediate = false;
            float speed = 1f;
            
            // 获取所有有效的角色引用并加入列表
            foreach (string s in data)
            {
                Character character = CharacterManager.instance.GetCharacter(s, createIfDoesNotExist: false);
                if (character != null)
                    characters.Add(character);
            }
            
            if (characters.Count == 0)
                yield break;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);
            parameters.TryGetValue(PARAM_SPEED, out speed, defaultValue: 1f);

            // 根据参数选择立即或渐进式隐藏角色
            foreach (Character character in characters)
            {
                if (immediate)
                    character.isVisible = false;
                else
                    character.Hide(speed);
            }

            // 如果不是立即模式，则等待所有角色完成隐藏动画
            if (!immediate)
            {
                CommandManager.instance.AddTerminationActionToCurrentProcess(() =>
                {
                    foreach (Character character in characters)
                        character.isVisible = false;
                });
                
                while (characters.Any(c => c.isHiding))
                    yield return null;
            }
        }
    }
}
