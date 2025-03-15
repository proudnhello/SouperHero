using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NavMeshPlus.Components;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int TILE_WIDTH;
    public int TILE_HEIGHT;

    public int _mapWidth;
    public int _mapHeight;

    private List<List<Block>> _map;
    private List<MapRoom> _intermediateRooms;

    [Header("START BLOCK")]
    public GameObject _startBlock;
    [Header("INTERMEDIATE BLOCKS")]
    public List<Block> _intermediateBlocks;
    [Header("END BLOCK")]
    public GameObject _endBlockLeft;
    public GameObject _endBlockRight;
    public GameObject _endBlockUp;
    public GameObject _endBlockDown;
    public GameObject _restRoomVertical;
    public GameObject _restRoomHorizontal;
    public GameObject _bossRoom;


    [Header("I CONNECTORS")]
    public GameObject connectorEW;
    public GameObject connectorNS;
    [Header("L CONNECTORS")]
    public GameObject connectorNW;
    public GameObject connectorNE;
    public GameObject connectorSE;
    public GameObject connectorSW;
    [Header("T CONNECTORS")]
    public GameObject connectorNEW;
    public GameObject connectorNSW;
    public GameObject connectorSEW;
    public GameObject connectorNSE;
    [Header("+ CONNECTORS")]
    public GameObject connector4;
    [Header("NavMesh")]
    public NavMeshSurface NavMesh;
    [Header("Generation Parameters")]
    public int mapSeed = -1;

    public int numIntermediates = 10;

    public GameObject spawnObject;

    void Start()
    {
        // Need to create a new map full of nulls, placeholders for the Blocks and to determine if there is/isnt a block at a position
        _map = new List<List<Block>>();
        _intermediateRooms = new List<MapRoom>();
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
        // After map is created, generate the rooms
        GenerateRoom();
        NavMesh.BuildNavMeshAsync();
    }

    // Obtains the offset needed to position the room along grid lines given a row and column
    private Vector2 getOffset(int row, int col, MapRoom b)
    {
        return new Vector2((row + (b.BlockWidth() / 2.0f)) * TILE_WIDTH, (col + (b.BlockHeight() / 2.0f)) * TILE_HEIGHT);
    }

    // Checks to see if an intermediate can be placed by checking bounds and if potential spot has blocks already there
    private bool canPlaceIntermediate(int row, int col, MapRoom b)
    {
        b.gameObject.transform.position = getOffset(row, col, b);
        for (int i = 0; i < b.BlockWidth(); ++i)
        {
            for (int j = 0; j < b.BlockHeight(); ++j)
            {
                if (row + i >= _mapWidth || col + j >= _mapHeight || _map[row + i][col + j] != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Fills map blocks to properly represent the room being placed
    private void fillBlock(int row, int col, MapRoom b)
    {
        for (int i = 0; i < b.BlockWidth(); ++i)
        {
            for(int j = 0; j < b.BlockHeight(); j++) 
            {
                _map[row + i][col + j] = b.At(i, j);
            }
        }
    }



    // BFS Random Placement for intermediates. CHATGPT GAVE ME PSEUDO CODE
    void placeIntermediates(int numIntermediates)
    {
        for (int i = 0; i < numIntermediates; i++)
        {
            int blockType = UnityEngine.Random.Range(0, _intermediateBlocks.Count);
            MapRoom b = Instantiate(_intermediateBlocks[blockType], spawnObject.transform).GetComponent<MapRoom>();

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
            _intermediateRooms.Add(b);
        }
    }


    // Checks for all blocks other than connectors
    private bool checkForBlock(int row, int col)
    {
        Block b = _map[row][col];
        return b != null && b.BlockType() != "Connector";
    }

    // Checks for all blocks other than the start
    private bool checkForBlockAll(Coordinate c)
    {
        Block b = _map[c.row][c.col];
        return b != null && b.BlockType() != "Start";
    }

    // Checks if a grid coordinate is empty (doesnt have a block)
    private bool checkForBlockAdvanced(Coordinate c)
    {
        return _map[c.row][c.col] == null;
    }

    private bool checkForBlockExtent(Coordinate c, int width, int height, char dir)
    {
        int startRow = 0;
        int endRow = 0;
        int startCol = 0;
        int endCol = 0;
        bool ret = true;
        if (dir == 'N')
        {
            startRow = -Mathf.FloorToInt(width / 2.0f);
            endRow = Mathf.FloorToInt(width / 2.0f);
            startCol = 0;
            endCol = height;
        }
        else if (dir == 'S')
        {
            startRow = -Mathf.FloorToInt(width / 2.0f);
            endRow = Mathf.FloorToInt(width / 2.0f);
            startCol = -height;
            endCol = 0;
        }
        else if (dir == 'E')
        {
            startRow = 0;
            endRow = width;
            startCol = -Mathf.FloorToInt(height / 2.0f);
            endCol = Mathf.FloorToInt(height / 2.0f);
        }
        else if (dir == 'W')
        {
            startRow = -width;
            endRow = 0;
            startCol = -Mathf.FloorToInt(height / 2.0f);
            endCol = Mathf.FloorToInt(height / 2.0f);
        }

        for (int i = startRow; i < endRow; i++)
        {
            for (int j = startCol; j < endCol; j++)
            {
                if(c.row + i >= 0 && c.row + i < _mapWidth && c.col + j >= 0 && c.col + j < _mapHeight)
                {
                    ret = ret && _map[c.row + i][c.col + j] == null;
                }
            }
        }
        return ret;
    }

    // Returns the string representation of the connections possible at a certain position
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

    // Returns the string representation of the connections possible at a certain position
    private string getConnectionsSelf(int row, int col)
    {
        Coordinate c = new Coordinate(row, col);
        Coordinate rowPlus = new Coordinate(row + 1, col);
        Coordinate rowMinus = new Coordinate(row - 1, col);
        Coordinate colPlus = new Coordinate(row, col + 1);
        Coordinate colMinus = new Coordinate(row, col - 1);

        //int endWidth = _bossRoom.GetComponent<MapRoom>().BlockWidth();
        //int endHeight = _bossRoom.GetComponent<MapRoom>().BlockHeight();

        int endWidth = 3;
        int endHeight = 7;

        string s = "";

        if (checkForBlockExtent(colPlus, endWidth, endHeight, 'N') && _map[row][col].northDoor && _map[row][col].northDoor.activeInHierarchy)
        {
            s += "N";
        }
        if (checkForBlockExtent(colMinus, endWidth, endHeight, 'S') && _map[row][col].southDoor && _map[row][col].southDoor.activeInHierarchy)
        {
            s += "S";
        }
        if (checkForBlockExtent(rowPlus, endHeight, endWidth, 'E') && _map[row][col].eastDoor && _map[row][col].eastDoor.activeInHierarchy)
        {
            s += "E";
        }
        if (checkForBlockExtent(rowMinus, endHeight, endWidth, 'W') && _map[row][col].westDoor && _map[row][col].westDoor.activeInHierarchy)
        {
            s += "W";
        }
        return s;
    }

    // Alternate version of above, checks for connectors, intermediates, and other blocks
    // Needed to use because of second pass connector updates - need to recgnize where other connectors are
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

    // Giiven a string of connections, represented in pairs (source direction_destination direction), selects
    // the appropriate connector block and rotates it to properly connect its surroundings
    private void pickAndPlaceDoubleAlternate(int row, int col, string c)
    {
        MapRoom b = null;
        switch (c)
        {
            case "EE":
                b = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "WW":
                b = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "NN":
                b = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "SS":
                b = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "EN":
                b = Instantiate(connectorNW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "ES":
                b = Instantiate(connectorSW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "EW":
                b = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "NE":
                b = Instantiate(connectorNE, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "NW":
                b = Instantiate(connectorNW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "NS":
                b = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "WN":
                b = Instantiate(connectorNW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "WE":
                b = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "WS":
                b = Instantiate(connectorSW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "SE":
                b = Instantiate(connectorSE, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "SW":
                b = Instantiate(connectorSW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "SN":
                b = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                break;
            default:
                break;
        }
        canPlaceIntermediate(row, col, b);
        fillBlock(row, col, b);
        _map[row][col] = b.At(0, 0);
    }

    // Technically used for both first pass and second pass connecting
    // Loops over all grid spaces, then finds adjacent blocks in the map
    // Based on the number and direction of adjacent blocks, selects the appropriate connector block and rotates it
    // NOTE: Second pass has connectors already placed. If it encounters a conector, it deletes and replaces it.
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
                    DestroyImmediate(_map[row][col].gameObject);
                } else
                {
                    continue;
                }
                switch (c.Length)
                {
                    case 2:
                        MapRoom b = null;
                        switch (c)
                        {
                            case "NW":
                                b = Instantiate(connectorNW, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "NE":
                                b = Instantiate(connectorNE, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "NS":
                                b = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "SW":
                                b = Instantiate(connectorSW, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "SE":
                                b = Instantiate(connectorSE, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "EW":
                                b = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            default:
                                break;
                        }
                        canPlaceIntermediate(row, col, b);
                        fillBlock(row, col, b);
                        _map[row][col] = b.At(0, 0);
                        break;
                    case 3:
                        MapRoom b2 = null;
                        switch(c)
                        {
                            case "NSE":
                                b2 = Instantiate(connectorNSE, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "NSW":
                                b2 = Instantiate(connectorNSW, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "SEW":
                                b2 = Instantiate(connectorSEW, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                            case "NEW":
                                b2 = Instantiate(connectorNEW, spawnObject.transform).GetComponent<MapRoom>();
                                break;
                        }
                        canPlaceIntermediate(row, col, b2);
                        fillBlock(row, col, b2);
                        _map[row][col] = b2.At(0, 0);
                        break;
                    case 4:
                        MapRoom b3 = Instantiate(connector4, spawnObject.transform).GetComponent<MapRoom>();
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

    // Function to create the path from one block to another giiven a string path, that contains direction pairs
    private void pathFromString(int row, int col, string path, bool isStart)
    {
        if (path.Length <= 1)
        {
            Debug.LogError("Something went wrong with path from string");
        }
        else
        {
            int offset = isStart ? 1 : 0;
            switch (path[offset])
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
            for (int i = offset; i < path.Length - 1; i++)
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

    // Coordinate struct to make row and column passing easier
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

    // Flood fill BFS from a source block that gets the path to the nearest intermediate
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

    // Contains the set of coordinates that are reachable from the start. Gets updated as the islands get joined
    HashSet<Coordinate> visitedStart = new HashSet<Coordinate>();

    // Flood fill BFS to find islands of blocks
    // Only goes in directions that are accepted by the surrounding blocks, i.e to go left, the block on the right must have its "EAST" connection open.
    private List<Coordinate> BFSGetGroup(Coordinate start)
    {
        Queue<Coordinate> queue = new Queue<Coordinate>();
        List<Coordinate> ret = new List<Coordinate>();
        if (_map[start.row][start.col] == null)
        {
            return ret;
        }

        if (checkForBlockAll(start))
        {
            ret.Add(start);
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

    // Another flood fill BFS that gets the path from an intermediate "start" to its closest other intermediate that is in the start island block group
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
            Debug.LogWarning("CANNOT FIND PATH FROM SOURCE TO DESTINATION!!");
            return "";
        }
        char[] charArray = path.Substring(0, path.Length - 1).ToCharArray();
        Array.Reverse(charArray);
        string ret = new string(charArray);

        return ret;
    }

    // Loop over all blocks to find which outlets lead to null
    // If direction is valid and has another block with accepting opposite direction, then open the door (deactivate the gameobject)
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
                        I.northDoorOpen.SetActive(true);
                    }
                    if (s.Contains('E') && I.eastDoor)
                    {
                        I.eastDoor.SetActive(false);
                        I.eastDoorOpen.SetActive(true);
                    }
                    if (s.Contains('S') && I.southDoor)
                    {
                        I.southDoor.SetActive(false);
                        I.southDoorOpen.SetActive(true);
                    }
                    if (s.Contains('W') && I.westDoor)
                    {
                        I.westDoor.SetActive(false);
                        I.westDoorOpen.SetActive(true);
                    }
                }
            }
        }
    }

    private void placeEnd(List<Coordinate> allIntermediates, Coordinate start)
    {
        List<Coordinate> sortedCoordinates = allIntermediates.OrderByDescending(c => c.squaredDistanceTo(start)).ToList();
        foreach (Coordinate c in sortedCoordinates)
        {
            string s = getConnectionsSelf(c.row, c.col);
            bool found = false;
            bool north = false;
            bool south = false;
            bool east = false;
            bool west = false;
            Coordinate dstCoord = new Coordinate(c.row, c.col);
            Coordinate frstCoord = new Coordinate(c.row, c.col);
            Coordinate connCord = new Coordinate(c.row, c.col);
            Coordinate bossCoord = new Coordinate(c.row, c.col);
            MapRoom b = null;
            if(s.Length == 0)
            {
                continue;
            }
            if (s.Contains('N'))
            {
                found = true;
                north = true;
                dstCoord.col += 2;
                frstCoord.col += 1;
                connCord.col += 4;
                bossCoord.col += 5;
                bossCoord.row -= 1;
                _map[c.row][c.col].northDoor.SetActive(false);
                _map[c.row][c.col].northDoorOpen.SetActive(true);
                b = Instantiate(_restRoomVertical, spawnObject.transform).GetComponent<MapRoom>();
            }
            if (s.Contains('E') && !found)
            {
                found = true;
                east = true;
                dstCoord.row += 2;
                frstCoord.row += 1;
                connCord.row += 4;
                bossCoord.row += 5;
                bossCoord.col -= 1;
                _map[c.row][c.col].eastDoor.SetActive(false);
                _map[c.row][c.col].eastDoorOpen.SetActive(true);
                b = Instantiate(_restRoomHorizontal, spawnObject.transform).GetComponent<MapRoom>();
            }
            if (s.Contains('S') && !found)
            {
                found = true;
                south = true;
                dstCoord.col -= 3;
                frstCoord.col -= 1;
                connCord.col -= 4;
                bossCoord.col -= 7;
                bossCoord.row -= 1;
                _map[c.row][c.col].southDoor.SetActive(false);
                _map[c.row][c.col].southDoorOpen.SetActive(true);
                b = Instantiate(_restRoomVertical, spawnObject.transform).GetComponent<MapRoom>();
            }
            if (s.Contains('W') && !found)
            {
                west = true;
                _map[c.row][c.col].westDoor.SetActive(false);
                _map[c.row][c.col].westDoorOpen.SetActive(true);
                dstCoord.row -= 3;
                frstCoord.row -= 1;
                connCord.row -= 4;
                bossCoord.row -= 7;
                bossCoord.col -= 1;
                b = Instantiate(_restRoomHorizontal, spawnObject.transform).GetComponent<MapRoom>();
            }
            b.gameObject.transform.position = getOffset(dstCoord.row, dstCoord.col, b); // place 1x2 or 2x1 

            if (north || south)
            {
                b.At(0, 0).southDoor.SetActive(false);
                b.At(0, 0).southDoorOpen.SetActive(true);
                b.At(0, 1).northDoor.SetActive(false);
                b.At(0, 1).northDoorOpen.SetActive(true);
            }
            else
            {
                b.At(0, 0).westDoor.SetActive(false);
                b.At(0, 0).westDoorOpen.SetActive(true);
                b.At(1, 0).eastDoor.SetActive(false);
                b.At(1, 0).eastDoorOpen.SetActive(true);
            }

            MapRoom connector;
            MapRoom connector1;
            if (north || south)
            {
                connector = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                connector1 = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
            } else
            {
                connector = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                connector1 = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
            }
            connector.gameObject.transform.position = getOffset(connCord.row, connCord.col, connector);
            connector1.gameObject.transform.position = getOffset(frstCoord.row, frstCoord.col, connector1);

            MapRoom bossRoom = Instantiate(_bossRoom, spawnObject.transform).GetComponent<MapRoom>();
            bossRoom.gameObject.transform.position = getOffset(bossCoord.row, bossCoord.col, bossRoom);
            bossRoom.At(0, 0).westDoor.SetActive(!east);
            bossRoom.At(0, 0).westDoorOpen.SetActive(east);
            bossRoom.At(0, 0).eastDoor.SetActive(!west);
            bossRoom.At(0, 0).eastDoorOpen.SetActive(west);
            bossRoom.At(0, 0).northDoor.SetActive(!south);
            bossRoom.At(0, 0).northDoorOpen.SetActive(south);
            bossRoom.At(0, 0).southDoor.SetActive(!north);
            bossRoom.At(0, 0).southDoorOpen.SetActive(north);

            break;
        }
    }

    // Main generation function
    void GenerateRoom() {
        int seed = seed = UnityEngine.Random.Range(0, int.MaxValue);

        if(mapSeed < 0) {
            UnityEngine.Random.InitState(seed);
            Debug.Log("SEED: " + seed);
        } else
        {
            UnityEngine.Random.InitState(mapSeed);
            Debug.Log("SEED: " + mapSeed);
        }

        // Get mid width and height to place the start block
        int midWidth = (_mapWidth - 1) / 2;
        int midHeight = (_mapHeight - 1) / 2;

        Coordinate startCoordinate = new Coordinate(midWidth, midHeight);

        MapRoom b = Instantiate(_startBlock, spawnObject.transform).GetComponent<MapRoom>();
        _intermediateRooms.Add(b);
        b.gameObject.transform.position = getOffset(midWidth - 1, midHeight, b);
        _map[midWidth - 1][midHeight] = b.At(0, 0);
        _map[midWidth][midHeight] = b.At(1, 0);

        // Randomly sparse intermediate blocks
        placeIntermediates(numIntermediates);

        // Connect all adjacent blocks together
        firstSweepConnect();

        // If there is no connecting block already, the start needs to be hooked up
        // So find the closest path from it to another intermediate and make the path
        if (_map[midWidth + 1][midHeight] == null)
        {
            string path = BFSPathFromStart(new Coordinate(midWidth + 1, midHeight));
            pathFromString(midWidth + 1, midHeight, path, true);
        }

        // Create a 4-way connector to symbolize that anything can connect
        // at the point right outside the start room
        MapRoom b2 = Instantiate(connector4, spawnObject.transform).GetComponent<MapRoom>();
        if (canPlaceIntermediate(midWidth + 1, midHeight, b2))
        {
            fillBlock(midWidth + 1, midHeight, b2);
            _map[midWidth + 1][midHeight] = b2.At(0, 0);
            b2.At(0, 0).setDirections(true, true, true, true);
        } else
        {
            DestroyImmediate(b2.gameObject);
        }

        // Get a list of all the intermediates that are in the starting "island"
        List<Coordinate> startIntermediates = new List<Coordinate>();
        List<Coordinate> startIsland = BFSGetGroup(new Coordinate(midWidth + 1, midHeight));
        foreach(Coordinate c in startIsland)
        {
            if (_map[c.row][c.col].BlockType() == "Intermediate")
            {
                startIntermediates.Add(c);
            }
        }

        // Now get all of the groups that are not connected to the start "island"
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

        // For each disconnected group, loop over pairs between the disconnected group and the start group
        // and check which path is shortest to connect the two. Keep trying until you have exhausted all possible connnections
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
                        if (visited.Contains(inter))
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
                    if (s.Length <= 1)
                    {
                        iter++;
                        visited.Add(closestInter);
                        continue;
                    }
                    pathFromString(disconnectedCoordinate.row, disconnectedCoordinate.col, s, false);
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

        // After all connectors have been placed, touch up the connectors by looping again
        // and re-doing the connections with new adjacencies
        firstSweepConnect();

        // Open the doors and cap the doors leading to nowhere
        openDoors();

        // Place the end block
        placeEnd(startIntermediates, new Coordinate(midWidth, midHeight));

        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].BlockType() == "Intermediate")
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    m.difficulty = int.MaxValue;
                }
            }
        }

        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].BlockType() == "Intermediate")
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    m.difficulty = (int)Mathf.Min(m.difficulty, Mathf.Sqrt(new Coordinate(i, j).squaredDistanceTo(startCoordinate) * 1.5f));
                }
            }
        }

        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].BlockType() == "Intermediate")
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    m.SpawnEnemies();
                }
            }
        }

        // Debug purposes, color the grid with debug lines
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
                        case "End":
                            c = new Color(1, 0.5f, 0f);
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
