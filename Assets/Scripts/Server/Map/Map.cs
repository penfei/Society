using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Society.Mapping
{
    public class Map
    {
        public int CellCountX = 20, CellCountZ = 15;

        public bool Wrapping;

        public HexCell[] Cells;

        public void Save(BinaryWriter writer)
        {
            writer.Write(CellCountX);
            writer.Write(CellCountZ);
            writer.Write(Wrapping);

            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i].Save(writer);
            }
        }

        public void Load(BinaryReader reader, int header)
        {
            if (header >= 1)
            {
                CellCountX = reader.ReadInt32();
                CellCountZ = reader.ReadInt32();
            }
            bool wrapping = header >= 5 ? reader.ReadBoolean() : false;

            Cells = new HexCell[CellCountZ * CellCountX];

            for (int i = 0; i < Cells.Length; i++)
            {
                Cells[i] = new HexCell();
                Cells[i].Load(reader, header);
            }

        }
    }
}
