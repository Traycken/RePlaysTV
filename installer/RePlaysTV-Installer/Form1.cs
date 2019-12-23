﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RePlaysTV_Installer {
    public partial class Form1 : Form {
        const string VERSION = "3.0.1";
        public string playsDirectory = Environment.GetEnvironmentVariable("LocalAppData") + "\\Plays";
        public static Form1 form1;
        Process p;
        StreamWriter SW;

        public static bool startImport = false;
        public Form1() {
            InitializeComponent();
            form1 = this;
        }

        private void Button1_Click(object sender, EventArgs e) {
            form1.TopMost = true;
            DialogResult dr1 = MessageBox.Show("This automated process can take up to 10 minutes or more.\n" +
                                                "Make sure the last latest version of Plays is installed and not currently open." +
                                                "\nPress Yes to start install.", "RePlaysTV Installer", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Information);
            if (dr1 == DialogResult.Yes) {
                if (Directory.Exists(playsDirectory + "\\app-3.0.0")) {
                    StartExtract();
                    button1.Enabled = false;
                    button2.Enabled = false;
                }
                else {
                    DialogResult dr2 = MessageBox.Show("You are missing the original Plays client files.\n" +
                                    "In order for Replays to install, it requires you to have Plays 3.0.0 installed." +
                                    "\nWould you like to be taken to the download page?", "RePlaysTV Installer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                    if (dr2 == DialogResult.Yes) {
                        Process.Start("https://drive.google.com/file/d/1YlQ-EU6wW8XvGUznIBrSqTvlzBv-6tkQ/view");
                    }
                }
            }
            form1.TopMost = false;
        }
        private void StartExtract() { //part one of installation
            SW.WriteLine("nodejs-portable.exe");
            if (Directory.Exists(Directory.GetCurrentDirectory() + "\\temp"))
                SW.WriteLine("rd /s /q temp");
            SW.WriteLine("mkdir temp");
            SW.WriteLine("asar extract \"" + playsDirectory + "\\app-3.0.0\\resources\\app.asar\" temp");
            SW.WriteLine("cd temp");
            SW.WriteLine("npm init -f");
            SW.WriteLine("npm install");
            SW.WriteLine("electron-forge import");
        }
        private void StartModify() { //part three of installation
            SW.WriteLine("cd ..");
            SW.WriteLine("rd /s /q \".\\temp\\.cache\"");
            SW.WriteLine("mkdir \".\\temp\\resources\\auger\\replays\"");
            SW.WriteLine("robocopy /E /NP /MT \".\\src\" \".\\temp\\resources\\auger\\replays\"");

            //start modifying original plays files
            ModifyFileAtLine("if (true) {", Directory.GetCurrentDirectory() + "\\temp\\src\\main\\main.js", 682); //dev tools
            ModifyFileAtLine("preload: AugerWindow.getPreload('preload.js'), devTools: true,", Directory.GetCurrentDirectory() + "\\temp\\src\\main\\UIManager.js", 419); //dev tools
            ModifyFileAtLine("devTools: true,", Directory.GetCurrentDirectory() + "\\temp\\src\\main\\UIManager.js", 548); //dev tools
            ModifyFileAtLine("const showurl = '/replays/index.html';", Directory.GetCurrentDirectory() + "\\temp\\src\\main\\UIManager.js", 571);
            for (int i = 608; i <= 634; i++) {
                ModifyFileAtLine("// removed", Directory.GetCurrentDirectory() + "\\temp\\src\\main\\UIManager.js", i);
            }
            for (int i = 637; i <= 640; i++) {
                ModifyFileAtLine("// removed", Directory.GetCurrentDirectory() + "\\temp\\src\\main\\UIManager.js", i);
            }
            ModifyFileAtLine("window.loadURL(path.join(__dirname, '/../../resources/auger', augerRouteUrl), urlOptions);", Directory.GetCurrentDirectory() + "\\temp\\src\\core\\AugerWindow.js", 38);
            ModifyFileAtLine("nodeIntegration: true,", Directory.GetCurrentDirectory() + "\\temp\\src\\core\\AugerWindow.js", 53);
            ModifyFileAtLine("if (false) {", Directory.GetCurrentDirectory() + "\\temp\\src\\core\\Updater.js", 62);    //disables updater by code
            ModifyFileAtLine("const AUGER_URL_IG_WIDGETS = '/replays/IngameOverlay.html';", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\IngameOverlay\\IngameHUDService.js", 15);  //custom hud
            ModifyFileAtLine("return true;", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\RunningGamesService.js", 105);    //disables check for login required to recording
            ModifyFileAtLine("return null;", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\BaseService.js", 48);     //disables online user check
            ModifyFileAtLine("return {};", Directory.GetCurrentDirectory() + "\\temp\\src\\core\\Settings.js", 239);     //disables online user check
            for (int i = 159; i <= 166; i++) {
                ModifyFileAtLine("// removed", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\Notifications\\FlowListener.js", i);     //disables online user check
            }
            ModifyFileAtLine("// removed", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\PresenceService.js", 79);     //disables online user check
            ModifyFileAtLine("// removed", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\PresenceService.js", 94);     //disables online user check

            ModifyFileAtLine("const GAMECATALOGS_URL = 'https://raw.githubusercontent.com/lulzsun/RePlaysTV/master/detections/';", Directory.GetCurrentDirectory() + "\\temp\\src\\core\\Utils.js", 39); //changes gamecatalog url
            ModifyFileAtLine("const CATALOG_GAME_DETECTION = 'game_detections';", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\DetectionRequests\\GameCatalogRequest.js", 6);
            ModifyFileAtLine("const CATALOG_NONGAME_DETECTION = 'nongame_detections';", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\DetectionRequests\\GameCatalogRequest.js", 7);
            ModifyFileAtLine("static getLatestVersionFileUrl() {", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\DetectionRequests\\GameCatalogRequest.js", 13);
            ModifyFileAtLine("return `/version.json`;", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\DetectionRequests\\GameCatalogRequest.js", 14);
            ModifyFileAtLine("const url = GameCatalogRequest.getLatestVersionFileUrl();", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\DetectionRequests\\GameCatalogRequest.js", 27);
            ModifyFileAtLine("return {version: response[`${catalog}_version`].version, key: `${catalog}.json`};", Directory.GetCurrentDirectory() + "\\temp\\src\\service\\DetectionRequests\\GameCatalogRequest.js", 34);
            //end modifying

            StartPackage();
        }

        private void StartPackage() { //part four of installation
            SW.WriteLine("cd temp");
            SW.WriteLine("npm run package");
            SW.WriteLine("rename " + playsDirectory + "\\Update.exe noUpdate.exe");
            SW.WriteLine("rmdir /s /q \"" + playsDirectory + "\\app-" + VERSION + "\"");
            SW.WriteLine("mkdir \"" + playsDirectory + "\\app-" + VERSION + "\"");
            SW.WriteLine("asar pack \".\\out\\Plays-win32-ia32\\resources\\app\" \".\\out\\Plays-win32-ia32\\resources\\app.asar\"");
            SW.WriteLine("rd /s /q \".\\out\\Plays-win32-ia32\\resources\\app\"");
            SW.WriteLine("robocopy /E /NP /MT \".\\out\\Plays-win32-ia32\" \"" + playsDirectory + "\\app-" + VERSION + "\"");
            SW.WriteLine("cd ..");
            SW.WriteLine("rd /s /q temp");
            SW.WriteLine("exit");
        }

        private void InstallComplete() {
            form1.TopMost = true;
            DialogResult dr = MessageBox.Show("Installation Complete!\n\nOpen Replays?", "RePlaysTV Installer", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            if (dr == DialogResult.Yes) {
                Process.Start(playsDirectory + "\\app-" + VERSION + "\\Plays.exe");
            }
            form1.TopMost = false;
        }

        private void ModifyFileAtLine(string newText, string fileName, int line_to_edit) {
            string[] arrLine = File.ReadAllLines(fileName);
            arrLine[line_to_edit - 1] = newText;
            File.WriteAllLines(fileName, arrLine);
            richTextBox1.AppendText(Environment.NewLine + "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + fileName + ">>> Writing to line " + line_to_edit + ": " + newText);
            richTextBox1.ScrollToCaret();
        }

        private void Button2_Click(object sender, EventArgs e) {
            var fbd = new FolderBrowserDialog {
                RootFolder = Environment.SpecialFolder.Desktop,
                SelectedPath = Environment.GetEnvironmentVariable("LocalAppData")
            };
            using (fbd) {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK) {
                    textBox1.Text = fbd.SelectedPath;
                    playsDirectory = fbd.SelectedPath;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            if (Directory.Exists(playsDirectory)) {
                textBox1.Text = playsDirectory;
            }
            p = new Process();

            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            p.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
            p.ErrorDataReceived += new DataReceivedEventHandler(SortOutputHandler);

            p.Start();

            SW = p.StandardInput;

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
        }
        private static void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine) {
            if (!String.IsNullOrEmpty(outLine.Data)) {
                if (form1.richTextBox1.InvokeRequired) {
                    form1.richTextBox1.Invoke(new MethodInvoker(delegate {
                        if (outLine.Data.Contains("All rights reserved.")) {
                            form1.richTextBox1.Text = "[" + DateTime.Now.ToString("h:mm:ss tt") + "] Ready";
                        } else if (outLine.Data.Contains("We will now attempt to import: ") && !startImport) { //part two of installation
                            var enterThread = new Thread(
                            new ThreadStart(
                                () => {
                                    Thread.Sleep(5000);
                                    form1.Invoke(new MethodInvoker(delegate { form1.SW.WriteLine("y"); }));
                                    Thread.Sleep(5000);
                                    form1.Invoke(new MethodInvoker(delegate { form1.SW.WriteLine("y"); }));
                                    Thread.Sleep(5000);
                                    form1.Invoke(new MethodInvoker(delegate { form1.SW.WriteLine("src/main/main.js"); }));
                                    Thread.Sleep(5000);
                                    form1.Invoke(new MethodInvoker(delegate { form1.SW.WriteLine("n"); }));
                                    Thread.Sleep(5000);
                                    form1.Invoke(new MethodInvoker(delegate {
                                        form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                        form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                        form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                        form1.richTextBox1.AppendText(Environment.NewLine + "[" + DateTime.Now.ToString("h:mm:ss tt") + "] This next process will take awhile (with no sign of progress)... Please be patient.");
                                        form1.richTextBox1.ScrollToCaret();
                                    }));
                                }
                            ));
                            startImport = true;
                            enterThread.Start();
                        } else {
                            if (outLine.Data.Contains("npm install") || outLine.Data.Contains("electron-forge package") || outLine.Data.Contains("asar extract")) {
                                form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                form1.richTextBox1.AppendText(Environment.NewLine + "[" + DateTime.Now.ToString("h:mm:ss tt") + "] This next process will take awhile (with no sign of progress)... Please be patient.");
                            }
                            if (outLine.Data.Contains("Thanks for using ") && outLine.Data.Contains("electron-forge")) {
                                form1.Invoke(new MethodInvoker(delegate { form1.StartModify(); }));
                            }
                            if (outLine.Data.Contains("npm ERR!")) {
                                form1.TopMost = true;
                                System.Windows.Forms.MessageBox.Show("An unhandled error has occurred during the install, It is possible that the installation has corrupted.\nTry restarting your computer and turn off anti-virus before installing.\n\nReport this issue by copying the logs and sending it to a developer.");
                                form1.TopMost = false;
                            }
                            if (outLine.Data.Contains(">exit")) {
                                form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                form1.richTextBox1.AppendText(Environment.NewLine + "=======================================");
                                form1.richTextBox1.AppendText(Environment.NewLine + "[" + DateTime.Now.ToString("h:mm:ss tt") + "] Installation Complete!");
                                form1.Invoke(new MethodInvoker(delegate { form1.InstallComplete(); }));

                            }
                            form1.richTextBox1.AppendText(Environment.NewLine + "[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + outLine.Data.ToString());
                            form1.richTextBox1.ScrollToCaret();
                        }
                    }));
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            p.Kill();
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Process.Start("https://github.com/lulzsun/RePlaysTV");
        }

        private void Button3_Click(object sender, EventArgs e) {
            Clipboard.SetText(richTextBox1.Text);
        }
    }
}
