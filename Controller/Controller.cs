// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable ConvertToAutoProperty

using System;
using System.Diagnostics;
using System.Threading;
using MachineData.Model;
using MachineData.Utils;

namespace MachineData.Controller {
    internal class Controller {
        private int _choice;

        internal int Choice {
            get => _choice;
            set => _choice = value;
        }

        internal void Action() {
            if (Choice == 0) {
                Program.Exit = true;
            } else if (Choice == 1) {
                CollectMachineDataUtils.CreateMachineDataDir();
                if (DataLab.GetInstance().MachineDataDirInfo == null ||
                    !DataLab.GetInstance().MachineDataDirInfo.Exists) {
                    Console.WriteLine("The program cannot be executed because the main folder was not created.");
                    return;
                }

                // Start stopwatch
                Stopwatch stopwatch = Stopwatch.StartNew();

                Thread backgroundThread = new Thread(CollectMachineDataUtils.GetJournalsFiles) {
                    Name = "backgroundThread"
                };
                backgroundThread.Start();
                Console.WriteLine(backgroundThread.Name + " IsAlive at start = " + backgroundThread.IsAlive);

                CollectMachineDataUtils.GetIp();
                CollectMachineDataUtils.GetRegistryBranches();
                CollectMachineDataUtils.GetKeyRegistryValues();

                Console.WriteLine(backgroundThread.Name + " IsAlive before Join() = " + backgroundThread.IsAlive);
                backgroundThread.Join();
                Console.WriteLine(backgroundThread.Name + " IsAlive after Join() = " + backgroundThread.IsAlive);

                // Finish stopwatch
                stopwatch.Stop();

                Console.WriteLine(
                    $"\n=>=>=>\nLogs/files collection is finished. Elapsed time: {(double) stopwatch.ElapsedMilliseconds / 1000:F2} second(s)\n=>=>=>");

                Program.Exit = true;
            } else if (Choice == 2) {
                Console.WriteLine("Select the file to analyze...");
                var filePath = CdmAnalyzerUtils.GetFilePath();
                if (filePath.Equals(string.Empty)) {
                    Console.WriteLine("Cdm file was not selected.");
                    return;
                }

                var listOfLines = CdmAnalyzerUtils.ReadDataLineByLine(filePath);
                var listOfBlocsWithScodErrors = CdmAnalyzerUtils.GetStringBlocksWithScod(listOfLines);
                if (listOfBlocsWithScodErrors.Count == 0) {
                    Console.WriteLine("\nThere are no cmd errors in the file.");
                } else {
                    Console.WriteLine($"\nCDM ERRORS ({listOfBlocsWithScodErrors.Count}) =>=>=>\n");
                    foreach (var blockWithScodError in listOfBlocsWithScodErrors) {
                        Console.WriteLine(blockWithScodError);
                    }

                    Console.WriteLine("<=<=<= CDM ERRORS");
                }
            } else if (Choice == 3) {
                Console.WriteLine("USBIO.SYS section");
            }
        }
    }
}