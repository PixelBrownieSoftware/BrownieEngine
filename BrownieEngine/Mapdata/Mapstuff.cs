using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

using TRead = BrownieEngine.s_map;
using System.ComponentModel;

namespace BrownieEngine
{
    public class s_saver {

        public void Save(string directory, s_map mp) {

            XmlWriterSettings xsettings = new XmlWriterSettings();
            xsettings.Indent = true;
            using (XmlWriter wr = XmlWriter.Create(directory, xsettings))
            {
                IntermediateSerializer.Serialize(wr, mp, null);
            }
        }
    }

    public enum TYPE_MODE
    {
        INT,
        FLOAT,
        STRING
    }

    [Serializable]
    [XmlRoot("globals")]
    public class s_GameGlobals {
        public string game;
        public List<s_objData> objData;
        public List<e_variable> flags;
        public List<ev_detail> staticEvents;
    }

    public struct o_tileRot
    {
        public o_tileRot(ushort tileAmount, byte tileRotNum)
        {
            this.tileAmount = tileAmount;
            this.tileRotNum = tileRotNum;
        }
        public ushort tileAmount;
        public byte tileRotNum;
    }
    public struct o_tile {

        public o_tile(ushort tileAmount, ushort tileNum) {
            this.tileAmount = tileAmount;
            this.tileNum = tileNum;
        }

        public ushort tileAmount;
        public ushort tileNum;
    }

    [XmlType("o_entity")]
    public class s_objData
    {
        public string name;
        public List<e_variable> types;
    }

    [XmlType("ev_detail")]
    public class ev_detail
    {
        public int evType;
        public dynamic var1;
        public dynamic var2;
        public dynamic var3;
        public dynamic var4;
        public dynamic var5;
        public dynamic var6;
        public dynamic var7;
    }

    [Serializable]
    [XmlRoot("map")]
    [XmlInclude(typeof(o_entity))]
    public class s_map
    {
        [XmlElement("game")]
        public string game;
        [XmlElement("ver")]
        public string version;
        [XmlArray("entites"), XmlArrayItem("object")]
        public List<o_entity> entities = new List<o_entity>();
        [XmlArray("tilesC"), XmlArrayItem("t")]
        public ushort[] tiles;
        public o_tile[] tilesCompr;
        [XmlArray("tileRot"), XmlArrayItem("tr")]
        public o_tileRot[] tileRotation;
        [XmlElement("tilsizeX")]
        public byte tileSizeX;
        [XmlElement("tilsizeY")]
        public byte tileSizeY;

        [XmlElement("mapSizeX")]
        public ushort mapSizeX;
        [XmlElement("mapSizeY")]
        public ushort mapSizeY;
        [XmlElement("tilesetN")]
        public string tileSetName;
        [XmlArray("events"), XmlArrayItem("script")]
        public List<ev_detail> Events;

    }

    //A small workaround for serializable tuples
    [XmlType("e_variable")]
    public class e_variable {

        [XmlElement("nam")]
        public string name { get; set; }
        [XmlElement("var")]
        public dynamic variable { get; set; }

        public e_variable(string name, dynamic variable) {
            this.name = name;
            this.variable = variable;
        }
        public e_variable() { }

    }

    [Serializable]
    [XmlType("o_entity")]
    public class o_entity
    {
        [XmlElement("name")]
        public string name;
        [XmlElement("id")]
        public ushort id = 0;
        [XmlElement("posX")]
        public float posX;
        [XmlElement("posY")]
        public float posY;
        [XmlIgnore]
        public Point position = new Point(0, 0);
        [XmlElement("label")]
        public ushort labelToCall = 0;
        [XmlIgnore]
        public List<Tuple<string, string>> stringlist;
        [XmlIgnore]
        public List<Tuple<string, float>> floatlist;
        [XmlIgnore]
        public List<Tuple<string, int>> intlist;
        [XmlIgnore]
        public List<Tuple<string, short>> shortlist;
        [XmlArray("varlist"),XmlArrayItem("var")]
        //[XmlIgnore] 
        public List<e_variable> variables;

        public string GetFlag(string name)
        {
            if (stringlist == null)
                return "";
            if (stringlist.Find(x => name == x.Item1) != null)
                return stringlist.Find(x => name == x.Item1).Item2;
            else
                return "";
        }
        public float GetFlagFloat(string name)
        {
            if (stringlist == null)
                return 0;
            if (stringlist.Find(x => name == x.Item1) != null)
            {
                float result = 0;
                string r = stringlist.Find(x => name == x.Item1).Item2;
                result = float.Parse(r);
                return result;
            }
            else
                return 0;
        }
    }

    public class s_levelreader : ContentTypeReader<TRead>
    {
        protected override TRead Read(ContentReader input, TRead existingInstance)
        {
            TRead map = new TRead();
            map.version = input.ReadString();
            var s1 = input.ReadByte();
            //Console.Out.WriteLine("Integer: " + s1 + " At memory address: " + input.BaseStream.Position);
            var s2 = input.ReadByte();
            //Console.Out.WriteLine("Integer: " + s2 + " At memory address: " + input.BaseStream.Position);

            map.tileSizeX = s1;
            map.tileSizeY = s2;
            
            var ws1 = input.ReadUInt16();
            var ws2 = input.ReadUInt16();

            map.mapSizeX = ws1;
            map.mapSizeY = ws2;
            
            //Tiles
            ushort[] tiles = new ushort[ws1 * ws2];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = input.ReadUInt16();
            }
            map.tiles = tiles;
            string tileSetName = input.ReadString();
            map.tileSetName = tileSetName;
            
            if ((int)(input.BaseStream.Position + sizeof(ushort)) > (int)input.BaseStream.Length)
            {
                input.Close();
                return map;
            }
            ushort leng = input.ReadUInt16();

            map.entities = new List<o_entity>();
            for (ushort i = 0; i < leng; i++)
            {
                o_entity ent = new o_entity();
                ushort id = input.ReadUInt16();
                int x = input.ReadInt32();
                int y = input.ReadInt32();
                ushort label = input.ReadUInt16();

                ent.id = id;
                ent.position = new Point(x, y);
                ent.labelToCall = label;

                if (input.ReadBoolean())
                {
                    ent.stringlist = new List<Tuple<string, string>>();
                    int lengthOfFlags = input.ReadInt16();
                    for (int i2 = 0; i2 < lengthOfFlags; i2++)
                    {
                        string st = input.ReadString();
                        short l = input.ReadInt16();
                        for (int i3 = 0; i3 < l; i3++)
                        {
                            Tuple<string, string> str = new Tuple<string, string>(st, input.ReadString());
                            ent.stringlist.Add(str);
                        }
                    }
                }
                map.entities.Add(ent);
            }

            input.Close();
            return map;
        }
    }


}
