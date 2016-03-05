using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macrophotography.Classes
{
    public class Sensor
    {
        private int idSensor;
        private string nameSensor;
        private double pitch;
        private double e;
        private int lambda;
        private double n;

        public Sensor() { }

        public int IdSensor
        {
            get { return idSensor; }
            set { idSensor = value; }
        }

        public string NameSensor
        {
            get { return nameSensor; }
            set { nameSensor = value; }
        }
        public double Pitch
        {
            get { return pitch; }
            set { pitch = value; }
        }
        public double E
        {
            get { return e; }
            set { e = value; }
        }
        public int Lambda
        {
            get { return lambda; }
            set { lambda = value; }
        }

        public double N
        {
            get { return n; }
            set { n = value; }
        }
    }
}
