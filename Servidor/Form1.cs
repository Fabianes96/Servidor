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
        public string puntuacion;
        private int contador =0;
        private int maximo;
        //string ip = "192.168.0.14";
        string dir;
        private Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Form1()
        {
            InitializeComponent();
            __ClientSockets = new List<SocketT2h>();
            CheckForIllegalCrossThreadCalls = false;
        }       

        public void escuchar()
        {
            try
            {
                dir = Microsoft.VisualBasic.Interaction.InputBox(
                "Escriba la dirección ip", "Texto del dialogo", "");
                //Socket servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint direccion = new IPEndPoint(IPAddress.Parse(dir), puerto);
                _serverSocket.Bind(direccion);
                lb_stt.Text = "Preparando servidor";
                //_serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
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
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecibeMensaje), socket);
            _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
        }
        private void RecibeMensaje(IAsyncResult ar)
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
                            
                            if (label1.Text.Contains(__ClientSockets[i]._Name))
                            {
                                label1.Text= label1.Text.Replace(__ClientSockets[i]._Name, " ");
                            }
                           // lb_stt.Text = "Numero de clientes conectados: " + __ClientSockets.Count.ToString();
                            contador--;
                            label3.Text = contador.ToString();
                            Sendata(socket, label1.Text);
                            enviarInfo();
                            //label4.Text = __ClientSockets[i]._Name;
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
                    string aux;
                    lb_stt.Text = "Texto recibido: " + text;
                    if (text.Contains(": "))
                    {
                        aux = text.Substring(text.IndexOf(" ")+1);
                        label2.Text = aux;
                        if (maximo < int.Parse(aux))
                        {
                            maximo = int.Parse(aux);
                        }                        
                        lblScoreFinal.Text = lblScoreFinal.Text + text + Environment.NewLine;
                        contador++;
                        label3.Text = contador.ToString();
                        Sendata(socket ,lblScoreFinal.Text);
                        enviarPuntos();
                        return;
                    }                  

                    string reponse = string.Empty;         
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (socket.RemoteEndPoint.ToString().Equals(__ClientSockets[i]._Socket.RemoteEndPoint.ToString()))
                        {
                            __ClientSockets[i]._Name = text;
                            rich_Text.AppendText(":" + text+"\n");
                        }
                    }
                    label1.Text = label1.Text + text + Environment.NewLine;
                    //reponse = "Usuarios conectados";
                    Sendata(socket, label1.Text);
                    enviarInfo();
                }
                else
                {
                    for (int i = 0; i < __ClientSockets.Count; i++)
                    {
                        if (__ClientSockets[i]._Socket.RemoteEndPoint.ToString().Equals(socket.RemoteEndPoint.ToString()))
                        {
                            
                            if (label1.Text.Contains(__ClientSockets[i]._Name))
                            {
                                label1.Text= label1.Text.Replace(__ClientSockets[i]._Name, " ");
                            }                
                            contador--;
                            label3.Text = contador.ToString();
                            Sendata(socket, label1.Text);
                            enviarInfo();
                            __ClientSockets.RemoveAt(i);
                            lb_stt.Text = "Numero de clientes conectados: " + __ClientSockets.Count.ToString();
                        }
                    }
                }
            }
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(RecibeMensaje), socket);
        }
        void Sendata(Socket socket, string noidung)
        {
                if (socket.Connected)
                {
                    byte[] data = Encoding.ASCII.GetBytes(noidung);
                    socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
                    _serverSocket.BeginAccept(new AsyncCallback(AppceptCallback), null);
                }
                else
                {                    
                    return;
                }    
            

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
                        Sendata(__ClientSockets[j]._Socket, label1.Text);
                    }
                }
            }
            rich_Text.AppendText("\nServer: " + txt_Text.Text);

        }
        public void enviarPuntos()
        {
            
            for (int i = 0; i < lista_clientes.Items.Count; i++)
            {
                lista_clientes.SetSelected(i, true);
            }
            for (int i = 0; i < lista_clientes.SelectedItems.Count; i++)
            {
                string t = lista_clientes.SelectedItems[i].ToString();
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    {
                        Sendata(__ClientSockets[j]._Socket, lblScoreFinal.Text);
                        if (label3.Text == __ClientSockets.Count.ToString())
                        {
                            Sendata(__ClientSockets[j]._Socket, label3.Text+"°"+maximo.ToString());
                        }
                    }
                }
            }
        }
        public void enviarInfo()
        {
            for (int i = 0; i < lista_clientes.Items.Count; i++)
            {
                lista_clientes.SetSelected(i, true);
            }
            for (int i = 0; i < lista_clientes.SelectedItems.Count; i++)
            {
                string t = lista_clientes.SelectedItems[i].ToString();
                for (int j = 0; j < __ClientSockets.Count; j++)
                {
                    { 
                        Sendata(__ClientSockets[j]._Socket, label1.Text);
                    }
                }
            }
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
        public int puntos { get; set; }
        public SocketT2h(Socket socket)
        {
            _Socket = socket;
        }
    }
}
