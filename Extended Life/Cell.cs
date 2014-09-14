using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Extended_Life {
    [Serializable]
    class Cell {
        public int PreferedNeighboursNumber { get; set; }
        public bool IsAlive { get; set; }

        public Cell() { }
        public Cell(int preferedfNeigboursNum, bool isAlive) {
            PreferedNeighboursNumber = preferedfNeigboursNum;
            IsAlive = isAlive;
        }
    }
}
