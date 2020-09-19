using System;
using System.Windows.Forms;

namespace MachineData {
    static class Program {
        internal static bool Exit = false;
        private const string AboutAuthorText = "Produced by Art07. Last changes - 17_09_2020";
        private static readonly string[] MainMenuText = {
            "\n0 - Exit\n", "1 - Get machine data\n", "2 - CDM_Analyzer\n", "3 - CMD-V4 test for CINEO\nChoice => "
        };
        private static Controller.Controller _controller;
        private static int _choice;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            InitialApplicationJob();
            Console.WriteLine(AboutAuthorText);
            InitialMenu();
        }
        
        private static void InitialMenu() {
            while (!Exit) {
                foreach (var option in MainMenuText) Console.Write(option);

                if (!int.TryParse(Console.ReadLine(), out _choice)) {
                    Console.WriteLine("\nYou must enter a number.\n");
                    continue;
                }

                if (_choice < 0 || _choice > MainMenuText.Length - 1) {
                    Console.WriteLine($"\nThe choice must be within 0-{MainMenuText.Length - 1}\n");
                    continue;
                }

                _controller.Choice = _choice;
                _controller.Action();
            }

            Console.WriteLine("The main loop is stopped. The program is complete.");
        }

        private static void InitialApplicationJob() {
            _controller = new Controller.Controller();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }
    }
}