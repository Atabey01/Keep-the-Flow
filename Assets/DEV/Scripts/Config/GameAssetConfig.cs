using AYellowpaper.SerializedCollections;
using DEV.Scripts.GamePlay;
using UnityEngine;

namespace DEV.Scripts.Config
{
    [CreateAssetMenu(fileName = "GameAssets", menuName = "Data/GameAssets")]
    public class GameAssetsConfig : ScriptableObject
    {
        [Header("Game Settings")] 
        [Space(10)] 
        
        [Header("Game Prefabs")] 
        [Space(10)]
        public Cube cubePrefab;
        public ConveyorController ConveyorPrefab;
        public Cannon cannonPrefab;
        
        [Header("Game Parent Positions")]
        public Vector3 ConveyorParentPosition;
        public Vector3 CannonParentPosition;
        
        [Header("Game Offsets")]
        public Vector3 CannonColumnOffset;
        
        [Header("Game Materials")]
        public SerializedDictionary<Enums.ColorType, Material> Materials = new();
    }
}
