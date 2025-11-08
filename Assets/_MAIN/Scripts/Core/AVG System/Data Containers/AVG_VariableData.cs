using System;
using UnityEngine;

namespace ADVENTUREGAME
{
    /// <summary>
    /// AVG_VariableData类用于存储变量数据信息
    /// 包含变量的名称、值和类型信息
    /// </summary>
    [Serializable]
    public class AVG_VariableData
    {
        /// <summary>
        /// 变量的名称
        /// </summary>
        public string name;
        
        /// <summary>
        /// 变量的值
        /// </summary>
        public string value;
        
        /// <summary>
        /// 变量的类型
        /// </summary>
        public string type;
    }
}