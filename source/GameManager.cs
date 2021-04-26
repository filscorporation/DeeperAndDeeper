using System;
using Iron;

namespace IronCustom
{
    public enum GameMode
    {
        Tutorial,
        Easy,
        Hard,
    }
    
    public class GameManager
    {
        private static Map map;
        private static Player player;
        private static TemperatureManager temperatureManager;
        private static UIManager uiManager;
        private static BuilderManager bManager;

        private static Entity loseScreen;
        private static Entity winScreen;

        private static bool won = false;

        public static bool IsPaused { get; private set; }
        public static GameMode SelectedGameMode = GameMode.Easy;
        
        public static void EntryPoint()
        {
            Screen.Width = 1600;
            Screen.Height = 1000;
            Screen.Color = new Color(240, 233, 201);
            Camera.Main.Width = 32;
            Camera.Main.ResizingMode = CameraResizingMode.KeepWidth;

            map = new Map();
            map.Initialize();
            map.CreateMap();
            map.SpawnFirstBlock();
            
            Camera.Main.Entity.AddComponent<CameraController>().Init(map.GetTopLeft().Y, map.GetBottomRight().Y);

            Entity playerEntity = new Entity("Player");
            player = playerEntity.AddComponent<Player>();
            player.Map = map;
            
            Entity bmEntity = new Entity("Builder manager");
            bManager = bmEntity.AddComponent<BuilderManager>();
            bManager.Init(map, player);
            
            Entity tEntity = new Entity("Temperature manager");
            temperatureManager = tEntity.AddComponent<TemperatureManager>();
            temperatureManager.Init(map, player);

            Entity uiEntity = new Entity("UI manager");
            uiManager = uiEntity.AddComponent<UIManager>();
            uiManager.CreateUI();

            ApplyDifficulty();

            Log.LogInfo("EntryPoint completed");
        }

        private static void ApplyDifficulty()
        {
            switch (SelectedGameMode)
            {
                case GameMode.Tutorial:
                    player.DelayBeforeBlockDestroyed = 5;
                    player.ColdLevelSpeed = 0.025f;
                    break;
                case GameMode.Easy:
                    player.DelayBeforeBlockDestroyed = 3;
                    player.ColdLevelSpeed = 0.05f;
                    break;
                case GameMode.Hard:
                    player.DelayBeforeBlockDestroyed = 1;
                    player.ColdLevelSpeed = 0.25f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void Restart()
        {
            won = false;
            if (loseScreen != null)
                loseScreen.IsActiveSelf = false;
            if (winScreen != null)
                winScreen.IsActiveSelf = false;
            
            map.DeleteMap();
            foreach (RigidBody body in Component.FindAllOfType<RigidBody>())
            {
                body.Entity.Destroy();
            }
            map.CreateMap();
            map.SpawnFirstBlock();
            temperatureManager.Restart();
            uiManager.CloseMenu();
            player.Clear();
            bManager.Clear();
            Camera.Main.GetComponent<CameraController>().Reset();

            ApplyDifficulty();
        }

        public static void Lose()
        {
            if (won)
                return;
            IsPaused = true;
            Time.TimeScale = 0.0f;
            if (loseScreen == null)
                loseScreen = UILoseScreen.Create();
            else
                loseScreen.IsActiveSelf = true;
        }

        public static void Win()
        {
            won = true;
            IsPaused = true;
            Time.TimeScale = 0.0f;
            if (winScreen == null)
                winScreen = UIWinScreen.Create();
            else
                winScreen.IsActiveSelf = true;
        }

        public static void Pause()
        {
            IsPaused = true;
            Time.TimeScale = 0.0f;
        }

        public static void UnPause()
        {
            IsPaused = false;
            Time.TimeScale = 1.0f;
        }
    }
}