using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NavMeshPlus.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

// Coordinate struct to make row and column passing easier
public struct Coordinate
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

    // Check if coordinates are equal
    public readonly bool Equals(Coordinate other)
    {
        return row == other.row && col == other.col;
    }

    public override readonly string ToString()
    {
        return row + ", " + col;
    }

    // Check if this coordinate is within the bounds of a given map
    public readonly bool IsInBounds(List<List<Block>> map)
    {
        if (row < 0 || row >= map.Count) return false;
        if (col < 0 || col >= map[row].Count) return false;
        return true;
    }

    // Check if a block exist, if it does, put it in the out parameter
    public readonly bool BlockExists(List<List<Block>> map, out Block block)
    {
        if (!IsInBounds(map))
        {
            block = null;
            return false;
        }
        else
        {
            block = map[row][col];
            if (block == null) return false;
            return true;
        }

    }

    public static Coordinate operator +(Coordinate left, Coordinate right)
    {
        return new(left.row + right.row, left.col + right.col);
    }

    // Get squared euclidian distance, squared for performance reasons and cuz we don't need actual distance
    public readonly float SquaredDistanceTo(Coordinate other)
    {
        int dX = other.col - col;
        int dY = other.row - row;
        return (dX * dX) + (dY * dY);
    }
}

public struct GenSequence
{
    public Coordinate first;
    public Coordinate room;
    public Coordinate connect;
    public Coordinate boss;
    public bool north;
    public bool south;
    public bool east;
    public bool west;

    public GenSequence(Coordinate c1, Coordinate c2, Coordinate c3, Coordinate c4, bool n, bool s, bool e, bool w)
    {
        room = c1; first = c2; connect = c3; boss = c4; north = n; south = s; east = e; west = w;
    }

    public override readonly string ToString()
    {
        return first.ToString() + ", " + room.ToString() + ", " + connect.ToString() + ", " + boss.ToString() + ": " + north + ", " + south + ", " + east + ", " + west;
    }
}

public class RoomGenerator : MonoBehaviour
{
    public int TILE_WIDTH;
    public int TILE_HEIGHT;

    public int _mapBaseWidth;
    
    public int _mapBaseHeight;
    public int _mapPadding;

    int MapWidth
    {
        get { return _mapBaseWidth + _mapPadding*2; }
    }
    int MapHeight
    {
        get { return _mapBaseHeight + _mapPadding*2; }
    }
    int MapMinWidth
    {
        get { return _mapPadding; }
    }
    int MapMinHeight
    {
        get { return _mapPadding; }
    }

    private List<List<Block>> _map;
    private List<MapRoom> _intermediateRooms;
    private List<Coordinate> _intermediateCoordinates;

    [Header("START BLOCK")]
    public GameObject _startBlock;
    [Header("INTERMEDIATE BLOCKS")]
    public List<MapRoom> _intermediateRoomOptions;
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
    public NavMeshSurface _NavMeshSurface;
    [Header("Generation Parameters")]
    public int mapSeed = -1;
    public int numIntermediates = 10;
    public float difficultyMultiplier = 3f;

    public GameObject spawnObject;

    void Start()
    {
        RunStateManager.Singleton.InitializeGameState(mapSeed);

        // Need to create a new map full of nulls, placeholders for the Blocks and to determine if there is/isnt a block at a position
        _map = new List<List<Block>>();
        _intermediateRooms = new List<MapRoom>();
        _intermediateCoordinates = new List<Coordinate>();
        for(int i = 0; i < MapWidth+_mapPadding; i++)
        {
            _map.Add(new List<Block>());
        }
        for(int i = 0; i < MapWidth+_mapPadding; i++) { 
            for (int j = 0; j < MapHeight+_mapPadding; j++)
            {
                _map[i].Add(null);
            }
        }

        // After map is created, generate the rooms
        StartCoroutine(GenerateRoom());

        RunStateManager.Singleton.SaveRunState();
        GameManager.Singleton.StartRun();        
    }

    // Obtains the offset needed to position the room along grid lines given a row and column
    private Vector2 GetOffset(Coordinate c, MapRoom b)
    {
        return new Vector2((c.row + (b.BlockWidth() / 2.0f)) * TILE_WIDTH, (c.col + (b.BlockHeight() / 2.0f)) * TILE_HEIGHT);
    }

    // Fills map blocks to properly represent the room being placed
    private void FillBlock(Coordinate c, MapRoom b)
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
    private bool CanPlaceIntermediate(Coordinate c, MapRoom b)
    {
        if (c.row + b.BlockWidth() >= (MapMinWidth + _mapBaseWidth + 1) ||
            c.col + b.BlockHeight() >= (MapMinHeight + _mapBaseHeight + 1) ||
            c.row < MapMinWidth ||
            c.col < MapMinHeight)
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

        b.gameObject.transform.position = GetOffset(c, b);
        FillBlock(c, b);
        return true;
    }

    // Break limit is the max amount of times the algorithm looks for a random block placement before it walks the plank
    private readonly int breakLimit = 20;
    void PlaceIntermediates(int numIntermediates)
    {
        for (int i = 0; i < numIntermediates; i++)
        {
            bool placed = false;
            MapRoom b = null;
            int counter = 0;
            while (!placed && counter < breakLimit)
            {
                int blockType = UnityEngine.Random.Range(0, _intermediateRoomOptions.Count);
                b = Instantiate(_intermediateRoomOptions[blockType], spawnObject.transform).GetComponent<MapRoom>();

                Coordinate newCord = new(UnityEngine.Random.Range(MapMinWidth, _mapBaseWidth+MapMinWidth+1),
                UnityEngine.Random.Range(MapMinHeight, _mapBaseHeight+MapMinHeight+1));

                if (CanPlaceIntermediate(newCord, b))
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

    // Check if block exists and the connections returned have viable connections
    private bool CheckForBlock(Coordinate c, RoomType r, char dir)
    {
        if (!c.BlockExists(_map, out Block b))
        {
            return false;
        }
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

    // Returns the string representation of the connections possible at a certain position
    private string GetConnectionsAt(Coordinate c)
    {
        Coordinate rowPlus = new(c.row + 1, c.col);
        Coordinate rowMinus = new(c.row - 1, c.col);
        Coordinate colPlus = new(c.row, c.col + 1);
        Coordinate colMinus = new(c.row, c.col - 1);
        string s = "";

        if (CheckForBlock(colPlus, RoomType.NOT_CONNECTOR, 'N'))
        {
            s += "N";
        }
        if (CheckForBlock(colMinus, RoomType.NOT_CONNECTOR, 'S'))
        {
            s += "S";
        }
        if (CheckForBlock(rowPlus, RoomType.NOT_CONNECTOR, 'E'))
        {
            s += "E";
        }
        if (CheckForBlock(rowMinus, RoomType.NOT_CONNECTOR, 'W'))
        {
            s += "W";
        }
        return s;
    }

    // Check for the possibility of the end block being placed at a certain position and direction
    private bool CheckForBlockExtentEnd(Coordinate c, Coordinate src, int width, int height, char dir)
    {
        // I didnt even know you could inline a switch statement into a variable declaration like this WTF? THANKS CHATGPT

        int halfW = Mathf.FloorToInt(width / 2f);
        int halfH = Mathf.FloorToInt(height / 2f);
        bool cellExists = src.BlockExists(_map, out Block cell);
        bool ret = true;

        (int startRow, int endRow, int startCol, int endCol, bool doorActive) = dir switch
        {
            'N' => (-halfW, halfW, 0, height, cellExists && cell.northDoor != null && cell.northDoor.activeInHierarchy == true),
            'S' => (-halfW, halfW, -height, 0, cellExists && cell.southDoor != null && cell.southDoor.activeInHierarchy == true),
            'E' => (0, width, -halfH, halfH, cellExists && cell.eastDoor != null && cell.eastDoor.activeInHierarchy == true),
            'W' => (-width, 0, -halfH, halfH, cellExists && cell.westDoor != null && cell.westDoor.activeInHierarchy == true),
            _ => (0, 0, 0, 0, false)
        };

        for (int i = startRow; i < endRow; i++)
        {
            for (int j = startCol; j < endCol; j++)
            {
                if(c.row + i >= 0 && c.row + i < MapWidth && c.col + j >= 0 && c.col + j < MapHeight)
                {
                    ret = ret && _map[c.row + i][c.col + j] == null;
                }
            }
        }
        return ret && doorActive;
    }

    // Returns the string representation of the connections possible at a certain position
    private string GetConnectionsEnd(Coordinate c, int checkWidth, int checkHeight)
    {
        Coordinate rowPlus = new(c.row + 1, c.col);
        Coordinate rowMinus = new(c.row - 1, c.col);
        Coordinate colPlus = new(c.row, c.col + 1);
        Coordinate colMinus = new(c.row, c.col - 1);

        string s = "";

        if (CheckForBlockExtentEnd(colPlus, c, checkWidth, checkHeight, 'N'))
        {
            s += "N";
        }
        if (CheckForBlockExtentEnd(colMinus, c, checkWidth, checkHeight, 'S'))
        {
            s += "S";
        }
        if (CheckForBlockExtentEnd(rowPlus, c, checkHeight, checkWidth, 'E'))
        {
            s += "E";
        }
        if (CheckForBlockExtentEnd(rowMinus, c, checkHeight, checkWidth, 'W'))
        {
            s += "W";
        }
        return s;
    }

    // Alternate version of above, checks for connectors, intermediates, and other blocks
    // Needed to use because of second pass connector updates - need to recgnize where other connectors are
    private string GetConnectionsAtAdvanced(Coordinate c)
    {
        Coordinate rowPlus = new(c.row + 1, c.col);
        Coordinate rowMinus = new(c.row - 1, c.col);
        Coordinate colPlus = new(c.row, c.col + 1);
        Coordinate colMinus = new(c.row, c.col - 1);

        string s = "";

        if (colPlus.BlockExists(_map, out Block cell) && (cell.CompareType(RoomType.CONNECTOR) == true || cell.south == true))
        {
            s += "N";
        }
        if (colMinus.BlockExists(_map, out Block cell1) && (cell1.CompareType(RoomType.CONNECTOR) == true || cell1.north == true))
        {
            s += "S";
        }
        if (rowPlus.BlockExists(_map, out Block cell2) && (cell2.CompareType(RoomType.CONNECTOR) == true || cell2.west == true))
        {
            s += "E";
        }
        if (rowMinus.BlockExists(_map, out Block cell3) && (cell3.CompareType(RoomType.CONNECTOR) == true || cell3.east == true))
        {
            s += "W";
        }
        return s;
    }

    // Given a string of connections, represented in pairs (<DIRECTION IN SOURCE>, <DIRECTION IN CURRENT>), selects
    // the appropriate connector block. Different because this is for placing connectors from a path.

    // Paths are represented just like the tuple above, except multiple are connected in a string. For example,
    // If I have a path that looks like this:

    //         |   |____|_____|
    //         |
    //         |________|_____|
    //
    // This would be represented as "SEE". The first block is coming from the previous block's south, going to the east,
    // and the next block is coming from the previous block's east going east as well.
    private void PickAndPlaceDoubleAlternate(Coordinate c, string s)
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
        if(!CanPlaceIntermediate(c, b))
        {
            DestroyImmediate(b.gameObject);
        }
    }

    // Technically used for both first pass and second pass connecting
    // Loops over all grid spaces, then finds adjacent blocks in the map
    // Based on the number and direction of adjacent blocks, selects the appropriate connector block and rotates it
    // NOTE: Second pass has connectors already placed. If it encounters a conector, it deletes and replaces it.
    private void FirstSweepConnect()
    {
        for (int row = 0; row < MapWidth; row++)
        {
            for (int col = 0; col < MapHeight; col++)
            {
                string c;
                Coordinate currentCoord = new(row, col);
                bool exists = currentCoord.BlockExists(_map, out Block block);
                if (!exists)
                {
                    c = GetConnectionsAt(currentCoord);
                } else if (block.CompareType(RoomType.CONNECTOR))
                {
                    c = GetConnectionsAtAdvanced(currentCoord);
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
                        if(!CanPlaceIntermediate(currentCoord, b))
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
                        if (!CanPlaceIntermediate(currentCoord, b2))
                        {
                            DestroyImmediate(b2.gameObject);
                        }
                        break;
                    case 4:
                        MapRoom b3 = Instantiate(connector4, spawnObject.transform).GetComponent<MapRoom>();
                        if (!CanPlaceIntermediate(currentCoord, b3))
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

    // Function to create the path from one block to another given a string path, that contains direction pairs,
    // as specified in the comment block above.
    private bool PathFromString(Coordinate c, string path)
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
            PickAndPlaceDoubleAlternate(c, path.Substring(i, 2));
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
    readonly HashSet<Coordinate> visitedStart = new();

    // Flood fill BFS to find islands of blocks
    // Only goes in directions that are accepted by the surrounding blocks, i.e to go left, the block on the left must have its "EAST" (right) connection open.
    private List<Coordinate> BFSGetGroup(Coordinate start)
    {
        Queue<Coordinate> queue = new();
        List<Coordinate> ret = new();
        if (_map[start.row][start.col] == null)
        {
            return ret;
        }

        // if dont want, just use discard _
        if (start.BlockExists(_map, out _))
        {
            ret.Add(start);
            visitedStart.Add(start);
        }

        // Enqueue the starting block. While blocks are needed to be searched, search them, dafuq?
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new(b.row + 1, b.col);
            Coordinate rowMinus = new(b.row - 1, b.col);
            Coordinate colPlus = new(b.row, b.col + 1);
            Coordinate colMinus = new(b.row, b.col - 1);

            // TODO: can wrap this like the other BFS function. Not important, still readable
            if (rowMinus.BlockExists(_map, out Block block) && block.CompareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(rowMinus) && block.east)
            {
                queue.Enqueue(rowMinus);
                visitedStart.Add(rowMinus);
                ret.Add(rowMinus);
            }
            if (rowPlus.BlockExists(_map, out Block block2) && block2.CompareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(rowPlus) && block2.west)
            {
                queue.Enqueue(rowPlus);
                visitedStart.Add(rowPlus);
                ret.Add(rowPlus);
            }
            if (colPlus.BlockExists(_map, out Block block3) && block3.CompareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(colPlus) && block3.south)
            {
                queue.Enqueue(colPlus);
                visitedStart.Add(colPlus);
                ret.Add(colPlus);
            }
            if (colMinus.BlockExists(_map, out Block block4) && block4.CompareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR) && !visitedStart.Contains(colMinus) && block4.north)
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
        Dictionary<Coordinate, Tuple<Coordinate, char>> parents = new();
        HashSet<Coordinate> visited = new();
        // move this out
        HashSet<Coordinate> destinations = new();
        Queue<Coordinate> queue = new();

        foreach (Coordinate c in startIsland) {
            destinations.Add(c);
        }

        // This follows a more traditional BFS, where because we are eventually trying to find a path, we are saving the parents of the blocks
        // that we are able to find. This makes it so that when we find a suitable destination block we can just reverse search the parents
        // to find a path to the start (because if we found a destination from a start, we should be able to follow parents from the destination
        // back to the start)
        queue.Enqueue(start);
        visited.Add(start);
        parents[start] = new Tuple<Coordinate, char>(new(-1, -1), ' ');
        Coordinate closestIntermediate = new(-1, -1);
        float minDistance = float.MaxValue;
        while (queue.Count > 0)
        {
            Coordinate b = queue.Dequeue();
            Coordinate rowPlus = new(b.row + 1, b.col);
            Coordinate rowMinus = new(b.row - 1, b.col);
            Coordinate colPlus = new(b.row, b.col + 1);
            Coordinate colMinus = new(b.row, b.col - 1);

            var directions = new List<(Coordinate offset, char dir)>
                {
                    (rowMinus, 'W'),
                    (rowPlus, 'E'),
                    (colPlus, 'N'),
                    (colMinus, 'S'),
                };

            foreach (var (offset, dir) in directions)
            {
                if(!offset.IsInBounds(_map))
                {
                    continue;
                }
                if (!offset.BlockExists(_map, out Block block) && !visited.Contains(offset))
                {
                    queue.Enqueue(offset);
                    visited.Add(offset);
                    parents[offset] = new Tuple<Coordinate, char>(b, dir);
                }
                else
                {
                    if (offset.BlockExists(_map, out Block bloock) && destinations.Contains(offset))
                    {
                        visited.Add(offset);
                        parents[offset] = new Tuple<Coordinate, char>(b, dir);
                        if (start.SquaredDistanceTo(offset) < minDistance)
                        {
                            closestIntermediate = offset;
                            minDistance = start.SquaredDistanceTo(offset);
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

        // Go through the parents from the destination back to the start and collect the path.
        while (closestIntermediate.row != -1 && closestIntermediate.col != -1)
        {
            path += parents[closestIntermediate].Item2;
            if (closestIntermediate.Equals(parents[closestIntermediate].Item1))
            {
                Debug.LogError("NAHHHHHH UR FUCKED LMFAOOOO");
                return "";
            }
            closestIntermediate = parents[closestIntermediate].Item1;
        }

        // If the path is 0, for some reason, you probably fucked up. This means that the blocks are
        // right next to each other, so this function shouldn't have been called in the first place.
        if(path.Length == 0)
        {
            Debug.LogError("CANNOT FIND PATH FROM SOURCE TO DESTINATION!!");
            return "";
        }

        // Since the path is currently from the destination to the start, we need to reverse it. WTF IS A RANGE OPERATOR COPILOT??
        char[] charArray = path[..^1].ToCharArray();
        Array.Reverse(charArray);
        string ret = new(charArray);

        // The starting path works a bit differently. Since we are going from the block next to it instead of
        // the starting block itself, we have to add an "EAST" to the start of the path.

        // Why is the start path from the block directly east? because the current function to find paths
        // finds in all directions, and the start block only has an opening east. It was a coing toss whether to implement
        // the start block edge case here or above, with the choosing of directions to search,
        if(fromStart)
        {
            return "E" + ret;
        }

        return ret;
    }

    // Loop over all blocks to find which outlets lead to null
    // If direction is valid and has another block with accepting opposite direction, then open the door (deactivate the gameobject)
    private void OpenDoors()
    {
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapHeight; j++)
            {
                Coordinate c = new(i, j);
                if (c.BlockExists(_map, out Block b) && b.CompareType(RoomType.INTERMEDIATE) == true)
                {
                    Block I = _map[c.row][c.col];
                    string s = GetConnectionsAtAdvanced(c);
                    I.SetDoors(s.Contains('N'), s.Contains('S'), s.Contains('E'), s.Contains('W'));
                }
            }
        }
    }

    // list of potential boss generation sequences
    readonly List<GenSequence> sequences = new()
    {
        // In the order: room pos, first connector, second connector, boss room
        new(new(0, 2), new(0, 1), new(0, 4), new(-1, 5), false, true, false, false), // north sequence
        new(new(0, -3), new(0, -1), new(0, -4), new(-1, -7), true, false, false, false), // south sequence
        new(new(2, 0), new(1, 0), new(4, 0), new(5, -1), false, false, false, true), // east sequence
        new(new(-3, 0), new(-1, 0), new(-4, 0), new(-7, -1), false, false, true, false), // west sequence
    };

    // Big function to generate the end sequence. Currently places a single I connctor, followed by a 1x2 or a 2x1 depending
    // on the direction it is going, followed by another I connector and finally the 3x3 boss room.

    // This means that we have to search for a 7x3 or 3x7 empty space to be able to place all the blocks. Goes outside the map
    // cuz it would be impossible to place it in the map, no shit.
    private void PlaceEnd(List<Coordinate> allIntermediates, Coordinate start)
    {
        List<Coordinate> sortedCoordinates = allIntermediates.OrderByDescending(c => c.SquaredDistanceTo(start)).ToList();
        foreach (Coordinate c in sortedCoordinates)
        {
            // 3 and 7 are the current width and height of the boss sequence
            string s = GetConnectionsEnd(c, 3, 7);
            bool found = false;
            GenSequence chosenSequence = new();
            MapRoom b = null;
            if(s.Length == 0)
            {
                continue;
            }

            // GPT at it again, what the fuck is even an action????? All bless our AI overlord
            var directions = new[]
            {
                ('N', 0, new Action(() => {
                    _map[c.row][c.col].northDoor.SetActive(false);
                    _map[c.row][c.col].northDoorOpen.SetActive(true);
                })),
                ('S', 1, new Action(() => {
                    _map[c.row][c.col].southDoor.SetActive(false);
                    _map[c.row][c.col].southDoorOpen.SetActive(true);
                })),
                ('E', 2, new Action(() => {
                    _map[c.row][c.col].eastDoor.SetActive(false);
                    _map[c.row][c.col].eastDoorOpen.SetActive(true);
                })),
                ('W', 3, new Action(() => {
                    _map[c.row][c.col].westDoor.SetActive(false);
                    _map[c.row][c.col].westDoorOpen.SetActive(true);
                }))
            };

            var (dirChar, seqIndex, openDoor) = directions[UnityEngine.Random.Range(0, 4)];
            int i = 0;
            for (; i < directions.Length; i++) {
                if (s.Contains(dirChar))
                {
                    found = true;
                    chosenSequence = sequences[seqIndex];
                    openDoor(); // apparently actions can be used like named functions just stored as variables???? WOW!
                }
            }
            if(!found)
            {
                continue;
            }

            MapRoom connector;
            MapRoom connector1;

            if (chosenSequence.north || chosenSequence.south)
            {
                b = Instantiate(_restRoomVertical, spawnObject.transform).GetComponent<MapRoom>();
                b.gameObject.transform.position = GetOffset(c + chosenSequence.room, b); // place 1x2 or 2x1 
                b.At(0, 0).SetDoors(true, true, false, false);
                b.At(0, 1).SetDoors(true, true, false, false);

                connector = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
                connector1 = Instantiate(connectorNS, spawnObject.transform).GetComponent<MapRoom>();
            }
            else
            {
                b = Instantiate(_restRoomHorizontal, spawnObject.transform).GetComponent<MapRoom>();
                b.gameObject.transform.position = GetOffset(c + chosenSequence.room, b); // place 1x2 or 2x1 
                b.At(0, 0).SetDoors(false, false, true, true);
                b.At(1, 0).SetDoors(false, false, true, true);

                connector = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
                connector1 = Instantiate(connectorEW, spawnObject.transform).GetComponent<MapRoom>();
            }

            connector.gameObject.transform.position = GetOffset(c + chosenSequence.connect, connector);
            connector1.gameObject.transform.position = GetOffset(c + chosenSequence.first, connector1);

            MapRoom bossRoom = Instantiate(_bossRoom, spawnObject.transform).GetComponent<MapRoom>();
            bossRoom.gameObject.transform.position = GetOffset(c + chosenSequence.boss, bossRoom);
            bossRoom.At(0, 0).SetDoors(chosenSequence.north, chosenSequence.south, chosenSequence.east, chosenSequence.west);

            break;
        }
    }

    // Main generation function
    IEnumerator GenerateRoom() {
        // Get mid width and height to place the start block
        int midWidth = (MapWidth - 1) / 2;
        int midHeight = (MapHeight - 1) / 2;
        Coordinate startCoordinate = new(midWidth, midHeight);

        MapRoom startRoom = Instantiate(_startBlock, spawnObject.transform).GetComponent<MapRoom>();
        RunStateManager.Singleton.InitialPlacePlayer(startRoom.gameObject.GetComponentInChildren<PlayerSpawnLocation>());
        CanPlaceIntermediate(new(startCoordinate.row - 1, startCoordinate.col), startRoom);
        _intermediateRooms.Add(startRoom);

        // Randomly sparse intermediate blocks
        PlaceIntermediates(numIntermediates);

        // Connect all adjacent blocks together
        FirstSweepConnect();

        // If there is no connecting block already, the start needs to be hooked up
        // So find the closest path from it to another intermediate and make the path
        Coordinate directlyWest = new(midWidth + 1, midHeight);
        if (!directlyWest.BlockExists(_map, out Block bl))
        {
            string path = BFSPathToClosestIntermediate(directlyWest, _intermediateCoordinates, true);
            if (!PathFromString(startCoordinate, path))
            {
                Debug.LogError("failed to make path from start");
                yield break;
            }
        }

        // Create a 4-way connector to symbolize that anything can connect
        // at the point right outside the start room
        MapRoom b2 = Instantiate(connector4, spawnObject.transform).GetComponent<MapRoom>();
        if (!CanPlaceIntermediate(directlyWest, b2))
        {
            DestroyImmediate(b2.gameObject);
            if(directlyWest.BlockExists(_map, out Block blo) && blo.CompareType(RoomType.CONNECTOR))
            {
                blo.SetDirections(true, true, true, true);
            }
        }

        // Get a list of all the intermediates that are in the starting "island"
        List<Coordinate> startIsland = BFSGetGroup(startCoordinate);
        if(startIsland.Count == 0)
        {
            Debug.LogError("Breakout hit for start island");
            yield break;
        }

        // Now get all of the groups that are not connected to the start "island"
        List<List<Coordinate>> disconnectedGroups = new();
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapHeight; j++)
            {
                Coordinate c = new(i, j);
                if (CheckForBlock(c, RoomType.ALL, 'X') && !visitedStart.Contains(c))
                {
                    List<Coordinate> newGroup = BFSGetGroup(c);
                    if(newGroup == null)
                    {
                        Debug.LogError("Breakout hit for disconnected island");
                        yield break;
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
            HashSet<Coordinate> visited = new();
            while (maxIter < group.Count && maxIter >= 0)
            {
                // find mid dist between intermediate from start group and list
                float closestDistance = int.MaxValue;
                Coordinate disconnectedCoordinate = new(-1, -1);
                Coordinate closestStartIntermediate = new(-1, -1);
                foreach (Coordinate currentGroupCoord in group)
                {
                    foreach (Coordinate currentIntermediate in startIsland)
                    {
                        if (currentGroupCoord.SquaredDistanceTo(currentIntermediate) < closestDistance)
                        {
                            disconnectedCoordinate = currentGroupCoord;
                            closestStartIntermediate = currentIntermediate;
                            closestDistance = currentGroupCoord.SquaredDistanceTo(currentIntermediate);
                        }
                    }
                }
                if (disconnectedCoordinate.row != -1 && closestStartIntermediate.row != -1)
                {
                    List<Coordinate> oneStartCoord = new()
                    {
                        closestStartIntermediate
                    };
                    string s = BFSPathToClosestIntermediate(disconnectedCoordinate, oneStartCoord, false);
                    if(!PathFromString(disconnectedCoordinate, s))
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
                Debug.Log("Group: ");
                foreach (Coordinate currentGroupCoord in group)
                {
                    Debug.Log(currentGroupCoord);
                }
                Debug.Log("Start: ");
                foreach(Coordinate currentIntermediate in startIsland)
                {
                    Debug.Log(currentIntermediate);
                }
                Debug.LogError("Could not find a path from one island to another");
                yield break;
            }
            foreach (Coordinate coord in group)
            {
                if (_map[coord.row][coord.col].CompareType(RoomType.INTERMEDIATE | RoomType.CONNECTOR))
                {
                    startIsland.Add(coord);
                }
            }
        }

        // After all connectors have been placed, touch up the connectors by looping again (SECOND SWEEP CONNECT)
        // and re-doing the connections with new adjacencies
        FirstSweepConnect();

        // Open the doors and cap the doors leading to nowhere
        OpenDoors();

        // Place the end block
        PlaceEnd(startIsland, new(midWidth, midHeight));

        yield return null;
        _NavMeshSurface.BuildNavMeshAsync();
        yield return null;

        // PLEASE CHANGE !!!!!!!!! this to a list of intermediate rooms instead of iterating through everything
        for (int i = 0; i < MapWidth; i++)
        {
            for (int j = 0; j < MapHeight; j++)
            {
                if (_map[i][j] != null && _map[i][j].CompareType(RoomType.INTERMEDIATE))
                {
                    MapRoom m = _map[i][j].gameObject.GetComponentInParent<MapRoom>();
                    int difficulty = (int)Mathf.Sqrt(new Coordinate(i, j).SquaredDistanceTo(startCoordinate) * difficultyMultiplier);
                    if (m != null) m.InitializeContents(difficulty);
                }
            }
        }

        // Debug purposes, color the grid with debug lines
#if DEBUG
        ColorGrid();
#endif
    }

#if DEBUG
    // DEBUG DRAWING
    private void ColorGrid()
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
