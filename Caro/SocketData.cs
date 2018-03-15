using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro
{
    /// <summary>
    /// Dữ liệu sẽ được gửi qua lại giữa hai bên client/server
    /// </summary>
    [Serializable]
    public class SocketData
    {
        private int command;

        public int Command
        {
            get { return command; }
            set { command = value; }
        }

        private Point point;

        public Point Point
        {
            get { return point; }
            set { point = value; }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public SocketData(int command, string message, Point point)
        {
            this.Command = command;
            this.Point = point;
            this.Message = message;
        }
        public SocketData(int command, string message)
        {
            this.Command = command;
            this.Message = message;
        }
        public SocketData(int command)
        {
            this.Command = command;
        }
    }

    /// <summary>
    /// Các dạng gói tin sẽ gửi
    /// </summary>
    public enum SocketCommand
    {
        NOTIFY,
        SEND_POINT,
        NEW_GAME,
        TIME_OUT,
        END_GAME,
        QUIT
    }
}
