using DG.Tweening;
using DG.Tweening.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Antlr3.Runtime.Tree.TreeWizard;

public class RoomGenerator : MonoBehaviour
{
    public int TILE_WIDTH;
    public int TILE_HEIGHT;

    public int _mapWidth;
    public int _mapHeight;

    private List<List<Block>> _map;

    public List<Block> _startBlocks;
    public List<Block> _intermediateBlocks;

    public GameObject connector2;
    public GameObject connector25;
    public GameObject connector3;
    public GameObject connector4;

    void Start()
    {
        // Start the scene identifying where the player will spawn
        // Have a select number of rooms you want
        _map = new List<List<Block>>();
        for(int i = 0; i < _mapWidth; i++)
        {
            _map.Add(new List<Block>());
        }
        for(int i = 0; i < _mapWidth; i++) { 
            for (int j = 0; j < _mapHeight; j++)
            {
                _map[i].Add(null);
            }
        }
        GenerateRoom();
    }

    private Vector2 getOffset(int row, int col, MapRoom b)
    {
        return new Vector2((row + (b.BlockWidth() / 2.0f)) * TILE_WIDTH, (col + (b.BlockHeight() / 2.0f)) * TILE_HEIGHT);
    }

    private bool canPlaceIntermediate(int row, int col, MapRoom b)
    {
        b.gameObject.transform.position = getOffset(row, col, b);
        for(int i = 0; i < b.BlockWidth(); ++i)
        {
            if(row + i >= _mapWidth || _map[row + i][col] != null)
            {
                return false;
            }
        }
        for (int i = 0; i < b.BlockHeight(); ++i)
        {
            if (col + i >= _mapHeight || _map[row][col + i] != null)
            {
                return false;
            }
        }
        return true;
    }

    private void fillBlock(int row, int col, MapRoom b)
    {
        for (int i = 0; i < b.BlockWidth(); ++i)
        {
            for(int j = 0; j < b.BlockHeight(); j++) 
            {
                _map[row + i][col] = b.At(i, j);
            }
        }
    }

    // BFS Random Placement for intermediates // CHATGPT GAVE ME PSEUDO CODE
    void placeIntermediates(int numIntermediates)
    {
        for (int i = 0; i < numIntermediates; i++)
        {
            int blockType = UnityEngine.Random.Range(0, _intermediateBlocks.Count);
            MapRoom b = Instantiate(_intermediateBlocks[blockType]).GetComponent<MapRoom>();

            bool placed = false;
            while (!placed)
            {
                int row = UnityEngine.Random.Range(0, _mapWidth);
                int col = UnityEngine.Random.Range(0, _mapHeight);

                b.gameObject.transform.rotation = Quaternion.identity;
                if (row + b.BlockWidth() < _mapWidth && canPlaceIntermediate(row, col, b))
                {
                    fillBlock(row, col, b);
                    placed = true;
                    Vector2 offset = getOffset(row, col, b);
                }
            }
        }
    }

    private bool checkForBlock(int row, int col)
    {
        Block b = _map[row][col];
        return b != null && b.BlockType() != "Connector";
    }
    private bool checkForBlockAll(Coordinate c)
    {
        Block b = _map[c.row][c.col];
        return b != null && b.BlockType() != "Start";
    }

    private bool checkForBlockAdvanced(Coordinate c)
    {
        return _map[c.row][c.col] == null;
    }

    private string getConnectionsAt(int row, int col)
    {
        int rowPlus = row + 1;
        int rowMinus = row - 1;
        int colPlus = col + 1;
        int colMinus = col - 1;

        string s = "";

        if (colPlus < _mapHeight)
        {
            if (checkForBlock(row, colPlus) && _map[row][colPlus].south)
            {
                s += "N";
            }
        }
        if (colMinus >= 0)
        {
            if (checkForBlock(row, colMinus) && _map[row][colMinus].north)
            {
                s += "S";
            }
        }
        if (rowPlus < _mapWidth)
        {
            if (checkForBlock(rowPlus, col) && _map[rowPlus][col].west)
            {
                s += "E";
            }
        }
        if (rowMinus >= 0)
        {
            if (checkForBlock(rowMinus, col) && _map[rowMinus][col].east)
            {
                s += "W";
            }
        }
        return s;
    }

    private string getConnectionsAtAdvanced(int row, int col)
    {
        Coordinate rowPlus = new Coordinate(row + 1, col);
        Coordinate rowMinus = new Coordinate(row - 1, col);
        Coordinate colPlus = new Coordinate(row, col + 1);
        Coordinate colMinus = new Coordinate(row, col - 1);

        string s = "";

        if (colPlus.col < _mapHeight)
        {
            if (!checkForBlockAdvanced(colPlus))
            {
                if (_map[colPlus.row][colPlus.col].BlockType() == "Connector")
                {
                    s += "N";
                } else
                {
                    if(_map[colPlus.row][colPlus.col].south)
                    {
                        s += "N";
                    }
                }
            }
        }
        if (colMinus.col >= 0)
        {
            if (!checkForBlockAdvanced(colMinus))
            {
                if (_map[colMinus.row][colMinus.col].BlockType() == "Connector")
                {
                    s += "S";
                }
                else
                {
                    if (_map[colMinus.row][colMinus.col].north)
                    {
                        s += "S";
                    }
                }
            }
        }
        if (rowPlus.row < _mapWidth)
        {
            if (!checkForBlockAdvanced(rowPlus))
            {
                if (_map[rowPlus.row][rowPlus.col].BlockType() == "Connector")
                {
                    s += "E";
                }
                else
                {
                    if (_map[rowPlus.row][rowPlus.col].west)
                    {
                        s += "E";
                    }
                }
            }
        }
        if (rowMinus.row >= 0)
        {
            if (!checkForBlockAdvanced(rowMinus))
            {
                if (_map[rowMinus.row][rowMinus.col].BlockType() == "Connector")
                {
                    s += "W";
                }
                else
                {
                    if (_map[rowMinus.row][rowMinus.col].east)
                    {
                        s += "W";
                    }
                }
            }
        }
        return s;
    }

    private void pickAndPlaceDoubleAlternate(int row, int col, string c)
    {
        float angle = 0.0f;
        bool connectorType2Need = false;
        bool north = false;
        bool south = false;
        bool east = false;
        bool west = false;
        switch (c)
        {
            case "EE":
                east = true;
                west = true;
                connectorType2Need = true;
                break;
            case "WW":
                east = true;
                west = true;
                connectorType2Need = true;
                break;
            case "NN":
                north = true;
                south = true;
                connectorType2Need = true;
                angle = 90.0f;
                break;
            case "SS":
                north = true;
                south = true;
                connectorType2Need = true;
                angle = 90.0f;
                break;
            case "EN":
                west = true;
                north = true;
                break;
            case "ES":
                west = true;
                south = true;
                angle = 90.0f;
                break;
            case "EW":
                east = true;
                west = true;
                connectorType2Need = true;
                break;
            case "NE":
                south = true;
                east = true;
                angle = 180.0f;
                break;
            case "NW":
                south = true;
                west = true;
                angle = 90.0f;
                break;
            case "NS":
                south = true;
                north = true;
                connectorType2Need = true;
                angle = 90.0f;
                break;
            case "WN":
                east = true;
                north = true;
                angle = -90.0f;
                break;
            case "WE":
                east = true;
                west = true;
                connectorType2Need = true;
                break;
            case "WS":
                east = true;
                south = true;
                angle = 180.0f;
                break;
            case "SE":
                north = true;
                east = true;
                angle = -90.0f;
                break;
            case "SW":
                north = true;
                west = true;
                break;
            case "SN":
                north = true;
                south = true;
                connectorType2Need = true;
                angle = 90.0f;
                break;
            default:
                break;
        }
        MapRoom b = null;
        if (connectorType2Need)
        {
            b = Instantiate(connector2).GetComponent<MapRoom>();
        }
        else
        {
            b = Instantiate(connector25).GetComponent<MapRoom>();
        }
        b.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle));
        canPlaceIntermediate(row, col, b);
        fillBlock(row, col, b);
        _map[row][col] = b.At(0, 0);
        b.At(0, 0).setDirections(north, south, east, west);
    }

    private void firstSweepConnect()
    {
        for (int row = 0; row < _mapWidth; row++)
        {
            for (int col = 0; col < _mapHeight; col++)
            {
                string c = "";
                if (!_map[row][col])
                {
                    c = getConnectionsAt(row, col);
                } else if (_map[row][col].BlockType() == "Connector")
                {

                    c = getConnectionsAtAdvanced(row, col);
                    Destroy(_map[row][col].gameObject);
                } else
                {
                    continue;
                }
                bool north = false;
                bool south = false;
                bool east = false;
                bool west = false;
                switch (c.Length)
                {
                    case 2:
                        float angle = 0.0f;
                        bool connectorType2Need = false;
                        switch (c)
                        {
                            case "NW":
                                north = true;
                                west = true;
                                break;
                            case "NE":
                                north = true;
                                east = true;
                                angle = -90.0f;
                                break;
                            case "NS":
                                north = true;
                                south = true;
                                angle = 90.0f;
                                connectorType2Need = true;
                                break;
                            case "SW":
                                south = true;
                                west = true;
                                angle = 90.0f;
                                break;
                            case "SE":
                                south = true;
                                east = true;
                                angle = 180.0f;
                                break;
                            case "EW":
                                east = true;
                                west = true;
                                connectorType2Need = true;
                                break;
                            default:
                                break;
                        }
                        MapRoom b = null;
                        if (connectorType2Need)
                        {
                            b = Instantiate(connector2).GetComponent<MapRoom>();
                        }
                        else
                        {
                            b = Instantiate(connector25).GetComponent<MapRoom>();
                        }
                        b.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle));
                        canPlaceIntermediate(row, col, b);
                        fillBlock(row, col, b);
                        _map[row][col] = b.At(0, 0);
                        b.At(0, 0).setDirections(north, south, east, west);
                        break;
                    case 3:
                        float angle2 = 0.0f;
                        switch(c)
                        {
                            case "NSE":
                                north = true;
                                south = true;
                                east = true;
                                angle2 = -90.0f;
                                break;
                            case "NSW":
                                north = true;
                                south = true;
                                west = true;
                                angle2 = 90.0f;
                                break;
                            case "SEW":
                                south = true;
                                east = true;
                                west = true;
                                angle2 = 180.0f;
                                break;
                            case "NEW":
                                north = true;
                                east = true;
                                west = true;
                                break;
                        }
                        MapRoom b2 = Instantiate(connector3).GetComponent<MapRoom>();
                        b2.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle2));
                        canPlaceIntermediate(row, col, b2);
                        fillBlock(row, col, b2);
                        _map[row][col] = b2.At(0, 0);
                        b2.At(0, 0).setDirections(north, south, east, west);
                        break;
                    case 4:
                        MapRoom b3 = Instantiate(connector4).GetComponent<MapRoom>();
                        canPlaceIntermediate(row, col, b3);
                        fillBlock(row, col, b3);
                        _map[row][col] = b3.At(0, 0);
                        b3.At(0, 0).setDirections(true, true, true, true);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void pathFromStringStart(int row, int col, string path)
    {
        if (path.Length <= 1)
        {
            Debug.LogError("Something went wrong with path from string");
        }
        else
        {
            switch (path[1])
            {
                case 'N':
                    col++;
                    break;
                case 'S':
                    col--;
                    break;
                case 'E':
                    row++;
                    break;
                case 'W':
                    row--;
                    break;
            }
            for (int i = 1; i < path.Length - 1; i++)
            {
                pickAndPlaceDoubleAlternate(row, col, path.Substring(i, 2));
                char dir = path[i + 1];
                switch (dir)
                {
                    case 'N':
                        col++;
                        break;
                    case 'S':
                        col--;
                        break;
                    case 'E':
                        row++;
                        break;
                    case 'W':
                        row--;
                        break;
                }
            }
        }
    }

    private void pathFromString(int row, int col, string path)
    {
        if (path.Length <= 1)
        {
            Debug.LogError("Something went wrong with path from string");
        }
        else
        {
            switch (path[0])
            {
                case 'N':
                    col++;
                    break;
                case 'S':
                    col--;
                    break;
                case 'E':
                    row++;
                    break;
                case 'W':
                    row--;
                    break;
            }
            for (int i = 0; i < path.Length - 1; i++)
            {
                pickAndPlaceDoubleAlternate(row, col, path.Substring(i, 2));
                char dir = path[i + 1];
                switch (dir)
                {
                    case 'N':
                        col++;
                        break;
                    case 'S':
                        col--;
                        break;
                    case 'E':
                        row++;
                        break;
                    case 'W':
                        row--;
                        break;
                }
            }
        }
    }

    struct Coordinate
    {
        public int row;
        public int col;

        public Coordinate(int x, int y)
        {
            row = x;
            col = y;
        }

        public override string ToString()
        {
            return row + ", " + col;
        }

        public float squaredDistanceTo(Coordinate other)
        {
            int dX = other.col - col;
            int dY = other.row - row;
            return (dX * dX) + (dY * dY);
        }
    }

    private string BFSPathFromStart(Coordinate start)
    {
        string path = "";
        Dictionary<Coordinate, Tuple<Coordinate, char>> parents = new Dictionary<Coordinate, Tuple<Coordinate, char>>();
        HashSet<Coordinate> visited = new HashSet<Coordinate>();
        Queue<Coordinate> queue = new Queue<Coordinate>();

        queue.Enqueue(start);
        visited.Add(start);
        parents[start] = new Tuple<Coordinate, char>(new Coordinate(-1, -1), ' ');
        Coordinate closestIntermediate = new Coordinate(-1, -1);
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new Coordinate(b.row + 1, b.col);
            Coordinate rowMinus = new Coordinate(b.row - 1, b.col);
            Coordinate colPlus = new Coordinate(b.row, b.col + 1);
            Coordinate colMinus = new Coordinate(b.row, b.col - 1);

            if (rowMinus.row >= 0)
            {
                if (checkForBlockAdvanced(rowMinus) && !visited.Contains(rowMinus))
                {
                    queue.Enqueue(rowMinus);
                    visited.Add(rowMinus);
                    parents[rowMinus] = new Tuple<Coordinate, char>(b, 'W');
                }
                else
                {
                    if (!checkForBlockAdvanced(rowMinus) && _map[rowMinus.row][rowMinus.col].BlockType() == "Intermediate")
                    {
                        parents[rowMinus] = new Tuple<Coordinate, char>(b, 'W');
                        closestIntermediate = rowMinus;
                        break;
                    }
                }
            }
            if (rowPlus.row < _mapWidth)
            {
                if (checkForBlockAdvanced(rowPlus) && !visited.Contains(rowPlus))
                {
                    queue.Enqueue(rowPlus);
                    visited.Add(rowPlus);
                    parents[rowPlus] = new Tuple<Coordinate, char>(b, 'E');
                }
                else
                {
                    if (!checkForBlockAdvanced(rowPlus) && _map[rowPlus.row][rowPlus.col].BlockType() == "Intermediate")
                    {
                        parents[rowPlus] = new Tuple<Coordinate, char>(b, 'E');
                        closestIntermediate = rowPlus;
                        break;
                    }
                }
            }
            if (colPlus.col < _mapHeight)
            {
                if (checkForBlockAdvanced(colPlus) && !visited.Contains(colPlus))
                {
                    queue.Enqueue(colPlus);
                    visited.Add(colPlus);
                    parents[colPlus] = new Tuple<Coordinate, char>(b, 'N');
                }
                else
                {
                    if (!checkForBlockAdvanced(colPlus) && _map[colPlus.row][colPlus.col].BlockType() == "Intermediate")
                    {
                        parents[colPlus] = new Tuple<Coordinate, char>(b, 'N');
                        closestIntermediate = colPlus;
                        break;
                    }
                }
            }
            if (colMinus.col >= 0)
            {
                if (checkForBlockAdvanced(colMinus) && !visited.Contains(colMinus))
                {
                    queue.Enqueue(colMinus);
                    visited.Add(colMinus);
                    parents[colMinus] = new Tuple<Coordinate, char>(b, 'S');
                } else
                {
                    if (!checkForBlockAdvanced(colMinus) && _map[colMinus.row][colMinus.col].BlockType() == "Intermediate")
                    {
                        parents[colMinus] = new Tuple<Coordinate, char>(b, 'S');
                        closestIntermediate = colMinus;
                        break;
                    }
                }
            }
        }

        while(closestIntermediate.row != -1 && closestIntermediate.col != -1)
        {
            path += parents[closestIntermediate].Item2;
            closestIntermediate = parents[closestIntermediate].Item1;
        }

        char[] charArray = path.ToCharArray();
        Array.Reverse(charArray);
        string ret = new string(charArray);
        ret = ret.Substring(1, ret.Length - 1);

        return "E" + ret;
    }

    HashSet<Coordinate> visitedStart = new HashSet<Coordinate>();

    private List<Coordinate> BFSGetGroup(Coordinate start)
    {
        Queue<Coordinate> queue = new Queue<Coordinate>();
        List<Coordinate> ret = new List<Coordinate>();
        if (_map[start.row][start.col] == null)
        {
            return ret;
        }

        queue.Enqueue(start);
        visitedStart.Add(start);
        Coordinate closestIntermediate = new Coordinate(-1, -1);
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new Coordinate(b.row + 1, b.col);
            Coordinate rowMinus = new Coordinate(b.row - 1, b.col);
            Coordinate colPlus = new Coordinate(b.row, b.col + 1);
            Coordinate colMinus = new Coordinate(b.row, b.col - 1);

            if (rowMinus.row >= 0)
            {
                if (checkForBlockAll(rowMinus) && !visitedStart.Contains(rowMinus) && _map[rowMinus.row][rowMinus.col].east)
                {
                    queue.Enqueue(rowMinus);
                    visitedStart.Add(rowMinus);
                    ret.Add(rowMinus);
                }
            }
            if (rowPlus.row < _mapWidth)
            {
                if (checkForBlockAll(rowPlus) && !visitedStart.Contains(rowPlus) && _map[rowPlus.row][rowPlus.col].west)
                {
                    queue.Enqueue(rowPlus);
                    visitedStart.Add(rowPlus);
                    ret.Add(rowPlus);
                }
            }
            if (colPlus.col < _mapHeight)
            {
                if (checkForBlockAll(colPlus) && !visitedStart.Contains(colPlus) && _map[colPlus.row][colPlus.col].south)
                {
                    queue.Enqueue(colPlus);
                    visitedStart.Add(colPlus);
                    ret.Add(colPlus);
                }
            }
            if (colMinus.col >= 0)
            {
                if (checkForBlockAll(colMinus) && !visitedStart.Contains(colMinus) && _map[colMinus.row][colMinus.col].north)
                {
                    queue.Enqueue(colMinus);
                    visitedStart.Add(colMinus);
                    ret.Add(colMinus);
                }
            }
        }

        return ret;
    }

    private string BFSPathFromIntermediate(Coordinate start, List<Coordinate> startIsland)
    {
        string path = "";
        Dictionary<Coordinate, Tuple<Coordinate, char>> parents = new Dictionary<Coordinate, Tuple<Coordinate, char>>();
        HashSet<Coordinate> visited = new HashSet<Coordinate>();
        // move this out
        HashSet<Coordinate> destinations = new HashSet<Coordinate>();
        Queue<Coordinate> queue = new Queue<Coordinate>();

        foreach (Coordinate c in startIsland) {
            destinations.Add(c);
        }

        queue.Enqueue(start);
        visited.Add(start);
        parents[start] = new Tuple<Coordinate, char>(new Coordinate(-1, -1), ' ');
        Coordinate closestIntermediate = new Coordinate(-1, -1);
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new Coordinate(b.row + 1, b.col);
            Coordinate rowMinus = new Coordinate(b.row - 1, b.col);
            Coordinate colPlus = new Coordinate(b.row, b.col + 1);
            Coordinate colMinus = new Coordinate(b.row, b.col - 1);

            if (rowMinus.row >= 0)
            {
                if (checkForBlockAdvanced(rowMinus) && !visited.Contains(rowMinus))
                {
                    queue.Enqueue(rowMinus);
                    visited.Add(rowMinus);
                    parents[rowMinus] = new Tuple<Coordinate, char>(b, 'W');
                }
                else
                {
                    if (!checkForBlockAdvanced(rowMinus) && destinations.Contains(rowMinus))
                    {
                        parents[rowMinus] = new Tuple<Coordinate, char>(b, 'W');
                        closestIntermediate = rowMinus;
                        break;
                    }
                }
            }
            if (rowPlus.row < _mapWidth)
            {
                if (checkForBlockAdvanced(rowPlus) && !visited.Contains(rowPlus))
                {
                    queue.Enqueue(rowPlus);
                    visited.Add(rowPlus);
                    parents[rowPlus] = new Tuple<Coordinate, char>(b, 'E');
                }
                else
                {
                    if (!checkForBlockAdvanced(rowPlus) && destinations.Contains(rowPlus))
                    {
                        parents[rowPlus] = new Tuple<Coordinate, char>(b, 'E');
                        closestIntermediate = rowPlus;
                        break;
                    }
                }
            }
            if (colPlus.col < _mapHeight)
            {
                if (checkForBlockAdvanced(colPlus) && !visited.Contains(colPlus))
                {
                    queue.Enqueue(colPlus);
                    visited.Add(colPlus);
                    parents[colPlus] = new Tuple<Coordinate, char>(b, 'N');
                }
                else
                {
                    if (!checkForBlockAdvanced(colPlus) && destinations.Contains(colPlus))
                    {
                        parents[colPlus] = new Tuple<Coordinate, char>(b, 'N');
                        closestIntermediate = colPlus;
                        break;
                    }
                }
            }
            if (colMinus.col >= 0)
            {
                if (checkForBlockAdvanced(colMinus) && !visited.Contains(colMinus))
                {
                    queue.Enqueue(colMinus);
                    visited.Add(colMinus);
                    parents[colMinus] = new Tuple<Coordinate, char>(b, 'S');
                }
                else
                {
                    if (!checkForBlockAdvanced(colMinus) && destinations.Contains(colMinus))
                    {
                        parents[colMinus] = new Tuple<Coordinate, char>(b, 'S');
                        closestIntermediate = colMinus;
                        break;
                    }
                }
            }
        }

        while (closestIntermediate.row != -1 && closestIntermediate.col != -1)
        {
            path += parents[closestIntermediate].Item2;
            closestIntermediate = parents[closestIntermediate].Item1;
        }

        if(path.Length == 0)
        {
            Debug.LogError("CANNOT FIND PATH FROM SOURCE TO DESTINATION!!");
            return "";
        }
        char[] charArray = path.Substring(0, path.Length - 1).ToCharArray();
        Array.Reverse(charArray);
        string ret = new string(charArray);

        return ret;
    }

    private void openDoors()
    {
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                Coordinate c = new Coordinate(i, j);
                if (!checkForBlockAdvanced(c) && _map[c.row][c.col].BlockType() == "Intermediate")
                {
                    Block I = _map[c.row][c.col];
                    string s = getConnectionsAtAdvanced(c.row, c.col);
                    if(s.Contains('N') && I.northDoor)
                    {
                        I.northDoor.SetActive(false);
                    }
                    if (s.Contains('E') && I.eastDoor)
                    {
                        I.eastDoor.SetActive(false);
                    }
                    if (s.Contains('S') && I.southDoor)
                    {
                        I.southDoor.SetActive(false);
                    }
                    if (s.Contains('W') && I.westDoor)
                    {
                        I.westDoor.SetActive(false);
                    }
                }
            }
        }
    }

    void GenerateRoom() {
        int midWidth = (_mapWidth - 1) / 2;
        int midHeight = (_mapHeight - 1) / 2;

        MapRoom b = Instantiate(_startBlocks[0]).GetComponent<MapRoom>();
        b.gameObject.transform.position = getOffset(midWidth, midHeight, b);
        _map[midWidth][midHeight] = b.At(0, 0);

        placeIntermediates(10);

        firstSweepConnect();

        if (_map[midWidth + 1][midHeight] == null)
        {
            string path = BFSPathFromStart(new Coordinate(midWidth + 1, midHeight));
            pathFromStringStart(midWidth + 1, midHeight, path);
        }

        MapRoom b2 = Instantiate(connector4).GetComponent<MapRoom>();
        if (canPlaceIntermediate(midWidth + 1, midHeight, b2))
        {
            fillBlock(midWidth + 1, midHeight, b2);
            _map[midWidth + 1][midHeight] = b2.At(0, 0);
            b2.At(0, 0).setDirections(true, true, true, true);
        } else
        {
            Destroy(b2.gameObject);
        }

        List<Coordinate> startIntermediates = new List<Coordinate>();
        List<Coordinate> startIsland = BFSGetGroup(new Coordinate(midWidth + 1, midHeight));
        foreach(Coordinate c in startIsland)
        {
            if (_map[c.row][c.col].BlockType() == "Intermediate")
            {
                startIntermediates.Add(c);
            }
        }

        List<List<Coordinate>> disconnectedGroups = new List<List<Coordinate>>();

        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                Coordinate c = new Coordinate(i, j);
                if (checkForBlockAll(c) && !visitedStart.Contains(c))
                {
                    disconnectedGroups.Add(BFSGetGroup(c));
                }
            }
        }

        foreach (List<Coordinate> list in disconnectedGroups)
        {
            int iter = 0;
            HashSet<Coordinate> visited = new HashSet<Coordinate>();
            while (iter < list.Count)
            {
                if (list.Count == 0)
                {
                    continue;
                }
                // find mind dist between intermediate from start group and list
                float closestDistance = int.MaxValue;
                Coordinate disconnectedCoordinate = new Coordinate(-1, -1);
                Coordinate closestInter = new Coordinate(-1, -1);
                foreach (Coordinate coord in list)
                {
                    foreach (Coordinate inter in startIntermediates)
                    {
                        if(visited.Contains(inter))
                        {
                            continue;
                        }
                        if (_map[inter.row][inter.col].BlockType() == "Intermediate" && coord.squaredDistanceTo(inter) < closestDistance)
                        {
                            disconnectedCoordinate = coord;
                            closestInter = inter;
                            closestDistance = coord.squaredDistanceTo(inter);
                        }
                    }
                }
                if (disconnectedCoordinate.row != -1)
                {
                    string s = BFSPathFromIntermediate(disconnectedCoordinate, startIntermediates);
                    if (s == "" || s.Length <= 1)
                    {
                        iter++;
                        visited.Add(closestInter);
                        continue;
                    }
                    pathFromString(disconnectedCoordinate.row, disconnectedCoordinate.col, s);
                }
                iter = list.Count;
            }
            foreach (Coordinate coord in list)
            {
                if (_map[coord.row][coord.col].BlockType() == "Intermediate")
                {
                    startIntermediates.Add(coord);
                }
            }
        }

        firstSweepConnect();

        openDoors();

        colorGrid();
    }

    private void colorGrid()
    {
        for (int i = 0; i < _mapWidth; ++i)
        {
            for (int j = 0; j < _mapHeight; ++j)
            {
                DrawRect(new Vector3(i * TILE_WIDTH, j * TILE_WIDTH, 0), new Vector3((i + 1) * TILE_WIDTH, (j + 1) * TILE_HEIGHT, 0), Color.red);
            }
        }
        for (int i = 0; i < _mapWidth; ++i)
        {
            for (int j = 0; j < _mapHeight; ++j)
            {
                if (_map[i][j])
                {
                    Color c;
                    switch (_map[i][j].BlockType())
                    {
                        case "Start":
                            c = Color.yellow;
                            break;
                        case "Connector":
                            c = Color.green;
                            break;
                        case "Intermediate":
                            c = Color.cyan;
                            break;
                        default:
                            c = Color.magenta;
                            break;
                    }
                    DrawRect(new Vector3(i * TILE_WIDTH, j * TILE_WIDTH, 0), new Vector3((i + 1) * TILE_WIDTH, (j + 1) * TILE_HEIGHT, 0), c);
                }
            }
        }
    }

    public static void DrawRect(Vector3 min, Vector3 max, Color color)
    {
        UnityEngine.Debug.DrawLine(min, new Vector3(min.x, max.y), color);
        UnityEngine.Debug.DrawLine(new Vector3(min.x, max.y), max, color);
        UnityEngine.Debug.DrawLine(max, new Vector3(max.x, min.y), color);
        UnityEngine.Debug.DrawLine(min, new Vector3(max.x, min.y), color);
    }
}
