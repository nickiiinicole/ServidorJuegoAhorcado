using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorJuegoAhorcado
{
    internal class BinReaderRecord : BinaryReader
    {
        public BinReaderRecord(Stream str) : base(str)
        {
        }

        public Record ReadRecord()
        {
            Record record = new Record();
            record.Name = base.ReadString();
            record.Seconds = base.ReadInt32();
            return record;
        }
    }
}
