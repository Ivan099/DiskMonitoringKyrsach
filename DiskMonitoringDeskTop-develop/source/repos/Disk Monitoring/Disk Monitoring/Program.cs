namespace Disk_Monitoring
{
    class DiskMonitoring
    {
        private static FileSystemWatcher watcher;
        private static int totalChanges = 0;
        public void GetAvailableDisks()
        {
            string[] availableDisks = Directory.GetLogicalDrives();
            Console.WriteLine("Available disks");
            foreach (var disk in availableDisks)
            {
                Console.WriteLine(disk);
            }
        }
        public void MemoryMonitoring(object pathObj)
        {
            string path = pathObj as string;
            string pathRoot = Path.GetPathRoot(path);
            var driveInfo = new DriveInfo(Path.GetPathRoot(pathRoot));
            long driveSizeMb = driveInfo.AvailableFreeSpace / 1000; //kb
            long temp;
            while (true)
            {
                Thread.Sleep(4000);
                temp = driveInfo.AvailableFreeSpace / 1000; //kb
                if(driveSizeMb > temp)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(new String('*', 50));
                    Console.WriteLine($"Memory of disk changed! ({pathRoot})  {temp - driveSizeMb}kb");
                    Console.WriteLine(new String('*', 50));
                }
                else if(driveSizeMb < temp)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(new String('*', 50));
                    Console.WriteLine($"Memory of disk changed! ({pathRoot}) + {temp - driveSizeMb}kb");
                    Console.WriteLine(new String('*', 50));
                }
                driveSizeMb = temp;
            }
        }

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
            catch(Exception)
            {
                
            }
            return isFile;
        }         
        public void WatcherChanged(object sender, FileSystemEventArgs e)
        {
            Random random = new Random();
            Console.ForegroundColor = (ConsoleColor)random.Next(1, 15);

            if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                bool isFile = IsFile(e.FullPath);

                if (isFile)
                {
                    Console.WriteLine($"File was changed! Path: {e.FullPath} Name:{e.Name}");
                    FileInfo fileInfo = new FileInfo(e.FullPath);
                    try
                    {
                        Console.WriteLine($"Size of file {fileInfo.Length / 1024}kb");
                    }
                    catch 
                    {

                    }                   
                }
            }
            else
            {
                Console.WriteLine($"Directory changed with path: {e.FullPath}\nType: {e.ChangeType}\nName: {e.Name}");
               
            }
            Console.WriteLine($"Total changes {++totalChanges}");
        }
        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            Console.WriteLine(ex.Message);
        }
        private void WatcherRenamed(object sender, RenamedEventArgs e)
        {
            Random random = new Random();
            Console.ForegroundColor = (ConsoleColor)random.Next(1, 15);
            Console.WriteLine($"Directory was renamed old path: {e.OldFullPath}\n" +
                $"New path  {e.FullPath}\n" +
                $"Old path  {e.OldFullPath}\n" +
                $"Old Name: {e.OldName}\n" +
                $"New Name: {e.Name}");
            Console.WriteLine($"Total changes {++totalChanges}");
        }
        public string GetPathFromUser()
        {
            string diskPath;
            do
            {
                diskPath = Console.ReadLine();
                if (Path.IsPathFullyQualified(diskPath))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Your path / disk was unknown");
                }

            } while (true);
            return diskPath;
        }
        public void DiskMemoryLeft(string diskPath)
        {
            var drive = new DriveInfo(diskPath);
            if (!drive.IsReady)
            {
                return;
            }
            long diskSizeLeftByte = drive.AvailableFreeSpace;
            int diskSizeLeftMb = (int)(diskSizeLeftByte / 1048576);
            Console.WriteLine($"Total disk space left: {diskSizeLeftMb}mb");
        }
        public void GetGeneralInfo(string diskPath)
        {
            var drive = new DriveInfo(Path.GetPathRoot(diskPath));
            if (!drive.IsReady)
            {
                Console.WriteLine("Disk is not ready");
                return;
            }
            long diskSizeByte = drive.TotalSize;
            int diskSizeMb = (int)(diskSizeByte / 1048576);
            Console.WriteLine($"Ok i will be monitoring {diskPath}");
            Console.WriteLine($"Total disk space ({Path.GetPathRoot(diskPath)}): {diskSizeMb}mb");
        }
        public void MonitoringDirectories(string path)
        {          
            watcher = new FileSystemWatcher() { Path = path };
            watcher.NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.Security
                               | NotifyFilters.Size;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Renamed += new RenamedEventHandler(WatcherRenamed);
            watcher.Changed += new FileSystemEventHandler(WatcherChanged);
            watcher.Created += new FileSystemEventHandler(WatcherChanged);
            watcher.Deleted += new FileSystemEventHandler(WatcherChanged);
            watcher.Error += Watcher_Error;
            GC.KeepAlive(watcher);
            
        }


    }
    class Program
    {
        private static void Main()
        {
            DiskMonitoring monitorer = new DiskMonitoring();
            monitorer.GetAvailableDisks();
            Console.WriteLine("Welcome! which disk should I monitor? or Enter q to quit");
            string diskPath = monitorer.GetPathFromUser();
            string root = Path.GetPathRoot(diskPath);
            monitorer.MonitoringDirectories(diskPath);
            monitorer.GetGeneralInfo(diskPath);
            monitorer.DiskMemoryLeft(root);
            new Thread(monitorer.MemoryMonitoring).Start(diskPath);
            Console.ReadLine();
        }
    }

}
           