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
        public static void Extend(CommandDatabase database)
        {
        }
    }
}

