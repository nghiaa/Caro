using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Threading;

namespace Caro
{
    public partial class Form1 : Form
    {        
        private GameHandling gh;
        SocketManager socket;
        bool isPlaying;
        #region Phương thức
        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;

            gh = new GameHandling(panelBoard, txtName, picNext);
            gh.EClickedButton += Gh_EClickedButton; //sự kiện nhấn vào ô cờ do GameHandling gửi qua
            gh.EGameEnded += Gh_EGameEnded; //sự kiện kết thúc game do GameHandling gửi qua
            gh.DrawBoard();
            panelBoard.Enabled = false;
            socket = new SocketManager();
            isPlaying = false;
            CheckIP();
        }


        /// <summary>
        /// Kiểm tra ip của máy
        /// </summary>
        public void CheckIP()
        {
            txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);
            if (string.IsNullOrEmpty(txtIP.Text))
                txtIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
        }

        /// <summary>
        /// Hàm xử lý kết thúc game
        /// </summary>
        public void EndGame()
        {
            CoolDownTimer.Stop();
            panelBoard.Enabled = false;
        }

        private void NewGame()
        {
            pbCoolDown.Value = 0;
            gh.DrawBoard();
        }

        private void QuitGame(FormClosingEventArgs e)
        {
            if (MessageBox.Show("Bạn có muốn thoát không?", "Confirm", MessageBoxButtons.OKCancel) != System.Windows.Forms.DialogResult.OK)
            {
                e.Cancel = true;
            }
            else
            {
                try
                {
                    socket.Send(new SocketData((int)SocketCommand.QUIT, ""));
                    socket.Close();
                }
                catch (Exception)
                {

                }
            }
        }

        private void Listen()
        {
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();
                    DataHandling(data);
                    isPlaying = true;
                }
                catch (Exception e)
                {
                }
            });
            listenThread.IsBackground = true;
            listenThread.Start();
        }

        /// <summary>
        /// Xử lý gói dữ liệu dạng SocketData
        /// </summary>
        /// <param name="data"></param>
        private void DataHandling(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.NOTIFY:

                    break;

                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        panelBoard.Enabled = false;
                        CoolDownTimer.Stop();
                    }));
                    break;

                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pbCoolDown.Value = 0;
                        panelBoard.Enabled = true;
                        CoolDownTimer.Start();
                        gh.OtherPlayerMark(data.Point);
                    }));
                    break;

                //case (int)SocketCommand.END_GAME:
                //    MessageBox.Show("Bạn đã thua");
                //    break;

                case (int)SocketCommand.TIME_OUT:
                    EndGame();
                    MessageBox.Show("Bạn đã thắng!");
                    break;

                case (int)SocketCommand.QUIT:
                    CoolDownTimer.Stop();
                    MessageBox.Show("Đối thủ đã thoát ra!");
                    break;

                default:
                    break;
            }
            Listen();
        }
        #endregion

        #region Xử lý sự kiện
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CoolDownTimer.Stop();
            NewGame();
            socket.Send(new SocketData((int)SocketCommand.NEW_GAME, ""));
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            QuitGame(e);
        }

        private void Gh_EGameEnded(object sender, EventArgs e)
        {
            EndGame();
        }

        private void Gh_EClickedButton(object sender, ButtonClickEvent e)
        {
            CoolDownTimer.Start();
            panelBoard.Enabled = false;
            pbCoolDown.Value = 0;
            socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));
            isPlaying = false;
            Listen();
        }

        private void CoolDownTimer_Tick(object sender, EventArgs e)
        {
            pbCoolDown.PerformStep();
            if (isPlaying && pbCoolDown.Value == pbCoolDown.Maximum)
            {
                EndGame();
                socket.Send(new SocketData((int)SocketCommand.TIME_OUT));
                MessageBox.Show("Hết giờ! Bạn đã thua");
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            socket.IP = txtIP.Text;

            if (!socket.ConnectServer())
            {
                socket.isServer = true;
                status.Text = "Chờ kết nối từ client...";
                socket.CreateServer();
                Thread listenThread = new Thread(() =>
                {
                    while (true)
                    {
                        if (socket.IsConnected())
                        {
                            status.Text = "Kết nối thành công!";
                            panelBoard.Enabled = true;
                            break;
                        }
                    }
                });
                listenThread.IsBackground = true;
                listenThread.Start();
            }
            else
            {
                socket.isServer = false;
                string message = "Kết nối thành công!";
                status.Text = message;
                Listen();
            }
            btnConnect.Enabled = false;
        }
        #endregion
    }
}
