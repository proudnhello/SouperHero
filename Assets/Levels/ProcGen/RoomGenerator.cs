using System;
using System.Collections.Generic;
using System.Linq;
using NavMeshPlus.Components;
using Unity.VisualScripting;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    public int TILE_WIDTH;
    public int TILE_HEIGHT;

    public int _mapBaseWidth;
    
    public int _mapBaseHeight;
    public int _mapPadding;

    int _mapWidth
    {
        get { return _mapBaseWidth + _mapPadding*2; }
    }
    int _mapHeight
    {
        get { return _mapBaseHeight + _mapPadding*2; }
    }
    int _mapMinWidth
    {
        get { return _mapPadding; }
    }
    int _mapMinHeight
    {
        get { return _mapPadding; }
    }

    private List<List<Block>> _map;
    private List<MapRoom> _intermediateRooms;
    private List<Coordinate> _intermediateCoordinates;

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
    public int newSeed = -1;
    public int numIntermediates = 10;
    public float difficultyMultiplier = 3f;

    public GameObject spawnObject;

    private List<String> enemiesList;
    private List<String> foragablesList;

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

        public Coordinate(Coordinate other)
        {
            row = other.row;
            col = other.col;
        }

        public bool equals(Coordinate other)
        {
            return row == other.row && col == other.col;
        }

        public override string ToString()
        {
            return row + ", " + col;
        }

        public Block getBlock(List<List<Block>> map)
        {
            if (!isInBounds(map)) return null;
            return map[row][col];
        }

        public bool isInBounds(List<List<Block>> map)
        {
            if (row < 0 || row >= map.Count) return false;
            if (col < 0 || col >= map[row].Count) return false;
            return true;
        }

        public bool blockExists(List<List<Block>> map)
        {
            return getBlock(map) != null;
        }

        public float squaredDistanceTo(Coordinate other)
        {
            int dX = other.col - col;
            int dY = other.row - row;
            return (dX * dX) + (dY * dY);
        }
    }

    void Start()
    {
        // Need to create a new map full of nulls, placeholders for the Blocks and to determine if there is/isnt a block at a position
        _map = new List<List<Block>>();
        _intermediateRooms = new List<MapRoom>();
        _intermediateCoordinates = new List<Coordinate>();
        for(int i = 0; i < _mapWidth+_mapPadding; i++)
        {
            _map.Add(new List<Block>());
        }
        for(int i = 0; i < _mapWidth+_mapPadding; i++) { 
            for (int j = 0; j < _mapHeight+_mapPadding; j++)
            {
                _map[i].Add(null);
            }
        }

        if(enemiesList == null){
            enemiesList = new List<String>();
        }
        if(foragablesList == null){
            foragablesList = new List<String>();
        }
        // After map is created, generate the rooms
        bool worked = GenerateRoom();
        if (worked) NavMesh.BuildNavMeshAsync();
    }

    // Obtains the offset needed to position the room along grid lines given a row and column
    private Vector2 getOffset(Coordinate c, MapRoom b)
    {
        return new Vector2((c.row + (b.BlockWidth() / 2.0f)) * TILE_WIDTH, (c.col + (b.BlockHeight() / 2.0f)) * TILE_HEIGHT);
    }

    // Fills map blocks to properly represent the room being placed
    private void fillBlock(Coordinate c, MapRoom b)
    {
        for (int i = 0; i < b.BlockWidth(); ++i)
        {
            for (int j = 0; j < b.BlockHeight(); j++)
            {
                _map[c.row + i][c.col + j] = b.At(i, j);
            }
        }
    }

    // Checks to see if an intermediate can be placed by checking bounds and if potential spot has blocks already there
    private bool canPlaceIntermediate(Coordinate c, MapRoom b)
    {
        if (c.row + b.BlockWidth() >= (_mapMinWidth + _mapBaseWidth + 1) ||
            c.col + b.BlockHeight() >= (_mapMinHeight + _mapBaseHeight + 1) ||
            c.row < _mapMinWidth ||
            c.col < _mapMinHeight)
        {
            return false;
        }

        for (int i = 0; i < b.BlockWidth(); ++i)
        {
            for (int j = 0; j < b.BlockHeight(); ++j)
            {
                if (_map[c.row + i][c.col + j] != null)
                {
                    return false;
                }
            }
        }

        b.gameObject.transform.position = getOffset(c, b);
        fillBlock(c, b);
        return true;
    }

    private int breakLimit = 20;

    void placeIntermediates(int numIntermediates)
    {
        for (int i = 0; i < numIntermediates; i++)
        {
            bool placed = false;
            MapRoom b = null;
            int counter = 0;
            while (!placed && counter < breakLimit)
            {
                int blockType = UnityEngine.Random.Range(0, _intermediateBlocks.Count);
                b = Instantiate(_intermediateBlocks[blockType], spawnObject.transform).GetComponent<MapRoom>();

                Coordinate newCord = new Coordinate(UnityEngine.Random.Range(_mapMinWidth, _mapBaseWidth+_mapMinWidth+1),
                UnityEngine.Random.Range(_mapMinHeight, _mapBaseHeight+_mapMinHeight+1));

                if (canPlaceIntermediate(newCord, b))
                {
                    placed = true;
                    _intermediateCoordinates.Add(newCord);
                } else
                {
                    DestroyImmediate(b.gameObject, true);
                    counter++;
                }
            }
            if(b != null)
            {
                _intermediateRooms.Add(b);
            }
        }
    }

    private bool checkForBlock(Coordinate c, RoomType r, char dir)
    {
        if(!c.blockExists(_map))
        {
            return false;
        }
        Block b = _map[c.row][c.col];
        bool ret = b != null && ((b.BlockType() & r) != 0);
        switch (dir) {
            case 'N':
                ret = ret && b.south;
                break;
            case 'S':
                ret = ret && b.north;
                break;
            case 'E':
                ret = ret && b.west;
                break;
            case 'W':
                ret = ret && b.east;
                break;
            default:
                break;
        }
        return ret;
    }

    private bool checkForBlockExtentEnd(Coordinate c, Coordinate src, int width, int height, char dir)
    {
        // I didnt even know you could inline a switch statement into a variable declaration like this WTF? THANKS CHATGPT

        int halfW = Mathf.FloorToInt(width / 2f);
        int halfH = Mathf.FloorToInt(height / 2f);
        var cell = src.getBlock(_map);
        bool ret = true;

        (int startRow, int endRow, int startCol, int endCol, bool doorActive) = dir switch
        {
            'N' => (-halfW, halfW, 0, height, cell?.northDoor != null && cell.northDoor.activeInHierarchy == true),
            'S' => (-halfW, halfW, -height, 0, cell?.southDoor != null && cell.southDoor.activeInHierarchy == true),
            'E' => (0, width, -halfH, halfH, cell?.eastDoor != null && cell.eastDoor.activeInHierarchy == true),
            'W' => (-width, 0, -halfH, halfH, cell?.westDoor != null && cell.westDoor.activeInHierarchy == true),
            _ => (0, 0, 0, 0, false)
        };

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
        return ret && doorActive;
    }

    // Returns the string representation of the connections possible at a certain position
    private string getConnectionsAt(Coordinate c)
    {
        Coordinate rowPlus = new Coordinate(c.row + 1, c.col);
        Coordinate rowMinus = new Coordinate(c.row - 1, c.col);
        Coordinate colPlus = new Coordinate(c.row, c.col + 1);
        Coordinate colMinus = new Coordinate(c.row, c.col - 1);
        string s = "";

        if (checkForBlock(colPlus, RoomType.NOT_CONNECTOR, 'N'))
        {
            s += "N";
        }
        if (checkForBlock(colMinus, RoomType.NOT_CONNECTOR, 'S'))
        {
            s += "S";
        }
        if (checkForBlock(rowPlus, RoomType.NOT_CONNECTOR, 'E'))
        {
            s += "E";
        }
        if (checkForBlock(rowMinus, RoomType.NOT_CONNECTOR, 'W'))
        {
            s += "W";
        }
        return s;
    }

    // Returns the string representation of the connections possible at a certain position
    private string getConnectionsEnd(int row, int col)
    {
        Coordinate c = new Coordinate(row, col);
        Coordinate rowPlus = new Coordinate(row + 1, col);
        Coordinate rowMinus = new Coordinate(row - 1, col);
        Coordinate colPlus = new Coordinate(row, col + 1);
        Coordinate colMinus = new Coordinate(row, col - 1);

        int endWidth = 3;
        int endHeight = 7;

        string s = "";

        if (checkForBlockExtentEnd(colPlus, c, endWidth, endHeight, 'N'))
        {
            s += "N";
        }
        if (checkForBlockExtentEnd(colMinus, c, endWidth, endHeight, 'S'))
        {
            s += "S";
        }
        if (checkForBlockExtentEnd(rowPlus, c, endHeight, endWidth, 'E'))
        {
            s += "E";
        }
        if (checkForBlockExtentEnd(rowMinus, c, endHeight, endWidth, 'W'))
        {
            s += "W";
        }
        return s;
    }

    // Alternate version of above, checks for connectors, intermediates, and other blocks
    // Needed to use because of second pass connector updates - need to recgnize where other connectors are
    private string getConnectionsAtAdvanced(Coordinate c)
    {
        Coordinate rowPlus = new Coordinate(c.row + 1, c.col);
        Coordinate rowMinus = new Coordinate(c.row - 1, c.col);
        Coordinate colPlus = new Coordinate(c.row, c.col + 1);
        Coordinate colMinus = new Coordinate(c.row, c.col - 1);

        string s = "";

        var cell = colPlus.getBlock(_map);
        if (cell?.compareType(RoomType.CONNECTOR) == true || cell?.south == true)
        {
            s += "N";
        }
        cell = colMinus.getBlock(_map);
        if (cell?.compareType(RoomType.CONNECTOR) == true || cell?.north == true)
        {
            s += "S";
        }
        cell = rowPlus.getBlock(_map);
        if (cell?.compareType(RoomType.CONNECTOR) == true || cell?.west == true)
        {
            s += "E";
        }
        cell = rowMinus.getBlock(_map);
        if (cell?.compareType(RoomType.CONNECTOR) == true || cell?.east == true)
        {
            s += "W";
        }
        return s;
    }

    // Given a string of connections, represented in pairs (source direction_destination direction), selects
    // the appropriate connector block
    private void pickAndPlaceDoubleAlternate(Coordinate c, string s)
    {
        MapRoom b = null;
        switch (s)
        {
            case "EE":
            case "WW":
                b = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "NN":
            case "SS":
                b = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "SW":
            case "EN":
                b = Instantiate(connectorNW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "NW":
            case "ES":
                b = Instantiate(connectorSW, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "WN":
            case "SE":
                b = Instantiate(connectorNE, spawnObject.transform).GetComponent<MapRoom>();
                break;
            case "WS":
            case "NE":
                b = Instantiate(connectorSE, spawnObject.transform).GetComponent<MapRoom>();
                break;
            default:
                break;
        }
        if(!canPlaceIntermediate(c, b))
        {
            DestroyImmediate(b.gameObject);
        }
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
                Coordinate currentCoord = new Coordinate(row, col);
                var block = currentCoord.getBlock(_map);
                if (!block)
                {
                    c = getConnectionsAt(currentCoord);
                } else if (block.compareType(RoomType.CONNECTOR))
                {
                    c = getConnectionsAtAdvanced(currentCoord);
                    DestroyImmediate(block.gameObject);
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
                        if(!canPlaceIntermediate(currentCoord, b))
                        {
                            DestroyImmediate(b.gameObject);
                        }
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
                        if (!canPlaceIntermediate(currentCoord, b2))
                        {
                            DestroyImmediate(b2.gameObject);
                        }
                        break;
                    case 4:
                        MapRoom b3 = Instantiate(connector4, spawnObject.transform).GetComponent<MapRoom>();
                        if (!canPlaceIntermediate(currentCoord, b3))
                        {
                            DestroyImmediate(b3.gameObject);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // Function to create the path from one block to another given a string path, that contains direction pairs
    private bool pathFromString(Coordinate c, string path)
    {
        if (path.Length <= 1)
        {
            Debug.LogError("Something went wrong with path from string");
            return false;
        }
        switch (path[0])
        {
            case 'N':
                c.col++;
                break;
            case 'S':
                c.col--;
                break;
            case 'E':
                c.row++;
                break;
            case 'W':
                c.row--;
                break;
        }
        for (int i = 0; i < path.Length-1; i++)
        {
            pickAndPlaceDoubleAlternate(c, path.Substring(i, 2));
            char dir = path[i + 1];
            switch (dir)
            {
                case 'N':
                    c.col++;
                    break;
                case 'S':
                    c.col--;
                    break;
                case 'E':
                    c.row++;
                    break;
                case 'W':
                    c.row--;
                    break;
            }
        }
        return true;
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

        if (start.blockExists(_map))
        {
            ret.Add(start);
            visitedStart.Add(start);
        }

        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new Coordinate(b.row + 1, b.col);
            Coordinate rowMinus = new Coordinate(b.row - 1, b.col);
            Coordinate colPlus = new Coordinate(b.row, b.col + 1);
            Coordinate colMinus = new Coordinate(b.row, b.col - 1);

            var block = rowMinus.getBlock(_map);
            if (block != null && block.compareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(rowMinus) && block.east)
            {
                queue.Enqueue(rowMinus);
                visitedStart.Add(rowMinus);
                ret.Add(rowMinus);
            }
            block = rowPlus.getBlock(_map);
            if (block != null && block.compareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(rowPlus) && block.west)
            {
                queue.Enqueue(rowPlus);
                visitedStart.Add(rowPlus);
                ret.Add(rowPlus);
            }
            block = colPlus.getBlock(_map);
            if (block != null && block.compareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(colPlus) && block.south)
            {
                queue.Enqueue(colPlus);
                visitedStart.Add(colPlus);
                ret.Add(colPlus);
            }
            block = colMinus.getBlock(_map);
            if (block != null && block.compareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(colMinus) && block.north)
            {
                queue.Enqueue(colMinus);
                visitedStart.Add(colMinus);
                ret.Add(colMinus);
            }
        }

        return ret;
    }

    // Another flood fill BFS that gets the path from an intermediate "start" to its closest other intermediate that is in the start island block group
    private string BFSPathToClosestIntermediate(Coordinate start, List<Coordinate> startIsland, bool fromStart)
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
        float minDistance = float.MaxValue;
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new Coordinate(b.row + 1, b.col);
            Coordinate rowMinus = new Coordinate(b.row - 1, b.col);
            Coordinate colPlus = new Coordinate(b.row, b.col + 1);
            Coordinate colMinus = new Coordinate(b.row, b.col - 1);

            var directions = new List<(Coordinate offset, char dir)>
                {
                    (rowMinus, 'W'),
                    (rowPlus, 'E'),
                    (colPlus, 'N'),
                    (colMinus, 'S'),
                };

            foreach (var (offset, dir) in directions)
            {
                if(!offset.isInBounds(_map))
                {
                    continue;
                }
                if (offset.getBlock(_map) == null && !visited.Contains(offset))
                {
                    queue.Enqueue(offset);
                    visited.Add(offset);
                    parents[offset] = new Tuple<Coordinate, char>(b, dir);
                }
                else
                {
                    if (offset.getBlock(_map) != null && destinations.Contains(offset))
                    {
                        visited.Add(offset);
                        parents[offset] = new Tuple<Coordinate, char>(b, dir);
                        if (start.squaredDistanceTo(offset) < minDistance)
                        {
                            closestIntermediate = offset;
                            minDistance = start.squaredDistanceTo(offset);
                        }
                        path += dir;
                        break;
                    }
                }
            }
            if(closestIntermediate.row != -1)
            {
                break;
            }
        }

        while (closestIntermediate.row != -1 && closestIntermediate.col != -1)
        {
            path += parents[closestIntermediate].Item2;
            if (closestIntermediate.equals(parents[closestIntermediate].Item1))
            {
                Debug.LogError("NAHHHHHH UR FUCKED LMFAOOOO");
                return "";
            }
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

        if(fromStart)
        {
            return "E" + ret;
        }

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
                var cell = c.getBlock(_map);
                if (cell?.compareType(RoomType.INTERMEDIATE) == true)
                {
                    Block I = _map[c.row][c.col];
                    string s = getConnectionsAtAdvanced(c);
                    I.setDoors(s.Contains('S'), s.Contains('N'), s.Contains('W'), s.Contains('E'));
                }
            }
        }
    }

    private void placeEnd(List<Coordinate> allIntermediates, Coordinate start)
    {
        List<Coordinate> sortedCoordinates = allIntermediates.OrderByDescending(c => c.squaredDistanceTo(start)).ToList();
        foreach (Coordinate c in sortedCoordinates)
        {
            string s = getConnectionsEnd(c.row, c.col);
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

            void NCheck()
            {
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
            }

            void ECheck()
            {
                if (s.Contains('E'))
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
            }

            void SCheck()
            {
                if (s.Contains('S'))
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
            }
            
            void WCheck()
            {
                if (s.Contains('W'))
                {
                    found = true;
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
            }

            
            for (int count = 0, i = UnityEngine.Random.Range(0, 4); count < 4 && !found; count++, i=(i+1)%4)
            {
                if (i == 0) NCheck();
                else if (i == 1) ECheck();
                else if (i == 2) SCheck();
                else if (i == 3) WCheck();
            }
            
            
            b.gameObject.transform.position = getOffset(dstCoord, b); // place 1x2 or 2x1 

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
            connector.gameObject.transform.position = getOffset(connCord, connector);
            connector1.gameObject.transform.position = getOffset(frstCoord, connector1);

            MapRoom bossRoom = Instantiate(_bossRoom, spawnObject.transform).GetComponent<MapRoom>();
            bossRoom.gameObject.transform.position = getOffset(bossCoord, bossRoom);
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
    bool GenerateRoom() {
        int seed = UnityEngine.Random.Range(0, int.MaxValue);
        if(mapSeed < 0) { // random seed if the seed value is -1
            UnityEngine.Random.InitState(seed);
            newSeed = seed;
        } else // else we are being given a seed value for debug testing
        {
            UnityEngine.Random.InitState(mapSeed);
            newSeed = mapSeed;
        }
        Debug.Log("SEED: " + newSeed);

        // Get mid width and height to place the start block
        int midWidth = (_mapWidth - 1) / 2;
        int midHeight = (_mapHeight - 1) / 2;
        Coordinate startCoordinate = new Coordinate(midWidth, midHeight);

        MapRoom startRoom = Instantiate(_startBlock, spawnObject.transform).GetComponent<MapRoom>();
        canPlaceIntermediate(new Coordinate(startCoordinate.row - 1, startCoordinate.col), startRoom);
        _intermediateRooms.Add(startRoom);

        // Randomly sparse intermediate blocks
        placeIntermediates(numIntermediates);

        // Connect all adjacent blocks together
        firstSweepConnect();

        // If there is no connecting block already, the start needs to be hooked up
        // So find the closest path from it to another intermediate and make the path
        Coordinate directlyWest = new Coordinate(midWidth + 1, midHeight);
        if (!directlyWest.blockExists(_map))
        {
            string path = BFSPathToClosestIntermediate(directlyWest, _intermediateCoordinates, true);
            if (!pathFromString(startCoordinate, path))
            {
                Debug.LogError("failed to make path from start");
                return false;
            }
        }

        // Create a 4-way connector to symbolize that anything can connect
        // at the point right outside the start room
        MapRoom b2 = Instantiate(connector4, spawnObject.transform).GetComponent<MapRoom>();
        if (canPlaceIntermediate(directlyWest, b2))
        {
            b2.At(0, 0).setDirections(true, true, true, true);
        } else
        {
            DestroyImmediate(b2.gameObject);
        }

        // Get a list of all the intermediates that are in the starting "island"
        List<Coordinate> startBlocks = new List<Coordinate>();
        List<Coordinate> startIsland = BFSGetGroup(startCoordinate);
        if(startIsland == null)
        {
            Debug.LogError("Breakout hit for start island");
            return false;
        }
        foreach(Coordinate c in startIsland)
        {
            if (_map[c.row][c.col].compareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR))
            {
                startBlocks.Add(c);
            }
        }

        // Now get all of the groups that are not connected to the start "island"
        List<List<Coordinate>> disconnectedGroups = new List<List<Coordinate>>();
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                Coordinate c = new Coordinate(i, j);
                if (checkForBlock(c, RoomType.ALL, 'X') && !visitedStart.Contains(c))
                {
                    List<Coordinate> newGroup = BFSGetGroup(c);
                    if(newGroup == null)
                    {
                        Debug.LogError("Breakout hit for disconnected island");
                        return false;
                    }
                    disconnectedGroups.Add(newGroup);
                }
            }
        }

        // For each disconnected group, loop over pairs between the disconnected group and the start group
        // and check which path is shortest to connect the two. Keep trying until you have exhausted all possible connnections
        foreach (List<Coordinate> group in disconnectedGroups)
        {
            int maxIter = 0;
            HashSet<Coordinate> visited = new HashSet<Coordinate>();
            while (maxIter < group.Count && maxIter >= 0)
            {
                // find mid dist between intermediate from start group and list
                float closestDistance = int.MaxValue;
                Coordinate disconnectedCoordinate = new Coordinate(-1, -1);
                Coordinate closestStartIntermediate = new Coordinate(-1, -1);
                foreach (Coordinate currentGroupCoord in group)
                {
                    foreach (Coordinate currentIntermediate in startBlocks)
                    {
                        if (currentGroupCoord.squaredDistanceTo(currentIntermediate) < closestDistance)
                        {
                            disconnectedCoordinate = currentGroupCoord;
                            closestStartIntermediate = currentIntermediate;
                            closestDistance = currentGroupCoord.squaredDistanceTo(currentIntermediate);
                        }
                    }
                }
                if (disconnectedCoordinate.row != -1 && closestStartIntermediate.row != -1)
                {
                    List<Coordinate> oneStartCoord = new List<Coordinate>
                    {
                        closestStartIntermediate
                    };
                    string s = BFSPathToClosestIntermediate(disconnectedCoordinate, oneStartCoord, false);
                    if(!pathFromString(disconnectedCoordinate, s))
                    {
                        maxIter++;
                        visited.Add(closestStartIntermediate);
                        continue;
                    }
                    maxIter = -1;
                } else
                {
                    maxIter++;
                    continue;
                }
            }
            if(maxIter == group.Count)
            {
                Debug.LogError("Could not find a path from one island to another");
                return false;
            }
            foreach (Coordinate coord in group)
            {
                if (_map[coord.row][coord.col].compareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR))
                {
                    startBlocks.Add(coord);
                }
            }
        }

        // After all connectors have been placed, touch up the connectors by looping again
        // and re-doing the connections with new adjacencies
        firstSweepConnect();

        // Open the doors and cap the doors leading to nowhere
        openDoors();

        // Place the end block
        placeEnd(startBlocks, new Coordinate(midWidth, midHeight));

        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].compareType(RoomType.INTERMEDIATE))
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    m.difficulty = (int)Mathf.Sqrt(new Coordinate(i, j).squaredDistanceTo(startCoordinate) * difficultyMultiplier);
                }
            }
        }

        if(foragablesList.Count == 0 || enemiesList.Count == 0){
            generateNewContent();
        }
        else{
            loadContent();
        }

        // Debug purposes, color the grid with debug lines
#if DEBUG
        colorGrid();
#endif
        return true;
    }

    public void generateNewContent(){
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].compareType(RoomType.INTERMEDIATE))
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    m.SpawnEntities();
                }
            }
        }
    }

    public void loadContent(){
        int count = 0;
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].compareType(RoomType.INTERMEDIATE))
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    if(count >= enemiesList.Count) return;
                    if(m.LoadEntities(enemiesList[count], foragablesList[count])) count++;
                }
            }
        }
    }

    public List<String> exportEnemyStrings(){
        List<String> list = new List<String>();
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].compareType(RoomType.INTERMEDIATE))
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    String res = m.ExportEnemies();
                    if(res != ":(") list.Add(res);
                }
            }
        }
        flushExportBooleans();
        return list;
    }

    public void importEnemyStrings(List<String> list){
        enemiesList = list;
    }

    public List<String> exportForagableStrings(){
        List<String> list = new List<String>();
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].compareType(RoomType.INTERMEDIATE))
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    String res = m.ExportForagables();
                    if(res != ":(") list.Add(res);
                }
            }
        }
        flushExportBooleans();
        return list;
    }

    public void importForagableStrings(List<String> list){
        foragablesList = list;
    }

    public void flushExportBooleans(){
        for (int i = 0; i < _mapWidth; i++)
        {
            for (int j = 0; j < _mapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].compareType(RoomType.INTERMEDIATE))
                {
                    EntityManager m = _map[i][j].gameObject.GetComponent<EntityManager>();
                    m.hasExportedEnemies = false;
                    m.hasExportedForagables = false;
                }
            }
        }
    }

#if DEBUG
    // DEBUG DRAWING
    private void colorGrid()
    {
        for (int i = 0; i < _mapBaseWidth; ++i)
        {
            for (int j = 0; j < _mapBaseHeight; ++j)
            {
                DrawRect(new Vector3((i + _mapPadding) * TILE_WIDTH, (j + _mapPadding) * TILE_WIDTH, 0), new Vector3(((i + _mapPadding) + 1) * TILE_WIDTH, ((j + _mapPadding) + 1) * TILE_HEIGHT, 0), Color.red);
            }
        }
        for (int i = _mapPadding; i < _mapBaseWidth + _mapPadding; ++i)
        {
            for (int j = _mapPadding; j < _mapBaseHeight + +_mapPadding; ++j)
            {
                if (_map[i][j])
                {
                    Color c;
                    switch (_map[i][j].BlockType())
                    {
                        case RoomType.START:
                            c = Color.yellow;
                            break;
                        case RoomType.CONNECTOR:
                            c = Color.green;
                            break;
                        case RoomType.INTERMEDIATE:
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
#endif
}
