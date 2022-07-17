using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Society.Mapping
{
    public class HexCell
    {
        public int X { get; set; }
        public int Z { get; set; }

        public HexCell[] Neighbors = new HexCell[6];

        public bool[] Roads = new bool[6];

        public int Index { get; set; }

        public int ColumnIndex { get; set; }

        public int Elevation { get { return _elevation; } set { _elevation = value; } }

        public int WaterLevel { get { return _waterLevel; } set { _waterLevel = value; } }

        public int ViewElevation { get { return _elevation >= _waterLevel ? _elevation : _waterLevel; } }

        public bool IsUnderwater { get { return _waterLevel > _elevation; } }

        public HexDirection IncomingRiver { get { return _incomingRiver; } set { _incomingRiver = value; } }

        public HexDirection OutgoingRiver { get { return _outgoingRiver; } set { _outgoingRiver = value; } }

        public bool HasIncomingRiver { get { return _hasIncomingRiver; } set { _hasIncomingRiver = value; } }

        public bool HasOutgoingRiver { get { return _hasOutgoingRiver; } set { _hasOutgoingRiver = value; } }

        public bool HasRiver { get { return _hasIncomingRiver || _hasOutgoingRiver; } }

        public bool HasRiverBeginOrEnd { get { return _hasIncomingRiver != _hasOutgoingRiver; } }

        public HexDirection RiverBeginOrEndDirection { get { return _hasIncomingRiver ? _incomingRiver : _outgoingRiver; } }

        public bool HasRoads
        {
            get
            {
                for (int i = 0; i < Roads.Length; i++)
                {
                    if (Roads[i])
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public int UrbanLevel { get { return _urbanLevel; } set { _urbanLevel = value; } }

        public int FarmLevel { get { return _farmLevel; } set { _farmLevel = value; } }

        public int PlantLevel { get { return _plantLevel; } set { _plantLevel = value; } }

        public int SpecialIndex { get { return _specialIndex; } set { _specialIndex = value; } }

        public bool IsSpecial { get { return _specialIndex > 0; } }

        public bool Walled { get { return _walled; } set { _walled = value; } }

        public int TerrainTypeIndex {get { return _terrainTypeIndex; } set { _terrainTypeIndex = value; } }

        public int Distance{ get { return _distance; } set { _distance = value; } }

        private int _terrainTypeIndex;

        private int _elevation = int.MinValue;
        private int _waterLevel;

        private int _urbanLevel, _farmLevel, _plantLevel;

        private int _specialIndex;

        private int _distance;

        private bool _walled;

        private bool _hasIncomingRiver, _hasOutgoingRiver;
        private HexDirection _incomingRiver, _outgoingRiver;

        public HexCell GetNeighbor(HexDirection direction)
        {
            return Neighbors[(int)direction];
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            Neighbors[(int)direction] = cell;
            cell.Neighbors[(int)direction.Opposite()] = this;
        }

        public bool HasRiverThroughEdge(HexDirection direction)
        {
            return
                _hasIncomingRiver && _incomingRiver == direction ||
                _hasOutgoingRiver && _outgoingRiver == direction;
        }

        public bool HasRoadThroughEdge(HexDirection direction)
        {
            return Roads[(int)direction];
        }

        public int GetElevationDifference(HexDirection direction)
        {
            int difference = _elevation - GetNeighbor(direction)._elevation;
            return difference >= 0 ? difference : -difference;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write((byte)_terrainTypeIndex);
            writer.Write((byte)(_elevation + 127));
            writer.Write((byte)_waterLevel);
            writer.Write((byte)_urbanLevel);
            writer.Write((byte)_farmLevel);
            writer.Write((byte)_plantLevel);
            writer.Write((byte)_specialIndex);
            writer.Write(_walled);

            if (_hasIncomingRiver)
            {
                writer.Write((byte)(_incomingRiver + 128));
            }
            else
            {
                writer.Write((byte)0);
            }

            if (_hasOutgoingRiver)
            {
                writer.Write((byte)(_outgoingRiver + 128));
            }
            else
            {
                writer.Write((byte)0);
            }

            int roadFlags = 0;
            for (int i = 0; i < Roads.Length; i++)
            {
                if (Roads[i])
                {
                    roadFlags |= 1 << i;
                }
            }
            writer.Write((byte)roadFlags);
        }

        public void Load(BinaryReader reader, int header)
        {
            _terrainTypeIndex = reader.ReadByte();
            _elevation = reader.ReadByte();
            if (header >= 4)
            {
                _elevation -= 127;
            }
            _waterLevel = reader.ReadByte();
            _urbanLevel = reader.ReadByte();
            _farmLevel = reader.ReadByte();
            _plantLevel = reader.ReadByte();
            _specialIndex = reader.ReadByte();
            _walled = reader.ReadBoolean();

            byte riverData = reader.ReadByte();
            if (riverData >= 128)
            {
                _hasIncomingRiver = true;
                _incomingRiver = (HexDirection)(riverData - 128);
            }
            else
            {
                _hasIncomingRiver = false;
            }

            riverData = reader.ReadByte();
            if (riverData >= 128)
            {
                _hasOutgoingRiver = true;
                _outgoingRiver = (HexDirection)(riverData - 128);
            }
            else
            {
                _hasOutgoingRiver = false;
            }

            int roadFlags = reader.ReadByte();
            for (int i = 0; i < Roads.Length; i++)
            {
                Roads[i] = (roadFlags & (1 << i)) != 0;
            }
        }
    }
}

public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

public static class HexDirectionExtensions
{

    public static HexDirection Opposite(this HexDirection direction)
    {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }

    public static HexDirection Previous2(this HexDirection direction)
    {
        direction -= 2;
        return direction >= HexDirection.NE ? direction : (direction + 6);
    }

    public static HexDirection Next2(this HexDirection direction)
    {
        direction += 2;
        return direction <= HexDirection.NW ? direction : (direction - 6);
    }
}
