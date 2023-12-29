using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace DiskMonitoring_DeskTop
{
    delegate void SetTextCallback(string text);
    public partial class DiskMonitoring 
    {
        private static FileSystemWatcher watcher;
        private const string LOGPATHFILECHANGES = @"..\log\LogFileChanges.txt";
        private const string LOGPATHMEMORYCHANGES = @"..\log\LogMemoryChanges.txt";
        public bool KeepWatch { get; set; } = true;
        private bool IsFile(string path)
        {
            bool isFile = false;
            try                                                                 // Checking if path is directory or path 
            {
                string[] subfolders = Directory.GetDirectories(path);
                isFile = false;
            }
            catch (System.IO.IOException)
            {
                isFile = true;
            }
            catch (Exception)
            {

            }
            return isFile;
        }
        private void PrinterFileChange(string text)
        {
            if (this.FileChangesLable.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(PrinterFileChange);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                text += $"\nTime: {DateTime.Now}";
                text += $"\nUser: {Environment.UserName}\n\n";
                FileChangesLable.Text += text;
                flowLayoutPanel1.AutoScrollPosition = new System.Drawing.Point(flowLayoutPanel1.HorizontalScroll.Minimum, flowLayoutPanel1.VerticalScroll.Maximum);
                LogTXT(text, LogFileChanges, LOGPATHFILECHANGES);
            }
        }
        private void PrinterMemory(string text)
        {
            if (this.MemoryChangesLabel.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(PrinterMemory);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                text += "\n";
                MemoryChangesLabel.Text += text;
                flowLayoutPanel2.AutoScrollPosition = new System.Drawing.Point(flowLayoutPanel2.HorizontalScroll.Minimum, flowLayoutPanel2.VerticalScroll.Maximum);
                LogTXT(text, logMemoryChanges, LOGPATHMEMORYCHANGES);
            }
        }
        private void LogTXT(string text, CheckBox checkBox, string path)
        {
            if (!checkBox.Checked)
            {
                return;
            }
            if (!File.Exists(path))
            {
                Program.CheckOrCreateLogDirectory();                
            }
            File.AppendAllText(path, text);
        }
        private void MemoryMonitoringAddon(string pathRoot, long temp, long driveSizeMb)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine(new String('*', 50));
            if (temp > driveSizeMb)
            {
                builder.AppendLine($"Memory of disk changed! ({pathRoot})\nTotal free memory space + {temp - driveSizeMb}kb");
            }
            else if (temp < driveSizeMb)
            {
                builder.AppendLine($"Memory of disk changed! ({pathRoot})\nTotal free memory space  {temp - driveSizeMb}kb");
            }
            else
            {
                builder.AppendLine("No changes was found");
            }
            builder.AppendLine($"{DateTime.Now}");
            builder.AppendLine(new String('*', 50));
            PrinterMemory(builder.ToString());
        }
        private void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                bool isFile = IsFile(e.FullPath);

                if (isFile)
                {
                    PrinterFileChange($"File was changed! Path: {e.FullPath} Name:{e.Name}");
                    FileInfo fileInfo = new FileInfo(e.FullPath);
                    try
                    {
                        PrinterFileChange($"Size of file {fileInfo.Length / 1024}kb");
                    }
                    catch
                    {
                        PrinterFileChange($"Accsess to file was denied!, {e.FullPath}");
                    }
                }
            }
            else
            {
                PrinterFileChange($"Directory changed with path: {e.FullPath}\nType: {e.ChangeType}\nName: {e.Name}");

            }
        }
        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            PrinterFileChange(ex.Message);
        }
        private void WatcherRenamed(object sender, RenamedEventArgs e)
        {
            PrinterFileChange($"Directory was renamed old path: {e.OldFullPath}\n" +
                $"New path  {e.FullPath}\n" +
                $"Old path  {e.OldFullPath}\n" +
                $"Old Name: {e.OldName}\n" +
                $"New Name: {e.Name}");
        }
        public void GetAvailableDisks()
        {
            string[] availableDisks = Directory.GetLogicalDrives();
            PrinterFileChange("Available disks");
            foreach (var disk in availableDisks)
            {
                PrinterFileChange(disk);
            }
        }
        public void MemoryMonitoring(object pathObj)
        {
            ParamsMemoryMonitoring parameters = pathObj as ParamsMemoryMonitoring;
            string path = parameters.Path;
            string pathRoot = Path.GetPathRoot(path);
            var driveInfo = new DriveInfo(Path.GetPathRoot(pathRoot));
            long driveSizeMb = driveInfo.AvailableFreeSpace / 1000; //kb
            long temp;
            while (parameters.KeepRun)
            {              
                temp = driveInfo.AvailableFreeSpace / 1000; //kb
                MemoryMonitoringAddon(pathRoot, temp, driveSizeMb);
                driveSizeMb = temp;
                Thread.Sleep(2000);
            }
        }
        public string GetPathFromUser()
        {
            string diskPath;
            using (var fbd = new FolderBrowserDialog())
            {
                while (true)
                {
                    DialogResult result = fbd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        diskPath = fbd.SelectedPath;
                        break;
                    }
                }
            }
            FileChangesLable.ResetText();
            currentPathUsedLabel.Text = $"Current path used: {diskPath}";
            currentPathUsedLabel.Text += $"\n{GetGeneralInfo(diskPath)}";
            currentPathUsedLabel.Text += $"\n{DiskMemoryLeft(diskPath)}";
            return diskPath;
        }
        public string DiskMemoryLeft(string diskPath)
        {
            var drive = new DriveInfo(diskPath);
            if (!drive.IsReady)
            {
                return "Disk is not ready!";
            }
            long diskSizeLeftByte = drive.AvailableFreeSpace;
            int diskSizeLeftMb = (int)(diskSizeLeftByte / 1048576);
            return $"Total disk space left: {diskSizeLeftMb}mb";
        }
        public string GetGeneralInfo(string diskPath)
        {
            var drive = new DriveInfo(Path.GetPathRoot(diskPath));
            if (!drive.IsReady)
            {
                return "Disk is not ready";
            }
            long diskSizeByte = drive.TotalSize;
            int diskSizeMb = (int)(diskSizeByte / 1048576);
            return $"\nTotal disk space ({Path.GetPathRoot(diskPath)}): {diskSizeMb}mb";
        }
        public void MonitoringDirectories(string path)
        {
            watcher = new FileSystemWatcher() { Path = path };
            watcher.EnableRaisingEvents = KeepWatch;
            watcher.IncludeSubdirectories = IncludeSubDirectoriesCB.Checked;
            watcher.Renamed += new RenamedEventHandler(WatcherRenamed);
            watcher.Changed += new FileSystemEventHandler(WatcherChanged);
            watcher.Created += new FileSystemEventHandler(WatcherChanged);
            watcher.Deleted += new FileSystemEventHandler(WatcherChanged);
            watcher.Error += Watcher_Error;
            GC.KeepAlive(watcher);
        }
    }
}
