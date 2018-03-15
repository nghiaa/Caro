using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro
{
    public class Player
    {
        private string name;
        private Image mark;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public Image Mark
        {
            get
            {
                return mark;
            }

            set
            {
                mark = value;
            }
        }

        public Player(string _name, Image _mark)
        {
            Name = _name;
            Mark = _mark;
        }
    }
}
