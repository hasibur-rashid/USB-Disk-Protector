using System;
using System.Text;
using System.Data;
using System.Linq;
using System.Media;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Collections;

using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

using System.Speech.Synthesis;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Microsoft.Win32;
using Microsoft.CSharp;


namespace USB_Antivirus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int viruses = 0;
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
		
		readonly SpeechSynthesizer _voice = new SpeechSynthesizer();
        readonly SoundPlayer _sound = new SoundPlayer();
        //NotifyIcon nIcon = new NotifyIcon();

        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        private System.Windows.Forms.ContextMenuStrip m_contextMenu;
        private DriveDetector driveDetector;
        public List<ReportFiles> listabove = new List<ReportFiles>();

        public MainWindow()
        {
            Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, Microsoft.Win32.RegistryValueKind.DWord);
			Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 1, Microsoft.Win32.RegistryValueKind.DWord); 
			this.InitializeComponent(); Clock(); StartingMusic();
			dinfo();
            Contex_Menu_And_Notify_Icon(); LoadDeafult();
            this.AboutPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Hidden;

            _voice.SpeakAsync("Welcome to USB Disk Protector");

            driveDetector = new DriveDetector();
            driveDetector.DeviceArrived += new DriveDetectorEventHandler(OnDriveArrived);
            driveDetector.DeviceRemoved += new DriveDetectorEventHandler(OnDriveRemoved);
            DetecUsbDevices();

        }

        public class ReportFiles
        {
            public string FileName { get; set; }
            public string Size { get; set; }
            public string Path { get; set; }
        }

        public void DetecUsbDevices()
        {
            
			var drv = from dr in DriveInfo.GetDrives()
                      where dr.IsReady == true && dr.DriveType == DriveType.Removable
                      select dr;
            foreach (DriveInfo dinfo in drv)
            {
               // _DriveLetterCombo.Items.Add(dinfo.Name);
               //_DriveLetterCombo.SelectedIndex = 0;

                _DriveLetterCombo.Items.Add(dinfo.Name.Remove(2));
               // _DriveLetterCombo1.Items.Add(dinfo.Name.Remove(2));

            }
        }

        // Called by DriveDetector when removable device in inserted 
        private void OnDriveArrived(object sender, DriveDetectorEventArgs e)
        {
			dinfo();
            // Report the event in the listbox.
            // e.Drive is the drive letter for the device which just arrived, e.g. "E:\\"
            // string s = "Drive arrived " + e.Drive;
		
            this.Dashboard.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Visible;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.AboutPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ScanSubPanel.Visibility=System.Windows.Visibility.Visible;
			this.QuarantainSubPanel.Visibility=System.Windows.Visibility.Hidden;
			this.Fotter.Visibility = System.Windows.Visibility.Hidden;
            this.Fotter_Copy.Visibility = System.Windows.Visibility.Hidden;
			
			
			Show();
            this.WindowState = System.Windows.WindowState.Normal;//undoes the minimized state of the form
            
			_DriveLetterCombo.Items.Clear();
            _DriveLetterCombo.Items.Add(e.Drive.Remove(2));
			//_virusScanButton.Click+=new System.Windows.RoutedEventHandler(_virusScanButton_Click);

            _virusScanButton_Click(sender,new RoutedEventArgs());//calling virus scan button
			
			//_virusScanButton.RaiseEvent(new  RoutedEventArgs(_virusScanButton.Click));
			
			//notifyIcon1.Visible = false;//hides tray icon again
            //button1.PerformClick();

            // If you want to be notified when drive is being removed (and be able to cancel it), 

        }

        // Called by DriveDetector after removable device has been unpluged 
        private void OnDriveRemoved(object sender, DriveDetectorEventArgs e)
        {
            
			// TODO: do clean up here, etc. Letter of the removed drive is in e.Drive;

            // Just add report to the listbox
            // string s = "Drive removed " + e.Drive;
			dinfo();
            _DriveLetterCombo.Items.Remove(e.Drive);
            //_DriveLetterCombo1.Items.Remove(e.Drive);
        }

        // Called by DriveDetector when removable drive is about to be removed


		//clear temp, prefetch, temporary file etc.
		public void QuickClean()
		{
			try
			{
			String tempFolder = Environment.ExpandEnvironmentVariables("%TEMP%");    
            String prefetch = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%") + "\\Prefetch";
            String temp = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%") + "\\Temp";
			String recent = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + "\\Recent";
			String cookies = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + "\\cookies";
			String history = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + "\\Local Settings\\History";
			String temp_internet_file  = Environment.ExpandEnvironmentVariables("%USERPROFILE%") + "\\Local Settings\\Temporary Internet Files";	
            String dllchache = Environment.ExpandEnvironmentVariables("%SYSTEMROOT%") + "\\system32\\dllcache";
			EmptyFolderContents(tempFolder);
            EmptyFolderContents(temp);
            EmptyFolderContents(prefetch);
            EmptyFolderContents(recent);
			EmptyFolderContents(cookies);
			EmptyFolderContents(history);
			EmptyFolderContents(temp_internet_file);
			EmptyFolderContents(dllchache);
			}
			catch
			{
    			
			}
		}
		
		private void EmptyFolderContents(string folderName)
        {
            foreach (var folder in Directory.GetDirectories(folderName))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception excep)
                {
                    System.Diagnostics.Debug.WriteLine(excep);
                }
            }
            foreach (var file in Directory.GetFiles(folderName))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception excep)
                {
                    System.Diagnostics.Debug.WriteLine(excep);
                }
            }
        }




        //Notify Icon and It's Contex Menu Methood ////////////////////////////////////////////
        public void Contex_Menu_And_Notify_Icon()
        {
            //Initalize the context menu strip
            m_contextMenu = new System.Windows.Forms.ContextMenuStrip();
            System.Windows.Forms.ToolStripMenuItem mI1 = new System.Windows.Forms.ToolStripMenuItem();
            mI1.Text = "Main window";
            mI1.Click += new EventHandler(m_notifyIcon_DoubleClick); //Add Click Handler
            m_contextMenu.Items.Add(mI1);

            System.Windows.Forms.ToolStripMenuItem mI2 = new System.Windows.Forms.ToolStripMenuItem();
            mI2.Text = "Update";
            mI2.Click += new EventHandler(contexUpdate);
            m_contextMenu.Items.Add(mI2);

            System.Windows.Forms.ToolStripMenuItem mI3 = new System.Windows.Forms.ToolStripMenuItem();
            mI3.Text = "About";
            mI3.Click += new EventHandler(contexAbout);
            m_contextMenu.Items.Add(mI3);

            System.Windows.Forms.ToolStripMenuItem mI4 = new System.Windows.Forms.ToolStripMenuItem();
            mI4.Text = "Exit";
            mI4.Click += new EventHandler(contexExit);
            m_contextMenu.Items.Add(mI4);


            //Initalize Notify Icon
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "USB Disk Protector";
            m_notifyIcon.Icon = new System.Drawing.Icon("Application.ico");
            m_notifyIcon.ShowBalloonTip(3000, "USB Disk Protector", "USB Disk Protector, protecting your computer.", ToolTipIcon.Info);
            m_notifyIcon.ContextMenuStrip = m_contextMenu; //Associate the contextmenustrip with notify icon
            m_notifyIcon.Visible = true;
            m_notifyIcon.DoubleClick += new EventHandler(m_notifyIcon_DoubleClick);

        }

        private void m_notifyIcon_DoubleClick(object sender, EventArgs args)
        {
            this.Show(); //this.Topmost = true;
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private void contexExit(object sender, EventArgs args)
        {
            this.Close();
        }

        private void contexUpdate(object sender, EventArgs args)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.facebook.com/pages/Software-Art/124544400963976");
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Sorry, link doesn't found.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void contexAbout(object sender, EventArgs args)
        {
            this.Show();
            this.Dashboard.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.AboutPanel.Visibility = System.Windows.Visibility.Visible;

            this.SupportSubPanel.Visibility = System.Windows.Visibility.Visible;
            this.SettingsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.DeveloperSubPanel.Visibility = System.Windows.Visibility.Hidden;

        }

        //###################################################################################///



        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            /*
            C1ThemeExpressionDark theme = new C1ThemeExpressionDark();
            //Using Merged Dictionaries
            Application.Current.Resources.MergedDictionaries.Add(C1Theme.GetCurrentThemeResources(theme));
            */

            CheckInstance();
        }

        //////// Drag window anywhere you can.////////////

        private void LayoutRoot_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //////// Startup music function//////////////////

        public void StartingMusic()
        {
            try
            {
                _sound.SoundLocation = "Resources\\startup.wav";
                _sound.PlaySync();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("File missing!", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        //////// Only one application run at a time/////////////////////////////// 

        private static void CheckInstance()
        {
            Process[] thisnameprocesslist;
            string modulename, processname;
            Process p = Process.GetCurrentProcess();
            modulename = p.MainModule.ModuleName.ToString();
            processname = System.IO.Path.GetFileNameWithoutExtension(modulename);
            thisnameprocesslist = Process.GetProcessesByName(processname);
            if (thisnameprocesslist.Length > 1)
            {
                System.Windows.MessageBox.Show("Instance of this application is already running.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Stop);
                System.Windows.Application.Current.Shutdown();
            }
        }

        ////////For Clock function//////////////////////////////////////// 

        public void Clock()
        {

            var timer = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                this.lableClock.Content = DateTime.Now.ToString("hh:mm:ss");
                this.ampm.Text = DateTime.Now.ToString("tt");	//("HH:mm:ss tt"); //for AM PM HH=24 hours
                this.dateLabel.Text = "" + DateTime.Now.ToLongDateString();
            }, this.Dispatcher);
        }

        ///////// Get all Drive information function////////////////////////// 

        public void dinfo()
        {
            try
            {
                _DriveLetterCombo1.Items.Clear();
				DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        string ko = d.VolumeLabel;
                        string dt = System.Convert.ToString(d.DriveType);
                        // _DriveLetterCombo.Items.Add(d.Name.Remove (2));
                        _DriveLetterCombo1.Items.Add(d.Name.Remove (2));
                    }

                }
            }
            catch { System.Windows.MessageBox.Show("Error Fetching Drive Info", "Error"); }
        }


        //Reset Attribute////////////////////////////////////////////////
        public void attributReset()
        {
            try
            {
                for (int i = 0; i < _DriveLetterCombo.Items.Count; i++)
                {
                    string a = "" + _DriveLetterCombo.Items[i].ToString() + @"\";
                    //Create A Batch File
                    StreamWriter w_r;
                    w_r = File.CreateText(@"AttributeReset.bat");
                    w_r.WriteLine(@"attrib -h -a -s -r " + a + "*.* /s /d ");
                    w_r.Close();

                    //Run Batch File
                    System.Diagnostics.Process Proc1 = new System.Diagnostics.Process();
                    Proc1.StartInfo.FileName = @"AttributeReset.bat";
                    Proc1.StartInfo.UseShellExecute = false;
                    Proc1.StartInfo.CreateNoWindow = true;
                    Proc1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    Proc1.Start();
                    Proc1.WaitForExit();
                    // Delete Batch File
                    File.Delete(@"AttributeReset.bat");
                    // MessageBox.Show("Convertion Complete.", "USB Protector", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch { }

        }

        // convet size to MB, GB/////////////////////////////////////////
        public static string ConvertSizeToString(long Length)
        {
            if (Length <= 0)
                return "0 B";

            float nSize;
            string strSizeFmt, strUnit = "";

            if (Length < 1000)             // 1KB
            {
                nSize = Length;
                strUnit = " B";
            }
            else if (Length < 1000000)     // 1MB
            {
                nSize = Length / (float)0x400;
                strUnit = " KB";
            }
            else if (Length < 1000000000)   // 1GB
            {
                nSize = Length / (float)0x100000;
                strUnit = " MB";
            }
            else
            {
                nSize = Length / (float)0x40000000;
                strUnit = " GB";
            }

            if (nSize == (int)nSize)
                strSizeFmt = nSize.ToString("0");
            else if (nSize < 10)
                strSizeFmt = nSize.ToString("0.00");
            else if (nSize < 100)
                strSizeFmt = nSize.ToString("0.0");
            else
                strSizeFmt = nSize.ToString("0");

            return strSizeFmt + strUnit;
        }



        //Encryption method////////////////////////////////////////////////
        private void EncryptFile(string inputFile, string outputFile)
        {

            try
            {
                string password = @"myKey123"; // Your Key Here
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                string cryptFile = @"Quarantine\" + outputFile;
                FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateEncryptor(key, key),
                    CryptoStreamMode.Write);

                FileStream fsIn = new FileStream(inputFile, FileMode.Open);

                int data;
                while ((data = fsIn.ReadByte()) != -1)
                    cs.WriteByte((byte)data);


                fsIn.Close();
                cs.Close();
                fsCrypt.Close();
            }
            catch
            {
                System.Windows.MessageBox.Show("Encryption failed!", "Error");
            }
        }


        //Decryption method ////////////////////////////////////////
        private void DecryptFile(string inputFile, string outputFile)
        {

            {
                string password = @"myKey123"; // Your Key Here

                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);

                FileStream fsCrypt = new FileStream(inputFile, FileMode.Open);

                RijndaelManaged RMCrypto = new RijndaelManaged();

                CryptoStream cs = new CryptoStream(fsCrypt,
                    RMCrypto.CreateDecryptor(key, key),
                    CryptoStreamMode.Read);

                FileStream fsOut = new FileStream(outputFile, FileMode.Create);

                int data;
                while ((data = cs.ReadByte()) != -1)
                    fsOut.WriteByte((byte)data);

                fsOut.Close();
                cs.Close();
                fsCrypt.Close();

            }
        }

	//Restart Explorer Method ////////////////////////////////////////////
		
		public void RestartExplorer()
		{
		try
			{
			foreach(System.Diagnostics.Process myProcess in System.Diagnostics.Process.GetProcessesByName("explorer"))
				{
					myProcess.Kill();
					myProcess.Start();
				}
			}
			catch(Exception){}
		}
			
		
		public void ExeVirusScan()
		{
		try
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
				this.dataGrid1.Items.Clear();
				int aa= Convert.ToInt32(_filesize_txt.Text);
				ReportFiles list = new ReportFiles();
               
                 //progressBar1.Value = 0;

                string fileName, filePath, fileSize; 
                string [] virusExtention = {".exe",".sys",".dll",".chm"};

                for (int i = 0; i < _DriveLetterCombo.Items.Count; i++)
                {
                    string a = @"" + _DriveLetterCombo.Items[i].ToString() + @"\";
                    foreach (string virusFile in Directory.GetFiles(@"" + a, "*.*", System.IO.SearchOption.TopDirectoryOnly).OrderByDescending(f => new FileInfo(f).Length).Reverse().Where(s => virusExtention.Contains(System.IO.Path.GetExtension(s).ToLower())))
                    {
						
                        FileInfo fileInfo = new FileInfo(virusFile);
                        if (fileInfo.Length < (aa * 1024))//numericUpDown1.Value*1024))
                        {
                            fileName = fileInfo.Name;
                            fileSize = ConvertSizeToString(fileInfo.Length);
                            filePath = fileInfo.FullName;

                            //System.Windows.Forms.ListViewItem listViewItem = new System.Windows.Forms.ListViewItem(new string[] { fileName, fileSize, filePath });
                            //System.Windows.Controls.ListViewItem listViewItem = new System.Windows.Controls.ListViewItem();
                            //listViewItem.IsSelected = true;

                            viruses += 1;

                            // progressBar1.Increment(1);

                            list.FileName = fileName;
                            list.Size = fileSize;
                            list.Path = filePath;
                            dataGrid1.Items.Add(new { FileName = list.FileName, Size = list.Size, Location = list.Path });

                            // listView1.Items.Add(new { FileName = fileName, FileSize = fileSize, FilePath = filePath });
                            // listView1.DataContext = fileInfo.Name;

                        }
                    }
                }

                //progressBar1.Value = 1000; 
               // label1.Text = "Found: ";
               // label1.Text += dataGrid1.Items.Count.ToString();
				this.Cursor = System.Windows.Input.Cursors.Arrow;
               
            }
            catch { }
		
		
		}
	

		
		
		
		
		public void VirusScan()
		{
		
		try
            {
                
				if (_ck5_st.IsChecked == true)
            	{
				
            	this.Cursor = System.Windows.Input.Cursors.Wait;
				//this.dataGrid1.Items.Clear();
				int aa= Convert.ToInt32(_filesize_txt.Text);
				ReportFiles list = new ReportFiles();
               
                 //progressBar1.Value = 0;

                string fileName, filePath, fileSize; 
                string [] virusExtention = {".tmp",".386",".application",".aru",".atm",".aut",".bat",".bin",".bkd",".blf",".bll",".bmw",".boo",".bqf",".buk",".bxz",".cc",".ce0",".ceo",".cfxxe",".cih",".cla",".cmd",".com",".csh",".cpl",".cxq",".cyw",".docm",".dotm",".dbd",".dev",".dlb",".dli",".dllx",".dll",".dom",".drv",".dx",".dxz",".dyz",".dyv",".eml",".exe",".exe1",".exe_renamed",".ezt",".fag",".fjl",".fnr",".fuj",".fini",".hip",".hlw",".hsq",".hts",".hta",".inf",".inx",".isu",".init",".iva",".iws",".job",".jse",".kcd",".let",".lik",".lkh",".lnk",".lok",".mfu",".mjz",".msi",".msp",".msc",".msh",".msh1",".msh2",".mshxml",".msh1xml",".msh2xml",".nls",".oar",".ocx",".osa",".ozd",".paf",".pcx",".pgm",".php2",".php3",".pif","pf",".pid",".plc",".pr",".ps1",".ps1xml",".ps2",".ps2xml",".psc1",".psc2",".pptm",".potm",".ppam",".ppsm",".qit",".rhk",".rna",".rgs",".reg",".rsc_tmp",".s7p",".scr",".shs",".ska",".smm",".smtmp",".sop",".spam",".ssy",".sys",".swf",".scf",".sct",".shb",".sldm",".tif",".tko",".tps",".tsa",".tti",".txs",".upa",".uzy",".vb",".vba",".vbe",".vbs",".vbscript",".vbx",".vexe",".vsd",".vzr",".wlpginstall",".wmf",".ws",".wsc",".wsf",".wsh",".wss",".xir",".xlm",".xlv",".xnt",".xlsm",".xltm",".xlam",".zix",".zvz"};

                for (int i = 0; i < _DriveLetterCombo.Items.Count; i++)
                {
                    string a = @"" + _DriveLetterCombo.Items[i].ToString() + @"\";
                    foreach (string virusFile in Directory.GetFiles(@"" + a, "*.*", System.IO.SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).Length).Reverse().Where(s => virusExtention.Contains(System.IO.Path.GetExtension(s).ToLower())))
                    {
						
                        FileInfo fileInfo = new FileInfo(virusFile);
                        if (fileInfo.Length < (aa * 1024))
                        {
                            fileName = fileInfo.Name;
                            fileSize = ConvertSizeToString(fileInfo.Length);
                            filePath = fileInfo.FullName;

                         
                            viruses += 1;

                            // progressBar1.Increment(1);

                            list.FileName = fileName;
                            list.Size = fileSize;
                            list.Path = filePath;
                            dataGrid1.Items.Add(new { FileName = list.FileName, Size = list.Size, Location = list.Path });

                          

                        }
                    }
                }

                //progressBar1.Value = 1000; 
                label1.Text = "Found: ";
                label1.Text += dataGrid1.Items.Count.ToString();
				this.Cursor = System.Windows.Input.Cursors.Arrow;
               
				}
            	
				else
            	{
				
				//string [] virusExtention = {".tmp",".386",".application",".aru",".atm",".aut",".bat",".bin",".bkd",".blf",".bll",".bmw",".boo",".bqf",".buk",".bxz",".cc",".ce0",".ceo",".cfxxe",".cih",".cla",".cmd",".com",".csh",".cpl",".cxq",".cyw",".docm",".dotm",".dbd",".dev",".dlb",".dli",".dllx",".dom",".drv",".dx",".dxz",".dyz",".dyv",".eml",".exe1",".exe_renamed",".ezt",".fag",".fjl",".fnr",".fuj",".fini",".hip",".hlw",".hsq",".hts",".hta",".ini",".inf",".inx",".isu",".init",".iva",".iws",".job",".jse",".kcd",".let",".lik",".lkh",".lnk",".lok",".mfu",".mjz",".msp",".msc",".msh",".msh1",".msh2",".mshxml",".msh1xml",".msh2xml",".nls",".oar",".ocx",".osa",".ozd",".paf",".pcx",".pgm",".php2",".php3",".pif","pf",".pid",".plc",".pr",".ps1",".ps1xml",".ps2",".ps2xml",".psc1",".psc2",".pptm",".potm",".ppam",".ppsm",".qit",".rhk",".rna",".rgs",".reg",".rsc_tmp",".s7p",".scr",".shs",".ska",".smm",".smtmp",".sop",".spam",".ssy",".swf",".scf",".sct",".shb",".sldm",".tif",".tko",".tps",".tsa",".tti",".txs",".upa",".uzy",".vb",".vba",".vbe",".vbs",".vbscript",".vbx",".vexe",".vsd",".vzr",".wlpginstall",".wmf",".ws",".wsc",".wsf",".wsh",".wss",".xir",".xlm",".xlv",".xnt",".xlsm",".xltm",".xlam",".zix",".zvz"};
            	
				this.Cursor = System.Windows.Input.Cursors.Wait;
				//this.dataGrid1.Items.Clear();
				int aa= Convert.ToInt32(_filesize_txt.Text);
				ReportFiles list = new ReportFiles();
               
                 //progressBar1.Value = 0;

                string fileName, filePath, fileSize; 
                string [] virusExtention = {".tmp",".386",".application",".aru",".atm",".aut",".bat",".bin",".bkd",".blf",".bll",".bmw",".boo",".bqf",".buk",".bxz",".cc",".ce0",".ceo",".cfxxe",".cih",".cla",".cmd",".com",".csh",".cpl",".cxq",".cyw",".docm",".dotm",".dbd",".dev",".dlb",".dli",".dllx",".dom",".drv",".dx",".dxz",".dyz",".dyv",".eml",".exe1",".exe_renamed",".ezt",".fag",".fjl",".fnr",".fuj",".fini",".hip",".hlw",".hsq",".hts",".hta",".inf",".inx",".isu",".init",".iva",".iws",".job",".jse",".kcd",".let",".lik",".lkh",".lnk",".lok",".mfu",".mjz",".msi",".msp",".msc",".msh",".msh1",".msh2",".mshxml",".msh1xml",".msh2xml",".nls",".oar",".ocx",".osa",".ozd",".paf",".pcx",".pgm",".php2",".php3",".pif","pf",".pid",".plc",".pr",".ps1",".ps1xml",".ps2",".ps2xml",".psc1",".psc2",".pptm",".potm",".ppam",".ppsm",".qit",".rhk",".rna",".rgs",".reg",".rsc_tmp",".s7p",".scr",".shs",".ska",".smm",".smtmp",".sop",".spam",".ssy",".swf",".scf",".sct",".shb",".sldm",".tif",".tko",".tps",".tsa",".tti",".txs",".upa",".uzy",".vb",".vba",".vbe",".vbs",".vbscript",".vbx",".vexe",".vsd",".vzr",".wlpginstall",".wmf",".ws",".wsc",".wsf",".wsh",".wss",".xir",".xlm",".xlv",".xnt",".xlsm",".xltm",".xlam",".zix",".zvz"};

                for (int i = 0; i < _DriveLetterCombo.Items.Count; i++)
                {
                    string a = @"" + _DriveLetterCombo.Items[i].ToString() + @"\";
                    foreach (string virusFile in Directory.GetFiles(@"" + a, "*.*", System.IO.SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).Length).Reverse().Where(s => virusExtention.Contains(System.IO.Path.GetExtension(s).ToLower())))
                    {
						
                        FileInfo fileInfo = new FileInfo(virusFile);
                        if (fileInfo.Length < (aa * 1024))//numericUpDown1.Value*1024))
                        {
                            fileName = fileInfo.Name;
                            fileSize = ConvertSizeToString(fileInfo.Length);
                            filePath = fileInfo.FullName;

                            //System.Windows.Forms.ListViewItem listViewItem = new System.Windows.Forms.ListViewItem(new string[] { fileName, fileSize, filePath });
                            //System.Windows.Controls.ListViewItem listViewItem = new System.Windows.Controls.ListViewItem();
                            //listViewItem.IsSelected = true;

                            viruses += 1;

                            // progressBar1.Increment(1);

                            list.FileName = fileName;
                            list.Size = fileSize;
                            list.Path = filePath;
                            dataGrid1.Items.Add(new { FileName = list.FileName, Size = list.Size, Location = list.Path });

                            // listView1.Items.Add(new { FileName = fileName, FileSize = fileSize, FilePath = filePath });
                            // listView1.DataContext = fileInfo.Name;

                        }
                    }
                }

                //progressBar1.Value = 1000; 
                label1.Text = "Found: ";
                label1.Text += dataGrid1.Items.Count.ToString();
				this.Cursor = System.Windows.Input.Cursors.Arrow;
				}
			
				
				
            }
            catch { }
			
		
		}

		public void RemoveableDiskVaccination()
		{
		
			try{
				DirectorySecurity addSecurityRules = new DirectorySecurity();
                DirectorySecurity removeSecurityRules = new DirectorySecurity();

                addSecurityRules.AddAccessRule(new FileSystemAccessRule(@"Everyone", FileSystemRights.Delete, AccessControlType.Deny));
                removeSecurityRules.AddAccessRule(new FileSystemAccessRule(@"Everyone", FileSystemRights.FullControl, AccessControlType.Allow));


				for (int i = 0; i <= _DriveLetterCombo.Items.Count; i++)
                    {
                        try
                        {
                            System.IO.DirectoryInfo dinfo = new DirectoryInfo(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\Autorun.inf");
							System.IO.DirectoryInfo dinfo1 = new DirectoryInfo(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\Secure Data Folder");
                            System.IO.FileInfo finfo = new FileInfo(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\autorun.inf");
							System.IO.FileInfo finfo1 = new FileInfo(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\Secure Data Folder\\desktop.ini");
                           
							if (!dinfo.Exists || finfo.Exists || !dinfo1.Exists) //checks if the directory already exists or not  
                            {
                                finfo.Delete();
                                dinfo.Create();
								dinfo1.Create();

                                //USB protector.text file writing method under autorun.inf folder
                                StreamWriter w_r;
                                w_r = File.CreateText(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\Autorun.inf\\USB Protector.txt");
                                w_r.WriteLine("//////////////////////////////DONT WORRY//////////////////////////");
                                w_r.WriteLine("This is a protection created by the 'Software-Art USB Protector' to prevent Autorun.inf viruses.");
                                w_r.Close();

                                dinfo.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
                                dinfo.SetAccessControl(addSecurityRules);
								
								w_r = File.CreateText(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\Secure Data Folder\\desktop.ini");
                                w_r.WriteLine(@"[.ShellClassInfo]");
                                w_r.WriteLine(@"IconResource=C:\Windows\system32\SHELL32.dll,7");
                                w_r.Close();
								dinfo1.Attributes = FileAttributes.ReadOnly;
								finfo1.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;

                            }
                            else
                            {
                               
                                dinfo.SetAccessControl(removeSecurityRules);
                                dinfo.Attributes &= ~FileAttributes.System & ~FileAttributes.Hidden & ~FileAttributes.ReadOnly;                                
                                dinfo.Delete(true);                             
                                dinfo.Create();
                                StreamWriter w_r;
                                w_r = File.CreateText(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\autorun.inf\\USB Protector.txt");
                                w_r.WriteLine("//////////////////////////////DONT WORRY//////////////////////////");
                                w_r.WriteLine("This is a protection created by the 'Software-Art USB Protector' to prevent Autorun.inf viruses.");
                                w_r.Close();

                                dinfo.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
                                dinfo.SetAccessControl(addSecurityRules);
								
								
								finfo1.Delete();
								w_r = File.CreateText(@"" + _DriveLetterCombo.Items.GetItemAt(i) + "\\Secure Data Folder\\desktop.ini");
                                w_r.WriteLine(@"[.ShellClassInfo]");
                                w_r.WriteLine(@"IconResource=C:\Windows\system32\SHELL32.dll,7");
                                w_r.Close();
								dinfo1.Attributes = FileAttributes.ReadOnly;
								finfo1.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
								dinfo1.Attributes = FileAttributes.ReadOnly;
                                

                            }

                        }
                        catch { }
		
					}
			}
			
			catch{}
		
		
		
		}



        ///////// Software-Art official webpage///////////////////////////

        private void Software_art_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.facebook.com/pages/Software-Art/124544400963976");
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Sorry, link doesn't found.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        ////////// Software-Art official facebook page /////////////////////////////

        private void Facebook_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.facebook.com/hasib.cse.pstu");
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("Sorry, link doesn't found.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        ////////// Minimize Button function////////////////////////////////////////

        private void minimize_button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            Hide();
            //this.m_notifyIcon.Icon = new Icon(@"Application.ico");
            //this.m_notifyIcon.Visible = true;
            this.m_notifyIcon.ShowBalloonTip(2000, "USB Disk Protector", "USB Disk Protector, protecting your computer.", ToolTipIcon.Info);
        }


        ////////// Exit button music function////////////////////////////////////

        private void Exit_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                this.Close();
				 _voice.SpeakAsync("USB Disk Protector is shutting down.");
                _sound.SoundLocation = "Resources\\Exit.wav";
                _sound.PlaySync();
            }
            catch (Exception)
            {
                System.Windows.MessageBox.Show("File missing!", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }


        // ////////////////////// Load Default Value /////////////////////////

        public void LoadDeafult() 
        {
            this._ck1.IsChecked = Properties.Settings.Default.ck1;
            this._ck2.IsChecked = Properties.Settings.Default.ck2;
            this._ck3.IsChecked = Properties.Settings.Default.ck3;
            this._ck4.IsChecked = Properties.Settings.Default.ck4;
            this._ck5.IsChecked = Properties.Settings.Default.ck5;
            this._ck6.IsChecked = Properties.Settings.Default.ck6;
            this._ck7.IsChecked = Properties.Settings.Default.ck7;
            this._ck8.IsChecked = Properties.Settings.Default.ck8;
            this._ck9.IsChecked = Properties.Settings.Default.ck9;
            this._ck10.IsChecked = Properties.Settings.Default.ck10;
            this._ck11.IsChecked = Properties.Settings.Default.ck11;
            this._ck12.IsChecked = Properties.Settings.Default.ck12;
            this._ck13.IsChecked = Properties.Settings.Default.ck13;
            this._ck14.IsChecked = Properties.Settings.Default.ck14;
            this._ck15.IsChecked = Properties.Settings.Default.ck15;
            this._ck17.IsChecked = Properties.Settings.Default.ck17;
            this._ck18.IsChecked = Properties.Settings.Default.ck18;
            this._ck19.IsChecked = Properties.Settings.Default.ck19;
            this._ck20.IsChecked = Properties.Settings.Default.ck20;
            this._ck21.IsChecked = Properties.Settings.Default.ck21;
            this._ck22.IsChecked = Properties.Settings.Default.ck22;

            //settings Tab checkbox
            this._ck1_st.IsChecked = Properties.Settings.Default.ck1_st;
            this._ck2_st.IsChecked = Properties.Settings.Default.ck2_st;
            this._ck3_st.IsChecked = Properties.Settings.Default.ck3_st;
            this._ck4_st.IsChecked = Properties.Settings.Default.ck4_st;
            this._ck5_st.IsChecked = Properties.Settings.Default.ck5_st;
            this._ck6_st.IsChecked = Properties.Settings.Default.ck6_st;

            this.UsbLockButton.IsChecked = Properties.Settings.Default.usbLock;
            this.WriteProtectionButton.IsChecked = Properties.Settings.Default.writeProtection;
            this.VaccinationButton.IsChecked = Properties.Settings.Default.driveVaccination;
     
        }

    
        private void ScanButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dashboard.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Visible;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.AboutPanel.Visibility = System.Windows.Visibility.Hidden;

            this.ScanSubPanel.Visibility=System.Windows.Visibility.Visible;
			this.QuarantainSubPanel.Visibility=System.Windows.Visibility.Hidden;
			this.Fotter.Visibility = System.Windows.Visibility.Hidden;
            this.Fotter_Copy.Visibility = System.Windows.Visibility.Hidden;

        }

        private void ProtectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dashboard.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Visible;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.AboutPanel.Visibility = System.Windows.Visibility.Hidden;

            this.SystemTweakSubPanel.Visibility = System.Windows.Visibility.Hidden;

            this.Fotter_Copy.Visibility = System.Windows.Visibility.Visible;
            this.Fotter.Visibility = System.Windows.Visibility.Hidden;
        }

        private void ToolsButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dashboard.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Visible;
            this.AboutPanel.Visibility = System.Windows.Visibility.Hidden;

            this.Fotter.Visibility = System.Windows.Visibility.Hidden;
            this.Fotter_Copy.Visibility = System.Windows.Visibility.Visible;

            FormatToolsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            ErrorChekerSubPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void AboutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dashboard.Visibility = System.Windows.Visibility.Hidden;
            this.ScanPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.AboutPanel.Visibility = System.Windows.Visibility.Visible;

            this.SupportSubPanel.Visibility = System.Windows.Visibility.Visible;
            this.SettingsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.DeveloperSubPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.Dashboard.Visibility = System.Windows.Visibility.Visible;
            this.ScanPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ToolsPanel.Visibility = System.Windows.Visibility.Hidden;
            this.AboutPanel.Visibility = System.Windows.Visibility.Hidden;

            this.Fotter_Copy.Visibility = System.Windows.Visibility.Hidden;
            this.Fotter.Visibility = System.Windows.Visibility.Visible;
        }

        private void SupportTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SupportSubPanel.Visibility = System.Windows.Visibility.Visible;
            this.SettingsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.DeveloperSubPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SettingsTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SupportSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.SettingsSubPanel.Visibility = System.Windows.Visibility.Visible;
            this.DeveloperSubPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void DeveloperTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SupportSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.SettingsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.DeveloperSubPanel.Visibility = System.Windows.Visibility.Visible;
        }
		
		private void quarantineTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	this.ScanSubPanel.Visibility=System.Windows.Visibility.Hidden;
			this.QuarantainSubPanel.Visibility=System.Windows.Visibility.Visible;
			
			
			try
            {
                ReportFiles list = new ReportFiles();
               
                this.dataGrid2.Items.Clear(); 
				
                string fileName, filePath, fileSize; int viruses = 0;
                string virusExtention = "*.qur";

               
                    string a = @"Quarantine\";
                    foreach (string virusFile in Directory.GetFiles(@"" + a, "*.*", System.IO.SearchOption.AllDirectories).OrderByDescending(f => new FileInfo(f).Length).Reverse().Where(s => virusExtention.Contains(System.IO.Path.GetExtension(s).ToLower())))
                    {

                        FileInfo fileInfo = new FileInfo(virusFile);
                        if (fileInfo.Length < (1000 * 1024))//numericUpDown1.Value*1024))
                        {

                            fileName = fileInfo.Name;
                            //fileSize = ConvertSizeToString(fileInfo.Length);
							fileSize = File.ReadAllText(@""+virusFile).Trim();
                            filePath = fileInfo.FullName;
							//filePath =  File.ReadAllText(@""+virusFile);

                            viruses += 1;

                            list.FileName = fileName;
                            list.Size = fileSize;
                            list.Path = filePath;
                            dataGrid2.Items.Add(new { FileName = list.FileName, Size = list.Size, Location = list.Path });

                           
                        }
                    }
             

                //progressBar1.Value = 1000; 
                label2.Text = "Found: ";
                label2.Text += dataGrid2.Items.Count.ToString();

            }
            catch { }

        }

		private void ScanTab_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	
        	this.ScanSubPanel.Visibility=System.Windows.Visibility.Visible;
			this.QuarantainSubPanel.Visibility=System.Windows.Visibility.Hidden;
        }
       
		
		/////////////////////////////////// System Tools////////////////////////////////////////////////////////////////



       
		
		///////////////////////////// Details Tools Tips //////////////////////////////////////////


        private void DiskCleaner_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "USB Disk Protector";
            _detailsLabel.Content = "All rights are reserved by Software-Art © 2013-14";
        }


        private void DiskCleaner_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Disk Cleaner";
            _detailsLabel.Content = "Delete unnecessary files, malicious programs from your system and" + "\nkeep your system fast and smooth. ";
        }


        private void ErrorCheker_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Disk Error Checker";
            _detailsLabel.Content = "It is usefull utility to check your drives for filesystem error, bad sector" + "\nand repair your drives from any disk error.   ";
        }

        private void Bitlocker_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Bitlocker Encryption";
            _detailsLabel.Content = "Protect your files and folders form unauthorized access by protecting" + "\nyour drives with BitLocker Encryption system (Doesn't work in xp).";
        }

        private void FormatTools_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Format Tools";
            _detailsLabel.Content = "It is a powerfull tools for format any drive as you can." + "\nImportant: Be carefull, all data will be lost of this drive.";
        }

        private void DiskFresher_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Disk Fresher";
            _detailsLabel.Content = "Refresh your hole Hardisk, USB disk and others removable media." + "\nIt help's for faster disk access.";
        }

        private void DiskOptimizer_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Disk Optimizer";
            _detailsLabel.Content = "Optimizing your computer's drives can help it run more efficiently.";
        }

        private void TaskManager_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Task Manager";
            _detailsLabel.Content = "Managing your system's all proccess, services and others.";
        }

        private void StartupManager_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Startup Manager";
            _detailsLabel.Content = "Manage list of application that are executed during windows startup.";
        }


        // /////////////////////////////////////////////////////////////////////////////////////



        private void DiskOptimizer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("dfrgui.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ErrorCheker_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ErrorChekerSubPanel.Visibility = System.Windows.Visibility.Visible;
            SystemToolsSubPanel.Visibility = System.Windows.Visibility.Hidden;

        }

        private void DiskFresher_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("Resources\\tree.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartupManager_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("msconfig.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskManager_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("taskmgr.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Bitlocker_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("Control.exe", "/name Microsoft.BitLockerDriveEncryption");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DiskCleaner_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            try
            {
                
				QuickClean();
                System.Windows.MessageBox.Show("Removing junk file from your disk succesfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, Disk cleaner does't perform.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


           /* try
            {
                System.Diagnostics.Process.Start("cleanmgr.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
        }

        private void FormatTools_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SystemToolsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            ErrorChekerSubPanel.Visibility = System.Windows.Visibility.Hidden;
            FormatToolsSubPanel.Visibility = System.Windows.Visibility.Visible;

        }

        private void FormatToolsBackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            FormatToolsSubPanel.Visibility = System.Windows.Visibility.Hidden;
            ErrorChekerSubPanel.Visibility = System.Windows.Visibility.Hidden;
            SystemToolsSubPanel.Visibility = System.Windows.Visibility.Visible;

        }

        private void _DriveLetterCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            try
            {
                //int _drivesize=0;_drivesize=Convert.ToInt32(_TotalSizeLabel.Text.Remove(4));
                //if (_drivesize<=4096){_FileSystemCombo.Items.Insert(5,"FAT");_FileSystemCombo.Items.Refresh();}
                //else{_FileSystemCombo.Items.Remove("FAT");_FileSystemCombo.Items.Refresh(); }


                DriveInfo[] allDrives = DriveInfo.GetDrives(); _DriveLetterCombo.Items.Refresh();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        if (_DriveLetterCombo.Text == d.Name.Remove(2))
                        {
                            _DriveNameLabel.Text = "" + d.VolumeLabel;
                            _FormatTypeLabel.Text = "" + d.DriveFormat;
                            _TotalSizeLabel.Text = "" + d.TotalSize / (1024 * 1024) + " MB";
                            _AvailableLabel.Text = "" + d.AvailableFreeSpace / (1024 * 1024) + " MB";
                            _DriveTypeLabel.Text = "" + d.DriveType;

                        }
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, no information found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void _FormatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
           
			if (System.Windows.MessageBox.Show("Formatting will erase all data on this disk. To Format the disk click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
    				_voice.SpeakAsync("Format operation is starting, please wait untile confirmation.");
            		this.Cursor = System.Windows.Input.Cursors.Wait;
            		string _format_option;
            		if (_FormatCombo.SelectedIndex == 0) { _format_option = "/q"; } else { _format_option = ""; }

            		if (_ConvertCombo.SelectedIndex == -1) { System.Windows.MessageBox.Show("Please select a drive first.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Information);this.Cursor = System.Windows.Input.Cursors.Arrow; }

            		else
            		{
                		try
                		{

							//Create A Batch File
							StreamWriter w_r;
							w_r = File.CreateText(@"FormatDrive.bat");
							w_r.WriteLine("format /y " + _DriveLetterCombo.SelectedItem + " /fs:" + _FileSystemCombo.SelectionBoxItem + " /v:" + _VolumeLabelTextbox.Text + " " + _format_option);
							w_r.Close();
		
							//Run Batch File
							System.Diagnostics.Process Proc1 = new System.Diagnostics.Process();
							Proc1.StartInfo.FileName = @"FormatDrive.bat";
							Proc1.StartInfo.UseShellExecute = false;
							Proc1.StartInfo.CreateNoWindow = true;
							Proc1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							Proc1.Start();
							Proc1.WaitForExit();
							//Delete Batch File
							File.Delete(@"FormatDrive.bat");
							_voice.SpeakAsync("Format operation successfuly Complete");
							System.Windows.MessageBox.Show("Format operation successfuly Complete.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Information);
							this.Cursor = System.Windows.Input.Cursors.Arrow;
						}
						catch
						{
							_voice.SpeakAsync("Format operation faild.");
							System.Windows.MessageBox.Show("Format operation does not conplete", "Faild", MessageBoxButton.OK, MessageBoxImage.Error);
						}

            		}
					
			}
			
			else
			{
   					 // Do not close the window
			} 
	
			
        }

        private void _ConvertButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			if (System.Windows.MessageBox.Show("Do you want to convert your removeable disk ? To convert the disk click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			
				_voice.SpeakAsync("Convert operation is starting, please wait untile confirmation");
				this.Cursor = System.Windows.Input.Cursors.Wait;
				if (_ConvertCombo.SelectedIndex == 0) { System.Windows.MessageBox.Show("Please select a drive and convertion option first.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Information); this.Cursor = System.Windows.Input.Cursors.Arrow;}
	
				else if (_ConvertCombo.SelectedIndex == 1)
				{
	
					try
					{
	
						//Create A Batch File
						StreamWriter w_r;
						w_r = File.CreateText(@"ConvertDrive.bat");
						w_r.WriteLine("convert " + _DriveLetterCombo.SelectedItem + " /fs:ntfs /x /nosecurity");
						w_r.Close();
	
						//Run Batch File
						System.Diagnostics.Process Proc1 = new System.Diagnostics.Process();
						Proc1.StartInfo.FileName = @"ConvertDrive.bat";
						Proc1.StartInfo.UseShellExecute = false;
						Proc1.StartInfo.CreateNoWindow = true;
						Proc1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						Proc1.Start();
						Proc1.WaitForExit();
						//Delete Batch File
						File.Delete(@"ConvertDrive.bat");
						_voice.SpeakAsync("Convert operation successfuly Complete.");
						System.Windows.MessageBox.Show("Convertion Complete.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Information);
						this.Cursor = System.Windows.Input.Cursors.Arrow;
					}
	
					catch
					{
						_voice.SpeakAsync("Convert operation Faild.");
						System.Windows.MessageBox.Show("Convertion does not successful.", "Faild", MessageBoxButton.OK, MessageBoxImage.Error);
						this.Cursor = System.Windows.Input.Cursors.Arrow;
					}
				}
				
				else if (_ConvertCombo.SelectedIndex == 2)
				{
					try
					{
	
						//Create A Batch File
						StreamWriter w_r;
						w_r = File.CreateText(@"ConvertDrive.bat");
						w_r.WriteLine("convert " + _DriveLetterCombo.SelectedItem + " /fs:fat32 /x /nosecurity");
						w_r.Close();
	
						//Run Batch File
						System.Diagnostics.Process Proc1 = new System.Diagnostics.Process();
						Proc1.StartInfo.FileName = @"ConvertDrive.bat";
						Proc1.StartInfo.UseShellExecute = false;
						Proc1.StartInfo.CreateNoWindow = true;
						Proc1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
						Proc1.Start();
						Proc1.WaitForExit();
						//Delete Batch File
						File.Delete(@"ConvertDrive.bat");
						_voice.SpeakAsync("Convert operation successfuly Complete.");
						System.Windows.MessageBox.Show("Convertion Complete.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Information);
						this.Cursor = System.Windows.Input.Cursors.Arrow;
					}
	
					catch
					{
					    _voice.SpeakAsync("Convert operation Faild.");
						System.Windows.MessageBox.Show("Convertion does not successful.", "Faild", MessageBoxButton.OK, MessageBoxImage.Error);
						this.Cursor = System.Windows.Input.Cursors.Arrow;
					}
				}
			}
			
			else
			{
   					 // Do not close the window
			} 
			
        }


        private void _DriveLetterCombo1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {

                DriveInfo[] allDrives = DriveInfo.GetDrives(); _DriveLetterCombo1.Items.Refresh();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.IsReady == true)
                    {
                        if (_DriveLetterCombo1.Text == d.Name.Remove(2))
                        {
                            _DriveNameLabel1.Text = "" + d.VolumeLabel;
                            _FormatTypeLabel1.Text = "" + d.DriveFormat;
                            _TotalSizeLabel1.Text = "" + d.TotalSize / (1024 * 1024) + " MB";
                            _AvailableLabel1.Text = "" + d.AvailableFreeSpace / (1024 * 1024) + " MB";
                            _DriveTypeLabel1.Text = "" + d.DriveType;

                        }
                    }
                }
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, drive information does not found.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void _CheckButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _voice.SpeakAsync("Disk check is now running, please wait untile confirmation.");
            this.Cursor = System.Windows.Input.Cursors.Wait;
            string _RepairBadSector;
            if (_RepairBadSector_CheckBox.IsChecked == true) { _RepairBadSector = "/r"; } else { _RepairBadSector = ""; }


            try
            {

                //Create A Batch File
                StreamWriter w_r;
                w_r = File.CreateText(@"CheckDrive.bat");
                w_r.WriteLine("chkdsk " + _DriveLetterCombo1.SelectedItem + " /f /x " + _RepairBadSector);
                w_r.Close();

                //Run Batch File
                System.Diagnostics.Process Proc1 = new System.Diagnostics.Process();
                Proc1.StartInfo.FileName = @"CheckDrive.bat";
                //Proc1.StartInfo.UseShellExecute = false;
                //Proc1.StartInfo.CreateNoWindow = true;
                //Proc1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Proc1.Start();
                Proc1.WaitForExit();
                //Delete Batch File
                File.Delete(@"CheckDrive.bat");
                _voice.SpeakAsync("Disk error check process complete.");
                System.Windows.MessageBox.Show(" Disk check Complete.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
            catch
            {
                _voice.SpeakAsync("Disk error check process Faild.");
                System.Windows.MessageBox.Show("Disk checking operation does not conplete", "Faild", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        //////////////// Protect ///////////////////////////////////////////////////////////////////////////////////////////////




        /////////////////////////// Details Tools Tips //////////////////////////////////////////


        private void UsbLockButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "USB Lock";
            _detailsLabel.Content = "Prevent unauthorized persons from connecting USB drives to your" + "\ncomputer, and stop any threats from USB drives. ";
        }

        private void UsbLockButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "USB Disk Protector";
            _detailsLabel.Content = "All rights are reserved by Software-Art © 2013-14";
        }

        private void WriteProtectionButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "USB write protection";
            _detailsLabel.Content = "Prevent unauthorized persons from copying your data to USB drives.";
        }

        private void VaccinationButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Vaccination";
            _detailsLabel.Content = "Disable autorun feature on your PC and USB drives to avoid" + "\nsome malware infections. ";

        }

        private void SystemTweakButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "System Tweak";
            _detailsLabel.Content = "This is most comprehensive tool that you can easily optimize," + "\ncustomize and enhance your Windows.";

        }


        private void RepairSystemButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Repair System";
            _detailsLabel.Content = "Restore and repair your system from malicious changes produced" + "\nby malware. ";

        }


        private void ProcessExplorerButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _headerLabel.Content = "Process Explorer";
            _detailsLabel.Content = "A full feature advance third party Taskmanager application by " + "\nMark Russinovich.";

        }




        /////////////////////////// Details Tools Tips //////////////////////////////////////////




        private void UsbLockButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UsbLockButton.IsChecked == true)
            {
                //disable USB storage...
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UsbStor", "Start", 4, Microsoft.Win32.RegistryValueKind.DWord);
                _headerLabel.Content = "USB Lock disable";
                _detailsLabel.Content = "You must restart your computer for take this effect.";
                _voice.SpeakAsync("USB port lock enable.");

            }
            else
            {
                //enable USB storage...
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\UsbStor", "Start", 3, Microsoft.Win32.RegistryValueKind.DWord);
                _headerLabel.Content = "USB Lock enable";
                _detailsLabel.Content = "You must restart your computer for take this effect.";
                _voice.SpeakAsync("USB port lock disable.");

            }
			RestartExplorer();	
            Properties.Settings.Default.usbLock = UsbLockButton.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void WriteProtectionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (WriteProtectionButton.IsChecked == true)
            {
                
				// Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\StorageDevicePolicies", "WriteProtect", 1, Microsoft.Win32.RegistryValueKind.DWord);
				
				RegistryKey key = Registry.LocalMachine.OpenSubKey
             	("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
                if (key == null)
                {
                    Registry.LocalMachine.CreateSubKey
                    ("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", RegistryKeyPermissionCheck.ReadWriteSubTree);
                    key = Registry.LocalMachine.OpenSubKey
                    ("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
                    key.SetValue("WriteProtect", 1, RegistryValueKind.DWord);
                }
                else if (key.GetValue("WriteProtect") != (object)(1))
                {
                    key.SetValue("WriteProtect", 1, RegistryValueKind.DWord);
                }


                _headerLabel.Content = "Write protection enable";
                _detailsLabel.Content = "You must restart your computer for take this effect.";
                _voice.SpeakAsync("USB write protection enabled, please unplug your usb drive and plug it again for take effect.");

            }
            else
            {
                // Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\StorageDevicePolicies", "WriteProtect", 0, Microsoft.Win32.RegistryValueKind.DWord);
				
				RegistryKey key = Registry.LocalMachine.OpenSubKey
              	("SYSTEM\\CurrentControlSet\\Control\\StorageDevicePolicies", true);
                if (key != null)
                {
                    key.SetValue("WriteProtect", 0, RegistryValueKind.DWord);
                }
                key.Close();

                _headerLabel.Content = "Write protection disable";
                _detailsLabel.Content = "You must restart your computer for take this effect.";
                _voice.SpeakAsync("USB write protection disabled, please unplug your usb drive and plug it again for take effect.");

            }
			RestartExplorer();
            Properties.Settings.Default.writeProtection = WriteProtectionButton.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        public static void AddDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);

            // current security settings of this directory.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.AddAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }




        public static void RemoveDirectorySecurity(string FileName, string Account, FileSystemRights Rights, AccessControlType ControlType)
        {
            // Create a new DirectoryInfo object.
            DirectoryInfo dInfo = new DirectoryInfo(FileName);


            // current security settings othis directory.
            DirectorySecurity dSecurity = dInfo.GetAccessControl();

            // Add the FileSystemAccessRule to the security settings. 
            dSecurity.RemoveAccessRule(new FileSystemAccessRule(Account, Rights, ControlType));

            // Set the new access settings.
            dInfo.SetAccessControl(dSecurity);

        }


        private void VaccinationButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            /*try{
            DirectorySecurity addSecurityRules = new DirectorySecurity();
            DirectorySecurity removeSecurityRules = new DirectorySecurity();
            
            addSecurityRules.AddAccessRule(new FileSystemAccessRule(@"Everyone", FileSystemRights.Delete, AccessControlType.Deny));
            removeSecurityRules.AddAccessRule(new FileSystemAccessRule(@"Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                System.IO.DirectoryInfo dinfo = new DirectoryInfo(@"F:\hasib"); 
                if(!dinfo.Exists)
                {
                dinfo.Create();
                    StreamWriter w_r;
                                w_r = File.CreateText(@"F:\hasib\USB Protector.txt");
                                w_r.WriteLine("//////////////////////////////DONT WORRY//////////////////////////");
                                w_r.WriteLine("This is a protection created by the 'Software-Art USB Protector' to prevent Autorun.inf viruses.");
                                w_r.Close();
					
                    dinfo.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
                    dinfo.SetAccessControl(addSecurityRules);
					
                }
                else
                {
                    dinfo.SetAccessControl(removeSecurityRules);
                    //Directory.SetAccessControl(@"F:\hasib",removeSecurityRules);
                    dinfo.Attributes &= ~FileAttributes.System & ~FileAttributes.Hidden & ~FileAttributes.ReadOnly;
                    Directory.Delete(@"F:\hasib",true);}
            }
            catch{}*/


            try
            {
               
                DirectorySecurity addSecurityRules = new DirectorySecurity();
                DirectorySecurity removeSecurityRules = new DirectorySecurity();

                addSecurityRules.AddAccessRule(new FileSystemAccessRule(@"Everyone", FileSystemRights.Delete, AccessControlType.Deny));
                removeSecurityRules.AddAccessRule(new FileSystemAccessRule(@"Everyone", FileSystemRights.FullControl, AccessControlType.Allow));


                if (VaccinationButton.IsChecked == true)
                {

                    for (int i = 0; i <= _DriveLetterCombo1.Items.Count; i++)
                    {
                        try
                        {
                            System.IO.DirectoryInfo dinfo = new DirectoryInfo(@"" + _DriveLetterCombo1.Items.GetItemAt(i) + "\\Autorun.inf");
                            System.IO.FileInfo finfo = new FileInfo(@"" + _DriveLetterCombo1.Items.GetItemAt(i) + "\\autorun.inf");
                            if (!dinfo.Exists || finfo.Exists) //checks if the directory already exists or not  
                            {
                                finfo.Delete();
                                dinfo.Create();

                                //USB protector.text file writing method under autorun.inf folder
                                StreamWriter w_r;
                                w_r = File.CreateText(@"" + _DriveLetterCombo1.Items.GetItemAt(i) + "\\Autorun.inf\\USB Protector.txt");
                                w_r.WriteLine("//////////////////////////////DONT WORRY//////////////////////////");
                                w_r.WriteLine("This is a protection created by the 'Software-Art USB Protector' to prevent Autorun.inf viruses.");
                                w_r.Close();

                                dinfo.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;

                                dinfo.SetAccessControl(addSecurityRules);

                                //AddDirectorySecurity(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf", @"Everyone", FileSystemRights.Delete, AccessControlType.Deny);

                            }
                            else
                            {
                                //Directory.SetAccessControl(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf",removeSecurityRules);
                                dinfo.SetAccessControl(removeSecurityRules);
                                dinfo.Attributes &= ~FileAttributes.System & ~FileAttributes.Hidden & ~FileAttributes.ReadOnly;
                                //Directory.Delete(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf",true);
                                dinfo.Delete(true);
                                //Directory.CreateDirectory(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf", addSecurityRules);
                                dinfo.Create();
                                StreamWriter w_r;
                                w_r = File.CreateText(@"" + _DriveLetterCombo1.Items.GetItemAt(i) + "\\autorun.inf\\USB Protector.txt");
                                w_r.WriteLine("//////////////////////////////DONT WORRY//////////////////////////");
                                w_r.WriteLine("This is a protection created by the 'Software-Art USB Protector' to prevent Autorun.inf viruses.");
                                w_r.Close();

                                dinfo.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
                                //Directory.SetAccessControl(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf",addSecurityRules);
                                dinfo.SetAccessControl(addSecurityRules);
                                //AddDirectorySecurity(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf", @"Everyone", FileSystemRights.Delete, AccessControlType.Deny);

                            }

                        }
                        catch { }
                    }

                    _headerLabel.Content = "Vaccinated";
                    _detailsLabel.Content = "All drives are vaccinated from autorun.inf viruses";
                    _voice.SpeakAsync("Drive vaccination enable.");

                }

                else
                {
                    for (int i = 0; i <= _DriveLetterCombo1.Items.Count; i++)
                    {
                        try
                        {
                            System.IO.DirectoryInfo dinfo = new DirectoryInfo(@"" + _DriveLetterCombo1.Items.GetItemAt(i) + "\\Autorun.inf");
                            if (dinfo.Exists)
                            {

                                //RemoveDirectorySecurity(@""+_DriveLetterCombo1.Items.GetItemAt(i)+"\\Autorun.inf", @"Everyone", FileSystemRights.FullControl, AccessControlType.Allow);
                                dinfo.SetAccessControl(removeSecurityRules);
                                dinfo.Attributes &= ~FileAttributes.ReadOnly & ~FileAttributes.System & ~FileAttributes.Hidden;
                                dinfo.Delete(true); //delete autorun folder and it's all component

                            }

                        }
                        catch { }


                    }

                    _headerLabel.Content = "Disable Vaccination";
                    _detailsLabel.Content = "Your computer may affected by harmfull autorun.inf viruses.";
                    _voice.SpeakAsync("Drive vaccination disable.");

                }
                
                
                Properties.Settings.Default.driveVaccination = VaccinationButton.IsChecked.Value;
                Properties.Settings.Default.Save();
            }
            catch { }

        }

        private void SystemTweakButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SystemTweakSubPanel.Visibility = System.Windows.Visibility.Visible;
            this.ProtectSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectSubPanel_Copy1.Visibility = System.Windows.Visibility.Hidden;
        }

        private void SystemTweakBackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.SystemTweakSubPanel.Visibility = System.Windows.Visibility.Hidden;
            this.ProtectSubPanel.Visibility = System.Windows.Visibility.Visible;
            this.ProtectSubPanel_Copy1.Visibility = System.Windows.Visibility.Hidden;


            Properties.Settings.Default.ck1 = _ck1.IsChecked.Value;
            Properties.Settings.Default.ck2 = _ck2.IsChecked.Value;
            Properties.Settings.Default.ck3 = _ck3.IsChecked.Value;
            Properties.Settings.Default.ck4 = _ck4.IsChecked.Value;
            Properties.Settings.Default.ck5 = _ck5.IsChecked.Value;
            Properties.Settings.Default.ck6 = _ck6.IsChecked.Value;
            Properties.Settings.Default.ck7 = _ck7.IsChecked.Value;
            Properties.Settings.Default.ck8 = _ck8.IsChecked.Value;
            Properties.Settings.Default.ck9 = _ck9.IsChecked.Value;
            Properties.Settings.Default.ck10 = _ck10.IsChecked.Value;
            Properties.Settings.Default.ck11 = _ck11.IsChecked.Value;
            Properties.Settings.Default.ck12 = _ck12.IsChecked.Value;
            Properties.Settings.Default.ck13 = _ck13.IsChecked.Value;
            Properties.Settings.Default.ck14 = _ck14.IsChecked.Value;
            Properties.Settings.Default.ck15 = _ck15.IsChecked.Value;
            Properties.Settings.Default.ck17 = _ck17.IsChecked.Value;
            Properties.Settings.Default.ck18 = _ck18.IsChecked.Value; 
            Properties.Settings.Default.ck19= _ck19.IsChecked.Value;
            Properties.Settings.Default.ck20 = _ck20.IsChecked.Value;
            Properties.Settings.Default.ck21= _ck21.IsChecked.Value;
            Properties.Settings.Default.ck22 = _ck22.IsChecked.Value;
            Properties.Settings.Default.Save();


        }


        private void RepairSystemButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
           
			try
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableTaskMgr", 0, Microsoft.Win32.RegistryValueKind.DWord);                //ck1=F
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableRegistryTools", 0, Microsoft.Win32.RegistryValueKind.DWord);          //ck2=F 
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoControlPanel", 0, Microsoft.Win32.RegistryValueKind.DWord);              //ck4=F
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 1, Microsoft.Win32.RegistryValueKind.DWord);          //ck6=T
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, Microsoft.Win32.RegistryValueKind.DWord);                 //ck9=T
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun", 0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoViewContextMenu", 0, Microsoft.Win32.RegistryValueKind.DWord);            
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoFind",0, Microsoft.Win32.RegistryValueKind.DWord);
				
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoDesktopCleanupWizard",1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoDesktop",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoLogOff",0, Microsoft.Win32.RegistryValueKind.DWord);
				
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoRecentDocsMenu",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoRecentDocsHistory",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","ClearRecentDocsOnExit",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoInstrumentation",0, Microsoft.Win32.RegistryValueKind.DWord);
				
				
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies", "NoLowDiskSpaceChecks", 0, Microsoft.Win32.RegistryValueKind.DWord);    
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "MaxConnectionsPerServer", 12, Microsoft.Win32.RegistryValueKind.DWord);            
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "MaxConnectionsPer1_0Server", 12, Microsoft.Win32.RegistryValueKind.DWord);            
				
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop","LowLevelHooksTimeout",5000, Microsoft.Win32.RegistryValueKind.DWord);				
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop","MenuShowDelay",400, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop","WaitToKillAppTimeout",1000, Microsoft.Win32.RegistryValueKind.DWord);
				//Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop","HungAppTimeout",10000, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop","AutoEndTasks",1, Microsoft.Win32.RegistryValueKind.DWord);
				
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\ActiveDesktop","NoChangingWallpaper",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\System", "DisableCMD", 0, Microsoft.Win32.RegistryValueKind.DWord);                                   //ck3=F
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\MMC\{8FC0B734-A0E1-11D1-A7D3-0000F87571E3}","Restrict_Run",0, Microsoft.Win32.RegistryValueKind.DWord); //enable gpedit
				
				
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoFolderOptions", 0, Microsoft.Win32.RegistryValueKind.DWord);            //ck8=F
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1, Microsoft.Win32.RegistryValueKind.DWord);                                //ck11=F
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoClose", 0, Microsoft.Win32.RegistryValueKind.DWord);                    //ck12=F
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "VDMDisallowed", 1, Microsoft.Win32.RegistryValueKind.DWord);                 	
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AlwaysUnloadDLL", "Default", 0, Microsoft.Win32.RegistryValueKind.DWord);            
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management","ClearPageFileAtShutdown",0, Microsoft.Win32.RegistryValueKind.DWord);//ck14=F
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\srservice", "Start", 2, Microsoft.Win32.RegistryValueKind.DWord);                    					//ck17=F
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableConfig", 0, Microsoft.Win32.RegistryValueKind.DWord);				
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU", "NoAutoRebootWithLoggedOnUsers", 1, Microsoft.Win32.RegistryValueKind.DWord);      //ck19=F
                
				//Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Psched","NonBestEfforLimit", 0, Microsoft.Win32.RegistryValueKind.DWord);                     		 //ck20=F
                
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\LanManServer\Parameters", "AutoShareWks", 0, Microsoft.Win32.RegistryValueKind.DWord);                  //ck21=T
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\WindowsMediaPlayer", "DisableAutoUpdate", 1, Microsoft.Win32.RegistryValueKind.DWord);            
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\LanmanServer\Parameters", "AutoShareServer", 0, Microsoft.Win32.RegistryValueKind.DWord);            				
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Uninstall","NoAddRemovePrograms",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\CrashControl","AutoReboot",1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control","WaitToKillServiceTimeout",1000, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management","DisablePagingExecutive",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Memory Management","LargeSystemCache",0, Microsoft.Win32.RegistryValueKind.DWord);											
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer","NoPropertiesMyComputer",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\Explorer","NoDrives",0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\RRamdisk\Parameters","UsePAE",1, Microsoft.Win32.RegistryValueKind.DWord);				
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile","EnableFirewall",1, Microsoft.Win32.RegistryValueKind.DWord);				
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Terminal Server","fDenyTSConnections",1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\ Control\ SessionManager", "ProtectionMode ",1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System","AllowBlockingAppsAtShutdown",0, Microsoft.Win32.RegistryValueKind.DWord);
				
				
				// shortcut icon fixer for future work
				
				RestartExplorer();
				_voice.SpeakAsync("System repair has been compeleted");
            }
            catch
            {
				System.Windows.MessageBox.Show("Sorry, application could not perform this operation.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Error);
            }  
        }

        private void ProcessExplorerButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("Resources\\procexp.exe");
            }
            catch
            {
                System.Windows.MessageBox.Show("Sorry, this application could not found.", "USB Disk Protector", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        // scan tab/////////////////////////////////////////////////////////////////////



        private void _virusScanButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
          try
			{
			_voice.SpeakAsync("Usb disk detected, please wait untile scan complete."); 
				
				
				//before check write protection (must be apply)
				
			attributReset();
			ExeVirusScan();
			VirusScan();
			RemoveableDiskVaccination();
		    _voice.SpeakAsync("Scan Compelete.");
			if (viruses!=0){_voice.SpeakAsync("Suspicious file has been detected.");}else{_voice.SpeakAsync("No threat detected.");}
			}
			catch{}
			
			
			
        }

        private void _virusDeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            /*System.Windows.Controls.ListView.CheckedListViewItemCollection breakfast = this.listView1.CheckedItems;      
            foreach (System.Windows.Forms.ListViewItem item in breakfast)
            {
                string a = item.SubItems[2].Text; System.Windows.MessageBox.Show("Are you sure delete this file:  " + a);  
                File.Delete(a);           
            }
            
            _virusScanButton.Click+=_virusScanButton_Click;*/

            // ReportFiles list = new ReportFiles();
            /*  foreach (ReportFiles reportFile in dataGrid1.SelectedItems) 
              {
                  string a =reportFile.Path;System.Windows.MessageBox.Show("Are you sure delete this file:  " + a);  
                  File.Delete(a);
            
              }*/

            /* foreach (DataGridRow row in dataGrid1.SelectedItems)
             {
                 System.Data.DataRow MyRow = (System.Data.DataRow)row.Item;
                 string value = MyRow[2].ToString();
               
                 System.Windows.MessageBox.Show("Are you sure delete this file:  " + value);
                 File.Delete(value);
             }*/


            /* if (dataGrid1.SelectedItems.Count > 0)
             {
                 for (int i = 0; i < dataGrid1.SelectedItems.Count; i++)
                 {
                     System.Data.DataRowView selectedFile = (System.Data.DataRowView)dataGrid1.SelectedItems[i];
                     string str = Convert.ToString(selectedFile.Row.ItemArray[2]);
                     System.Windows.MessageBox.Show("Are you sure delete this file:  " + str);
                 }
             }*/
            /* ReportFiles rep = new ReportFiles();
             DataRowView paths = (System.Data.DataRowView)dataGrid1.SelectedItems;
             rep.FileName = Convert.ToString(paths.Row.ItemArray[1]);
             //MessageBox.Show("Are you sure delete this file:  " + rep.FileName);*/


            /* for (int i = 0; i < dataGrid1.SelectedItems.Count; i++)
             {

                 IList rows = dataGrid1.SelectedItems;
                 //DataRowView row = (DataRowView)dataGrid1.SelectedItems[0];
                 string val = rows.ToString();
                 System.Windows.MessageBox.Show("Are you sure delete this file:  " + val);
             }*/


            /*  for (int i = 0; i < _DriveLetterCombo.Items.Count; i++)
              {
                 string a= ""+_DriveLetterCombo.Items[i].ToString() ;
                 System.Windows.MessageBox.Show("Are you sure delete this file:  " + a);
              }*/

			
			if (System.Windows.MessageBox.Show("Do you want to delete the suspecious file ? To delete the suspecious file click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			
			try
			{
            foreach (var hasib in dataGrid1.SelectedItems)
            {
                // object item = dataGrid1.SelectedItem;
                string ID = (dataGrid1.SelectedCells[2].Column.GetCellContent(hasib) as TextBlock).Text;
                //System.Windows.MessageBox.Show("Are you sure delete this file:  " + ID);
                File.Delete(ID);
            }
				
			}
			catch
			{//VirusScan();
				}
			
			ExeVirusScan();
			VirusScan();
			}
			else{}
        }

		
		 private void _forceDeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	
			if (System.Windows.MessageBox.Show("Do you want to delete the suspicious file ? To delete the suspicious file click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			try
			{
			foreach(System.Diagnostics.Process myProcess in System.Diagnostics.Process.GetProcessesByName("explorer"))
				{
					myProcess.Kill();
					//myProcess.Start();
					
				}
				
			foreach (var hasib in dataGrid1.SelectedItems)
            {
                // object item = dataGrid1.SelectedItem;
                string ID = (dataGrid1.SelectedCells[2].Column.GetCellContent(hasib) as TextBlock).Text;
                // System.Windows.MessageBox.Show("Are you sure delete this file:  " + ID);
				//System.Windows.MessageBox.Show("Are you sure delete these file.", "USB Protector", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                File.Delete(ID);
				
            }
			_voice.SpeakAsync("All Suspicious file has been deleted.");
	
			}
			
			catch
			{//VirusScan();
			}
			
			ExeVirusScan();
			VirusScan();
			}
			else {}
	
        }

		

		
		
        private void _quarantineButton_Click(object sender, RoutedEventArgs e)
        {
             /* try
              {
                  System.Windows.Controls.ListView.CheckedListViewItemCollection breakfast = this.listView1.CheckedItems;
                  foreach (ListViewItem item in breakfast)
                  {
                      string a = item.SubItems[3].Text;
                      FileInfo f = new FileInfo(a);
                      //a.Replace(Path.GetExtension(a), ".qur");
                      //f.MoveTo(Path.ChangeExtension(a, ".qur"));//this line acctive for file change it's own directory
                      string quarantineFile = @"Quarantine\" + f.Name + ".qur";
                      //f.MoveTo(@"D:\Hasib\"+f.Name+".jkl");//renames extention files move to this directory
                      string encryptFile = @"" + f.Name + ".qur";
                      //f.MoveTo(quarantineFile);

                      EncryptFile(a, encryptFile);
                      f.Delete();
                      //File.WriteAllText(@"D:\Hasib\log.txt",""+quarantineFile);
                      TextWriter tsw = new StreamWriter(@"Quarantine\log.txt", true);
                      tsw.WriteLine(quarantineFile);
                      tsw.Close();
                      MessageBox.Show("Suspicious file quarantine succesfully.");
                  }
              }
              catch { System.Windows.MessageBox.Show("Error to quarantine."); }*/
        
			
			
			if (System.Windows.MessageBox.Show("Do you want to quarantine the suspicious file ? To quarantine the suspicious file click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			
			try
              {
                  
                  foreach (var item in dataGrid1.SelectedItems)
                  {
                      string a = (dataGrid1.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text;
                      FileInfo f = new FileInfo(a);
                      //a.Replace(Path.GetExtension(a), ".qur");
                      //f.MoveTo(Path.ChangeExtension(a, ".qur"));//this line acctive for file change it's own directory
                      string quarantineFile = @"Quarantine\" + f.Name + ".qurt";
                      //f.MoveTo(@"D:\Hasib\"+f.Name+".jkl");//renames extention files move to this directory
                      string encryptFile = @"" + f.Name + ".qurt";
                      //f.MoveTo(quarantineFile);

                      EncryptFile(a, encryptFile);
                      f.Delete();
                      //File.WriteAllText(@"D:\Hasib\log.txt",""+quarantineFile);
                      //TextWriter tsw = new StreamWriter(@"Quarantine\log.txt", true);
                      //tsw.WriteLine(quarantineFile);
					
					TextWriter tsw = new StreamWriter(@"Quarantine\"+""+f.Name+".qur", true);
					tsw.WriteLine(a);					
                    tsw.Close();
                     
                  }
				 System.Windows.MessageBox.Show("Suspicious file quarantine succesfully.");
				
              }
              catch { System.Windows.MessageBox.Show("Error to quarantine."); }	
			
			ExeVirusScan();
			VirusScan();
			}
			else{}
		
		}



        // system tweak////////////////////////////////////////////////////////////////////////////////////////////////////////
       
		private void _ck1_Click(object sender, RoutedEventArgs e)
        {

            if (_ck1.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableTaskMgr", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableTaskMgr", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck2_Click(object sender, RoutedEventArgs e)
        {
            if (_ck2.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableRegistryTools", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System", "DisableRegistryTools", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck3_Click(object sender, RoutedEventArgs e)
        {
            if (_ck3.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\System", "DisableCMD", 2, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\System", "DisableCMD", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck4_Click(object sender, RoutedEventArgs e)
        {
            if (_ck4.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoControlPanel", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoControlPanel", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck5_Click(object sender, RoutedEventArgs e)
        {
            if (_ck5.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRun", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck6_Click(object sender, RoutedEventArgs e)
        {
            if (_ck6.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck7_Click(object sender, RoutedEventArgs e)
        {
            if (_ck7.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck8_Click(object sender, RoutedEventArgs e)
        {
            if (_ck8.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoFolderOptions", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoFolderOptions", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck9_Click(object sender, RoutedEventArgs e)
        {
            if (_ck9.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRecentDocsHistory", 1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoInstrumentation", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoRecentDocsHistory", 0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoInstrumentation", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck10_Click(object sender, RoutedEventArgs e)
        {

            if (_ck10.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoWindowsUpdate", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoWindowsUpdate", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }

        }

        private void _ck11_Click(object sender, RoutedEventArgs e)
        {
            if (_ck11.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 1, Microsoft.Win32.RegistryValueKind.DWord);
				
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck12_Click(object sender, RoutedEventArgs e)
        {
            if (_ck12.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "Persistent", 0, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\Curre ntVersion\Policies\Explorer", "ClearRecentDocsOnExit", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Cache\", "Persistent", 1, Microsoft.Win32.RegistryValueKind.DWord);
				Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "ClearRecentDocsOnExit", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck13_Click(object sender, RoutedEventArgs e)
        {
            if (_ck13.IsChecked == true)
            {
               Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System","AllowBlockingAppsAtShutdown",0, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\System","AllowBlockingAppsAtShutdown",1, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck14_Click(object sender, RoutedEventArgs e)
        {
            if (_ck14.IsChecked == true)
            {
               Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management","ClearPageFileAtShutdown",1, Microsoft.Win32.RegistryValueKind.DWord);
				
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management","ClearPageFileAtShutdown",0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck15_Click(object sender, RoutedEventArgs e)
        {

            if (_ck15.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AlwaysUnloadDLL", "Default", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\AlwaysUnloadDLL", "Default", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }

        }

        private void _ck17_Click(object sender, RoutedEventArgs e)
        {
            if (_ck17.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableConfig", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows NT\SystemRestore", "DisableConfig", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck18_Click(object sender, RoutedEventArgs e)
        {
            if (_ck17.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "VDMDisallowed", 1, Microsoft.Win32.RegistryValueKind.DWord);                 	
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\AppCompat", "VDMDisallowed", 0, Microsoft.Win32.RegistryValueKind.DWord);                 	
            }
        }

        private void _ck19_Click(object sender, RoutedEventArgs e)
        {
            if (_ck17.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\CrashControl","AutoReboot",1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\CrashControl","AutoReboot",0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void _ck20_Click(object sender, RoutedEventArgs e)
        {
            if (_ck20.IsChecked == true)
            {
               Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Psched","NonBestEfforLimit", 0, Microsoft.Win32.RegistryValueKind.DWord);  
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\Psched","NonBestEfforLimit", 1, Microsoft.Win32.RegistryValueKind.DWord);  
            }
        }

        private void _ck21_Click(object sender, RoutedEventArgs e)
        {
            if (_ck21.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\LanmanServer\Parameters", "AutoShareServer", 0, Microsoft.Win32.RegistryValueKind.DWord);            
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\LanmanServer\Parameters", "AutoShareServer", 1, Microsoft.Win32.RegistryValueKind.DWord);            
            }
        }

        private void _ck22_Click(object sender, RoutedEventArgs e)
        {
            if (_ck22.IsChecked == true)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Class\{36FC9E60-C465-11CF-8056-444553540000}\0000", "IdleEnable", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Class\{36FC9E60-C465-11CF-8056-444553540000}\0000", "IdleEnable", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }
     
        
		// /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		
		//Quarantine and restore
		
		private void _quarantineDeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if (System.Windows.MessageBox.Show("Do you want to delete the quarantine file ? To delete the file click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			try
			{ 
			foreach (var hasib in dataGrid2.SelectedItems)
            {
               // dataGrid2.SelectAll();
                string ID1 = (dataGrid2.SelectedCells[2].Column.GetCellContent(hasib) as TextBlock).Text;
				string ID2 = ""+(dataGrid2.SelectedCells[2].Column.GetCellContent(hasib) as TextBlock).Text+"t";
               // System.Windows.MessageBox.Show("Are you sure you want to delete this file:  " + ID1);
                File.Delete(ID1);File.Delete(ID2);
            }
			quarantineTab_Click(sender,new RoutedEventArgs());
			}
			catch(Exception)
			{
				System.Windows.MessageBox.Show("Please select file which you want to delete.");
			}
			}
			else {}
        }

        private void _quarantineDeleteAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if (System.Windows.MessageBox.Show("Do you want to delete all quarantined file ? To delete these files click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			try
			{
			foreach (var hasib in dataGrid2.Items)
            {
                //object item = dataGrid2.SelectAll;
                string ID1 = (dataGrid2.SelectedCells[2].Column.GetCellContent(hasib) as TextBlock).Text;
				string ID2 = ""+(dataGrid2.SelectedCells[2].Column.GetCellContent(hasib) as TextBlock).Text+"t";
                //System.Windows.MessageBox.Show("Are you sure delete this file:  " + ID);
                File.Delete(ID1);File.Delete(ID2);
				
            }
			quarantineTab_Click(sender,new RoutedEventArgs());
			System.Windows.MessageBox.Show("Qurantaine is now empty.");
			}
			catch(Exception)
			{
			System.Windows.MessageBox.Show("Please select all files.");
			}
			}
			else {}
        }

        private void _restoreButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	if (System.Windows.MessageBox.Show("Do you want to restore the quarantine file original location? To restore the file click OK ","Confirmation", MessageBoxButton.OKCancel,MessageBoxImage.Warning) == MessageBoxResult.OK)
				
			{
			
			try
              {
                  
                  foreach (var item in dataGrid2.SelectedItems)
                  {
                    string output = (dataGrid2.SelectedCells[1].Column.GetCellContent(item) as TextBlock).Text;
					string input = ""+(dataGrid2.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text+"t";  
					string ID = (dataGrid2.SelectedCells[2].Column.GetCellContent(item) as TextBlock).Text;  
					FileInfo f = new FileInfo(input);
                      
                    
                      DecryptFile(input, output);
                      f.Delete();File.Delete(ID);
                      
					
                     
                  }
				quarantineTab_Click(sender,new RoutedEventArgs());
				 System.Windows.MessageBox.Show("File restore to original location succesfully.");
				
              }
              catch { System.Windows.MessageBox.Show("Restore process faild."); }	
			}
			else{}
	
			
        }

           
        //#################  settings subpanel settings Tab ####################################################################################
		
        private void _ck1_st_Click(object sender, RoutedEventArgs e)
        {
            if (_ck1_st.IsChecked == true)
            {
                registryKey.SetValue("USB Protector", System.Windows.Forms.Application.ExecutablePath);
            }
            else
            {
               registryKey.DeleteValue("USB Protector");
            }
            
            Properties.Settings.Default.ck1_st = _ck1_st.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void _ck2_st_Click(object sender, RoutedEventArgs e)
        {
            if (_ck2_st.IsChecked == true)
            {
				 RemoveableDiskVaccination();
            }
            else
            {

            }

            Properties.Settings.Default.ck2_st = _ck2_st.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void _ck3_st_Click(object sender, RoutedEventArgs e)
        {
            if (_ck3_st.IsChecked == true)
            {
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 1, Microsoft.Win32.RegistryValueKind.DWord);
            }
            else
            {
				Microsoft.Win32.Registry.SetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoDriveTypeAutoRun", 0, Microsoft.Win32.RegistryValueKind.DWord);
            }

            Properties.Settings.Default.ck3_st = _ck3_st.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void _ck4_st_Click(object sender, RoutedEventArgs e)
        {
            if (_ck4_st.IsChecked == true)
            {
				this.Topmost=true;
            }
            else
            {
				this.Topmost=false;
            }

            Properties.Settings.Default.ck4_st = _ck4_st.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void _ck5_st_Click(object sender, RoutedEventArgs e)
        {
            if (_ck5_st.IsChecked == true)
            {

            }
            else
            {

            }

            Properties.Settings.Default.ck5_st = _ck5_st.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

        private void _ck6_st_Click(object sender, RoutedEventArgs e)
        {
            if (_ck6_st.IsChecked == true)
            {
				 DirectoryInfo source = new DirectoryInfo(@"Quarantine\");

				// Get info of each file into the directory
				foreach (FileInfo fi in source.GetFiles())
				{
					var creationTime = fi.CreationTime;
			
					if(creationTime < (DateTime.Now- new TimeSpan(5, 0, 0, 0)))
					{
						fi.Delete();
					}
				}
				
				//OR
				
				/*string[] files = Directory.GetFiles("C:/temp");

				foreach (string file in files)
				{
				FileInfo fi = new FileInfo(file);
				if (fi.CreationTime < DateTime.Now.AddMonths(-3))//auto delete after 3 month 
					fi.Delete();
				}*/
            }
            else
            {

            }

            Properties.Settings.Default.ck6_st = _ck6_st.IsChecked.Value;
            Properties.Settings.Default.Save();
        }

		
		private void _applyEffectButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	RestartExplorer();
        }

        private void _ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	Process.Start("shutdown", "/r /t 0"); // the argument /r is to restart the computer
        }      
       
    }
}