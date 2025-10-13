using COMMANDS;

namespace COMMANDS
{
    /// <summary>
    /// 数据库命令扩展抽象类，提供数据库功能扩展的基类
    /// </summary>
    public abstract class CMD_DatabaseExtension
    {
        /// <summary>
        /// 扩展数据库命令功能的静态方法
        /// </summary>
        /// <param name="database">要扩展的命令数据库实例</param>
        public static void Extend(CommandDatabase database) { }
        
        /// <summary>
        /// 将字符串数组数据转换为命令参数对象
        /// </summary>
        /// <param name="data">要转换的字符串数组数据</param>
        /// <returns>转换后的命令参数对象</returns>
        protected static CommandParameters ConvertDataToParameters(string[] data) => new CommandParameters(data);
    }
}

