using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macrophotography.Classes
{
    public class Rail
    {
        private int idRail;
        private string nameRail;
        private string motor_steps;
        private string micro_steps;
        private string ball_screw;
        private string gear_box;

        public Rail() { }

        public int IdRail
        {
            get { return idRail; }
            set { idRail = value; }
        }

        public string NameRail
        {
            get { return nameRail; }
            set { nameRail = value; }
        }
        public string Motor_steps
        {
            get { return motor_steps; }
            set { motor_steps = value; }
        }
        public string Micro_steps
        {
            get { return micro_steps; }
            set { micro_steps = value; }
        }
        public string Ball_screw
        {
            get { return ball_screw; }
            set { ball_screw = value; }
        }

        public string Gear_box
        {
            get { return gear_box; }
            set { gear_box = value; }
        }
    }
}
