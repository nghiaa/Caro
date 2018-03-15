using Caro.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro
{
    public class GameHandling
    {
        #region Property         
        private Panel Board; //Bàn cờ
        private int currentPlayer; //Biến đếm xem người chơi hiện tại là ai
        List<Player> playerList; //Danh sách người chơi 
        TextBox playerName;
        PictureBox playerIcon;
        private List<List<Button>> matrix; //Danh sách các quân cờ
        public Panel BoardSetGet
        {
            get
            {
                return Board;
            }

            set
            {
                Board = value;
            }
        }

        public List<Player> PlayerList
        {
            get
            {
                return playerList;
            }

            set
            {
                playerList = value;
            }
        }

        public int CurrentPlayer
        {
            get
            {
                return currentPlayer;
            }

            set
            {
                currentPlayer = value;
            }
        }

        public TextBox PlayerName
        {
            get
            {
                return playerName;
            }

            set
            {
                playerName = value;
            }
        }

        public PictureBox PlayerIcon
        {
            get
            {
                return playerIcon;
            }

            set
            {
                playerIcon = value;
            }
        }

        public List<List<Button>> Matrix
        {
            get
            {
                return matrix;
            }

            set
            {
                matrix = value;
            }
        }

        /// <summary>
        /// Sự kiện click vào ô cờ
        /// </summary>
        private event EventHandler<ButtonClickEvent> eClickedButton;
        public event EventHandler<ButtonClickEvent> EClickedButton
        {
            add
            {
                eClickedButton += value;
            }
            remove
            {
                eClickedButton -= value;
            }
        }

        /// <summary>
        /// Sự kiện game kết thúc
        /// </summary>
        private event EventHandler eGameEnded;
        public event EventHandler EGameEnded
        {
            add
            {
                eGameEnded += value;
            }
            remove
            {
                eGameEnded -= value;
            }
        }
        #endregion


        #region Initialize
        /// <summary>
        /// Hàm khởi tạo bàn cờ, người chơi và quân cờ
        /// </summary>
        /// <param name="_panel"></param>
        /// <param name="_playerName"></param>
        /// <param name="_playerIcon"></param>
        public GameHandling(Panel _panel, TextBox _playerName, PictureBox _playerIcon)
        {
            BoardSetGet = _panel;
            PlayerList = new List<Player>()
            {
                new Player("nghia", Resources.circle),
                new Player("tai", Resources.cross)
            };
            PlayerName = _playerName;
            PlayerIcon = _playerIcon;
        }


        #endregion

        #region Method
        /// <summary>
        /// Hàm vẽ bàn cờ
        /// </summary>
        public void DrawBoard()
        {
            BoardSetGet.Controls.Clear();
            BoardSetGet.Enabled = true;
            CurrentPlayer = 0;
            ChangePlayer();

            matrix = new List<List<Button>>();
            Button origin = new Button() { Width = 0, Location = new Point(0, 0) }; //button ảo dùng để set vị trí ban đầu
            for (int i = 0; i < Constant.CHESSBOARD_HEIGHT; i++)
            {
                matrix.Add(new List<Button>()); //thêm 1 dòng vào danh sách
                for (int j = 0; j < Constant.CHESSBOARD_WIDTH; j++)
                {
                    Button btn = new Button()
                    {
                        Width = Constant.CHESS_WIDTH,
                        Height = Constant.CHESS_HEIGHT,
                        Location = new Point(origin.Location.X + origin.Width, origin.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i
                    };
                    btn.Click += Btn_Click; //gán sự kiện cho các ô cờ
                    BoardSetGet.Controls.Add(btn);
                    matrix[i].Add(btn); //Thêm một button vào dòng ở trên
                    origin = btn;
                }
                origin.Location = new Point(0, origin.Location.Y + Constant.CHESS_HEIGHT);
                origin.Width = 0;
                origin.Height = 0;
            }
        }

        /// <summary>
        /// Xử lý khi nhận cờ từ đối phương
        /// </summary>
        /// <param name="point"></param>
        public void OtherPlayerMark(Point point)
        {
            Button btn = Matrix[point.Y][point.X];
            if (btn.BackgroundImage != null)
                return;
            ChangePlayer();
            btn.BackgroundImage = PlayerList[CurrentPlayer].Mark;
            if (isEndGame(btn))
            {
                EndGame();
                MessageBox.Show("Bạn đã thua");
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn.BackgroundImage != null)
                return;
            ChangePlayer();
            btn.BackgroundImage = PlayerList[CurrentPlayer].Mark;

            eClickedButton?.Invoke(this, new ButtonClickEvent(GetPos(btn))); //Khi bấm nút sẽ gửi sự kiện click button lên form             
            if (isEndGame(btn))
            {
                EndGame();
                MessageBox.Show("Bạn đã thắng!");
            }
        }

        /// <summary>
        /// Lấy vị trí của nút trong List
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        private Point GetPos(Button btn)
        {
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = matrix[vertical].IndexOf(btn);
            Point point = new Point(horizontal, vertical);
            return point;
        }

        private void EndGame()
        {
            eGameEnded?.Invoke(this, new EventArgs());  //Khi kết thúc game bằng 5 nước cờ sẽ gửi sự kiện end game lên form            
        }

        /// <summary>
        /// Kiểm tra xem một trong các trường hợp kết thúc game có thỏa không
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        private bool isEndGame(Button btn)
        {
            return HangNgang(btn) || HangDoc(btn) || DuongCheoChinh(btn) || DuongCheoPhu(btn);
        }

        private bool HangNgang(Button btn)
        {
            Point btnPos = GetPos(btn);
            int count = 0;
            for (int i = btnPos.X; i >= 0; i--)
            {
                if (matrix[btnPos.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    count++;
                }
                else
                    break;
            }
            for (int i = btnPos.X + 1; i < Constant.CHESSBOARD_WIDTH; i++)
            {
                if (matrix[btnPos.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    count++;
                }
                else
                    break;
            }
            return count == 5;
        }
        private bool HangDoc(Button btn)
        {
            Point btnPos = GetPos(btn);
            int count = 0;
            for (int i = btnPos.Y; i >= 0; i--)
            {
                if (matrix[i][btnPos.X].BackgroundImage == btn.BackgroundImage)
                {
                    count++;
                }
                else
                    break;
            }
            for (int i = btnPos.Y + 1; i < Constant.CHESSBOARD_HEIGHT; i++)
            {
                if (matrix[i][btnPos.X].BackgroundImage == btn.BackgroundImage)
                {
                    count++;
                }
                else
                    break;
            }
            return count == 5;
        }
        private bool DuongCheoChinh(Button btn)
        {
            int count = 0;
            Point btnPos = GetPos(btn);
            for (int i = 0; i <= btnPos.X; i++)
            {
                if (btnPos.Y - i < 0 || btnPos.X - i < 0)
                    break;
                if (matrix[btnPos.Y - i][btnPos.X - i].BackgroundImage == btn.BackgroundImage)
                    count++;
                else
                    break;
            }

            for (int i = 1; i <= Constant.CHESSBOARD_WIDTH - btnPos.X; i++)
            {
                if (btnPos.Y + i >= Constant.CHESSBOARD_HEIGHT || btnPos.X + i >= Constant.CHESSBOARD_WIDTH)
                    break;
                if (matrix[btnPos.Y + i][btnPos.X + i].BackgroundImage == btn.BackgroundImage)
                    count++;
                else
                    break;
            }
            return count == 5;
        }
        private bool DuongCheoPhu(Button btn)
        {
            int count = 0;
            Point btnPos = GetPos(btn);
            for (int i = 0; i <= btnPos.X; i++)
            {
                if (btnPos.Y - i < 0 || btnPos.X + i >= Constant.CHESSBOARD_WIDTH)
                    break;
                if (matrix[btnPos.Y - i][btnPos.X + i].BackgroundImage == btn.BackgroundImage)
                    count++;
                else
                    break;
            }

            for (int i = 1; i < Constant.CHESSBOARD_WIDTH - btnPos.X; i++)
            {
                if (btnPos.Y + i >= Constant.CHESSBOARD_HEIGHT || btnPos.X - i < 0)
                    break;
                if (matrix[btnPos.Y + i][btnPos.X - i].BackgroundImage == btn.BackgroundImage)
                    count++;
                else
                    break;
            }
            return count == 5;
        }

        /// <summary>
        /// Thay đổi tên và quân cờ người chơi
        /// </summary>
        private void ChangePlayer()
        {
            PlayerName.Text = PlayerList[CurrentPlayer].Name;
            PlayerIcon.BackgroundImage = PlayerList[CurrentPlayer].Mark;
            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;
        }
        #endregion
    }
}
