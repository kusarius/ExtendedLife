using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Extended_Life {
    struct Cell {
        public int PreferedNeighboursNumber;
        public bool IsAlive;

        public Cell(int preferedfNeigboursNum, bool isAlive) {
            PreferedNeighboursNumber = preferedfNeigboursNum;
            IsAlive = isAlive;
        }
    }
}
