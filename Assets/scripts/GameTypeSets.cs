using System;

public partial class Loader
{
    public GameType gameType = GameType.race;
    public bool dmOnly{get { return gameType == GameType.dm; }}
    public bool dm { get { return gameType == GameType.dm || gameType == GameType.team || gameType == GameType.ctf || gameType == GameType.dmRace; } }
    public bool dmRace { get { return gameType == GameType.dmRace; } }
    public bool dmNoCtf { get { return gameType == GameType.dm || gameType == GameType.dmRace; } }
    public bool race { get { return gameType == GameType.race || gameType == GameType.dmRace; } }
    public bool team { get { return gameType == GameType.team || gameType == GameType.ctf; } }
    public bool teamOrPursuit { get { return team || pursuit; } }
    private bool m_enableZombies = false;
    public bool enableZombies { get { return dmOrPursuit && m_enableZombies; } set { m_enableZombies = value; } }
    public bool ctf { get { return gameType == GameType.ctf; } }
    public bool ctfRandomSpawn = false;
    public bool dmOrCoins { get { return dm; } }
    public bool dmNoRace { get { return dm && !dmRace; } }
    public bool pursuit { get { return gameType == GameType.pursuit || sGameType == SGameType.Cops; } }
    public bool dmOrPursuit { get { return dmOnly || ctf || pursuit; } }
    
}

public enum GameType { race, dm, team, ctf, zombies, dmRace, pursuit }