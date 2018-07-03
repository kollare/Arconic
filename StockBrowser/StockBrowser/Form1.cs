using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockBrowser
{
    public partial class Form1 : Form
    {
        private string address = "http://marketwatch.com";

        private Rectangle FormBounds;

        private bool TextBoxShowing = true;
        private bool LockMethods = false;

        private FormBorderStyle shownFormBorderStyle = FormBorderStyle.Sizable;
        private FormBorderStyle hiddenFormBorderStyle = FormBorderStyle.FixedToolWindow;

        private string shownText;

        public Form1()
        {
            //int startpoint = Screen.PrimaryScreen.WorkingArea.Size;
            //startpoint = startpoint/2;
            
            //this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            //this.WindowState = FormWindowState.Maximized;
            InitializeComponent();

            //this.width = Screen.FromControl(this).Bounds;
            //this.Height = ClientRectangle.Height / 2;
            //this.Width = 
            //double width = SystemParameters.FullPrimaryScreenWidth;
            //this.Location = new Point(0, 0);
            shownText = this.Text;
            shownFormBorderStyle = this.FormBorderStyle;

            this.TopMost = true;
            toolStripContainer1.TopToolStripPanelVisible = false;
            toolStripContainer1.BottomToolStripPanelVisible = false;
            webBrowser1.ScriptErrorsSuppressed = true;
            //toolStripTextBox1.BackColor = Color.Gray;
            button1.MouseHover += new EventHandler(b1_Hover);
            toolStripTextBox1.KeyDown += new KeyEventHandler(tb_KeyDown);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //B = new System.Windows.Forms.WebBrowser();
            //webBrowser1.Navigate("http.marketwatch.com");
            FormBounds = this.Bounds;
            ScreenSize();

        }

            private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            toolStripContainer1.TopToolStripPanelVisible = false;
        }

        public Rectangle GetScreen()
        {
            return Screen.FromControl(this).Bounds;
        }

        public void ScreenSize()
        {
            Int32 height = 0;
            Int32 width = 0;
            try
            {
                height = Screen.AllScreens[0].WorkingArea.Height;
                width = Screen.AllScreens[0].WorkingArea.Width;
                this.Location = Screen.AllScreens[0].WorkingArea.Location;
                toolStripTextBox1.Width = width - 45;
                toolStripTextBox1.Text = address;
                button1.Left = width - 100;
            }
            //catch { }
            catch (Exception)
            {
                height = Screen.AllScreens[0].WorkingArea.Height;
                width = Screen.AllScreens[0].WorkingArea.Width;
                this.Location = Screen.AllScreens[0].WorkingArea.Location;
            }
            this.Width = width;
            this.Height = Convert.ToInt32(height / 2.95);
            int buffer = height * 2 / 3;
            this.Top = Convert.ToInt32(height / 1.5);
            //button1.Top = Convert.ToInt32(height / 2);
        }

        // Navigates to the given URL if it is valid.
        private void Navigate(String newAddress)
        {
            //address = newAddress

            if (String.IsNullOrEmpty(newAddress)) return;
            if (newAddress.Equals("about:blank")) return;
            if (!newAddress.StartsWith("http://") &&
                !newAddress.StartsWith("https://"))
            {
                newAddress = "http://" + newAddress;
            }
            try
            {
                address = newAddress;
                webBrowser1.Navigate(new Uri(newAddress));
            }
            catch (System.UriFormatException)
            {
                return;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Navigate(toolStripTextBox1.Text);
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(toolStripTextBox1.Text);
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Navigate(toolStripTextBox1.Text);
            }
        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }

        private void button1__MouseHover(object sender, EventArgs e)
        {
            //toolStripContainer1.TopToolStripPanelVisible = true;
            //ScreenSize();
        }

        private void b1_Hover(object sender, EventArgs e)
        {
            toolStripContainer1.TopToolStripPanelVisible = true;
            //ScreenSize();
        }

        private void toolStripContainer1_BottomToolStripPanel_Click(object sender, EventArgs e)
        {

        }
        

        private void UpdateTextBoxVisibility(bool isvisible)
        {
            if (LockMethods) return;

            if (isvisible)
            {
                if (!TextBoxShowing)
                {
                    // see explanation in comment #1
                    if (!FormBounds.Contains(MousePosition)) return;

                    TextBoxShowing = true;

                    LockMethods = true;

                    this.SuspendLayout();
                    this.Text = shownText;
                    this.ControlBox = true;
                    this.FormBorderStyle = shownFormBorderStyle;
                    this.ResumeLayout();

                    // see explanation in comment #2
                    Application.DoEvents();
                    this.Visible = true;
                    this.BringToFront();
                    this.Refresh();

                    LockMethods = false;
                }
            }
            else if (TextBoxShowing)
            {
                if (FormBounds.Contains(MousePosition)) return;

                TextBoxShowing = false;

                LockMethods = true;

                this.SuspendLayout();
                this.Text = "";
                this.ControlBox = false;
                this.FormBorderStyle = hiddenFormBorderStyle;
                this.ResumeLayout();

                LockMethods = false;
            }
        }

        private void Form1_Enter(object sender, EventArgs e)
        {
            UpdateTextBoxVisibility(true);
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            UpdateTextBoxVisibility(true);
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            UpdateTextBoxVisibility(true);
        }

        private void Form1_Leave(object sender, EventArgs e)
        {
            UpdateTextBoxVisibility(false);
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            UpdateTextBoxVisibility(false);
        }
    }
}
