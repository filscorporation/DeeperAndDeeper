using System.Linq;
using Iron;

namespace IronCustom
{
    public class TemperatureManager : ScriptComponent
    {
        private Map map;
        private Player player;
        
        private const float ARROW_SIZE = 60;
        private const float ARROW_Y = 120;
        private const float ICON_SIZE = 60;

        private float currentColdLevel = -1;

        public void Init(Map map, Player player)
        {
            this.map = map;
            this.player = player;
            
            CreateIcons();
        }

        public void Restart()
        {
            map.ClearFreeze();
            currentColdLevel = -1;
        }

        public override void OnUpdate()
        {
            currentColdLevel += Time.DeltaTime * player.ColdLevelSpeed;
            if (currentColdLevel > map.CurrentFrozenLevel + 1)
            {
                map.FreezeLevel(map.CurrentFrozenLevel + 1);
            }
        }

        private void CreateIcons()
        {
            AsepriteData aData = ResourcesManager.LoadAsepriteData("arrow.aseprite");
            AsepriteData heatData = ResourcesManager.LoadAsepriteData("heat_icon.aseprite", true);
            AsepriteData coldData = ResourcesManager.LoadAsepriteData("cold_icon.aseprite", true);
            
            Entity taEntity = UI.CreateUIElement("Top arrow");
            UIImage taImage = taEntity.AddComponent<UIImage>();
            taImage.Sprite = aData.Sprites.First();
            taImage.RectTransform.AnchorMin = new Vector2(1, 1);
            taImage.RectTransform.AnchorMax = new Vector2(1, 1);
            taImage.RectTransform.Size = new Vector2(ARROW_SIZE, ARROW_SIZE);
            taImage.RectTransform.AnchoredPosition = new Vector2(-ARROW_SIZE / 2 - 5, -ARROW_Y);
            
            Entity baEntity = UI.CreateUIElement("Bottom arrow");
            UIImage baImage = baEntity.AddComponent<UIImage>();
            baImage.Sprite = aData.Sprites.First();
            baImage.RectTransform.AnchorMin = new Vector2(1, 0);
            baImage.RectTransform.AnchorMax = new Vector2(1, 0);
            baImage.RectTransform.Size = new Vector2(ARROW_SIZE, ARROW_SIZE);
            baImage.RectTransform.Rotation = new Vector3(0, 0, Math.Pi);
            baImage.RectTransform.AnchoredPosition = new Vector2(-ARROW_SIZE / 2 - 5, ARROW_Y);
            
            Entity coldEntity = UI.CreateUIElement("Cold icon");
            UIImage coldImage = coldEntity.AddComponent<UIImage>();
            coldImage.Sprite = coldData.Sprites.First();
            coldImage.RectTransform.AnchorMin = new Vector2(1, 1);
            coldImage.RectTransform.AnchorMax = new Vector2(1, 1);
            coldImage.RectTransform.Size = new Vector2(ICON_SIZE, ICON_SIZE);
            coldImage.RectTransform.AnchoredPosition = new Vector2(-ICON_SIZE / 2 - 5, -ARROW_Y - ICON_SIZE);
            //coldEntity.AddComponent<Animator>().Play(coldData.Animations.First());
            
            Entity heatEntity = UI.CreateUIElement("Heat icon");
            UIImage heatImage = heatEntity.AddComponent<UIImage>();
            heatImage.Sprite = heatData.Sprites.First();
            heatImage.RectTransform.AnchorMin = new Vector2(1, 0);
            heatImage.RectTransform.AnchorMax = new Vector2(1, 0);
            heatImage.RectTransform.Size = new Vector2(ICON_SIZE, ICON_SIZE);
            heatImage.RectTransform.AnchoredPosition = new Vector2(-ICON_SIZE / 2 - 5, ARROW_Y + ICON_SIZE);
            //heatEntity.AddComponent<Animator>().Play(heatData.Animations.First());
        }
    }
}