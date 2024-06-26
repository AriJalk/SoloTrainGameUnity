﻿using Engine;
using Engine.ResourceManagement;
using HexSystem;
using SoloTrainGame.GameLogic;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SoloTrainGame.Core
{
    public class HexGridController : MonoBehaviour
    {
        const float TILE_SIZE = 0.5f;

        [SerializeField]
        [Range(0f, 2f)]
        float _tileGap = 0.1f;
        [SerializeField]
        private Transform _worldTransform;

        public Vector3 Center = Vector3.zero;
        private PrefabManager _prefabManager;


        // Holds all tiles in the map
        public Dictionary<Hex, HexTileObject> HexTileDictionary;

        // Last recorded tile gap
        private float lastTileGap;

        public HexTileObject StartingTile { get; set; }

        Vector3 avaragePosition = Vector3.zero;

        // Bounds
        public float MinX { get; private set; }
        public float MaxX { get; private set; }
        public float MinZ { get; private set; }
        public float MaxZ { get; private set; }


        void Start()
        {

        }

        void Update()
        {
            if (lastTileGap != _tileGap)
            {
                lastTileGap = _tileGap;
                foreach (HexTileObject tileObj in HexTileDictionary.Values)
                {
                    UpdateTilePosition(tileObj);
                }
            }
        }

        /// <summary>
        /// Provide a list of hexList, the first one is the starting tile
        /// </summary>
        /// <param name="hexList"></param>
        public void Initialize(List<HexGameData> hexList = null)
        {
            _prefabManager = ServiceLocator.PrefabManager;
            _prefabManager.LoadAndRegisterPrefab<HexTileObject>(PrefabFolder.PREFAB_3D, "HexTile", 30);
            HexTileDictionary = new Dictionary<Hex, HexTileObject>();
            lastTileGap = _tileGap;
            //BuildTestMapNew();
            //StartingTile = HexTileDictionary[Hex.ZERO];
            if (hexList != null)
            {
                BuildMap(hexList);
            }
        }

        // TODO: move to tile
        private void UpdateTilePosition(HexTileObject tile)
        {
            tile.CachedTransform.position = Hex.HexToWorld(tile.HexGameData.Hex, TILE_SIZE, _tileGap, HexOrientation.FlatLayout);
        }

        private void BuildMap(List<HexGameData> hexList)
        {
            foreach (HexGameData hex in hexList)
            {
                
            }
        }

        /*
        // TODO: move to tile
        private void SetTileMaterial(HexTileObject tile)
        {
            if (tile != null)
            {
                Material material = ServiceLocator.MaterialManager.GetColorMaterial(tile.HexGameData.TileType.TerrainColor);
                if (material != null)
                {
                    tile.MeshRenderer.material = material;
                }
                MeshRenderer mesh = tile.CachedTransform.Find("Town").Find("ProductionSocket").Find("Holder").GetComponent<MeshRenderer>();
                // TODO: Randomize from stack
                mesh.material = ServiceLocator.MaterialManager.GetWoodColorMaterial((Enums.GameColor)UnityEngine.Random.Range(0, 4));
            }
        }*/

        // TODO: no params, just data
        public HexTileObject CreateTile(Hex hex, Enums.TerrainType type, Tracks tracks = null, Town town = null, City city = null)
        {
            if (!HexTileDictionary.ContainsKey(hex))
            {
                HexTileObject tile = _prefabManager.RetrievePoolObject<HexTileObject>();
                TerrainTypeSO terrainType = ServiceLocator.ScriptableObjectManager.TerrainTypes[type];
                tile.HexGameData = new HexGameData(hex, terrainType);
                tile.CachedTransform.SetParent(transform);
                HexTileDictionary.Add(hex, tile);
                tile.CostText.text = string.Empty;
                tile.Initialize(tile.HexGameData);
                ConnectNeighbors(tile);
                UpdateTilePosition(tile);   
                UpdateBoundsFromHex(tile);
                return tile;
            }
            return null;
        }




        private void UpdateBoundsFromHex(HexTileObject hexTile)
        {
            if (hexTile.CachedTransform.position.x < MinX)
                MinX = hexTile.CachedTransform.position.x;
            if (hexTile.CachedTransform.position.x > MaxX)
                MaxX = hexTile.CachedTransform.position.x;
            if (hexTile.CachedTransform.position.z < MinZ)
                MinZ = hexTile.CachedTransform.position.z;
            if (hexTile.CachedTransform.position.z > MaxZ)
                MaxZ = hexTile.CachedTransform.position.z;
            avaragePosition += hexTile.CachedTransform.position;
        }



        private void ConnectNeighbors(HexTileObject hexTile)
        {
            List<Hex> neighborList = Hex.GetAllNeighbors(hexTile.HexGameData.Hex);
            foreach (Hex neighborHex in neighborList)
            {
                HexTileObject neighborTile = GetHexTile(neighborHex);
                if (neighborTile != null)
                {
                    ConnectHexes(hexTile, neighborTile);
                }
            }
        }


        private void ConnectHexes(HexTileObject hexTileA, HexTileObject hexTileB)
        {
            if (!hexTileA.Neighbors.Contains(hexTileB))
            {
                hexTileA.Neighbors.Add(hexTileB);
                hexTileB.Neighbors.Add(hexTileA);
            }
        }

        public HexTileObject GetHexTile(Hex position)
        {
            if (HexTileDictionary.ContainsKey(position))
                return HexTileDictionary[position];
            return null;
        }


        public void BuildTestMap()
        {
            // Build test map
            Hex hex = Hex.ZERO;
            for (int i = 0; i < 10; i++)
            {
                CreateTile(hex, Enums.TerrainType.Fields);
                Hex northHex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH);
                for (int j = 0; j < 5; j++)
                {
                    CreateTile(northHex, Enums.TerrainType.Urban);
                    northHex = Hex.GetHexNeighbor(northHex, Hex.HexDirection.NORTH);
                }
                for (int j = 0; j < 5; j++)
                {
                    CreateTile(northHex, Enums.TerrainType.Mountains);
                    northHex = Hex.GetHexNeighbor(northHex, Hex.HexDirection.NORTH);
                }

                if (i % 2 == 0)
                    hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH_EAST);
                else
                    hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.SOUTH_EAST);
            }
            avaragePosition /= HexTileDictionary.Count;
            Center = avaragePosition;
        }
        public void BuildTestMapNew()
        {
            // Build test map
            Hex hex = Hex.ZERO;
            BuildTestRow(hex, Enums.TerrainType.Fields, 5);
            hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH);
            BuildTestRow(hex, Enums.TerrainType.Fields, 5);
            hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH);
            BuildTestRow(hex, Enums.TerrainType.Mountains, 5);
            hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH);
            BuildTestRow(hex, Enums.TerrainType.Mountains, 5);
            hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH);
            BuildTestRow(hex, Enums.TerrainType.Urban, 5);
            hex = Hex.GetHexNeighbor(hex, Hex.HexDirection.NORTH);
            BuildTestRow(hex, Enums.TerrainType.Urban, 5);

        }

        private void BuildTestRow(Hex origin, Enums.TerrainType type, int length)
        {
            CreateTile(origin, type);
            for (int i = 0; i < length; i++)
            {
                origin = Hex.GetHexNeighbor(origin, Hex.HexDirection.NORTH_EAST);
                CreateTile(origin, type);
                origin = Hex.GetHexNeighbor(origin, Hex.HexDirection.SOUTH_EAST);
                CreateTile(origin, type);
            }
        }

    }

}