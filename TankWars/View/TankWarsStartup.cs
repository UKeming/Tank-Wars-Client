// @File: TankWarsStartup.cs
// @Created: 2021/04/01
// @Last Modified: 2021/04/06
// @Author: Keming Chen, Yifei Sun

using System;
using System.Windows.Forms;

namespace View
{
    internal static class TankWarsStartup
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TankWarsWin());
        }
    }
}