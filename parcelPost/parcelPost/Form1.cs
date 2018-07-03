using System;
using System.Windows.Forms;

namespace parcelPost
{
    public partial class Form1 : Form
    {
        String address = "https://parcelpost.arconic.com/";

        public static void main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public Form1()
        {
            this.WindowState = FormWindowState.Maximized;
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = address;
            toolStrip1.Visible = false;
            webBrowser1.ScriptErrorsSuppressed = true;
        }

        // Navigates to the given URL if it is valid.
        private void Navigate(String newAddress)
        {
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
    }
}
