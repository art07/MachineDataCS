using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using MachineData.Model;
using Microsoft.Win32;

// ReSharper disable UseNullPropagation

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable once ClassNeverInstantiated.Global

namespace MachineData.Utils {
    internal class CollectMachineDataUtils {
        internal static void CreateMachineDataDir() {
            Console.WriteLine(
                $"\nSelect the folder where the program will create <{DataLab.GetInstance().MainFolderName}> folder for data collection...");
            Application.Run(new FormDir());
            if (DataLab.GetInstance().ChosenPathWithMachineData != "") {
                CreateFolder();
            } else {
                Console.WriteLine("You closed the window without making a choice.");
            }
        }

        private static void CreateFolder() {
            var directoryInfo = Directory.CreateDirectory(DataLab.GetInstance().ChosenPathWithMachineData);
            if (directoryInfo.Exists) {
                DataLab.GetInstance().MachineDataDirInfo = directoryInfo;
                Console.WriteLine(
                    $"Directory <{DataLab.GetInstance().MachineDataDirInfo.Name}> was created successfully!");
                Console.WriteLine(
                    $"Full path for all collecting data => {DataLab.GetInstance().MachineDataDirInfo.FullName}");
                CreateRegFolder();
                CreateTpcaFolder();
            } else {
                Console.WriteLine("Failed to create MachineData folder");
                Environment.Exit(0);
            }
        }

        private static void CreateRegFolder() {
            var directoryInfo =
                Directory.CreateDirectory(Path.Combine(DataLab.GetInstance().MachineDataDirInfo.FullName, "REG"));
            if (directoryInfo.Exists) {
                DataLab.GetInstance().RegDirInfo = directoryInfo;
                Console.WriteLine(
                    $"Directory <{DataLab.GetInstance().RegDirInfo.Name}> was created successfully!");
                Console.WriteLine(
                    $"Full path for REG data => {DataLab.GetInstance().RegDirInfo.FullName}");
                CreateFileForKeyValues();
            } else {
                Console.WriteLine("Failed to create REG folder");
                Environment.Exit(0);
            }
        }

        private static void CreateFileForKeyValues() {
            FileInfo keyValuesTxt =
                new FileInfo(Path.Combine(DataLab.GetInstance().RegDirInfo.FullName, "keyValues.txt"));
            FileStream fileStream = keyValuesTxt.Create();
            fileStream.Close();
            if (keyValuesTxt.Exists) {
                DataLab.GetInstance().KeyValuesTxt = keyValuesTxt;
                Console.WriteLine("keyValues.txt created!");
            } else {
                Console.WriteLine("keyValues.txt was not created!!!");
            }
        }

        private static void CreateTpcaFolder() {
            DirectoryInfo tpcaInfoOnMachine = new DirectoryInfo(@"C:\TPCA");
            if (!tpcaInfoOnMachine.Exists) return;
            var tpcaInfoInMachineData =
                Directory.CreateDirectory(Path.Combine(DataLab.GetInstance().MachineDataDirInfo.FullName, "TPCA"));
            if (tpcaInfoInMachineData.Exists) {
                DataLab.GetInstance().TpcaDirInfo = tpcaInfoInMachineData;
                Console.WriteLine(
                    $"Directory <{DataLab.GetInstance().TpcaDirInfo.Name}> was created successfully!");
                Console.WriteLine(
                    $"Full path for TPCA data => {DataLab.GetInstance().TpcaDirInfo.FullName}");
            } else {
                Console.WriteLine("Failed to create TPCA folder");
                Environment.Exit(0);
            }
        }

        internal static void GetJournalsFiles() {
            Console.WriteLine($"\n...Start collecting Journals/Files in {Thread.CurrentThread.Name}");
            if (DataLab.GetInstance().FilesToCopyList.Count != 0) {
                CopyFiles();
                Console.WriteLine($"Files were copied successfully! Job in = {Thread.CurrentThread.Name}");
            } else {
                Console.WriteLine("No files to copy");
            }

            if (DataLab.GetInstance().FoldersToCopyList.Count != 0) {
                CopyDirs();
                Console.WriteLine($"Folders were copied successfully! Job in = {Thread.CurrentThread.Name}");
            } else {
                Console.WriteLine("No folders to copy");
            }
        }

        private static void CopyFiles() {
            foreach (FileInfo file in DataLab.GetInstance().FilesToCopyList) {
                try {
                    if (!file.FullName.Contains("TPCA")) {
                        file.CopyTo(Path.Combine(DataLab.GetInstance().MachineDataDirInfo.FullName, file.Name), false);
                    } else {
                        file.CopyTo(Path.Combine(DataLab.GetInstance().MachineDataDirInfo.FullName + @"\TPCA", file.Name), false);
                    }
                } catch (IOException e) {
                    Console.WriteLine($"Не удалось скопировать файл <{file.Name}>");
                    Console.WriteLine(e);
                }
            }
        }

        private static void CopyDirs() {
            foreach (DirectoryInfo srcDirOnDiskC in DataLab.GetInstance().FoldersToCopyList) {
                DirectoryCopyRecMethod(srcDirOnDiskC, DataLab.GetInstance().MachineDataDirInfo);
            }
        }

        private static void DirectoryCopyRecMethod(DirectoryInfo srcDirOnDiskC, DirectoryInfo destDirOnFlashDrive) {
            /*Беру путь куда сохранять на флэшке (к примеру ...\MachineData), добавляю имя c папки на диске
             С (к примеру JOURNAL) и создаю новую папку на флэшке, у которой путь к примеру 
             ...\MachineData\JOURNAL.
             */
            DirectoryInfo newDirOnFlashDrive;
            if (!srcDirOnDiskC.FullName.Contains("TPCA")) {
                newDirOnFlashDrive =
                    Directory.CreateDirectory(Path.Combine(destDirOnFlashDrive.FullName, srcDirOnDiskC.Name));
            } else {
                newDirOnFlashDrive =
                    Directory.CreateDirectory(Path.Combine(destDirOnFlashDrive.FullName + @"\TPCA",
                        srcDirOnDiskC.Name));
            }

            // Извлекаю файлы и подпапки, папки srcDir.
            FileInfo[] files = srcDirOnDiskC.GetFiles();
            DirectoryInfo[] subDirs = srcDirOnDiskC.GetDirectories();

            /*----------------------------------------------------------------*/

            if (files.Length != 0) {
                foreach (FileInfo file in files) {
                    try {
                        file.CopyTo(Path.Combine(newDirOnFlashDrive.FullName, file.Name), false);
                    } catch (IOException e) {
                        Console.WriteLine($"Не удалось скопировать файл <{file.Name}>");
                        Console.WriteLine(e);
                    }
                }
            }

            if (subDirs.Length != 0) {
                foreach (DirectoryInfo subDir in subDirs) {
                    DirectoryCopyRecMethod(subDir, newDirOnFlashDrive);
                }
            }
        }

        internal static void GetIp() {
            Console.WriteLine("\n...Start getting IP");
            var fileName = "ip.txt";
            /* /с - После выполнения команды, завершить работу с консолью.*/
            var process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden; // Спрятать окно консоли.
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments =
                $"/c ipconfig /all > {Path.Combine(DataLab.GetInstance().MachineDataDirInfo.FullName, fileName)}";
            process.Start();
            process.WaitForExit();

            Console.WriteLine(IsFileCreated(Path.Combine(DataLab.GetInstance().MachineDataDirInfo.FullName, fileName))
                ? $"{fileName} created. Path => {DataLab.GetInstance().MachineDataDirInfo.FullName}"
                : $"Failed to create {fileName}");
        }

        internal static void GetRegistryBranches() {
            Console.WriteLine("\n...Start getting registry branches");
            foreach (var regBranch in DataLab.GetInstance().RegBranchesDic) GetBranch(regBranch);
        }

        private static void GetBranch(KeyValuePair<string, string> regBranch) {
            var process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments =
                $"/c reg export {regBranch.Value} {Path.Combine(DataLab.GetInstance().RegDirInfo.FullName, regBranch.Key)}"; /* /reg:64*/
            process.Start();
            process.WaitForExit();

            Console.WriteLine(IsFileCreated(Path.Combine(DataLab.GetInstance().RegDirInfo.FullName, regBranch.Key))
                ? $"{regBranch.Key} created. Path => {DataLab.GetInstance().RegDirInfo.FullName}"
                : $"Failed to create {regBranch.Key}");
        }

        private static bool IsFileCreated(string fullFileName) {
            return new FileInfo(fullFileName).Exists;
        }

        internal static void GetKeyRegistryValues() {
            Console.WriteLine("\n...Start getting main registry values");
            string superRegistryString = "";
            foreach (KeyValuePair<string, string> regPair in DataLab.GetInstance().RegValuesDic) {
                using (RegistryKey passToValue = Registry.LocalMachine.OpenSubKey(regPair.Value)) {
                    if (passToValue != null) {
                        superRegistryString += regPair.Key + " = " + passToValue.GetValue(regPair.Key) + "\n";
                    } else {
                        superRegistryString += regPair.Key + " = Not found" + "\n";
                    }
                }
            }

            superRegistryString = superRegistryString.Trim();
            Console.WriteLine(superRegistryString);
            WriteTxtFile(superRegistryString);
        }

        private static void WriteTxtFile(string str) {
            FileStream fileStream =
                File.Open(DataLab.GetInstance().KeyValuesTxt.FullName, FileMode.Open, FileAccess.Write);
            using (StreamWriter streamWriter = new StreamWriter(fileStream)) {
                streamWriter.WriteLine(str);
            }
            fileStream.Close();
        }
    }
}