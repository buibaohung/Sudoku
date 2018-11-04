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

namespace server
{
    public partial class Form1 : Form
    {
        //public static int NoSudoku = 0;
        //public static Sudoku sudoku;
        //string DATA = "";
        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;
        public Form1()
        {
            //sudoku = new Sudoku();
            InitializeComponent();
            Listen();
        }
        
        private void Listen()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            clientList = new List<Socket>();

            IP = new IPEndPoint(IPAddress.Any, 8000);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            server.Bind(IP);
            Thread listen = new Thread(ListenThread);
            listen.IsBackground = true;
            listen.Start();
        }
        public void ListenThread()
        {
            try
            {
                while (true)
                {
                    server.Listen(100);
                    Socket client = server.Accept();
                    clientList.Add(client);
                    Thread receive = new Thread(Receive);
                    receive.IsBackground = true;
                    receive.Start(client);
                }
            }
            catch
            {
                IP = new IPEndPoint(IPAddress.Any, 8000);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            }
        }
        private void Send(Socket client,string Data)
        {
            if (client != null )
                client.Send(Serialize(Data));

        }
        private void Receive(object obj)
        {
            Socket client = obj as Socket;
            Sudoku su = new Sudoku();
            su.Create_Sudoku();
            try
            {
                while (true)
                {
                    Socket s = client;
                    byte[] data = new byte[1024];
                    client.Receive(data);
                    string message = (string)Deserialize(data);
                    message += "_______________________";
                    string check = "";
                    for (int f = 0; f < 8; f++)
                    {
                        check += message[f];
                    }
                    if (check == "suCreate")
                    {
                        listBox1.Items.Add("Create request");

                        ConvertSudokuFromString(su.toSaveableString());
                        string SendMss = su.toSendableString();
                        Send(client, SendMss);
                        listBox1.Items.Add("Completed");
                    }
                    else
                    {
                        if (check[0]=='r')
                        {
                            su.Reset();
                        }
                        else
                        {
                            string cpStr = su.CheckInput(message);
                            listBox1.Items.Add("Check request");
                            Send(client, cpStr);
                            if (cpStr[0] == 't' && su.IsCompleted())
                            {
                                Send(client, "win00000000");
                            }
                            listBox1.Items.Add("completed");
                        }
                    }
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
            }
        }
        public int CheckMessage(string s) {
            string check = "";
            for(int i = 0; i < 7; i++)
            {
                check += s[i];
            }
            if (check=="suCreate")
            {
                return -1;// ktra chuỗi gửi đến, nếu chứa chuỗi yêu cầu phát sinh suCreate thì trả về giá trị -1
                //ngược lại trả về thông tin vị trí và kí tự chèn vào từ client
            }
            else return int.Parse(s.Substring(0, 3));
        }

        byte[] Serialize(object obj)
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
        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {
            server.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public class Sudoku{
            public bool[,] Fixable=new bool[9,9];
            public enum hoanvi { dong,cot};
            public int[,] Data = new int[9,9];
            public Sudoku()
            {
                string str = Properties.Resources.Sudoku_F;
                //load sudoku tu resources
                for (int i = 0; i < str.Length; i++)
                {

                    if (i == 0)
                    {
                        Data[0, 0] = int.Parse(str[i].ToString());
                    }
                    else Data[i / 9, i % 9] = int.Parse(str[i].ToString());
                    //Create_Sudoku();
                }
            }
            public void Create_Sudoku()
            {
                Thread.Sleep(100);
                bool[] targetArr1 = new bool[9];
                bool[] targetArr2 = new bool[9];
                for (int j = 0; j <= 8; j++)
                {
                    targetArr1[j] = RandomTarget(2)[j];
                    Thread.Sleep(100);
                    targetArr2[j] = RandomTarget(2)[j];
                    //ở đây ta chia xác xuất thành 2 phần (50%) 
                    //để Sudoku mới khác sudoku cũ nhiều hơn
                }
                for (int i = 0; i <= 8; i++)
                {
                    if (targetArr1[i])
                    {
                        HoanViNgauNhienTronglocal(i, hoanvi.cot);
                    }
                    if (targetArr2[i])
                    {
                        HoanViNgauNhienTronglocal(i, hoanvi.dong);
                    }
                }
            }
            
            public void Reset()
            {
                for(int i = 0; i < 9; i++)
                {
                    for(int j = 0; j < 9; j++)
                    {
                        if (Fixable[i, j])
                        {
                            Data[i, j] = 0;
                        }
                    }
                }
            }
            public bool CheckInput(int X, int Y, int data)
            {
                for(int i = 0; i < 9; i++)
                {
                    //Check dòng
                    if (i != Y && data == Data[X, i]) return false;
                    //check cột
                    if (i != Y && data == Data[i, Y]) return false;
                }
               
                int maxX, maxY, minX, minY;
                minX = ((int)X / 3) * 3;
                maxX = ((int)X / 3) * 3 + 2;
                minY = ((int)Y / 3) * 3;
                maxY = ((int)Y / 3) * 3 + 2;
                // check cụm 9 ô nhỏ
                for (int k = minX; k <= maxX; k++)
                {
                    for (int h = minY; h <= maxY; h++)
                    {
                        if (X != k && Y != h)
                        {
                            if (data == Data[k,h]) return false;
                        }
                    }
                }
                Data[X, Y] = data;
                return true;

            }
            public String CheckInput(string a)
            {
                if (!char.IsDigit(a[0])) return "f000000000000";
                int x = int.Parse(a[0]+"");
                int y = int.Parse(a[1]+"");
                int z = int.Parse(a[2]+"");
                if (CheckInput(x, y, z)) return "t000000000000";
                return "f000000000000";
            }
            public void HoanVi(hoanvi dong_hay_cot, int obj1, int obj2)
            {
                int[] swap = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                if (dong_hay_cot == hoanvi.dong)
                {
                    //dong
                    for (int i = 0; i < 9; i++)
                    {
                        swap[i] = Data[obj1, i];
                        Data[obj1, i] = Data[obj2, i];
                        Data[obj2, i] = swap[i];
                    }
                }
                else
                {
                    //cot
                    for (int i = 0; i < 9; i++)
                    {
                        swap[i] = Data[i, obj1];
                        Data[i, obj1] = Data[i, obj2];
                        Data[i, obj2] = swap[i];
                    }
                }
                
            }
            public void HoanViNgauNhienTronglocal(int pos, hoanvi hv)
            {
                //hàm này sẽ hoán vị cột/dòng ngẫu nhiên với 1 cột/dòng khác trong phạm vi local( 9 ô vuông nhỏ )
                int minPos, maxPos; // xác định khoảng hoán vị
                minPos = (pos / 3) * 3;
                maxPos = (pos / 3) * 3 + 2;

                for (int i = minPos; i <= maxPos; i++)
                {
                    if (i != pos)
                    {
                        int C = new Random().Next(0, 100);
                        if (new Random().Next(0, 100) % 2 == 0)
                        {
                            //nếu số đầu tiên trong 2 số còn lại được chọn thì sẽ hoán vị vs pos và return
                            HoanVi(hv, pos, i);
                            return;
                        }
                        else
                        { //ngược lại, nếu vị trí đầu k đk chọn tức là vị trí còn lại sẽ hoán vị vs pos
                            for (int j = minPos; j < maxPos; j++)
                            {
                                if (j != i && j != pos)
                                {
                                    HoanVi(hv, pos, j);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            public String toSaveableString()
            {
                String st = "";
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        st += Data[i, j];
                    }
                }
                return st;
            }
            public String toSendableString()
            {
                bool[] targetArr = new bool[9];
                string st = "";
                for (int i = 0; i < 9; i++)
                {

                    for (int j = 0; j < 9; j++)
                    {
                        targetArr[j] = RandomTarget(3)[j];

                    }
                    for (int k = 0; k < 9; k++)
                    {
                        if (!targetArr[k])
                        {
                            st += Data[i, k];
                        }
                        else {
                            Data[i, k] = 0;
                            Fixable[i, k] = true;
                            st += 0;
                        }
                    }
                }
                return st;
            }
            public bool IsCompleted()
            {
                for(int i = 0; i < 9; i++)
                {
                    for(int j = 0; j < 9; j++)
                    {
                        if (Data[i, j] == 0) return false;
                    }
                }
                return true;
            }
        }
        public string ConvertSudokuFromString(string s)
        {
            string Re = "";
            for (int i = 0; i < s.Length; i++)
            {
                Re += s[i] + "\t";
                if (i % 9 == 8)
                {
                    listBox1.Items.Add(Re);
                    Re = "";
                }
            }
            return Re;
        }
        public static bool[] RandomTarget(int ChiaXacSuatThanhBaoNhieuPhan)
        {
            // Hàm này tạo mảng 9 phần tử boolean, dùng trong chọn vị trí ngẫu nhiên khi chọn vị trí hoán vị
            // hoặc dùng trong việc xóa bớt phần tử trong sudoku để gửi cho client
            if (ChiaXacSuatThanhBaoNhieuPhan > 5) ChiaXacSuatThanhBaoNhieuPhan = 5;
            Random r = new Random();
            bool[] a = new bool[9];
            for (int i = 0; i < 9; i++)
            {
                Thread.Sleep(10);
                int o = r.Next(0, 100);
                if (o % ChiaXacSuatThanhBaoNhieuPhan == 0) a[i] = true;
                else a[i] = false;
            }
            return a;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
