using Microsoft.Win32;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using Arconic_Budget_Simulator.Properties;
using System.Timers;
using System.Resources;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Arconic_Budget_Simulator
{

    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer mainTimer;
        DataTable csvTable = new DataTable();
        Color Arconic = Color.FromArgb(0, 192, 192);

        int iterations = 10000;
        bool dataLoaded = false;
        bool flash = false;
        bool goodInput = true;
        bool isTempCSV = false;
        bool loading = false;
        bool maxWarning = false;
        bool minWarning = false;
        string outputLocation = @"./";
        string distributionType = "uniform"; // optional choice: triangular
        string userInputCSV = string.Empty;
        string userInputScript = string.Empty;
        string rPath = string.Empty;
        string rPathDest = string.Empty;
        string resourceDir = string.Empty;
        string RworkingDirectory = Environment.UserName + @"\Documents";
        object logo = Resources.ResourceManager.GetObject("arconic_logo.ico");

        public Form1()
        {
            InitializeComponent();
            toolStripButton1.Enabled = false;
            setScreenSize();
            pictureBox1.Image = Resources.RStudio;

            mainTimer = new System.Windows.Forms.Timer();
            mainTimer.Interval = 150;
            mainTimer.Tick += (sender, args) => {
                if (loading)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    toolStripButton1.Enabled = false;
                    if (!flash)
                    {
                        toolStripButton1.BackColor = Color.GreenYellow;
                        flash = true;
                    }
                    else if (flash)
                    {
                        toolStripButton1.BackColor = Arconic;
                        flash = false;
                    }
                    
                }
                else if (dataLoaded && goodInput)
                {
                    toolStripButton1.Enabled = true;
                    toolStripButton1.BackColor = Arconic;
                }
                Application.DoEvents();
            };
            mainTimer.Start();
        }

        // Get path to R installed directory
        private string RpathFinder()
        {
            var rCore = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core") ??
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\R-core");
            var is64Bit = Environment.Is64BitProcess;
            if (rCore != null)
            {
                var r = rCore.OpenSubKey(is64Bit ? "R64" : "R");
                var installPath = (string)r.GetValue("InstallPath");
                rPath = Path.Combine(installPath, "bin");
            }
            return rPath;
        }

        private void writePNG()
        {
            string path = RpathFinder() + @"\output.png";

            try
            {
                // Delete the file if it exists.
                if (File.Exists(path))
                {
                    // Note that no lock is put on the
                    // file and the possibility exists
                    // that another process could do
                    // something with it between
                    // the calls to Exists and Delete.
                    File.Delete(path);
                }

                // Create the file.
                using (FileStream fs = File.Create(path))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes("This is some text in the file.");
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }

                // Open the stream and read it back.
                using (StreamReader sr = File.OpenText(path))
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        Console.WriteLine(s);
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        // Simple synchronous file move operations with no user interface.
        private string simpleFileMove(string sourceFile, string destinationFile)
        {
            try
            {
                System.IO.Directory.Move(sourceFile, destinationFile);
            }
            catch (Exception Ex)
            {
                Console.WriteLine("Migration Failed: Error moving the file...");
            }
            Console.WriteLine("Migration Sucess: Succesful move from " + sourceFile + " to " + destinationFile);
            return destinationFile;
        }

        // This method will adjust the size of the form to utilize the working area of the screen.
        private void setScreenSize()
        {
            // Retrieve the working rectangle from the Screen class
            // using the PrimaryScreen and the WorkingArea properties.
            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            // Set the size of the form slightly less than size of working rectangle.
            this.Size = new System.Drawing.Size(workingRectangle.Width-24, workingRectangle.Height-24);

            // Set the location so the entire form is visible.
            this.Location = new System.Drawing.Point(12, 15);

            if (!dataLoaded)
                splitContainer1.Panel1MinSize = dataGridView1.Width
                = Convert.ToInt32(((workingRectangle.Width * (0.3))));
        }

        private void CSVfileBrowser()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                userInputCSV = openFileDialog.FileName;
        }

        /* Unused Method - Has potential for future applications...
        private void RscriptBrowser()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "R files (*.R)|*R";
            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
                userInputScript = openFileDialog.FileName;
        }*/

        private DataTable readCSV(string filePath)
        {
            try
            {
                DataTable dt = new DataTable();
                using (StreamReader sr = new StreamReader(filePath))
                {
                    string strLine = sr.ReadLine();
                    string[] strArray = strLine.Split(',');

                    foreach (string value in strArray)
                        dt.Columns.Add(value.Trim());

                    DataRow dr = dt.NewRow();

                    while (sr.Peek() >= 0)
                    {
                        strLine = sr.ReadLine();
                        strArray = strLine.Split(',');
                        dt.Rows.Add(strArray);
                    }
                }
                if (dt != null && goodInput)
                {
                    label1.Hide();
                    dataLoaded = true;
                    toolStripButton1.Enabled = true;
                }
                return dt;
            }
            catch (Exception Ex)
            {
                Console.WriteLine("ERROR loading CSV file...");
                return null;
            }
        }

        public void RunFromCmd(string batch, params string[] args)
        {
            string result = string.Empty;

            try
            {
                // Get path to R
                var rCore = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\R-core") ??
                            Registry.CurrentUser.OpenSubKey(@"SOFTWARE\R-core");
                var is64Bit = Environment.Is64BitProcess;
                if (rCore != null)
                {
                    var r = rCore.OpenSubKey(is64Bit ? "R64" : "R");
                    var installPath = (string)r.GetValue("InstallPath");
                    var binPath = Path.Combine(installPath, "bin");
                    /* OPTIONAL - Run 32 or 64 bit Rscript.exe */
                    // binPath = Path.Combine(binPath, is64Bit ? "x64" : "i386");
                    binPath = Path.Combine(binPath, "Rscript.exe");
                    string strCmdLine = @"/C """"" + binPath + @""" """ + batch + @"""";
                    Console.WriteLine("batch: " + batch);

                    // All appended args get ""
                    if (args.Any())
                        strCmdLine += @" """ + string.Join(@""" """, args);

                    // Last arg gets an extra "" to end cmd line
                    strCmdLine += @"""""";
                    Console.WriteLine(strCmdLine);
                    // Initializes a new ProcessStartInfo of name myProcessInfo
                    System.Diagnostics.ProcessStartInfo myProcessInfo = new System.Diagnostics.ProcessStartInfo();
                    /* Sets the FileName property of myProcessInfo to %SystemRoot%\System32\cmd.exe where
                       %SystemRoot% is a system variable which is expanded using Environment.ExpandEnvironmentVariables */
                    myProcessInfo.FileName = Environment.ExpandEnvironmentVariables("%SystemRoot%") + @"\System32\cmd.exe";
                    myProcessInfo.Arguments = strCmdLine;
                    // Sets the WindowStyle of myProcessInfo which indicates the window state to use when the process is started to Hidden
                    myProcessInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    myProcessInfo.Verb = "runas"; //The process should start with elevated permissions
                    System.Diagnostics.Process.Start(myProcessInfo);
                }
                else
                    Console.WriteLine("R-Core not found in registry");
            }
            catch (Exception ex)
            {
                Console.WriteLine("R failed to compute. Output: " + result, ex);
            }
        }

        private string CreateTempFile()
        {
            string fileName = string.Empty;

            try
            {
                // Get the full name of the newly created Temporary file. 
                // Note that the GetTempFileName() method actually creates
                // a 0-byte file and returns the name of the created file.
                fileName = Path.GetTempFileName();

                // Create a FileInfo object to set the file's attributes
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

        private void DeleteTempFile(string tempFile)
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

        private void RscriptWriter()
        {
            string userInputScript = CreateTempFile();

            userInputScript += (".R");
            Console.WriteLine(userInputScript);

            StreamWriter strWriter = new StreamWriter(userInputScript);
            strWriter.WriteLine("#!/usr/bin/env Rscript");
            strWriter.WriteLine("# [1] user input CSV");
            strWriter.WriteLine("# [2] output PNG");
            strWriter.WriteLine("# [3] number of simulations");
            strWriter.WriteLine("# [4] distribution type");
            strWriter.WriteLine("");
            strWriter.WriteLine("args = commandArgs(trailingOnly = TRUE)");
            strWriter.WriteLine("args[1] <-gsub('\\"+"\\', '/', args[1],fixed= T)");
            strWriter.WriteLine("args[2] <-gsub('\\"+"\\', '/', args[2],fixed= T)");
            strWriter.WriteLine("");
            strWriter.WriteLine("if (length(args) != 4)");
            strWriter.WriteLine("{");
            strWriter.WriteLine(@"    stop(""4 arguments must be supplied (input.csv, output.png, Number Of Simulations, Distribution Type)"", call.= FALSE)");
            strWriter.WriteLine("}");
            strWriter.WriteLine("");
            strWriter.WriteLine("# install.packages('ggplot2')");
            strWriter.WriteLine("library(ggplot2)");
            strWriter.WriteLine("# install.packages('reshape2')");
            strWriter.WriteLine("library(reshape2)");
            strWriter.WriteLine("# install.packages('gridExtra')");
            strWriter.WriteLine("library(gridExtra)");
            strWriter.WriteLine("# install.packages('triangle')");
            strWriter.WriteLine("library(triangle)");
            strWriter.WriteLine("");
            strWriter.WriteLine("cost <-read.csv(args[1], stringsAsFactors = F)");
            strWriter.WriteLine("");
            strWriter.WriteLine("TargetBudget <-sum(cost$TARGET)");
            strWriter.WriteLine("simNumber <-args[3]");
            strWriter.WriteLine("");
            strWriter.WriteLine("results <-1:simNumber");
            strWriter.WriteLine("for (i in 1:nrow(cost))");
            strWriter.WriteLine("            {");
            strWriter.WriteLine("                x <-cost[i,]");
            strWriter.WriteLine("  range <-x$LOW: x$HIGH");
            strWriter.WriteLine("");
            strWriter.WriteLine("  simCosts <- if (length(range) == 1)");
            strWriter.WriteLine("                {");
            strWriter.WriteLine("                    rep(range, simNumber)");
            strWriter.WriteLine("                }");
            strWriter.WriteLine("                else");
            strWriter.WriteLine("                {");
            strWriter.WriteLine("                    probs <-c(rep(x$PROB / which(range == x$TARGET), which(range == x$TARGET)),");
            strWriter.WriteLine("                               rep((100 - x$PROB) / (length(range) - which(range == x$TARGET)), length(range) - which(range == x$TARGET)))");
            strWriter.WriteLine("    sample(x = range, size = simNumber, prob = probs / 100, replace = T)");
            strWriter.WriteLine("  }");
            strWriter.WriteLine("");
            strWriter.WriteLine("        results<- cbind(results, simCosts)");
            strWriter.WriteLine("    }");
            strWriter.WriteLine("");
            strWriter.WriteLine("    results<- results[, -1]");
            strWriter.WriteLine("    results<- data.frame(results)");
            strWriter.WriteLine("   colnames(results) <- cost$ELEMENT");
            strWriter.WriteLine("");
            strWriter.WriteLine("   SimBudgetTotals<- data.frame(Cost=rowSums(results))");
            strWriter.WriteLine("");
            strWriter.WriteLine("hist<- ggplot(SimBudgetTotals, aes(x=Cost))+");
            strWriter.WriteLine("  geom_histogram(color= 'black', fill= 'blue')+");
            strWriter.WriteLine("  geom_vline(xintercept= TargetBudget, color= 'red') +");
            strWriter.WriteLine("  geom_text(aes(label= 'Target', x= TargetBudget, y= 0, vjust= 1),colour='red') +");
            strWriter.WriteLine("  geom_vline(xintercept= quantile(SimBudgetTotals$Cost, .95), color= 'green') +");
            strWriter.WriteLine("  geom_text(aes(label= '95%', x= quantile(SimBudgetTotals$Cost, .95), y= 0, vjust= 1),colour='green') +");
            strWriter.WriteLine("  labs(title = paste(as.integer(simNumber), 'Simulations of Project Budget'),");
            strWriter.WriteLine("       subtitle = paste(round(100 * nrow(results[rowSums(results) <= TargetBudget,]) / nrow(results), 2), '% Chance of Staying on Budget', '\\"+"n', round(quantile(SimBudgetTotals$Cost, .95) - TargetBudget, 2), ' Contingency Needed for 95% Probability of Staying on Budget', sep = ''))");
            strWriter.WriteLine("");
            strWriter.WriteLine("budgetChange<- data.frame(melt(round(sapply(results[rowSums(results) >= quantile(SimBudgetTotals$Cost, .95) & rowSums(results) <= quantile(SimBudgetTotals$Cost, .96),], mean) - cost$TARGET)))");
            strWriter.WriteLine("budgetChange$variable<- rownames(budgetChange)");
            strWriter.WriteLine("budgetChange$color<- 'red'");
            strWriter.WriteLine("budgetChange$color[budgetChange$value < 0] <- 'green'");
            strWriter.WriteLine("");
            strWriter.WriteLine("bar<- ggplot(budgetChange, aes(x=variable,y=value,fill=color))+");
            strWriter.WriteLine("  geom_bar(stat= 'identity')+");
            strWriter.WriteLine("  coord_flip()+");
            strWriter.WriteLine("  scale_fill_identity() + ");
            strWriter.WriteLine("  geom_text(aes(label = value),hjust=1) +");
            strWriter.WriteLine("  labs(title = paste('Allocation of Needed Contigency'))");
            strWriter.WriteLine("");
            strWriter.WriteLine("#Export Results to PNG");
            strWriter.WriteLine("png(paste(args[2],'output.png', sep= '/'),width=900,height=700)");
            strWriter.WriteLine("grid.arrange(hist, bar, ncol=2)");
            strWriter.WriteLine("dev.off()");
            strWriter.Close();

            string rPathSource = userInputScript;
            Console.WriteLine("rPathSource: " + rPathSource);
            string justPathName = Path.GetDirectoryName(userInputScript);
            Console.WriteLine("justPathName: " + justPathName);
            string justFileName = userInputScript.Replace(justPathName, "");
            Console.WriteLine("justFileName: " + justFileName);

            rPathDest = RpathFinder() + justFileName;
            Console.WriteLine("RpathDest: " + rPathDest);
            simpleFileMove(rPathSource, rPathDest);
        }

        private string getTime()
        {
            int time = 0;
            string units = " seconds";

            if (iterations >= 10000000)
                time = (Convert.ToInt32(iterations * 0.10));
            else if (iterations >= 100000 && iterations < 10000000)
                time = (Convert.ToInt32(iterations * 0.30));
            else if (iterations >= 10000 && iterations < 100000)
                time = (Convert.ToInt32(iterations * 0.50));
            else
                time = 2500;

            time = (Convert.ToInt32(time / 1000) + 1); //Convert units to seconds

            if (time >= 60)
            {
                time = (Convert.ToInt32(time / 60) + 1); //Convert units to minutes
                units = " minutes";
            }

            if (time >= 60)
            {
                time = (Convert.ToInt32(time / 60) + 1); //Convert units to minutes
                units = " hours";
            }

            return (time.ToString() + units);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (goodInput && !loading)
            {
                loading = true;
                toolStripButton1.Enabled = false;

                if (iterations >= 100000)
                {
                    DialogResult option = MessageBox.Show("Running " + iterations + " simulations will " +
                        "take approximately: " + getTime() + "\nDo you wish to continue?", "Alert", MessageBoxButtons.OKCancel);
                    
                    if (option == DialogResult.Cancel)
                        goto Done;
                }

                try
                {
                    if (pictureBox1.Image != null)
                        pictureBox1.Image.Dispose();
                }
                catch (Exception Ex0)
                {
                    var nullErrorMessageBox = MessageBox.Show
                        ("ERROR disposing old image file! \nOutput: "
                        + Ex0.ToString(), "Alert", MessageBoxButtons.OK);
                }

                RscriptWriter();

                userInputScript = rPathDest;
                Console.WriteLine("userInputScript: " + userInputScript);

                outputLocation = RpathFinder();
                Console.WriteLine("outputLocation: " + outputLocation);

                try
                {
                    RunFromCmd(userInputScript, userInputCSV, outputLocation, iterations.ToString(), distributionType);
                }
                catch (Exception Ex1)
                {
                    var nullErrorMessageBox = MessageBox.Show
                        ("ERROR on CMD process! \nOutput: " 
                        + Ex1.ToString(), "Alert", MessageBoxButtons.OK);
                }

                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    toolStripButton1.BackColor = Color.GreenYellow;

                    if (iterations >= 10000000)
                        Thread.Sleep(Convert.ToInt32(iterations * 0.10));
                    else if (iterations >= 100000 && iterations < 10000000)
                        Thread.Sleep(Convert.ToInt32(iterations * 0.30));
                    else if (iterations >= 10000 && iterations < 100000)
                        Thread.Sleep(Convert.ToInt32(iterations * 0.50));
                    else
                        Thread.Sleep(2500);
                }
                catch (Exception Ex2)
                {
                    var nullErrorMessageBox = MessageBox.Show
                        ("ERROR waiting for process to complete! \nOuput: "
                        + Ex2.ToString(), "Alert", MessageBoxButtons.OK);
                }

                try
                {
                    pictureBox1.Image = Image.FromFile(RpathFinder() + @"\output.png");
                }
                catch (Exception Ex3)
                {
                    var nullErrorMessageBox = MessageBox.Show
                        ("ERROR loading the image file! \nOuput: "
                        + Ex3.ToString(), "Alert", MessageBoxButtons.OK);
                }

                Done:
                try
                {
                    if (!string.IsNullOrWhiteSpace(userInputScript))
                        DeleteTempFile(userInputScript);

                    //if (!string.IsNullOrWhiteSpace(userInputScript))
                        //DeleteTempFile(RpathFinder() + @"\output.png");

                    loading = false;
                    maxWarning = false;
                    minWarning = false;
                }
                catch (Exception Ex4)
                {
                    var nullErrorMessageBox = MessageBox.Show
                        ("ERROR on cleanup process! \nOutput: "
                        + Ex4.ToString(), "Alert", MessageBoxButtons.OK);
                }
            }
        }

        private void cSVToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            CSVfileBrowser();
            csvTable = readCSV(userInputCSV);
            dataGridView1.DataSource = csvTable;
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            goodInput = true;

            try
            {
                if (Convert.ToInt32(toolStripTextBox1.Text) < 1000)
                {
                    goodInput = false;
                    toolStripButton1.Enabled = false;

                    if (minWarning == false)
                    {
                        var errorMessageBox = MessageBox.Show
                            ("Number of simulations must be greater than 999!", 
                            "Alert", MessageBoxButtons.OK);
                    }
                    minWarning = true;
                }
                else if (Convert.ToInt32(toolStripTextBox1.Text) > 100000000)
                {
                    goodInput = false;
                    toolStripButton1.Enabled = false;

                    if (maxWarning == false)
                    {
                        var intErrorMessageBox = MessageBox.Show
                            ("Number of simulations must be less than 10000000!", 
                            "Alert", MessageBoxButtons.OK);
                    }
                    maxWarning = true;
                }
                iterations = Convert.ToInt32(toolStripTextBox1.Text);
            }
            catch (Exception Ex)
            {
                goodInput = false;
                var nullErrorMessageBox = MessageBox.Show
                    ("Enter a number between 1000 & 100000000\n"
                    +"RANGE: One-Thousand to One-Hundred Million Simulations", 
                    "Alert", MessageBoxButtons.OK);
            }
        }

        private void toolStripTextBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)13 && dataLoaded && goodInput && !loading) // Enter key pressed
                    toolStripButton1_Click(sender, e);
            }
            catch (Exception Ex)
            {
                var nullErrorMessageBox = MessageBox.Show
                    ("ERROR on UI = Enter Key", "Alert", MessageBoxButtons.OK);
            }
        }

        private void lockTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!dataGridView1.ReadOnly)
            {
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AllowUserToDeleteRows = false;
                dataGridView1.AllowUserToResizeColumns = false;
                dataGridView1.AllowUserToResizeRows = false;
                dataGridView1.ReadOnly = true;
            }
            else
            {
                dataGridView1.AllowUserToAddRows = true;
                dataGridView1.AllowUserToDeleteRows = true;
                dataGridView1.AllowUserToResizeColumns = true;
                dataGridView1.AllowUserToResizeRows = true;
                dataGridView1.ReadOnly = false;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataLoaded = false;
            toolStripButton1.Enabled = false;
            dataGridView1.DataSource = null;
            label1.Show();
            pictureBox1.Image = Resources.RStudio;

            if (!string.IsNullOrWhiteSpace(userInputCSV) && isTempCSV)
            {
                DeleteTempFile(userInputCSV);
                DeleteTempFile(RpathFinder() + @"\output.png");
                Console.WriteLine("Image deleted from: " + RpathFinder() + @"\output.png");
            }
            isTempCSV = false;
        }

        private void exampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strFile = CreateTempFile();

            strFile += (".csv");
            Console.WriteLine(strFile);

            StreamWriter strWriter = new StreamWriter(strFile);
            strWriter.WriteLine("ELEMENT,TARGET,PROB,LOW,HIGH");
            strWriter.WriteLine("Finish Heat Treat Furnace, 7201, 5, 7200, 11200");
            strWriter.WriteLine("Post Argon Reclaim Vessel Sys, 585, 25, 500, 1000");
            strWriter.WriteLine("Wax pattern press 300 T, 501, 50, 500, 700");
            strWriter.WriteLine("Cast Preheat Ovens, 3001, 50, 3000, 4000");
            strWriter.WriteLine("Cast Casting Cooling Ovens, 1001, 50, 1000, 1500");
            strWriter.WriteLine("Post Vibratory Media Machine, 300, 50, 200, 500");
            strWriter.WriteLine("Post HIP Unit(in Bunker), 10245, 60, 9000, 12000");
            strWriter.WriteLine("VDT Semi Auto FPI, 950, 60, 900, 1800");
            strWriter.WriteLine("Cast Insulation Robot Spray, 563, 70, 422, 704");
            strWriter.WriteLine("NDT Auto Welder, 420, 75, 378, 450");
            strWriter.WriteLine("Shell M - Tech, 15279, 80, 13751, 16807");
            strWriter.WriteLine("Cast sys No maipulators, 11685, 80, 10600, 12500");
            strWriter.WriteLine("Post Water Blast Shell Removal, 2115, 80, 2100, 2400");
            strWriter.WriteLine("Wax pattern Press 100 T, 900, 85, 750, 1000");
            strWriter.WriteLine("Wax Gate Press 10 T, 401, 85, 400, 500");
            strWriter.WriteLine("Cast Dewax Oven, 1500, 85, 1200, 1600");
            strWriter.WriteLine("Cast Autoclave, 1000, 85, 900, 1100");
            strWriter.WriteLine("Post Auto Waterjet Cutoff, 1760, 85, 1500, 2000");
            strWriter.WriteLine("Cast Cast Sys Manipulator, 720, 90, 600, 800");
            strWriter.WriteLine("Finish Autoclave, 2500, 90, 1200, 2501");
            strWriter.WriteLine("Finish Salt Bath Lo Temp Caustic, 400, 90, 300, 450");
            strWriter.WriteLine("Bldg Equipment Allotment, 1150, 90, 750, 1151");
            strWriter.WriteLine("Cast Insulation Robot Spray Fixt, 360, 95, 292, 361");
            strWriter.WriteLine("Finish Auto Sand Blast, 800, 95, 600, 801");
            strWriter.WriteLine("Other, 4195, 100, 4195, 4195");
            strWriter.Close();

            userInputCSV = strFile;
            csvTable = readCSV(strFile);
            dataGridView1.DataSource = csvTable;
            isTempCSV = true;
        }

        private void saveCurrentDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newSaveFile = CreateTempFile();
            newSaveFile += (".csv");
            //userInputCSV = newSaveFile;
            
            //StreamWriter saveCSV = new StreamWriter(newSaveFile);

            var sb = new StringBuilder();
            string str = string.Empty;

            var headers = dataGridView1.Columns.Cast<DataGridViewColumn>();
            sb.AppendLine(string.Join(",", headers.Select(column => column.HeaderText).ToArray()));
            //saveCSV.WriteLine(sb);

            int rows = dataGridView1.RowCount;

            //foreach (DataGridViewRow row in dataGridView1.Rows)
            for (int i = 0; i < rows - 1; i++)
            {
                //var cells = row.Cells.Cast<DataGridViewCell>();
                var cells = dataGridView1.Rows[i].Cells.Cast<DataGridViewCell>();
                //sb.AppendLine(string.Join(",", cells.Select(cell => "\"" + cell.Value + "\"").ToArray()));
                sb.AppendLine(string.Join(",", cells.Select(cell => cell.Value).ToArray()));
                //saveCSV.WriteLine(sb);
            }

            str = sb.ToString();
                str = str.Replace("\"", "");

            Console.WriteLine(sb.ToString());

            StreamWriter saveCSV = new StreamWriter(newSaveFile);
            saveCSV.Write(sb);
            saveCSV.Close();

            userInputCSV = newSaveFile;
            csvTable = readCSV(newSaveFile);
            dataGridView1.DataSource = csvTable;
            isTempCSV = true;

            Console.WriteLine("New Save FIle creadted at: " + newSaveFile);
            Console.WriteLine("userInputCSV: " + userInputCSV);

            var saveMessageBox = MessageBox.Show
                            ("Data Saved Successfully!", "Alert", MessageBoxButtons.OK);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(userInputCSV) && isTempCSV)
            {
                DeleteTempFile(userInputCSV);
                DeleteTempFile(RpathFinder() + @"\output.png");
            }
        }
        
        private void toolStripTextBox1_Click(object sender, EventArgs e) { }

        private void Form1_Load(object sender, EventArgs e) { }

        private void toolStripLabel2_Click(object sender, EventArgs e) { }

        private void label1_Click(object sender, EventArgs e) { }

        private void openToolStripMenuItem_Click(object sender, EventArgs e) { }
    }
}
