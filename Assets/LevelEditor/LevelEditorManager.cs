using Engine;
using UnityEngine;
using System.Collections.Generic;
using SoloTrainGame.GameLogic;
using SoloTrainGame.UI;
using System;
using UnityEngine.Events;

namespace SoloTrainGame.Core
{
    public class LevelEditorManager : MonoBehaviour
    {
        [SerializeField]
        private HexGridController _gridController;
        [SerializeField]
        private Transform _prefabStorage;

        [SerializeField]
        private RotatedCamera _rotatedCamera;
        [SerializeField]
        private Transform _centerObject;
        [SerializeField]
        private LevelEditorGUI _userInterface;

        private InputManager _inputManager;


        private HexTileObject _selectedTile;



        // Start is called before the first frame update
        void Awake()
        {
            Initialize();
        }

        void Start()
        {
            //_userInterface.CardGridViewer.OpenViewer(cards);
            _rotatedCamera.ColliderClickDownEvent?.AddListener(RaycastColliderHitDown);
            _rotatedCamera.ColliderClickUpEvent?.AddListener(RaycastColliderHitUp);
        }



        // Update is called once per frame
        void Update()
        {
            _inputManager.UpdateInput();
            ServiceLocator.TimerManager.Update();
        }

        private void OnDestroy()
        {
            ServiceLocator.StateManager.ExitCurrentState();
            _rotatedCamera.ColliderClickDownEvent?.RemoveListener(RaycastColliderHitDown);
            _rotatedCamera.ColliderClickUpEvent?.RemoveListener(RaycastColliderHitUp);
        }

        private void Initialize()
        {
            ServiceLocator.SetPrefabManagerManager(_prefabStorage);
            _inputManager = ServiceLocator.InputManager;
            Vector2 min = transform.position;
            Vector2 max = transform.position;
            if (_gridController != null)
            {
                _gridController.Initialize();
                min = new Vector2(_gridController.MinX, _gridController.MinZ);
                max = new Vector2(_gridController.MaxX, _gridController.MaxZ);

            }
            else if (_centerObject != null)
            {
                min = new Vector2(_centerObject.position.x, _centerObject.position.z);
                max = min;
            }
            _rotatedCamera.Initialize(min, max);
            ServiceLocator.SetUserInterface(_userInterface);
        }

        private void RaycastColliderHitDown(RaycastHit hit)
        {
            if (hit.collider.transform.parent?.GetComponent<HexTileObject>() is HexTileObject tile)
            {
                _selectedTile = tile;
            }
        }
        private void RaycastColliderHitUp(RaycastHit hit)
        {
            if (hit.collider.transform.parent?.GetComponent<HexTileObject>() is HexTileObject tile)
            {
                if (tile == _selectedTile)
                {
                    ServiceLocator.GameEvents.TileSelectedEvent?.Invoke(tile);
                }
            }
        }
    }

}