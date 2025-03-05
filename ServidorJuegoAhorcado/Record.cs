using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorJuegoAhorcado
{
    internal class Record
    {
        private string name;
        private int seconds;
        public Record(string name, int seconds)
        {
            this.Name = name;
            this.Seconds = seconds;
        }
        public string Name
        {
            get { return name; }
            set
            {
                name = value.Length >= 3 ? value.Substring(0, 3) : value.PadRight(3, ' '); 
                // Si es menor, lo rellena con espacios ;
            }
        }
        public int Seconds
        {
            get { return seconds; }

            set { seconds = value; }
        }

        public Record()
        {

        }

    }
}
