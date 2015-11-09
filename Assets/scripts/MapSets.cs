using System;

[Flags]
public enum LevelFlags { none = 0, race = 1, advanced = 2, tested = 4, /*stunts = 8,*/ Ctf = 16, dm = 32, }

public class MapSets
{
    public bool usedAdvancedTools { get { return GetFlag(LevelFlags.advanced); } set { SetFlag(LevelFlags.advanced, value); } }
    public bool tested { get { return GetFlag(LevelFlags.tested); } set { SetFlag(LevelFlags.tested, value); } }
    //public bool enableCoins { get { return GetFlag(LevelFlags.stunts); } private set { SetFlag(LevelFlags.stunts, value); } }
    public bool enableCtf { get { return GetFlag(LevelFlags.Ctf); } set { SetFlag(LevelFlags.Ctf, value); } }
    public bool enableDm { get { return GetFlag(LevelFlags.dm); } set { SetFlag(LevelFlags.dm, value); } }
    public bool race { get { return GetFlag(LevelFlags.race) || levelFlags == 0; } private set { SetFlag(LevelFlags.race, value); } }
    
    public LevelFlags levelFlags;
    private void SetFlag(LevelFlags flag, bool value)
    {
        if (value)
            levelFlags |= flag;
        else
            levelFlags &= ~flag;
    }
    private bool GetFlag(LevelFlags flag)
    {
        return (levelFlags & flag) != 0;
    }
}