using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using CryptAES;
using System.Security.Cryptography;
namespace kodowanie
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static void Set(ref byte aByte, int pos, bool value)
        {
            if (value)
            {
                //left-shift 1, then bitwise OR
                aByte = (byte)(aByte | (1 << pos));
            }
            else
            {
                //left-shift 1, then take complement, then bitwise AND
                aByte = (byte)(aByte & ~(1 << pos));
            }
        }

        public static bool Get(byte aByte, int pos)
        {
            //left-shift 1, then bitwise AND, then check for non-zero
            return ((aByte & (1 << pos)) != 0);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {

                pictureBox1.Image = Image.FromFile(openFileDialog1.FileName);
            }
        }
        // kod haminga
        // rozproszenie informacji, permutacja za pomocą random
        //http://82.145.73.240/index.php/Informatyka_N2
        private void button2_Click(object sender, EventArgs e)
        {
            char dlugosc_ciagu = (char)textBox1.Text.Length;
            //char[] text = string.Concat(textBox1.Text.Select(aat => Convert.ToString(aat, 2).PadLeft(8))).ToArray();
            byte[] bajty= Encoding.UTF8.GetBytes(textBox1.Text);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(textBox3.Text);
            passwordBytes = SHA512.Create().ComputeHash(passwordBytes);
            byte[] bytesEncrypted = AES.AES_Encrypt(bajty, passwordBytes);
            bajty = bytesEncrypted;
            dlugosc_ciagu = (char)bajty.Length;
            BitArray text = new BitArray(bajty);
            Bitmap b = new Bitmap(pictureBox1.Image);
            int x = b.Width;
            int y = b.Height;
            Color color;
            Color ncolor;
            int bit = 7;
            byte br, bg = 0, bb = 0;
            bool gr;
            byte len = 0;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    color = b.GetPixel(i, j);
                    br = color.R;
                    bg = color.G;
                    bb = color.B;

                    if (bit >= 0)
                    {
                        //zmiana bitów
                        gr = Get((byte)dlugosc_ciagu, bit);
                        Set(ref br, 0, gr);

                        if (bit >= 1)
                        {
                            bg = color.G;
                            gr = Get((byte)dlugosc_ciagu, --bit);
                            Set(ref bg, 0, gr);
                        }
                        if (bit >= 1)
                        {
                            bb = color.B;
                            gr = Get((byte)dlugosc_ciagu, --bit);
                            Set(ref bb, 0, gr);
                        }

                        //wpisanie kloroów
                        ncolor = Color.FromArgb(br, bg, bb);
                        b.SetPixel(i, j, ncolor);
                       bit--;
                    }
                    else
                    {
                        if (len < text.Count)
                        {
                            //  gr = (text[len] == '1' ? true : false);
                            //  Set(ref br, 0, gr);
                            Console.WriteLine("Bit {0} {1}", len, text[len]);
                            Set(ref br, 0, text[len++]);
                            
                        }
                        if (len < text.Count)
                        {
                            //gr = (text[++len] == '1' ? true : false);
                            //Set(ref bg, 0, gr);
                            Console.WriteLine("Bit {0} {1}", len, text[len]);
                            Set(ref bg, 0, text[len++]);
                 
                        }
                        if (len < text.Count)
                        {
                            //gr = (text[++len] == '1' ? true : false);
                            //Set(ref bb, 0, gr);
                            Console.WriteLine("Bit {0} {1}", len, text[len]);
                            Set(ref bb, 0, text[len++]);
              
                        }
                        ncolor = Color.FromArgb(br, bg, bb);
                        b.SetPixel(i, j, ncolor);
                       
                    }

                }
            pictureBox2.Image = b;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(textBox3.Text);
            passwordBytes = SHA512.Create().ComputeHash(passwordBytes);
            byte dlugosc_ciagu = 0;
            bool[] bity = new bool[255 * 8];
            Bitmap b = new Bitmap(pictureBox2.Image);
            int x = b.Width;
            int y = b.Height;
            Color color;
            int bit = 7;
            int cbit = 0;
            byte br, bg = 0, bb = 0;
            bool gr;
            int len = 0;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    color = b.GetPixel(i, j);
                    br = color.R;
                    bg = color.G;
                    bb = color.B;
                    if (bit >= 0)
                    {

                            //odczytanie długości ukrytego tekstu
                            gr = Get((byte)br, 0);
                            Set(ref dlugosc_ciagu, bit, gr);

                            if (bit > 0)
                            {
                                bg = color.G;
                                gr = Get((byte)bg, 0);
                                Set(ref dlugosc_ciagu, --bit, gr);
                            }
                            if (bit > 0)
                            {
                                bb = color.B;
                                gr = Get((byte)bb, 0);
                                Set(ref dlugosc_ciagu, --bit, gr);
                            }
                            bit--;

                        
                    }
                    else
                    {
                        if (dlugosc_ciagu * 8 > cbit)
                        {
                            gr = Get((byte)br, 0);
                            bity[cbit] = gr;
                            bg = color.G;
                            gr = Get((byte)bg, 0);
                            bity[++cbit] = gr;
                            bb = color.B;
                            gr = Get((byte)bb, 0);
                            bity[++cbit] = gr;
                            cbit++;
                        }
                    }
                }
            byte[] text = new byte[dlugosc_ciagu];
            for (int i = 0; i<dlugosc_ciagu*8;i++)
            {
                Console.WriteLine("Bit {0} {1}", i, bity[i]);
                if ((i != 0) && (i % 8) == 0) len++;
                Set(ref text[len], (i%8), bity[i]);
                
            }
            byte[] bytesDecrypted = AES.AES_Decrypt(text, passwordBytes);
            String decoded;
            ASCIIEncoding ascii = new ASCIIEncoding();
            decoded = ascii.GetString(bytesDecrypted);
            //kod = decoded.Remove(dlugosc_ciagu);
            textBox2.Text = decoded;
        }
    }
}
