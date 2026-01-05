using AYellowpaper.SerializedCollections;
using DEV.Scripts.GamePlay;
using Dreamteck.Splines;
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
        public SplineComputer ConveyorPrefab;
        
        [Header("Game Materials")]
        public SerializedDictionary<Enums.ColorType, Material> Materials = new();
    }
}
