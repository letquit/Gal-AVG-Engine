using UnityEngine;

namespace CHARACTERS
{
    /// <summary>
    /// 角色配置ScriptableObject资产类，用于存储和管理对话系统中所有角色的配置数据
    /// </summary>
    [CreateAssetMenu(fileName = "Character Configuration Asset", menuName = "Dialogue System/Character Configuration Asset")]
    public class CharacterConfigSO : ScriptableObject
    {
        public CharacterConfigData[] characters;

        /// <summary>
        /// 根据角色名称获取对应的角色配置数据
        /// </summary>
        /// <param name="characterName">要查找的角色名称，可以是角色正式名称或别名</param>
        /// <returns>返回匹配的角色配置数据副本，如果未找到匹配项则返回默认配置</returns>
        public CharacterConfigData GetConfig(string characterName)
        {
            characterName = characterName.ToLower();

            // 遍历所有角色配置数据，查找匹配的角色
            for (int i = 0; i < characters.Length; i++)
            {
                CharacterConfigData data = characters[i];

                // 检查输入名称是否与角色名称或别名匹配（不区分大小写）
                if (string.Equals(characterName, data.name.ToLower()) ||
                    string.Equals(characterName, data.alias.ToLower()))
                    return data.Copy();
            }
            
            return CharacterConfigData.Default;
        }
    }
}
