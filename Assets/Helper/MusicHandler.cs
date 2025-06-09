using FMOD.Studio;
using System.Collections.Generic;
using UnityEngine;

public class MusicHandler
{
    #region DEFINITIONS
    public abstract class IGameState
    {
        protected MusicHandler handler;
        public virtual void OnEnter() { }
        public virtual void OnExit() { }
    }

    public enum MusicState
    {
        MAINMENU,
        HUB,
        EXPLORATION
    }
    #endregion


    AudioManager _manager;
    List<IGameState> MusicStates;
    Dictionary<int, byte> AgroEnemies = new();
    public MusicHandler(AudioManager manager)
    {
        _manager = manager;
        MusicStates = new() { 
            new MainMenu(manager), 
            new Hub(manager),
            new Exploration(manager)
        };
        GameManager.OnStartRun += () =>
        {
            CookingScreen.EnterCookingScreen += TryCheckMusicState;
            CookingScreen.ExitCookingScreen += TryCheckMusicState;
            AgroEnemies = new();
        };
        GameManager.OnEndRun += () =>
        {
            CookingScreen.EnterCookingScreen -= TryCheckMusicState;
            CookingScreen.ExitCookingScreen -= TryCheckMusicState;
            ChangeState(MusicState.MAINMENU);
        };
    }

    IGameState currMusicState;
    public void ChangeState(MusicState state)
    {
        currMusicState?.OnExit();
        currMusicState = MusicStates[(int)state];
        currMusicState.OnEnter();
        //Debug.Log("Enter state = " + state.ToString());
    }

    public void AddAgro(int uuid)
    {
        AgroEnemies.TryAdd(uuid, 0);
        TryCheckMusicState();
    }

    public void RemoveAgro(int uuid)
    {
        AgroEnemies.Remove(uuid);
        TryCheckMusicState();
    }

    public void TryCheckMusicState()
    {
        if (currMusicState is Exploration state)
        {
            state.CheckState();
        }
    }

    #region GAMESTATES
    public class MainMenu : IGameState
    {
        EventInstance MainMenuTheme;
        public MainMenu(AudioManager manager)
        {
            //MainMenuTheme = manager.CreateInstance(manager.MUSIC[0]);
        }
    }
    public class Hub : IGameState
    {
        EventInstance HubTheme;
        public Hub(AudioManager manager)
        {
            //HubTheme = manager.CreateInstance(manager.MUSIC[1]);
        }
    }
    public class Exploration : IGameState
    {
        EventInstance InGameTheme;
        int stateParameter = 0;
        AudioManager _manager;
        public Exploration(AudioManager manager)
        {
            _manager = manager;
            //InGameTheme = manager.CreateInstance(manager.MUSIC[2]);
        }

        public override void OnEnter()
        {
            CheckState();
        }

        public void CheckState()
        {
            if (_manager._MusicHandler.AgroEnemies.Keys.Count > 0) // combat
            {
                if (stateParameter == 2) return;
                stateParameter = 2;
                //Debug.Log("Exploration state is now = " + stateParameter);
            }
            else if (CookingScreen.Singleton.IsCooking) // cooking
            {
                if (stateParameter == 1) return;
                stateParameter = 1;
                //Debug.Log("Exploration state is now = " + stateParameter);
            }
            else // regular
            {
                if (stateParameter == 0) return;
                stateParameter = 0;
                //Debug.Log("Exploration state is now = " + stateParameter);
            }

        }
    }
    #endregion
}