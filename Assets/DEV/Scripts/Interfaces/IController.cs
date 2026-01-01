using DEV.Scripts.Config;
using DEV.Scripts.Data;

namespace DEV.Scripts.Interfaces
{
    public interface IController
    {
        void StartNewLevel(LevelData levelData,GameConfig gameConfig);
        void LevelDestroy();
        void Dispose();
    }
}