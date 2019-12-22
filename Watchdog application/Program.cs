using System;
using System.Diagnostics;
using CommandLine;
using System.Threading;
using System.Windows.Forms;

namespace Watchdog_application
{
    static class Program
    {

        private class Options
        {
            [Option('a', "application", Required = true, HelpText = "Application name for watchdog")]
            public string application { get; set; }

            [Option('p', "path", Required = true, HelpText = "Full path to program")]
            public string path { get; set; }

            [Option('t', "timer", Required = false, HelpText = "Watchdog triggering timer, sec", Default = 1)]
            public int timer { get; set; }
        }

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {

            string app = "";
            string path = "";
            int timer = 1;
            int j = 1;

            if (!CheckArgs(args, out app, out path, out timer))
            {
                return 1;
            }

            timer = timer * 1000;
            ProcessStartInfo startInfo = new ProcessStartInfo(path);

            for (; ; )
            {

                Process[] processes = Process.GetProcessesByName(app);

                if (processes.Length == 0)
                {
                    Process.Start(startInfo);
                    Thread.Sleep(5000);
                }

                Thread.Sleep(timer);

            }

            return 0;
            
        }

        private static bool CheckArgs(string[] args, out string app, out string path, out int timer)
        {

            bool errorFlag = false;
            string errorText = "";
            
            string _app = "";
            string _path = "";
            int _timer = 1;


            var parser = new Parser(config => config.HelpWriter = null);
            var parserResult = parser.ParseArguments<Options>(args);

            parserResult
                .WithParsed<Options>(o =>
                {
                    _app = o.application;
                    _path = o.path;
                    _timer = o.timer;
                })
                .WithNotParsed(errs =>
                {
                    if (errorText.Length > 0) errorText += "\n";
                    errorText += CommandLine.Text.HelpText.AutoBuild(parserResult);
                    errorFlag = true;
                });

            app = _app;
            path = _path;
            timer = _timer;

            if (!errorFlag)
            {
                if (!System.IO.File.Exists(path))
                {
                    if (errorText.Length > 0) errorText += "\n";
                    errorText += "File: \"" + path + "\" not found";
                    errorFlag = true;
                };

                if (timer < 1)
                {
                    if (errorText.Length > 0) errorText += "\n";
                    errorText += "Timer must be >= 1";
                    errorFlag = true;
                };

            }

            if (errorFlag)
            {
                ShowMessage(errorText);
            }

            return !errorFlag;
        }

        private static void ShowMessage(string textMessage)
        {
            MessageBox.Show(textMessage, "Watchdog application", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
