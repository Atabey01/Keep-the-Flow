using DEV.Scripts.Config;
using DEV.Scripts.Data;
using UnityEngine;

namespace DEV.Scripts.GamePlay
{
    public class Box : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        private BoxData _boxData;
        private LevelData _levelData;
        private GameConfig _gameConfig;
        
        public void Initialize(BoxData boxData, LevelData levelData, GameConfig gameConfig)
        {
            _boxData = boxData;
            _levelData = levelData;
            _gameConfig = gameConfig;
            var material = gameConfig.GameAssetsConfig.Materials[_boxData.colorType];
            SetColor(material);
        }

        private void SetColor(Material material)
        {
            meshRenderer.material = material;
        }
    }
}
