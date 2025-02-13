using DG.Tweening.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

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

    private Vector2 getOffset(int row, int col, Block b)
    {
        return new Vector2((row + (b.BlockWidth() / 2.0f)) * TILE_WIDTH, (col + (b.BlockHeight() / 2.0f)) * TILE_HEIGHT);
    }

    private bool canPlaceIntermediate(int row, int col, Block b)
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

    private void fillBlock(int row, int col, Block b)
    {
        for (int j = 0; j < b.BlockWidth(); ++j)
        {
            _map[row + j][col] = b;
        }
        for (int j = 0; j < b.BlockHeight(); ++j)
        {
            _map[row][col + j] = b;
        }
    }

    // BFS Random Placement for intermediates // CHATGPT GAVE ME PSEUDO CODE
    void placeIntermediates(int numIntermediates)
    {
        for (int i = 0; i < numIntermediates; i++)
        {
            int blockType = UnityEngine.Random.Range(0, _intermediateBlocks.Count);
            Block b = Instantiate(_intermediateBlocks[blockType]).GetComponent<Block>();

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

    private void pickAndPlaceDouble(int row, int col, string c)
    {
        float angle = 0.0f;
        bool connectorType2Need = false;
        switch (c)
        {
            case "NW":
                break;
            case "NE":
                angle = -90.0f;
                break;
            case "NS":
                angle = 90.0f;
                connectorType2Need = true;
                break;
            case "SW":
                angle = 90.0f;
                break;
            case "SE":
                angle = 180.0f;
                break;
            case "EW":
                connectorType2Need = true;
                break;
            default:
                break;
        }
        Block b = null;
        if (connectorType2Need)
        {
            b = Instantiate(connector2).GetComponent<Block>();
        }
        else
        {
            b = Instantiate(connector25).GetComponent<Block>();
        }
        b.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle));
        canPlaceIntermediate(row, col, b);
        fillBlock(row, col, b);
        _map[row][col] = b;
    }

    private void pickAndPlaceDoubleAlternate(int row, int col, string c)
    {
        float angle = 0.0f;
        bool connectorType2Need = false;
        switch (c)
        {
            case "EE":
                connectorType2Need = true;
                break;
            case "WW":
                connectorType2Need = true;
                break;
            case "NN":
                connectorType2Need = true;
                angle = 90.0f;
                break;
            case "SS":
                connectorType2Need = true;
                angle = 90.0f;
                break;
            case "EN":
                break;
            case "ES":
                angle = 90.0f;
                break;
            case "EW":
                connectorType2Need = true;
                break;
            case "NE":
                angle = 180.0f;
                break;
            case "NW":
                angle = 90.0f;
                break;
            case "NS":
                connectorType2Need = true;
                angle = 90.0f;
                break;
            case "WN":
                angle = -90.0f;
                break;
            case "WE":
                connectorType2Need = true;
                break;
            case "WS":
                angle = 180.0f;
                break;
            case "SE":
                angle = -90.0f;
                break;
            case "SW":
                break;
            case "SN":
                connectorType2Need = true;
                angle = 90.0f;
                break;
            default:
                break;
        }
        Block b = null;
        if (connectorType2Need)
        {
            b = Instantiate(connector2).GetComponent<Block>();
        }
        else
        {
            b = Instantiate(connector25).GetComponent<Block>();
        }
        b.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle));
        canPlaceIntermediate(row, col, b);
        fillBlock(row, col, b);
        _map[row][col] = b;
    }

    private void firstSweepConnect()
    {
        for (int row = 0; row < _mapWidth; row++)
        {
            for (int col = 0; col < _mapHeight; col++)
            {
                if (!_map[row][col])
                {
                    string c = getConnectionsAt(row, col);
                    switch (c.Length)
                    {
                        case 2:
                            pickAndPlaceDouble(row, col, c);
                            break;
                        case 3:
                            float angle2 = 0.0f;
                            switch(c)
                            {
                                case "NSE":
                                    angle2 = -90.0f;
                                    break;
                                case "NSW":
                                    angle2 = 90.0f;
                                    break;
                                case "SEW":
                                    angle2 = 180.0f;
                                    break;
                                case "NEW":
                                    break;
                            }
                            Block b2 = Instantiate(connector3).GetComponent<Block>();
                            b2.gameObject.transform.Rotate(new Vector3(0.0f, 0.0f, angle2));
                            canPlaceIntermediate(row, col, b2);
                            fillBlock(row, col, b2);
                            _map[row][col] = b2;
                            break;
                        case 4:
                            Block b3 = Instantiate(connector4).GetComponent<Block>();
                            canPlaceIntermediate(row, col, b3);
                            fillBlock(row, col, b3);
                            _map[row][col] = b3;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }

    private void pathFromString(int row, int col, string path)
    {
        if (path.Length <= 1)
        {
            Debug.Log("Something went wrong with path from string");
        }
        else
        {
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

    void GenerateRoom() {
        // place start
        int midWidth = (_mapWidth - 1) / 2;
        int midHeight = (_mapHeight - 1) / 2;

        Block b = Instantiate(_startBlocks[0]).GetComponent<Block>();
        b.gameObject.transform.position = getOffset(midWidth, midHeight, b);
        _map[midWidth][midHeight] = b;

        placeIntermediates(10);

        firstSweepConnect();

        if (_map[midWidth + 1][midHeight] == null)
        {
            string path = BFSPathFromStart(new Coordinate(midWidth + 1, midHeight));
            Debug.Log(path);
            pathFromString(midWidth + 1, midHeight, path);
        }

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

    //public static void Print2DArray(List<List<char>> matrix)
    //{
    //    string s = "";
    //    for (int i = 0; i < matrix.Count; i++)
    //    {
    //        for (int j = 0; j < matrix[0].Count; j++)
    //        {
    //            if (matrix[i][matrix.Count - j - 1] != ' ')
    //            {
    //                s += '*';
    //            }
    //            else
    //            {
    //                s += "_";
    //            }

    //        }
    //        s += '\n';
    //    }
    //    Debug.Log(s);
    //}
}
