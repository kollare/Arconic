using Microsoft.Win32;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using Arconic_Budget_Simulator.Properties;

namespace Arconic_Budget_Simulator
{

    public partial class Form1 : Form
    {
        //System.Windows.Forms.Timer timer;

        DataTable csvTable = new DataTable();
        //File Rfile = new File();

        int iterations = 10000;
        string outputLocation = @"./";
        string distributionType = "uniform"; // optional choice: triangular
        string userInputCSV = string.Empty;
        string userInputScript = string.Empty;//@"C:\Users\kollaea\Documents\R\R-3.5.0\bin\MonteCarloBudgetSimulation.R";
        string rPath = string.Empty;
        string resourceDir = string.Empty;
        string RworkingDirectory = Environment.UserName + @"\Documents";
        Image bmp = Resources.RStudio;
        object logo = Resources.ResourceManager.GetObject("arconic_logo.ico");
        //var bmp = new Bitmap(Arconic_Budget_Simulator.Properties.Resources.arconic_logo.ico);

        public Form1()
        {
            //setScreenSize();
            InitializeComponent();
            csvTable = null;
            toolStripButton1.Enabled = false;
            //executeToolStripMenuItem.Enabled = false;
            setScreenSize();
            pictureBox1.Image = bmp;

            //Console.WriteLine(RworkingDirectory);

            //this.Controls.Add(pictureBox1);

            //rPathFinder();
            //resourcesRelativePathFinder("budget.png");

            //simpleFileMove(resourceDir + "MonteCarloBudgetSimulation.R", rPath + "MonteCarloBudgetSimulation.R");


            //ResourceManager rm = Budget_Simulator.Properties.Resources.ResourceManager;
            //resourceDir = (string)rm.GetObject("Budget_Simulator.png");
            //resourceDir = Path.GetFullPath("budget.png");
            //resourceDir = 
            //Console.WriteLine("resourceDir: " + resourceDir);
            //Bitmap budgetPic = (Bitmap)rm.GetObject("budget.png");

            //timer = new System.Windows.Forms.Timer();
            //timer.Interval = 1000;
            //timer.Tick += (sender, args) => {
            //pictureBox1.Height = splitContainer1.Panel2.Height - 100;
            //pictureBox1.Width = splitContainer1.Panel2.Width - 200;
            //};
            //timer.Start();
        }

        //private void InitializeComponent()
        //{
            //throw new NotImplementedException();
        //}

        private string RpathFinder()
        {
            // Get path to R
            var rCore = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core") ??
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\R-core");
            var is64Bit = Environment.Is64BitProcess;
            if (rCore != null)
            {

                var r = rCore.OpenSubKey(is64Bit ? "R64" : "R");
                var installPath = (string)r.GetValue("InstallPath");
                //var binPath = Path.Combine(installPath, "bin");
                rPath = Path.Combine(installPath, "bin");
            }
            Console.WriteLine("rPath: " + rPath);
            return rPath;
        }

        private string resourcesRelativePathFinder(string resource)
        {
            resourceDir = Path.GetFullPath(resource);
            //var str = "My name @is ,Wan.;'; Wan";
            var charsToRemove = new string[] { resource };
            foreach (var c in charsToRemove)
            {
                resourceDir = resourceDir.Replace(c, string.Empty);
            }
            Console.WriteLine("resourceDir: " + resourceDir);
            return resourceDir;
        }

        // Simple synchronous file move operations with no user interface.
        private void simpleFileMove(string sourceFile, string destinationFile)
        {
            try
            {
                //string sourceFile = @"C:\Users\Public\public\test.txt";
                //string destinationFile = @"C:\Users\Public\private\test.txt";

                // To move a file or folder to a new location:
                System.IO.File.Copy(sourceFile, destinationFile);

                // To move an entire directory. To programmatically modify or combine
                // path strings, use the System.IO.Path class.
                System.IO.Directory.Move(resourceDir, rPath);
            }
            catch (Exception Ex)
            {

            }
        }

        // This method will adjust the size of the form to utilize 
        // the working area of the screen.
        private void setScreenSize()
        {
            // Retrieve the working rectangle from the Screen class
            // using the PrimaryScreen and the WorkingArea properties.
            System.Drawing.Rectangle workingRectangle =
                Screen.PrimaryScreen.WorkingArea;

            // Set the size of the form slightly less than size of 
            // working rectangle.
            this.Size = new System.Drawing.Size(
                workingRectangle.Width - 25, workingRectangle.Height - 25);

            // Set the location so the entire form is visible.
            this.Location = new System.Drawing.Point(5,5);

            //splitContainer1.Panel1.Width = Convert.ToInt32(((workingRectangle.Width * (0.30))));//* (2/4));
            //splitContainer1.Panel2.Width = Convert.ToInt32(((workingRectangle.Width * (0.70))));// * (3/4));
            if (csvTable == null)
            {
                dataGridView1.Width = splitContainer1.Panel1.Width;
            }
            else
                dataGridView1.Width = Convert.ToInt32(((workingRectangle.Width * (0.30))));//* (2/4));
            //pictureBox1.Width = Convert.ToInt32(((workingRectangle.Width * (0.70))));// * (3/4));
        }

        private void CSVfileBrowser()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "CSV files (*.csv)|*.csv";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                userInputCSV = openFileDialog.FileName;
            }
        }

        private void RscriptBrowser()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "R files (*.R)|*R";

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                userInputScript = openFileDialog.FileName;
            }
        }

        public DataTable readCSV(string filePath)
        {
            try
            {
                DataTable dt = new DataTable();
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string strLine = sr.ReadLine();

                    string[] strArray = strLine.Split(',');

                    foreach (string value in strArray)
                    {
                        dt.Columns.Add(value.Trim());
                    }
                    DataRow dr = dt.NewRow();

                    while (sr.Peek() >= 0)
                    {
                        strLine = sr.ReadLine();
                        strArray = strLine.Split(',');
                        dt.Rows.Add(strArray);
                    }
                }
                if (dt != null)
                    toolStripButton1.Enabled = true;
                //setScreenSize();
                //executeToolStripMenuItem.Enabled = false;
                return dt;
            }
            catch ( Exception Ex )
            {
                return null;
            }
        }

        public void RunFromCmd(string batch, params string[] args)
        {
            // Not required. But our R scripts use allmost all CPU resources if run multiple instances
            //lock (typeof(REngineRunner))
            //{
            //string fileName = string.Empty;// = userInputScript; //= string.Empty;
            //string fileContents = string.Empty;
            string result = string.Empty;
            //batch = File.ReadAllText(userInputScript);
            try
            {
                // Save R code to temp file
                //fileName = CreateTempFile();
                //using (var streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Open, FileAccess.Write)))
                //{
                //streamWriter.Write(batch);
                //}

                //fileContents = File.ReadAllText(fileName);

                // Get path to R
                var rCore = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core") ??
                            Registry.CurrentUser.OpenSubKey(@"SOFTWARE\R-core");
                var is64Bit = Environment.Is64BitProcess;
                if (rCore != null)
                {

                    var r = rCore.OpenSubKey(is64Bit ? "R64" : "R");
                    var installPath = (string)r.GetValue("InstallPath");
                    var binPath = Path.Combine(installPath, "bin");
                    //binPath = Path.Combine(binPath, is64Bit ? "x64" : "i386");
                    binPath = Path.Combine(binPath, "Rscript.exe");

                    //string binPath = resourceDir;// Budget_Simulator.Properties.Resources.Rscript.exe
                    string strCmdLine = @"/C """"" + binPath + @""" """ + batch + @""""; //binPath + @""" """ + batch + @""""""; // fileContents + @"""""";
                    //Console.WriteLine("binPath: " + binPath);
                    Console.WriteLine("batch: " + batch);
                    //string strCmdLine = @"/c """ + binPath + @""" " + file;
                    //string strCmdLine = @"C:\Users\kollaea\Documents\R\R-3.5.0\bin\Rscript.exe";
                    //var resource = Properties.Resources.Quiet;
                    //System.Diagnostics.Process.Start(//@"C:\Users\kollaea\Documents\R\R-3.5.0\bin\Quiet.exe" 
                    //"CMD.exe", strCmdLine);

                    // All appended args get ""
                    if (args.Any())
                    {
                        strCmdLine += @" """ + string.Join(@""" """, args);
                    }
                    // Last arg gets an extra "" to end cmd line
                    strCmdLine += @"""""";

                    System.Diagnostics.ProcessStartInfo myProcessInfo = new System.Diagnostics.ProcessStartInfo(); //Initializes a new ProcessStartInfo of name myProcessInfo
                    myProcessInfo.FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe"; //Sets the FileName property of myProcessInfo to %SystemRoot%\System32\cmd.exe where %SystemRoot% is a system variable which is expanded using Environment.ExpandEnvironmentVariables
                    myProcessInfo.Arguments = strCmdLine;// "cd..";//"CMD.exe";// + strCmdLine;
                    myProcessInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; //Sets the WindowStyle of myProcessInfo which indicates the window state to use when the process is started to Hidden
                    myProcessInfo.Verb = "runas"; //The process should start with elevated permissions
                    System.Diagnostics.Process.Start(myProcessInfo);

                    Console.WriteLine(strCmdLine);
                }
                //strCmdLine = "cd C:/Users/kollaea/Documents/R/R-3.5.0/bin/Rscript.exe" + file;

                //System.Diagnostics.Process.Start("CMD.exe", strCmdLine);

                //var info = new ProcessStartInfo("cmd", strCmdLine);
                //info.RedirectStandardInput = false;
                //info.RedirectStandardOutput = true;
                //info.UseShellExecute = false;
                //info.CreateNoWindow = true;
                //using (var proc = new Process())
                //{
                //proc.StartInfo = info;
                //proc.Start();
                //result = proc.StandardOutput.ReadToEnd();
                //}
                //}
                //else
                //{
                //result += "R-Core not found in registry";
                //}
                //Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("An ERROR has occurred... see log output", "Alert");
                throw new Exception("R failed to compute. Output: " + result, ex);
            }
            finally
            {
                //if (!string.IsNullOrWhiteSpace(fileName))
                //{
                //DeleteTempFile(fileName);
                //}
            }

        }

        private static string CreateTempFile()
        {
            string fileName = string.Empty;

            try
            {
                // Get the full name of the newly created Temporary file. 
                // Note that the GetTempFileName() method actually creates
                // a 0-byte file and returns the name of the created file.
                fileName = Path.GetTempFileName();

                // Craete a FileInfo object to set the file's attributes
                FileInfo fileInfo = new FileInfo(fileName);

                // Set the Attribute property of this file to Temporary. 
                // Although this is not completely necessary, the .NET Framework is able 
                // to optimize the use of Temporary files by keeping them cached in memory.
                fileInfo.Attributes = FileAttributes.Temporary;

                Console.WriteLine("TEMP file created at: " + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create TEMP file or set its attributes: " + ex.Message);
            }
            return fileName;
        }

        private static void DeleteTempFile(string tempFile)
        {
            try
            {
                // Delete the temp file (if it exists)
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    Console.WriteLine("TEMP file deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting TEMP file: " + ex.Message);
            }
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (File.Exists(@"C:\Users\kollaea\Source\Repos\Budget Simulator\Budget Simulator\Resources\budget.png"))
            //{
            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();
            //File.Delete(@"C:\Users\kollaea\Source\Repos\Budget Simulator\Budget Simulator\Resources\budget.png");
            //}

            //RscriptBrowser();
            //userInputScript = GetLocalResourceObject("MonteCarloBudgetSimulation.R");
            userInputScript = RpathFinder() + @"\MonteCarloBudgetSimulation.R";
            Console.WriteLine("userInputScript: " + userInputScript);

            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;
            outputLocation = RpathFinder();
            Console.WriteLine("outputLocation: " + outputLocation);
            RunFromCmd(userInputScript, userInputCSV, outputLocation, iterations.ToString(), distributionType);
            //AutoClosingMessageBox ACMB = new AutoClosingMessageBox(string, string, int);
            //ACMB.Show("Loading... Please Wait", "Alert,", 5);
            //MessageBox.Show("Loading", "Alert");
            //pictureBox1.Image = null;
            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;
            if (iterations >= 10000)
                Thread.Sleep(Convert.ToInt32(iterations / 4));
            else
                Thread.Sleep(2500);
            //ResourceManager rm = Budget_Simulator.Properties.Resources.ResourceManager;
            //Bitmap budgetPic = (Bitmap)rm.GetObject("budget.png");
            //var bmp = new Bitmap(Budget_Simulator.Properties.Resources.MonteCarloBudgetSimulation.png);
            //Image simBudget = Budget_Simulator.Properties.Resources.MonteCarloBudgetSimulation.R;
            pictureBox1.Image = Image.FromFile(RpathFinder() + @"\output.png");
            //Image.FromFile(@"C:\Users\kollaea\Source\Repos\Budget Simulator\Budget Simulator\Resources\budget.png");
            /*
            try
            {
                //pictureBox1.Image = Image.FromFile(RworkingDirectory + "\budget.png");
                pictureBox1.Image = Image.FromFile(@"C:\Users\kollaea\Documents\budget.png");
            }
            catch (Exception Ex)
            {
                MessageBox.Show("R Working Directory is incorrect... \nContact: Ed Kollar - Data Scientist - HRC - Whitehall, MI", "Alert");
            }
            */
            //pictureBox1.Image = budgetPic;
            // Set cursor as default arrow
            Cursor.Current = Cursors.Default;
        }

        private string scriptWriter(string script)
        {
            //File script;

            string scriptName = string.Empty;

            try
            {
                // Get the full name of the newly created Temporary file. 
                // Note that the GetTempFileName() method actually creates
                // a 0-byte file and returns the name of the created file.
                scriptName = "MonteCarloBudgetSimulation.R";//Path.GetTempFileName();

                // Create a FileInfo object to set the file's attributes
                FileInfo fileInfo = new FileInfo(scriptName);

                // Set the Attribute property of this file to Temporary. 
                // Although this is not completely necessary, the .NET Framework is able 
                // to optimize the use of Temporary files by keeping them cached in memory.
                fileInfo.Attributes = FileAttributes.Normal;

                Console.WriteLine("SCRIPT file created at: " + Path.GetPathRoot(scriptName));

                //using (var streamWriter = new StreamWriter(new FileStream(scriptName, FileMode.Open, FileAccess.Write)))
                //{
                //streamWriter.Write(batch);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create TEMP file or set its attributes: " + ex.Message);
            }
            return scriptName;

        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //if (File.Exists(@"C:\Users\kollaea\Source\Repos\Budget Simulator\Budget Simulator\Resources\budget.png"))
            //{
            if (pictureBox1.Image != null)
                pictureBox1.Image.Dispose();
            //File.Delete(@"C:\Users\kollaea\Source\Repos\Budget Simulator\Budget Simulator\Resources\budget.png");
            //}

            //RscriptBrowser();
            //userInputScript = GetLocalResourceObject("MonteCarloBudgetSimulation.R");
            userInputScript = RpathFinder() + @"\MonteCarloBudgetSimulation.R";
            Console.WriteLine("userInputScript: " + userInputScript);

            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;
            outputLocation = RpathFinder();
            Console.WriteLine("outputLocation: " + outputLocation);
            RunFromCmd(userInputScript, userInputCSV, outputLocation, iterations.ToString(), distributionType);
            //AutoClosingMessageBox ACMB = new AutoClosingMessageBox(string, string, int);
            //ACMB.Show("Loading... Please Wait", "Alert,", 5);
            //MessageBox.Show("Loading", "Alert");
            //pictureBox1.Image = null;
            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;
            if (iterations >= 10000)
                Thread.Sleep(Convert.ToInt32(iterations / 4));
            else
                Thread.Sleep(2500);
            //ResourceManager rm = Budget_Simulator.Properties.Resources.ResourceManager;
            //Bitmap budgetPic = (Bitmap)rm.GetObject("budget.png");
            //var bmp = new Bitmap(Budget_Simulator.Properties.Resources.MonteCarloBudgetSimulation.png);
            //Image simBudget = Budget_Simulator.Properties.Resources.MonteCarloBudgetSimulation.R;
            pictureBox1.Image = Image.FromFile(RpathFinder() + @"\output.png");
            //Image.FromFile(@"C:\Users\kollaea\Source\Repos\Budget Simulator\Budget Simulator\Resources\budget.png");
            /*
            try
            {
                //pictureBox1.Image = Image.FromFile(RworkingDirectory + "\budget.png");
                pictureBox1.Image = Image.FromFile(@"C:\Users\kollaea\Documents\budget.png");
            }
            catch (Exception Ex)
            {
                MessageBox.Show("R Working Directory is incorrect... \nContact: Ed Kollar - Data Scientist - HRC - Whitehall, MI", "Alert");
            }
            */
            //pictureBox1.Image = budgetPic;
            // Set cursor as default arrow
            Cursor.Current = Cursors.Default;
        }

        private void cSVToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            CSVfileBrowser();

            csvTable = readCSV(userInputCSV);

            dataGridView1.DataSource = csvTable;
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            iterations = Convert.ToInt32(toolStripTextBox1.Text);
        }

        private void toolStripTextBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13) // Enter key pressed
            {
                toolStripButton1_Click(sender, e);
            }
        }
    }
}
