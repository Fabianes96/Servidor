using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace Servidor
{
    
    public partial class Form1 : Form
    {
        private readonly int puerto = 2027;
        private byte[] _buffer = new byte[1024];
        public List<SocketT2h> __ClientSockets { get; set; }
        List<string> _names = new List<string>();
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            __ClientSockets = new List<SocketT2h>();
            CheckForIllegalCrossThreadCalls = false;

        }
        public void oajd8() { }

        public void escuchar()
        {
            try
            {
                //Socket servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //IPEndPoint direccion = new IPEndPoint(IPAddress.Parse(ip), puerto);
                //servidor.Bind(direccion);
                lb_stt.Text = "Preparando servidor";
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
                _serverSocket.Listen(1);
                _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);

            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }
        }
        private void AppceptCallback(IAsyncResult ar)
        {
            Socket socket = _serverSocket.EndAccept(ar);
            __ClientSockets.Add(new SocketT2h(socket));
            lista_clientes.Items.Add(socket.RemoteEndPoint.ToString());

            lb_stt.Text = "Numero de clientes conectados: " + __ClientSockets.Count.ToString();
            lb_stt.Text = "Cliente conectado. . .";
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            if (socket.Connected)
            {
                int received;
                try
                {
                    received = socket.EndReceive(ar);
                }
                catch (Exception)
                {
                    // Cerrar conexion cliente
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            lb_stt.Text = "Numero de clientes conectados: " + __ClientSockets.Count.ToString();
                        }
                    }
                    // Eliminar en la lista
                    return;
                }
                if (received != 0)
                {
                    byte[] dataBuf = new byte[received];
                    Array.Copy(_buffer, dataBuf, received);
                    string text = Encoding.ASCII.GetString(dataBuf);
                    lb_stt.Text = "Texto recibido: " + text;

                    string reponse = string.Empty;         
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            rich_Text.AppendText("\n" + __ClientSockets[i]._Name + ": " + text);
                        }
                    }                    
                    reponse = "Servidor recibido" + text;
                    Sendata(socket, reponse);
                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets.RemoveAt(i);
                            lb_stt.Text = "Clientes conectados: " + __ClientSockets.Count.ToString();
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);
        }
        void Sendata(Socket socket, string noidung)
        {
            byte[] data = Encoding.ASCII.GetBytes(noidung);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            socket.EndSend(AR);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            escuchar();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lista_clientes.SelectedItems.Count; i++)
            {
                string t = lista_clientes.SelectedItems[i].ToString();
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    {
                        Sendata(__ClientSockets[j]._Socket, txt_Text.Text);
                    }
                }
            }
            rich_Text.AppendText("\nServer: " + txt_Text.Text);

        }
        public void enviarInfo()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            escuchar();
        }
    }
    public class SocketT2h
    {
        public Socket _Socket { get; set; }
        public string _Name { get; set; }
        public SocketT2h(Socket socket)
        {
            this._Socket = socket;
        }
    }
}
