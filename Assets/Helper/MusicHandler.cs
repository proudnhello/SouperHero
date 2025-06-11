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
        public virtual void Reset() { }
    }

    public enum MusicState
    {
        MAINMENU,
        LOADING,
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
            new LoadingScreen(),
            new Hub(manager),
            new Exploration(manager)
        };
        GameManager.OnStartRun += () =>
        {
            CookingScreen.EnterCookingScreen += TryCheckMusicState;
            CookingScreen.ExitCookingScreen += TryCheckMusicState;
            foreach (var states in MusicStates) states.Reset();
            if (currMusicState == null || currMusicState is LoadingScreen) ChangeState(MusicState.EXPLORATION);
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
        if (currMusicState == MusicStates[(int)state]) return;
        currMusicState?.OnExit();
        currMusicState = MusicStates[(int)state];
        currMusicState.OnEnter();
        Debug.Log("Enter state = " + state.ToString());
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
            MainMenuTheme = manager.CreateInstance(manager.MUSIC[0]);
        }
        public override void OnEnter()
        {
            MainMenuTheme.start();
        }
        public override void OnExit()
        {
            MainMenuTheme.stop(STOP_MODE.ALLOWFADEOUT);
        }
    }

    public class LoadingScreen : IGameState
    {

    }

    public class Hub : IGameState
    {
        EventInstance HubTheme;
        public Hub(AudioManager manager)
        {
            HubTheme = manager.CreateInstance(manager.MUSIC[1]);
        }
        int timelinePos = -1;
        public override void OnEnter()
        {
            HubTheme.start();
            if (timelinePos >= 0) HubTheme.setTimelinePosition(timelinePos);
        }
        public override void OnExit()
        {
            HubTheme.getTimelinePosition(out timelinePos);
            HubTheme.stop(STOP_MODE.ALLOWFADEOUT);
        }
        public override void Reset()
        {
            timelinePos = -1;
        }
    }
    public class Exploration : IGameState
    {
        EventInstance InGameTheme;
        int stateParameter = 0;
        AudioManager _manager;
        PARAMETER_DESCRIPTION IsCooking;
        PARAMETER_DESCRIPTION IsCombat;
        public Exploration(AudioManager manager)
        {
            _manager = manager;
            InGameTheme = manager.CreateInstance(manager.MUSIC[2]);
            InGameTheme.getDescription(out EventDescription e);
            e.getParameterDescriptionByName("IsCooking", out IsCooking);
            e.getParameterDescriptionByName("IsCombat", out IsCombat);
        }

        int timelinePos = -1;
        public override void OnEnter()
        {
            InGameTheme.start();
            if (timelinePos >= 0) InGameTheme.setTimelinePosition(timelinePos);
            CheckState();
        }

        public void CheckState()
        {
            if (_manager._MusicHandler.AgroEnemies.Keys.Count > 0) // combat
            {
                if (stateParameter == 2) return;
                stateParameter = 2;
                InGameTheme.setParameterByID(IsCombat.id, 1);
                InGameTheme.setParameterByID(IsCooking.id, 0);
                Debug.Log("Exploration state = Combat");
            }
            else if (CookingScreen.Singleton.IsCooking) // cooking
            {
                if (stateParameter == 1) return;
                stateParameter = 1;
                InGameTheme.setParameterByID(IsCombat.id, 0);
                InGameTheme.setParameterByID(IsCooking.id, 1);
                Debug.Log("Exploration state = Cooking");
            }
            else // regular
            {
                if (stateParameter == 0) return;
                stateParameter = 0;
                InGameTheme.setParameterByID(IsCombat.id, 0);
                InGameTheme.setParameterByID(IsCooking.id, 0);
                Debug.Log("Exploration state = Regular");
            }

        }
        public override void OnExit()
        {
            InGameTheme.getTimelinePosition(out timelinePos);
            InGameTheme.stop(STOP_MODE.ALLOWFADEOUT);
        }
        public override void Reset()
        {
            timelinePos = -1;
        }
    }
    #endregion
}