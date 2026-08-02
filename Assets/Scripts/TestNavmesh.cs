using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NavMeshPlus.Components;

public class TestNavmesh : MonoBehaviour
{
    public NavMeshSurface _NavMeshSurface;
    // Start is called before the first frame update
    void Start()
    {
        _NavMeshSurface.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
