using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnvironmentGeneratorMaster : MonoBehaviour
{
    public StraightRoadGenerator roadGenerator;
    //public BuildingGenerator buildingGenerator;

    public List<MonoBehaviour> assetPlacers;

    public NavMeshSurface navMeshSurface;

    // Start is called before the first frame update
    void Start()
    {
        GenerateEnvironment();

        BakeNavMesh();
    }

    void GenerateEnvironment()
    {
        roadGenerator.GenerateRoad();

        foreach (MonoBehaviour placer in assetPlacers)
        {
            (placer as IAssetPlacer)?.PlacePowerUpInFreeSpace();
        }
    }

    void BakeNavMesh()
    {
        //navMeshSurface.BuildNavMesh();
        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
    }
}
