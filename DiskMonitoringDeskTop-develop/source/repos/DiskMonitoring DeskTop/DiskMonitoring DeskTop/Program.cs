using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DiskMonitoring_DeskTop
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            new Thread(CheckOrCreateLogDirectory).Start();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DiskMonitoring());           
        }
        public static void CheckOrCreateLogDirectory()
        {
            bool exists = Directory.Exists(@"..\log");
            if (!exists)
            {
                Directory.CreateDirectory(@"..\log");
            }
        }
    }
}
