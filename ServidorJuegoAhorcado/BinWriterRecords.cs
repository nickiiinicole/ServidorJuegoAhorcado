using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorJuegoAhorcado
{
    //realizamos un binarywriter/reader a medida cuando queremos escribir algo creado por nosotros...
    //1º heredar
    internal class BinWriterRecords : BinaryWriter
    {
        public BinWriterRecords(Stream str) : base(str)
        {
        }

        public void WriteRecord(Record record)
        {
            base.Write(record.Name);
            base.Write(record.Seconds);
        }

    }
}
