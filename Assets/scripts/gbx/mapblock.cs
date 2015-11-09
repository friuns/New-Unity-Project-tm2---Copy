using qint32 = System.Int32;
using QString = System.String;
public class MapBlock
{
	public MapBlock()
	{
	}
	public QString BlockName;
	public byte PositionX;
	public byte PositionY;
	public byte PositionZ;
	public byte Rotation;
	public uint Flags;
	public int BlockSkin = -1;
	public QString BlockSkinAuthor;
	public int BlockParams = -1;

}
