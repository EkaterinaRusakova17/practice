using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DirectorySyncApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string sourceDir = textBox1.Text;
            string targetDir = textBox1.Text; // changed textBox1 to textBox2

            SyncDirectories(sourceDir, targetDir);
            SyncDirectories(targetDir, sourceDir);

            MessageBox.Show("Синхронизация завершена.");
        }

        private void SyncDirectories(string sourceDir, string targetDir)
        {
            string[] filesSource = Directory.GetFiles(sourceDir);
            string[] filesTarget = Directory.GetFiles(targetDir);

            foreach (string fileSource in filesSource)
            {
                string fileName = Path.GetFileName(fileSource);
                string fileTarget = Path.Combine(targetDir, fileName);

                if (Array.IndexOf(filesTarget, fileTarget) < 0)
                {
                    MessageBox.Show($"Файл \"{fileName}\" создан");
                    File.Copy(fileSource, fileTarget);
                    WriteXmlLog($"Файл \"{fileName}\" скопирован в {targetDir}");
                }
                else
                {
                    FileInfo infoSource = new FileInfo(fileSource);
                    FileInfo infoTarget = new FileInfo(fileTarget);

                    if (infoSource.LastWriteTime != infoTarget.LastWriteTime)
                    {
                        MessageBox.Show($"Файл \"{fileName}\" изменен");
                        File.Copy(fileSource, fileTarget, true);
                        WriteJsonLog($"Файл \"{fileName}\" обновлен в {targetDir}");
                    }
                }
            }

            foreach (string fileTarget in filesTarget)
            {
                string fileName = Path.GetFileName(fileTarget);
                string fileSource = Path.Combine(sourceDir, fileName);

                if (Array.IndexOf(filesSource, fileSource) < 0)
                {
                    MessageBox.Show($"Файл \"{fileName}\" удален");
                    WriteXmlLog($"Файл \"{fileName}\" удален из {targetDir}");
                }
            }
        }

        private void WriteXmlLog(string logMessage)
        {
            string logFilePath = "log.xml";

            if (!File.Exists(logFilePath))
            {
                using (XmlWriter writer = XmlWriter.Create(logFilePath))
                {
                    writer.WriteStartElement("Log");
                }
            }

            using (XmlWriter writer = XmlWriter.Create(logFilePath))
            {
                writer.WriteStartElement("Entry");
                writer.WriteElementString("DateTime", DateTime.Now.ToString());
                writer.WriteElementString("Message", logMessage);
                writer.WriteEndElement();
            }
        }

        private void WriteJsonLog(string logMessage)
        {
            string logFilePath = "log.json";
            if (!File.Exists(logFilePath))
            {
                using (StreamWriter sw = File.CreateText(logFilePath))
                {
                    sw.WriteLine("[");
                }
            }

            string jsonEntry = $"{{\"DateTime\":\"{DateTime.Now}\", \"Message\":\"{logMessage}\"}}";
            string jsonData = File.ReadAllText(logFilePath);
            if (!jsonData.EndsWith("["))
            {
                jsonData = jsonData.Remove(jsonData.Length - 1);
                jsonData += ",";
            }

            jsonData += jsonEntry + "]";
            File.WriteAllText(logFilePath, jsonData);
        }
    }
}