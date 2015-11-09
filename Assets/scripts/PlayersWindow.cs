#define GA

using System.Linq;
//using Edelweiss.DecalSystem;
//using Vectrosity;
#if !UNITY_WP8
#endif
using gui = UnityEngine.GUILayout;
using UnityEngine;
#if UNITY_EDITOR

//using Exception = System.AccessViolationException;

#endif

public partial class MpGame
{
    public void PursuitWindow()
    {
        Setup(500,700);
        BeginScrollView();
        DrawMatchTimeLimit();

        if (_Game.none && winTeam != null)
        {
            LabelCenter(winTeam.teamName + " Win", 30);
            //if (winTeam.team == TeamEnum.Red)
            //{
            
            //}
        }


        if (_Game.none)
            LabelCenter("Game starts in " + (int)(timeCountMatch - _Loader.matchTime), 20);

        //gui.BeginHorizontal(new GUIContent("score"),win.editorSkin.window);
        //foreach (var team in _MpGame.teams)
        //    LabelCenter(team.teamName + team.score, 25);
        //gui.EndVertical();

        gui.BeginVertical(new GUIContent(""), win.editorSkin.window);
        LabelCenter("Most Wanted:");
        foreach (var a in redTeam.players)
            gui.Label(new GUIContent(a.replay.getText(true), a.avatar));
        gui.EndVertical();
        gui.EndScrollView();
    }
    public void PlayersWindow()
    {
        Setup(700, 600);
        DrawMatchTimeLimit();
                
        if (_Game.none)
            LabelCenter("Game starts in " + (int)(timeCountMatch - _Loader.matchTime), 25);

        BeginScrollView();
        gui.BeginHorizontal();
        foreach (var players in listOfPlayers.GroupBy(a => a.teamEnum))
        {
            if (_Loader.team)
            {
                gui.BeginVertical();
                var team = _MpGame.teams[(int)players.Key];
                LabelCenter(team.teamName + team.score, 25);
            }
            gui.BeginHorizontal();
            {
                GUILayoutOption h = gui.Height(64);
                //skin.label.alignment = TextAnchor.MiddleLeft;
                
                {
                    gui.BeginVertical();
                    Label("Name");
                    foreach (var a in players)
                        gui.Label(new GUIContent(a.replay.getText(false), a.avatar), h);
                    gui.EndVertical();
                }

                if (_Loader.race)
                {
                    gui.BeginVertical();
                    gui.Label("Time");
                    foreach (var a in players)
                        gui.Label(TimeToStr(a.finnishTime), h);
                    gui.EndVertical();
                }
                else if (_Loader.pursuit)
                {
                    gui.BeginVertical();
                    gui.Label("bandits killed");
                    foreach (var a in players)
                        gui.Label(a.kills.ToString(), h);
                    gui.EndVertical();
                }
                else
                {
                    gui.BeginVertical();
                    gui.Label("Score");
                    foreach (var a in players)
                        gui.Label(a.scoreInt.ToString(), h);
                    gui.EndVertical();
                }

                if (_Loader.enableZombies)
                {
                    gui.BeginVertical();
                    gui.Label("Zombies Killed");
                    foreach (var a in players)
                        gui.Label(a.zombieKills.ToString(), h);
                    gui.EndVertical();
                }

                if (!_Loader.teamOrPursuit)
                {
                    gui.BeginVertical();
                    gui.Label("Kills");
                    foreach (var a in players)
                        gui.Label(a.kills.ToString(), h);
                    gui.EndVertical();

                    gui.BeginVertical();
                    gui.Label("deaths");
                    foreach (var a in players)
                        gui.Label(a.deaths.ToString(), h);
                    gui.EndVertical();
                }
            }
            gui.EndHorizontal();

            if(_Loader.team)
                gui.EndVertical();
        }
        gui.EndHorizontal();        
        gui.EndScrollView();
    }   
}