using System.Collections.Generic;
using qint32 = System.Int32;
using QString = System.String;
//C++ TO C# CONVERTER WARNING: The following #include directive was ignored:
//#include <boost/any.hpp>

public class GbxClass
{
	public GbxClass()
	{
	}
	public List<GbxChunk> Chunks = new List<GbxChunk>();
	public qint32 Id = new qint32();
    public CGameCtnChallenge_Header Structure = new CGameCtnChallenge_Header();
    public string StructureName;
}

public class CGameCtnChallenge_Header
{
	// 03043002 (generic, header) - Incomplete, can not be written
	public int PTimeAuthor;
	public int PTimeGold;
	public int PTimeSilver;
	public int PTimeBronze;
	public int PMapPrice;
	public bool PMultilap;
	public bool PSimpleEditor;
	public int PMapCheckpoints;

	// 03043003 (generic, header) - Incomplete, can not be written
	public QString PMapName;
	public QString PMapUniqueId;
	public QString PMapEnvironment;
	public QString PMapAuthor;
	public QString PMapMood;
	public QString PMapDecorationEnvironment;
	public QString PMapDecorationEnvironmentAuthor;
	public QString PMapType;
	public QString PMapStyle;
	public QString PMapTitleUid;

	// 03043005 (xml)
	public QString PXml;

	// 03043007 (thumb/comments, header) - Snapshot size seems weird, can not be written
    public byte[] PMapSnapshot ;
	public QString PMapComments;

	// 03043008 (author info, header)
	public qint32 PAutVersion = new qint32();
	public qint32 PAutAuthorVersion = new qint32();
	public QString PAutLogin;
	public QString PAutNick;
	public QString PAutZone;
	public QString PAutExtra;



    // 0x0304301F (blocks)
    public QString MapUId;
    public QString Environment;
    public QString MapAuthor;
    public QString MapName;
    public QString Mood;
    public QString EnvironmentBackground;
    public QString EnvironmentAuthor;
    public qint32 MapSizeX = new qint32();
    public qint32 MapSizeY = new qint32();
    public qint32 MapSizeZ = new qint32();
    public qint32 ReqUnlock = new qint32();
    public qint32 Flags32 = new qint32();
    public List<MapBlock> Blocks = new List<MapBlock>();
    public byte[] Trails;

    // 0x03043029 (password)
    public byte[] PMapPasswordHash ;
    public qint32 PMapPasswordCrc = new qint32();

    // 03043024 (music)
    public QString PMapMusic;
    public QString PMapMusicLoc;

    // 0x03093002 (map file)
    public byte[] mapGbx ;

    // 0x03059002
    public QString Text;
    public QString Path;
    public QString PathLoc;
    public QString LocPath;
    public QString LocPathLoc;

    public qint32 Order = new qint32();
    public QString Tag;
    public qint32 Unknown = new qint32();

}

public class CGameCtnChallenge : CGameCtnChallenge_Header
{

}

public class CGameCtnReplayRecord : CGameCtnChallenge_Header
{
	
}

public class CGameCtnBlockSkin : CGameCtnChallenge_Header
{

}

public class CGameWaypointSpecialProperty : CGameCtnChallenge_Header
{

}
