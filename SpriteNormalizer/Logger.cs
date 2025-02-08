using System;

namespace SpriteNormalizer
{
    internal static class Logger
    {
        /// <summary>
        /// Hiển thị thông tin thông thường
        /// </summary>
        public static void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Hiển thị cảnh báo
        /// </summary>
        public static void LogWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING: " + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Hiển thị lỗi
        /// </summary>
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR: " + message);
            Console.ResetColor();
        }

        /// <summary>
        /// Hiển thị thành công
        /// </summary>
        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS: " + message);
            Console.ResetColor();
        }
    }
}
