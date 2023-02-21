using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Control = System.Windows.Forms.Control;

namespace MouseMovements.Model
{

    enum MovementType
    {
        RandomMovement,
        ButtonClick
    }
    public struct MousePoint
    {
        public Point coordinate;
        public long elapsedMS;
    }

}
