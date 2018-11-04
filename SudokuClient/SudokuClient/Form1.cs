using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SudokuClient
{
    public partial class Form1 : Form
    {
        Sudoku sudoku;
        Button clickedB;
        Button[,] Gbutton = new Button[9, 9];
        IPEndPoint IP;
        Socket client;
        string SendMss = "";
        public Form1()
        {

            InitializeComponent();
            Connect();
            CreateGameBoard();

        }
        private void Connect()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8000);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Cannot connect to server" + MessageBoxButtons.OK + MessageBoxIcon.Error);
                return;
            }
            Thread listen = new Thread(Receive);
            listen.IsBackground = true;
            listen.Start();
        }
        private void Send(string sendTxt)
        {
            client.Send(Serialize(sendTxt));

        }
        private void Receive()
        {

            try
            {
                while (true)
                {
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    string message = Deserialize(data) as string;
                    switch (message[0])
                    {
                        case 't':
                            break;
                        case 'f':
                            MessageBox.Show("false, try again");
                            break;
                        case 'w':
                            {
                                MessageBox.Show("Win");
                                break;
                            }
                        default:
                            {
                                sudoku = new Sudoku(message);
                                LoadSudoku();
                                break;
                            }
                    }
                }
            }
            catch
            {
                client.Close();
            }
        }

        byte[] Serialize(string obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatte = new BinaryFormatter();
            formatte.Serialize(stream, obj);
            return stream.ToArray();
        }
        object Deserialize(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatte = new BinaryFormatter();
            return formatte.Deserialize(stream);

        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            client.Close();

        }


        public class Sudoku {
            public int[,] Data = new int[9, 9];
            public bool[,] Fixable = new bool[9, 9];
            public Sudoku(string rec)
            {
                if (rec.Length >= 81)
                {
                    for(int i = 0; i < rec.Length; i++)
                    {
                        int z = int.Parse(rec[i].ToString());
                        Data[i / 9, i % 9] = z;
                        if (z == 0)
                        {
                            Fixable[i / 9, i % 9] = true;
                        }
                        else
                        {
                            Fixable[i / 9, i % 9] = false;
                        }
                    }
                }

            }
            public void Reset()
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (Fixable[i, j])
                        {
                            Data[i, j] = 0;
                            
                        }
                    }
                }
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public Button CreateButton(int x,int y,int px,int py)
        {
            Button button = new Button();
            button.Height = 35;
            button.Width = 35;
            button.Name = "b"+py +""+ px;
            button.Location = new Point(x, y);
            panel2.Controls.Add(button);
            button.BackgroundImage = Properties.Resources.slot;
            button.BackgroundImageLayout = ImageLayout.Zoom;
            button.FlatStyle = FlatStyle.Flat;
            button.ForeColor = Color.Yellow;
            button.Click += Button_Click;
            button.KeyPress += Button_KeyPress;
            return button;
        }

        private void Button_KeyPress(object sender, KeyPressEventArgs e)
        {
            char a = e.KeyChar;
            //throw new NotImplementedException();
            if (char.IsDigit(a))
            {
                Button b = sender as Button;
                SendMss = ""+b.Name[1] + b.Name[2] + a;

                Send(SendMss);
                b.Text = ""+a;
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            if (clickedB != null)
            {

                clickedB.BackgroundImage = Properties.Resources.slot;
            }
            Button btn = sender as Button;
            clickedB = btn;
            btn.BackgroundImage = Properties.Resources.target;
            
        }
        public void LoadSudoku()
        {
            for(int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (sudoku.Data[i, j] != 0)
                    {
                        Gbutton[j, i].Text = "" + sudoku.Data[i, j];
                        Gbutton[j, i].ForeColor = Color.Red;
                    }
                }
            }
        }
        public void CreateGameBoard()
        {
            int X = 20, Y = 20;
            for(int i = 0; i < 9; i++)
            {
                for
                   (int j = 0; j < 9; j++)
                {
                    Gbutton[i, j] = CreateButton(X, Y,i,j);
                    
                    Y += 40;
                }
                Y = 20;
                X += 40;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Send("suCreate");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (sudoku.Fixable[i, j])
                    {
                        Gbutton[j, i].Text = "";

                    }
                }
            }
            Send("r");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}