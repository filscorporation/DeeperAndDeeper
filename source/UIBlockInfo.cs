using System.Globalization;
using System.Linq;
using Iron;

namespace IronCustom
{
    public class UIBlockInfo : ScriptComponent
    {
        private static Sprite _infoPanelSprite = null;
        private static Sprite infoPanelSprite => _infoPanelSprite ?? (_infoPanelSprite = ResourcesManager.LoadImage("small_info_window.png"));
        
        private const float RESOURCE_ICON_SIZE = 120;
        private const float INFO_SIZE_X = 72 * 3.75f;
        private const float INFO_SIZE_Y = 64 * 3.75f;
        
        public static UIBlockInfo Create(Block block)
        {
            Entity entity = UI.CreateUIElement("Block Info");
            RectTransformation rt = entity.GetComponent<RectTransformation>();
            UIImage image = entity.AddComponent<UIImage>();
            image.Sprite = infoPanelSprite;
            rt.AnchorMin = new Vector2(0, 1);
            rt.AnchorMax = new Vector2(0, 1);
            rt.Size = new Vector2(INFO_SIZE_X, INFO_SIZE_Y);
            rt.AnchoredPosition = new Vector2(INFO_SIZE_X / 2 + 5, -INFO_SIZE_Y / 2 - RESOURCE_ICON_SIZE - 8);
            rt.SortingOrder = -2;

            UIBlockInfo info = entity.AddComponent<UIBlockInfo>();
            info.CreateInner(block);
            
            return info;
        }

        private UIText blockTypeText;
        private const float LABEL_HEIGHT = 18;

        private void CreateInner(Block block)
        {
            float y = 6;
            blockTypeText = CreateLabel("Block type: " + block.Type, y); y += LABEL_HEIGHT;
            blockTypeText = CreateLabel(block.Stats.Description, y); y += LABEL_HEIGHT;
            if (block.Stats.IsStatic)
            {
                blockTypeText = CreateLabel("Not simulated", y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Block info:", y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Durability: "
                    + (block.Stats.Durability > 0 ? block.Stats.Durability.ToString(CultureInfo.InvariantCulture) : "infinity"), y); y += LABEL_HEIGHT;
                Ore ore = block.Map.Ores.FirstOrDefault(o => o.BlockType == block.Type);
                if (ore != null)
                {
                    blockTypeText = CreateLabel("Yield: " + ore.ResourcesInOneBlock, y); y += LABEL_HEIGHT;
                }
            }
            else
            {
                blockTypeText = CreateLabel("Block status:", y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Mass: " + block.Stats.Mass, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Pressure strength: " + block.Stats.PressureStrength, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Mount strength: " + block.Stats.MountStrength, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Simulation:", y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Unhandled mass: " + block.UnhandledMass, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Pressure strength margin: " + block.PressureStrengthMargin, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Top mount margin: " + block.TopMountMargin, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Left mount margin: " + block.LeftMountMargin, y); y += LABEL_HEIGHT;
                blockTypeText = CreateLabel("Right mount margin: " + block.RightMountMargin, y); y += LABEL_HEIGHT;
            }
        }

        private UIText CreateLabel(string text, float y)
        {
            UIText uiText = UI.CreateUIElement("Label", Entity).AddComponent<UIText>();
            uiText.Text = text;
            uiText.TextSize = 16;
            uiText.Color = Color.Black;
            uiText.RectTransform.AnchorMin = new Vector2(0, 1);
            uiText.RectTransform.AnchorMax = new Vector2(1, 1);
            uiText.RectTransform.Size = new Vector2(0, LABEL_HEIGHT);
            uiText.RectTransform.AnchoredPosition = new Vector2(0, -LABEL_HEIGHT / 2 - y);
            uiText.RectTransform.OffsetMin = new Vector2(14, 0);
            uiText.RectTransform.SortingOrder = -1;

            return uiText;
        }
    }
}