using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

/*
- You may need to edit the file directories of the Q# program and external database, and change the start (C:\Users\Samar\Desktop\...)
- The quantum component compiled as a .DLL, so when it is opened through this program, it will open visual studio for it be run.
- Due to the nature of simulating the quantum program, it may fail at times. In this case, re-run again or try different data-set.
*/

namespace Quantum_Interface
{
    public partial class Main_Window : Form
    {
        //Public Variables
        char[] letters = new char[36] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        static int final_key = 0;
        bool quantum = false;
        string textfilepath = @"C:\Users\Samar\Desktop\Quantum Interface\Assets\Shor's Factorisation Algorithm\Shor's Factorisation Algorithm\bin\Debug\netcoreapp2.0\factors.txt";

        //Checks if Input Integer is Prime
        public bool isPrime(int x)
        {
            if (x == 2) { return true; }
            for (int i = 2; i <= Math.Ceiling(Math.Sqrt(x) + 1); i++) { if (x % i == 0 && i != x) { return false; } }
            return true;
        }

        //Prime Factoring Function - Calculates the Key for Modified Encryption form - Explained in Documentation
        public int Find_Key()
        {
            int n1 = Convert.ToInt32(NUM1.Text);
            int n2 = Convert.ToInt32(NUM2.Text);
            List<int> n1s = new List<int>();
            List<int> n2s = new List<int>();

            //Checks for Prime Factors of the Number
            for (int i = Convert.ToInt32(Math.Ceiling(Math.Sqrt(n1))); i >= 1; i--)
            {
                if (n1 % i == 0 && isPrime(n1 / i) && isPrime(i)) { n1s.Add(i); n1s.Add(n1 / i); }
            }

            for (int i = Convert.ToInt32(Math.Ceiling(Math.Sqrt(n2))); i >= 1; i--)
            {
                if (n2 % i == 0 && isPrime(n2 / i) && isPrime(i)) { n2s.Add(i); n2s.Add(n2 / i); }
            }

            foreach (int i in n1s)
            {
                foreach (int j in n2s)
                {
                    if (i == j)
                    { final_key = (n1 / i) * (n2 / j); }
                }
            }

            return final_key;
        }

        //Loading Program
        public Main_Window()
        {
            InitializeComponent();
            
            //Formats Window
            Menu_Panel.Height = Classical_Button.Height;
            Menu_Panel.Top = Classical_Button.Top;
            CenterToScreen();
            DoubleBuffered = true;

            //Key-Press Restrictions
            NUM1.KeyPress += new KeyPressEventHandler(NUM1_KeyPress);
            NUM2.KeyPress += new KeyPressEventHandler(NUM2_KeyPress);
        }

        //Restricts Input to Numbers
        private void NUM1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        //Restricts Input to Numbers
        private void NUM2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        //Menu Options - Changing Screens
        private void Classical_Button_Click(object sender, EventArgs e)
        {
            Menu_Panel.Height = Classical_Button.Height;
            Menu_Panel.Top = Classical_Button.Top;
            Title_Label.Text = "Classical RSA Encryption";
            quantum = false;
            NUM1.Visible = true;
            NUM2.Visible = true;
            Change_Key.Visible = false;
            Key_1_Val.Visible = false;
            Key_2_Val.Visible = false;
            key_1.Visible = true;
            Key_2.Visible = true;
            final_label.Visible = true;
            Encrypt_Button.Visible = true;
            Decrypt_Button.Visible = true;
            user_text_input.Visible = true;
            Information.Visible = false;
        }

        private void Quantum_Button_Click(object sender, EventArgs e)
        {
            Menu_Panel.Height = Quantum_Button.Height;
            Menu_Panel.Top = Quantum_Button.Top;
            Title_Label.Text = "Quantum RSA Encryption";
            quantum = true;
            NUM1.Visible = false;
            NUM2.Visible = false;
            Change_Key.Visible = true;
            File.WriteAllText(textfilepath, "");
            final.Text = "";
            key_1.Visible = true;
            Key_2.Visible = true;
            final_label.Visible = true;
            Encrypt_Button.Visible = true;
            Decrypt_Button.Visible = true;
            user_text_input.Visible = true;
            Information.Visible = false;
        }

        private void About_Button_Click(object sender, EventArgs e)
        {
            Menu_Panel.Height = About_Button.Height;
            Menu_Panel.Top = About_Button.Top;
            Title_Label.Text = "About";
            NUM1.Visible = false;
            NUM2.Visible = false;
            Change_Key.Visible = false;
            Key_1_Val.Visible = false;
            Key_2_Val.Visible = false;
            Encrypt_Button.Visible = false;
            Decrypt_Button.Visible = false;
            key_1.Visible = false;
            Key_2.Visible = false;
            final_label.Visible = false;
            user_text_input.Visible = false;
            Information.Visible = true;
        }

        private void Exit_Button_Click(object sender, EventArgs e)
        {
            Menu_Panel.Height = Exit_Button.Height;
            Menu_Panel.Top = Exit_Button.Top;
            System.Threading.Thread.Sleep(100);
            Application.Exit();
        }
        //End of Menu Options

        //Encryption Button - Encyrpts Inputted Text
        private void Encrypt_Button_Click(object sender, EventArgs e)
        {
            if (quantum)
            {
                //Check if Keys Have Been Entered
                if (new FileInfo(textfilepath).Length < 6)
                {
                    MessageBox.Show("    The keys have not been entered.");
                }
                else
                {
                    if (string.IsNullOrEmpty(user_text_input.Text)) { MessageBox.Show("    Error. Fill all required input fields correctly."); }
                    else
                    {
                        string[] values = File.ReadAllLines(textfilepath);

                        //Display Information
                        Key_1_Val.Text = values[0];
                        Key_2_Val.Text = values[3];
                        Key_1_Val.Visible = true;
                        Key_2_Val.Visible = true;

                        //For Internal Processing
                        NUM1.Text = values[0];
                        NUM2.Text = values[3];

                        //Calling Key Calculation Function
                        Find_Key();
                        final.Text = Convert.ToString(final_key);
                        final.Show();

                        //Check if Input is Valid According to Encryption Method
                        if (final_key == 0) { MessageBox.Show("    Error. The keys entered are not valid to the encryption method."); }
                        else
                        {
                            char[] message = user_text_input.Text.ToCharArray();
                            char[] message2 = new char[user_text_input.TextLength];

                            //Shifts the Chars by Magnitude of Final Key
                            for (int i = 0; i < user_text_input.TextLength; i++)
                            {
                                if (letters.Contains(message[i]))
                                {
                                    message2[i] = Convert.ToChar(letters[(Array.IndexOf(letters, message[i]) + (final_key % 36)) % 36]);
                                }
                                else { message2[i] = message[i]; }
                            }

                            //Outputs Final Encoded Text
                            string final_text = new string(message2);
                            user_text_input.Text = final_text;
                        }
                    }
                }
            }
            else if (quantum == false)
            {
                //Check if Input is Empty
                if (string.IsNullOrEmpty(user_text_input.Text) || string.IsNullOrEmpty(NUM1.Text) || string.IsNullOrEmpty(NUM2.Text)) { MessageBox.Show("    Error. Fill all required input fields correctly."); }
                else
                {
                    //Check if Input is Numeric
                    int l;
                    if (!int.TryParse(NUM1.Text, out l))
                    {
                        MessageBox.Show("    Enter a purely numeric key.");
                        return;
                    }

                    if (!int.TryParse(NUM2.Text, out l))
                    {
                        MessageBox.Show("    Enter a purely numeric key.");
                        return;
                    }

                    //Calling Key Calculation Function
                    Find_Key();
                    final.Text = Convert.ToString(final_key);
                    final.Show();

                    //Check if Input is Valid According to Encryption Method
                    if (final_key == 0) { MessageBox.Show("    Error. The keys entered are not valid to the encryption method."); }
                    else
                    {
                        char[] message = user_text_input.Text.ToCharArray();
                        char[] message2 = new char[user_text_input.TextLength];

                        //Shifts the Chars by Magnitude of Final Key
                        for (int i = 0; i < user_text_input.TextLength; i++)
                        {
                            if (letters.Contains(message[i]))
                            {
                                message2[i] = Convert.ToChar(letters[(Array.IndexOf(letters, message[i]) + (final_key % 36)) % 36]);
                            }
                            else { message2[i] = message[i]; }
                        }

                        //Outputs Final Encoded Text
                        string final_text = new string(message2);
                        user_text_input.Text = final_text;
                    }
                }
            }
        }

        //Decryption Button - Encyrpts Inputted Text
        private void Decrypt_Button_Click(object sender, EventArgs e)
        {
            if (quantum)
            {
                if (new FileInfo(textfilepath).Length < 6)
                {
                    MessageBox.Show("    The keys have not been entered.");
                }
                else
                {
                    if (string.IsNullOrEmpty(user_text_input.Text))
                    {
                        MessageBox.Show("    Error. Fill all required input fields correctly.");
                    }
                    else
                    {
                        string[] values = File.ReadAllLines(textfilepath);

                        //Display Information
                        Key_1_Val.Text = values[0];
                        Key_2_Val.Text = values[3];
                        Key_1_Val.Visible = true;
                        Key_2_Val.Visible = true;

                        //For Internal Processing
                        NUM1.Text = values[0];
                        NUM2.Text = values[3];

                        //Calling Key Calculation Function
                        Find_Key();
                        final.Text = Convert.ToString(final_key);
                        final.Show();

                        //Check if Input is Valid According to Encryption Method
                        if (final_key == 0) { MessageBox.Show("    Error. The keys entered are not valid to the encryption method."); }
                        else
                        {
                            char[] message = user_text_input.Text.ToCharArray();
                            char[] message2 = new char[user_text_input.TextLength];

                            //Reverse Shifts the Chars by Magnitude of Final Key
                            for (int i = 0; i < user_text_input.TextLength; i++)
                            {
                                if (letters.Contains(message[i]))
                                {
                                    message2[i] = Convert.ToChar(letters[(36 + (Array.IndexOf(letters, message[i]) - (final_key % 36))) % 36]);
                                }
                                else { message2[i] = message[i]; }
                            }

                            //Outputs Final Decoded Text
                            string final_text = new string(message2);
                            user_text_input.Text = final_text;
                        }
                    }
                }
            }
            else
            {

                //Check if Input is Empty
                if (string.IsNullOrEmpty(user_text_input.Text) || string.IsNullOrEmpty(NUM1.Text) || string.IsNullOrEmpty(NUM2.Text)) { MessageBox.Show("    Error. Fill all required input fields correctly."); }
                else
                {
                    //Check if Input is Numeric
                    int l;
                    if (!int.TryParse(NUM1.Text, out l))
                    {
                        MessageBox.Show("    Enter a purely numeric key.");
                        return;
                    }

                    if (!int.TryParse(NUM2.Text, out l))
                    {
                        MessageBox.Show("    Enter a purely numeric key.");
                        return;
                    }

                    //Calling Key Calculation Function
                    Find_Key();
                    final.Text = Convert.ToString(final_key);
                    final.Show();

                    //Check if Input is Valid According to Encryption Method
                    if (final_key == 0) { MessageBox.Show("    Error. The keys entered are not valid to the encryption method."); }
                    else
                    {
                        char[] message = user_text_input.Text.ToCharArray();
                        char[] message2 = new char[user_text_input.TextLength];

                        //Reverse Shifts the Chars by Magnitude of Final Key
                        for (int i = 0; i < user_text_input.TextLength; i++)
                        {
                            if (letters.Contains(message[i]))
                            {
                                message2[i] = Convert.ToChar(letters[(36 + (Array.IndexOf(letters, message[i]) - (final_key % 36))) % 36]);
                            }
                            else { message2[i] = message[i]; }
                        }

                        //Outputs Final Decoded Text
                        string final_text = new string(message2);
                        user_text_input.Text = final_text;
                    }
                }
            }
        }

        //Changing the Key in the Quantum Aspect of the Program
        private void Change_Key_Click(object sender, EventArgs e)
        {
            MessageBox.Show("    Enter the keys by running the following program. ");
            System.Diagnostics.Process.Start(@"C:\Users\Samar\Desktop\Quantum Interface\Assets\Shor's Factorisation Algorithm\Shor's Factorisation Algorithm.sln");
        }
    }
}
