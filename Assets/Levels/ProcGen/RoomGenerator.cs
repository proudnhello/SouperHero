using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shuffle function for a list. From: https://discussions.unity.com/t/clever-way-to-shuffle-a-list-t-in-one-line-of-c-code/535113
public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}

public class RoomGenerator : MonoBehaviour
{
    private List<Block> _blocks;
    public List<Block> startBlocks;
    public List<Block> connectorBlocks;
    public List<Block> intermediateBlocks;

    [SerializeField]
    private string _startString = "";

    void Start()
    {
        // Start the scene identifying where the player will spawn
        // Have a select number of rooms you want
        if(startBlocks.Count <= 0)
        {
            Debug.LogError("No start blocks, dumbass");
        } else if (connectorBlocks.Count <= 0)
        {
            Debug.LogError("No connectors, dumbass");
        }
        _blocks = new List<Block>();
        GenerateRoom();
    }

    private List<Portal> GetRandomPortalList()
    {
        List<Portal> portalList = new List<Portal>();
        foreach(Block b in _blocks)
        {
            foreach(Portal p in b.getPortals()) 
            {
                if(!p.isClosed())
                {
                    portalList.Add(p);
                }
            }
        }
        IListExtensions.Shuffle<Portal>(portalList);
        return portalList;
    }

    private bool checkBounds(Block block)
    {
        foreach (Block b in _blocks)
        {
            Debug.Log("First Bounds" + b.GetBounds());
            Debug.Log("Second Bounds" +  block.GetBounds());
            if(b.GetBounds().Intersects(block.GetBounds())) {
                return false;
            }
        }
        return true;
    }

    private void AlignBlockToPortal(Block block, Portal p)
    {
        float angleDiff = p.getDirectionAsAngle() - block.getOutgoingPortal().getDirectionAsAngle();
        Debug.Log("Angle Diff: " + angleDiff * Mathf.Rad2Deg);
        block.gameObject.transform.Rotate(new Vector3(0, 0, 1), Mathf.Rad2Deg * angleDiff);
        Vector2 positionDiff = p.gameObject.transform.position - block.getOutgoingPortal().gameObject.transform.position;
        block.gameObject.transform.position = (Vector2) block.gameObject.transform.position + positionDiff;
    }

    private bool TryPlaceBlock(Block block)
    {
        if (_blocks.Count > 0)
        {
            List<Portal> randomPortalList = GetRandomPortalList();
            Debug.Log("Num Portals available: " + randomPortalList.Count);
            foreach (Portal p in randomPortalList)
            {
                if (!p.isClosed())
                {
                    // match block portal to portal;
                    AlignBlockToPortal(block, p);
                    if (checkBounds(block))
                    {
                        p.setClosed(true);
                        randomPortalList.Clear();
                        _blocks.Add(block);
                        return true;
                    } else
                    {
                        Debug.Log("Failed Bounds Check");
                    }
                }
            }
            randomPortalList.Clear();
            return false;
        }
        else
        {
            Debug.Log("ADDED STARTER BLOCK!");
            _blocks.Add(block);
            return true;
        }
    }

    private bool PlaceNextBlock(int depth)
    {
        if(depth >= _startString.Length)
        {
            return true;
        }
        char currentBlockType = _startString[depth];
        Block b;
        switch(currentBlockType)
        {
            case 'S':
                b = Instantiate(startBlocks[0]).GetComponent<Block>();
                break;
            case 'C':
                b = Instantiate(connectorBlocks[0]).GetComponent<Block>();
                break;
            case 'I':
                b = Instantiate(intermediateBlocks[0]).GetComponent<Block>();
                break;
            default:
                return false;
        }
        if(TryPlaceBlock(b))
        {
            Debug.Log("Can Place!");
            return PlaceNextBlock(depth + 1);
        }
        return false;
    }

    void GenerateRoom() {
        // try to generate the rooms
        bool succeeded = PlaceNextBlock(0);
        if(succeeded)
        {
            Debug.Log("succeeded in placing blocks!");
        } else
        {
            Debug.Log("FAILED LMFAOOOOOOOOOOOOOOAOAOAOA");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

















    // I <3 ChatGPT - gets the bounds of a prefab from all of the sprites' renderer componenets within the prefab
    // Uses Unity's Bounds struct - 
    public static Bounds GetPrefabBounds(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null.");
            return new Bounds();
        }

        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found in prefab.");
            return new Bounds(prefab.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }
}
