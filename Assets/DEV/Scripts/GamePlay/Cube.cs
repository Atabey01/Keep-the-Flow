using DEV.Scripts.Config;
using DEV.Scripts.Data;
using DEV.Scripts.Enums;
using UnityEngine;

namespace DEV.Scripts.GamePlay
{
    public class Cube : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        private ColorType _colorType;
        private LevelData _levelData;
        private GameConfig _gameConfig;
        
        public void Initialize(ColorType colorType, LevelData levelData, GameConfig gameConfig)
        {
            _colorType = colorType;
            _levelData = levelData;
            _gameConfig = gameConfig;
            var material = gameConfig.GameAssetsConfig.Materials[_colorType];
            SetColor(material);
        }

        private void SetColor(Material material)
        {
            meshRenderer.material = material;
        }
    }
}
