using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macrophotography.Classes
{
    public class Lens
    {
        private int idLens;
        private string nameLens;
        private double aperture;
        private double na;
        private string manualLens;
        private string microLens;

        public Lens() { }

        public int IdLens
        {
            get { return idLens; }
            set { idLens = value; }
        }

        public string NameLens
        {
            get { return nameLens; }
            set { nameLens = value; }
        }
        public double Aperture
        {
            get { return aperture; }
            set { aperture = value; }
        }
        public double NA
        {
            get { return na; }
            set { na = value; }
        }
        public string ManualLens
        {
            get { return manualLens; }
            set { manualLens = value; }
        }

        public string MicroLens
        {
            get { return microLens; }
            set { microLens = value; }
        }
    }
}
