using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MetricsSO : ScriptableObject
{

    [SerializeField] private int _numEnemiesKilled;
    [SerializeField] private int _numIngredientsCollected;
    [SerializeField] private int _numSoupsCooked;
    [SerializeField] private float _timeElapsed;
    [SerializeField] private int _numDeaths;
    [SerializeField] private int _numWins;

    // trying out properties, idfk why lol
    public int NumEnemiesKilled
    {
        get { return _numEnemiesKilled; }
        set {  _numEnemiesKilled = value;}
    }
    public int NumIngredientsCollected
    {
        get { return _numIngredientsCollected; }
        set { _numIngredientsCollected = value; }
    }

    public int NumSoupsCooked
    {
        get { return _numSoupsCooked; }
        set { _numSoupsCooked = value; }
    }

    public float TimeElapsed
    {
        get { return _timeElapsed; }
        set { _timeElapsed = value; }
    }

    public int NumDeaths
    {
        get { return _numDeaths; }
        set { _numDeaths = value; }
    }

    public int NumWins
    {
        get { return _numWins; }
        set { _numWins = value; }
    }
}
