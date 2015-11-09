using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using qint32 = System.Int32;
using QString = System.String;
namespace ConsoleApplication1
{
    internal class GbxMap
    {

        public void Load()
        {
            var parser = new GbxParser(File.OpenRead(@"C:\Users\Administrator\Desktop\A06.Map.Gbx"));
            parser.ReadBytes(3 + 2 + 4); // GBX + Version + Storage settings
            qint32 cid = parser.ReadInt32(); // Main class id
            GbxClass dclass = new GbxClass();
            Classes.Add(-2, dclass);
            dclass.Id = cid;
            dclass.Structure = new CGameCtnChallenge_Header();
            qint32 length = parser.ReadInt32(); // Header length
            qint32 hcnkct = parser.ReadInt32(); // Header chunk count
            var cnkids = new int[hcnkct];
            var cnkszs = new int[length];
            long bigcnk = 2147483648; // Some constant (oh so ugly)
            // Go through all chunks
            for (int i = 0; i < hcnkct; i++)
            {
                qint32 cnkid = parser.ReadInt32();
                qint32 cnksz = parser.ReadInt32();
                if (cnksz < 0)
                {
                    cnksz = (int)(cnksz + bigcnk);
                }
                cnkids[i] = cnkid;
                cnkszs[i] = cnksz;
            }
            // Read every chunk
            for (int i = 0; i < hcnkct; i++)
            {
                GbxChunk dchunk = new GbxChunk();
                dchunk.Id = cnkids[i];
                qint32 start = parser.position;
                parser.ResetStringLB();
                switch (cnkids[i])
                {
                    case unchecked((int)0x03043002):
                        {
                            CGameCtnChallenge_Header c = dclass.Structure;
                            parser.ReadBytes(5);
                            c.PTimeBronze = parser.ReadInt32();
                            c.PTimeSilver = parser.ReadInt32();
                            c.PTimeGold = parser.ReadInt32();
                            c.PTimeAuthor = parser.ReadInt32();
                            c.PMapPrice = parser.ReadInt32();
                            c.PMultilap = (parser.ReadInt32() == 1);
                            parser.ReadBytes(12);
                            c.PSimpleEditor = (parser.ReadInt32() == 1);
                            parser.ReadBytes(4);
                            c.PMapCheckpoints = (parser.ReadInt32());
                            parser.ReadBytes(4);
                            break;
                        }
                    case unchecked((int)0x03043003):
                        {
                            CGameCtnChallenge_Header c = dclass.Structure;
                            var cp = parser.position;
                            parser.ReadBytes(1);
                            c.PMapUniqueId = (parser.ReadStringLB());
                            c.PMapEnvironment = (parser.ReadStringLB());
                            c.PMapAuthor = (parser.ReadStringLB());
                            c.PMapName = (parser.ReadString());
                            parser.Skip(5);
                            parser.ReadString();
                            c.PMapMood = parser.ReadStringLB();
                            c.PMapDecorationEnvironment = parser.ReadStringLB();
                            c.PMapDecorationEnvironmentAuthor = parser.ReadStringLB();
                            parser.Skip(4 * 4 + 16);
                            c.PMapType = parser.ReadString();
                            c.PMapStyle = parser.ReadString();
                            parser.Skip(9);
                            c.PMapTitleUid = parser.ReadStringLB();
                            parser.Skip(cnkszs[i] - (parser.position - cp));
                            break;
                        }
                    case unchecked((int)0x03043005):
                        {
                            CGameCtnChallenge_Header c = dclass.Structure;
                            c.PXml = parser.ReadString();
                            break;
                        }
                    case unchecked((int)0x03043007):
                        {
                            CGameCtnChallenge_Header c = dclass.Structure;
                            int cp = parser.position;
                            parser.Skip(1);
                            qint32 size = parser.ReadInt32();
                            parser.Skip(15 + 3);
                            //c.PMapSnapshot = parser.ReadChars(size);
                            parser.Skip(cnkszs[i] - (parser.position - cp));
                            break;
                        }
                    case unchecked((int)0x03043008):
                        {
                            CGameCtnChallenge_Header c = dclass.Structure;
                            c.PAutVersion = parser.ReadInt32();
                            c.PAutAuthorVersion = parser.ReadInt32();
                            c.PAutLogin = parser.ReadString();
                            c.PAutNick = parser.ReadString();
                            c.PAutZone = parser.ReadString();
                            c.PAutExtra = parser.ReadString();
                            break;
                        }
                    default:
                        {
                            parser.Skip(cnkszs[i]);
                            break;
                        }
                }
                // Copy this chunks data into some variable so while writing it's obtainable
                dchunk.Data = parser.GetChars(start, parser.position - start);
                dclass.Chunks.Add(dchunk);
            }

            // Starting to parse the data section
            Nodes = parser.ReadInt32();
            NodesExternal = parser.ReadInt32();
            // Not supported yet
            if (NodesExternal != 0)
                throw new Exception("Your map cannot be parsed as it has external dependencies.");

            qint32 datasz = parser.ReadInt32();
            qint32 datacsz = parser.ReadInt32();
            var data = new byte[datasz];
            var cdata = parser.ReadBytes(datacsz);
            MiniLZO.MiniLZO.Decompress(cdata, data);

            GbxParser parserd = new GbxParser(new MemoryStream(data));
            ReadNode(ref parserd, cid, -1);
            isLoaded = true;

        }
        //std::vector<MapBlock*> Blocks;
        public Dictionary<int, GbxClass> Classes = new Dictionary<int, GbxClass>(); // key: -2: header main class, -1 data main class, >= 0 subnode class

        // Nodes
        public int Nodes;
        public int NodesExternal;

        public QString sourceFileName;
        public bool isLoaded = false;
        qint32 cid = 0;
        qint32 oldcid = 0;
        public List<MapBlock> mapBlocks=new List<MapBlock>();
        private void ReadNode(ref GbxParser parserd, qint32 classid, int pushdepth)
        {
            GbxClass dclass = new GbxClass();
            Classes.Add(pushdepth, dclass);
            dclass.Id = classid;
            switch (classid)
            {
                case unchecked((int)0x03043000):
                    dclass.Structure = new CGameCtnChallenge();
                    break;
                case unchecked((int)0x03093000):
                    dclass.Structure = new CGameCtnReplayRecord();
                    break;
                case unchecked((int)0x03059000):
                    dclass.Structure = new CGameCtnBlockSkin();
                    break;
                case unchecked((int)0x0313B000):
                    dclass.Structure = new CGameWaypointSpecialProperty();
                    break;
                default:
                    Debug.Log("Can not find a class struct..");
                    break;
            }

            // Parse data section

            while (true)
            {
                GbxChunk dchunk = new GbxChunk();
                oldcid = cid;
                cid = parserd.ReadInt32();
                dchunk.Id = cid;
                // Quit once we see this..
                if (cid == unchecked((int)0xFACADE01))
                    break;

                qint32 skip = parserd.ReadInt32();
                qint32 skipsize = -1;
                dchunk.Skipable = false;
                if (skip == 1397442896)
                {
                    skipsize = parserd.ReadInt32();
                    dchunk.Skipable = true;
                }
                else
                {
                    parserd.position -= 4;
                }
                qint32 start = parserd.position;
                switch (cid)
                {
                    // MAPS //
                    case unchecked((int)0x0304300D):
                        {
                            parserd.ReadStringLB();
                            parserd.ReadStringLB();
                            parserd.ReadStringLB();
                            break;
                        }
                    case unchecked((int)0x03043011):
                        {
                            // NodeReference
                            qint32 nodid = parserd.ReadInt32();
                            if (nodid >= 0 && !Classes.ContainsKey(nodid))
                            {
                                qint32 nodeclassid = parserd.ReadInt32();
                                ReadNode(ref parserd, nodeclassid, nodid);
                            }
                            // NodeReference
                            qint32 nodid2 = parserd.ReadInt32();
                            if (nodid2 >= 0 && !Classes.ContainsKey(nodid2))
                            {
                                qint32 nodeclassid = parserd.ReadInt32();
                                ReadNode(ref parserd, nodeclassid, nodid2);
                            }
                            parserd.ReadInt32();
                            break;
                        }
                    case unchecked((int)0x0301B000):
                        {
                            qint32 itemsct = parserd.ReadInt32();
                            for (int i = 0; i < itemsct; i++)
                            {
                                parserd.ReadStringLB();
                                parserd.ReadStringLB();
                                parserd.ReadStringLB();
                                parserd.ReadInt32();
                            }
                            break;

                        }
                    case unchecked((int)0x0305B000):
                        {
                            //    parserd.Skip(8*4);
                            break;
                        }
                    case unchecked((int)0x0305B001):
                        {
                            parserd.ReadString();
                            parserd.ReadString();
                            parserd.ReadString();
                            parserd.ReadString();
                            break;
                            //} case unchecked((int)0x0305B002): {
                            //} case unchecked((int)0x0305B003): {
                        }
                    case unchecked((int)0x0305B004):
                        {
                            parserd.Skip(5 * 4);
                            break;
                            //} case unchecked((int)0x0305B005): {
                            //} case unchecked((int)0x0305B006): {
                        }
                    case unchecked((int)0x0305B008):
                        {
                            parserd.Skip(2 * 4);
                            break;
                        }
                    case unchecked((int)0x0305B00A):
                        {
                            parserd.Skip(9 * 4);
                            break;
                        }
                    case unchecked((int)0x0305B00D):
                        {
                            parserd.Skip(1 * 4);
                            break;
                        }
                    case unchecked((int)0x0304301F):
                        {
                            // Blocks chunk
                            //if (parserd.position == 344) Debugger.Break();
                            //var pos = parserd.position;
                            var c = dclass.Structure;
                            c.MapUId = parserd.ReadStringLB();
                            c.Environment = parserd.ReadStringLB();
                            c.MapAuthor = parserd.ReadStringLB();
                            c.MapName = parserd.ReadString();
                            c.Mood = parserd.ReadStringLB();
                            c.EnvironmentBackground = parserd.ReadStringLB();
                            c.EnvironmentAuthor = parserd.ReadStringLB();

                            c.MapSizeX = parserd.ReadInt32();
                            c.MapSizeY = parserd.ReadInt32();
                            c.MapSizeZ = parserd.ReadInt32();
                            c.ReqUnlock = parserd.ReadInt32();
                            c.Flags32 = parserd.ReadInt32();
                            int numblocks = parserd.ReadInt32();
                            // Read every single block..
                            bool oneMore = false;
                            for (int i = 0; i < numblocks; i++)
                            {
                                MapBlock block = new MapBlock();
                                mapBlocks.Add(block);
                                block.BlockName = parserd.ReadStringLB();
                                block.Rotation = parserd.ReadByte();
                                block.PositionX = parserd.ReadByte();
                                block.PositionY = parserd.ReadByte();
                                block.PositionZ = parserd.ReadByte();
                                block.Flags = (c.Flags32 > 0) ? parserd.ReadUInt32() : parserd.ReadUInt16();
                                if (block.Flags != 0xFFFFFFFF)
                                    if ((block.Flags & 0x8000) != 0)
                                    {
                                        block.BlockSkinAuthor = parserd.ReadStringLB();
                                        // NodeReference
                                        block.BlockSkin = parserd.ReadInt32();
                                        if (block.BlockSkin >= 0 && !Classes.ContainsKey(block.BlockSkin)) // If not yet read..
                                        {
                                            qint32 cidd = parserd.ReadInt32();
                                            ReadNode(ref parserd, cidd, block.BlockSkin);
                                        }
                                    }
                                if (block.Flags != 0xFFFFFFFF && (block.Flags & 0x100000) != 0)
                                {
                                    // NodeReference
                                    block.BlockParams = parserd.ReadInt32();
                                    if (block.BlockParams >= 0 && !Classes.ContainsKey(block.BlockParams)) // If not yet read..
                                    {
                                        qint32 cidd = parserd.ReadInt32();
                                        ReadNode(ref parserd, cidd, block.BlockParams);
                                    }
                                }
                                c.Blocks.Add(block);
                                if (block.Flags == 0xFFFFFFFF)
                                    i -= 1;
                                if (oneMore)
                                    break;
                                if (i + 1 == numblocks) // We're at the latest block now, check if there could be an undefined block next...
                                {
                                    if (parserd.ReadInt32() == 10)
                                    {
                                        oneMore = true;
                                        i -= 1;
                                    }
                                    parserd.Skip(-4);
                                }
                            }
                            c.Trails = parserd.ReadBytes(12);
                            break;
                        } 
                    case unchecked((int)0x03043022):
                        {
                            parserd.ReadInt32();
                            break;
                        }
                    case unchecked((int)0x03043024):
                        {
                            var c = Classes[-1].Structure;
                            parserd.ReadChar();
                            parserd.Skip(32); // ?
                            c.PMapMusic = parserd.ReadString();
                            c.PMapMusicLoc = parserd.ReadString();
                            break;
                        }
                    case unchecked((int)0x03043025):
                        {
                            parserd.Skip(16); // 2x vec2d, float: 4 bytes
                            break;
                        }
                    case unchecked((int)0x03043026):
                        {
                            qint32 ind = parserd.ReadInt32();
                            if (ind >= 0 && !Classes.ContainsKey(ind))
                            {
                                qint32 cidd = parserd.ReadInt32();
                                ReadNode(ref parserd, cidd, ind);
                            }
                            break;
                        }
                    case unchecked((int)0x03043028):
                        {
                            qint32 p = parserd.ReadInt32();
                            if (p != 0)
                            {
                                parserd.Skip(1 + 4 * 3 * 3 + 4 * 3 + 4 + 4 + 4);
                            }
                            // Ugly temporarily fix, just to show it
                            CGameCtnChallenge_Header c = Classes[-2].Structure;
                            c.PMapComments = parserd.ReadString();
                            break;
                        }
                    case unchecked((int)0x0304302A):
                        {
                            parserd.ReadInt32();
                            break;
                        }
                    case unchecked((int)0x03043049):
                        {
                            // MT stuff
                            // No idea how to parse it so for now just read the rest
                            parserd.ReadBytes((int)(parserd.Length() - parserd.position - 4)); // the rest of the data - 4 (final unchecked((int)0xfacade01) )
                            //return;
                            break;
                        }
                    case unchecked((int)0x03043029):
                        {
                            var c = Classes[-1].Structure;
                            c.PMapPasswordHash = parserd.ReadBytes(16);
                            c.PMapPasswordCrc = parserd.ReadInt32();
                            break;
                        }
                    case unchecked((int)0x03059002):
                        {
                            dclass.StructureName = parserd.ReadString();
                            int version = parserd.ReadChar();
                            if (version != 3)
                                throw new Exception("Please re-save your map in the latest ManiaPlanet version in order to load the map in MapEdit.\nReason: FileReference version is outdated.");
                            parserd.Skip(32); // ?
                            QString a = parserd.ReadString();
                            dclass.Structure.Path = a;
                            //if (a.length() > 0)
                            dclass.Structure.PathLoc = parserd.ReadString();
                            parserd.ReadChar();
                            parserd.Skip(32); // ?
                            QString b = parserd.ReadString();
                            dclass.Structure.LocPath = b;
                            //if (b.length() > 0)
                            dclass.Structure.LocPathLoc = parserd.ReadString();
                            break;
                        }
                    case unchecked((int)0x0313B000):
                        {
                            dclass.Structure.Unknown = parserd.ReadInt32();
                            if (dclass.Structure.Unknown != 2)
                                throw new Exception("Please re-save your map in the latest ManiaPlanet version in order to load the map in MapEdit.\nReason: CGameWaypointSpecialProperty version is outdated.");
                            dclass.Structure.Tag = parserd.ReadString();
                            dclass.Structure.Order = parserd.ReadInt32();
                            break;

                            // REPLAYS //
                        }
                    case unchecked((int)0x03093002):
                        {
                            qint32 size = parserd.ReadInt32();
                            dclass.Structure.mapGbx = parserd.ReadBytes(size);
                            return; // Stop parsing after this part, replays are not fully parsed besides this.

                            // DEFAULT HANDLING //
                        }
                    default:
                        {
                            if (skipsize != -1)
                            {
                                parserd.Skip(skipsize);
                                cid = oldcid;
                            }
                            else
                            {
                                Debug.Log("Unknown chunk id 0x");
                                return; // temporarily
                                continue;
                            }
                            break;
                        }
                }
                dchunk.Data = parserd.GetChars(start, parserd.position - start);
                dclass.Chunks.Add(dchunk);
            }
        }
    }
}
