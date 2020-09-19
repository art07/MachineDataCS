// ReSharper disable ClassNeverInstantiated.Global

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MachineData.Utils {
    internal class CdmAnalyzerUtils {
        internal static string GetFilePath() {
            var result = string.Empty;

            using (var fileDialog = new OpenFileDialog()) {
                fileDialog.Title = "CDM file selection window";
                fileDialog.InitialDirectory = new DirectoryInfo(@"C:\WOSASSP\LOG").Exists
                    ? @"C:\WOSASSP\LOG"
                    : @"C:\Probase\Prodevice\LOG";
                // fileDialog.Filter = "cdm files (*.TRC.XML)|*.TRC.XML|All files (*.*)|*.*";
                fileDialog.Filter = "cdm files (*.TRC.XML)|*.TRC.XML";

                if (fileDialog.ShowDialog() == DialogResult.OK) {
                    result = fileDialog.FileName;
                }
            }

            return result;
        }

        internal static List<string> ReadDataLineByLine(string filePath) {
            var listOfLines = new List<string>();

            var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
            using (StreamReader streamReader = new StreamReader(fileStream, Encoding.UTF8)) {
                string textLine;
                while ((textLine = streamReader.ReadLine()) != null) {
                    listOfLines.Add(textLine.Trim());
                }
            }

            fileStream.Close();
            return listOfLines;
        }

        internal static List<string> GetStringBlocksWithScod(List<string> listOfLines) {
            var listOfBlocsWithScodErrors = new List<string>();

            var command = String.Empty;
            foreach (var textLine in listOfLines) {
                if (textLine.Contains("<COMMAND") || textLine.Contains("<ENTRY")) {
                    command += textLine + "\n";
                } else if (textLine.Contains("</COMMAND>")) {
                    command += textLine + "\n";
                    var result = CheckScod(command);
                    if (result) {
                        listOfBlocsWithScodErrors.Add(command);
                        command = string.Empty;
                    } else {
                        command = string.Empty;
                    }
                } else if (textLine.Contains("<EVENT")) {
                    var result = CheckScod(textLine);
                    if (result) {
                        listOfBlocsWithScodErrors.Add(textLine + "\n");
                    }
                }
            }

            return listOfBlocsWithScodErrors;
        }

        private static bool CheckScod(string textBlock) {
            if (!textBlock.Contains("SCOD")) return false;
            
            var includesAnError = false;

            // "SCOD=28"
            var scod = textBlock.Substring(textBlock.IndexOf("SCOD", StringComparison.Ordinal), 7);
            
            // "28"
            var code = scod.Split('=')[1];

            if (!code.Equals("00") && !code.Equals("14")) {
                includesAnError = true;
            }

            return includesAnError;
        }
    }
}