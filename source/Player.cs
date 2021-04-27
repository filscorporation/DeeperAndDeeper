using System;
using System.Collections.Generic;
using System.Linq;
using Iron;

namespace IronCustom
{
    public enum Resources
    {
        Iron,
        Titanium,
        Uranium,
        Diamond,
    }

    public enum Upgrades
    {
        BlockStrength,
        MinerSpeed,
        DrillSpeed,
    }
    
    public class Player : ScriptComponent
    {
        public const int WINNING_HEIGHT = 3;
        
        public Map Map;
        private BlockType placedBlockType = BlockType.BuildingBlock;

        public float DelayBeforeBlockDestroyed = 3f;
        public float ColdLevelSpeed = 0.2f;
        public int StartingIronAmount = 20;
        
        public float DrillingSpeed = 1.0f;
        public int DrillingRange = 4;
        public float MiningSpeed = 0.5f;
        public int MiningRange = 2;

        private readonly Dictionary<Resources, int> resourcesAmount = new Dictionary<Resources, int>();
        private readonly Dictionary<Resources, UIText> resourcesTexts = new Dictionary<Resources, UIText>();
        private readonly Dictionary<BlockType, int> buildIronCost = new Dictionary<BlockType, int>();
        private readonly Dictionary<BlockType, UIImage> buildIcons = new Dictionary<BlockType, UIImage>();
        private readonly Dictionary<BlockType, UIButton> buildButtons = new Dictionary<BlockType, UIButton>();
        
        private readonly Dictionary<Upgrades, UIButton> upgradesButtons = new Dictionary<Upgrades, UIButton>();

        private Sprite buildIconSprite;
        private Sprite selectedIconSprite;
        private Sprite smallButtonSprite;
        private Sprite helpButtonSprite;
        private Sprite closeButtonSprite;
        private Sprite infoSprite;
        
        private Entity buttonInfo;

        private const float ICON_SIZE = 120;
        private const float ICON_OFFSET = 10;
        private const int TEXT_SIZE = 32;
        private const float BUILD_ICON_SIZE = 135;
        private const float BUILD_ICON_OFFSET = 20;
        private const float BUILD_ICON_IMAGE_OFFSET = 15;
        private const float SMALL_BUTTON_SIZE_X = 150;
        private const float SMALL_BUTTON_SIZE_Y = 45;
        private const float HELP_BUTTON_SIZE = 45;
        
        private const float BUTTON_INFO_SIZE_X = 144 * 3.75f;
        private const float BUTTON_INFO_SIZE_Y = 64 * 3.75f;

        public void Clear()
        {
            resourcesAmount[Resources.Iron] = StartingIronAmount;
            resourcesAmount[Resources.Titanium] = 0;
            resourcesAmount[Resources.Uranium] = 0;
            resourcesAmount[Resources.Diamond] = 0;
            
            if (Map != null)
            {
                Map.BlocksStats[BlockType.BuildingBlock].PressureStrength = 5.0f;
                Map.BlocksStats[BlockType.BuildingBlock].MountStrength = 2.0f;
            }
            DrillingSpeed = 1.0f;
            DrillingRange = 4;
            MiningSpeed = 0.5f;
            MiningRange = 2;

            UpdateResourcesText();
            UpdateConstructionUI();
            UpdateUpgradesUI();
        }

        public override void OnCreate()
        {
            buildIronCost[BlockType.BuildingBlock] = 1;
            buildIronCost[BlockType.Miner] = 3;
            buildIronCost[BlockType.Drill] = 3;
            
            CreateResourcesUI();
            CreateConstructionUI();
            CreateUpgradesUI();
            UpdateUpgradesUI();
            Clear();
        }

        public bool AddResource(Resources type, int amount)
        {
            if (amount < 0 && resourcesAmount[type] < -amount)
                return false;
            resourcesAmount[type] += amount;
            UpdateResourcesText();
            UpdateConstructionUI();
            UpdateUpgradesUI();
            
            return true;
        }

        private Entity CreateButtonInfoUI(List<string> text)
        {
            if (infoSprite == null)
                infoSprite = ResourcesManager.LoadImage("info_window.png");
            
            Entity entity = UI.CreateUIElement("Info");
            UIImage image = entity.AddComponent<UIImage>();
            image.Sprite = infoSprite;
            image.RectTransform.AnchorMin = new Vector2(0, 0);
            image.RectTransform.AnchorMax = new Vector2(0, 0);
            image.RectTransform.Size = new Vector2(BUTTON_INFO_SIZE_X, BUTTON_INFO_SIZE_Y);
            image.RectTransform.AnchoredPosition = new Vector2(
                BUTTON_INFO_SIZE_X / 2 + BUILD_ICON_OFFSET,
                BUTTON_INFO_SIZE_Y / 2 + BUILD_ICON_SIZE + SMALL_BUTTON_SIZE_Y + BUILD_ICON_OFFSET + 5
            );

            float y = -4;
            bool first = true;
            foreach (string line in text)
            {
                y -= AddTextLine(entity, line, y, first);
                y -= 5;
                first = false;
            }
            
            Entity entity2 = UI.CreateUIElement("Close", entity);
            UIButton button = entity2.AddComponent<UIButton>();
            button.Sprite = closeButtonSprite;
            button.RectTransform.AnchorMin = new Vector2(1, 1);
            button.RectTransform.AnchorMax = new Vector2(1, 1);
            button.RectTransform.Size = new Vector2(HELP_BUTTON_SIZE, HELP_BUTTON_SIZE);
            button.RectTransform.AnchoredPosition = new Vector2(-HELP_BUTTON_SIZE / 2 - 2, -HELP_BUTTON_SIZE / 2 - 2);
            button.RectTransform.SortingOrder = -2;
            button.OnClick.AddCallback(() => {
                buttonInfo.Destroy();
                buttonInfo = null;
            });

            return entity;
        }

        private float AddTextLine(Entity parent, string text, float y, bool isCapital)
        {
            Entity entity = UI.CreateUIElement("Info line", parent);
            UIText uiText = entity.AddComponent<UIText>();
            uiText.Text = text;
            int height = isCapital ? 48 : 32;
            uiText.TextSize = height;
            uiText.Color = Color.Black;
            uiText.RectTransform.AnchorMin = new Vector2(0, 1);
            uiText.RectTransform.AnchorMax = new Vector2(0, 1);
            uiText.RectTransform.Size = new Vector2(BUTTON_INFO_SIZE_X, height);
            uiText.RectTransform.AnchoredPosition = new Vector2(BUTTON_INFO_SIZE_X / 2 + 14, y - height / 2.0f);
            uiText.RectTransform.SortingOrder = -1;

            return height;
        }

        private void CreateResourcesUI()
        {
            float x = ICON_OFFSET;
            Sprite ironIcon = ResourcesManager.LoadAsepriteData("iron_icon.aseprite").Sprites.First();
            CreateResourceIron(ironIcon, x);
            resourcesTexts[Resources.Iron] = CreateResourceText("Iron", x);
            x += ICON_SIZE + ICON_OFFSET;
            Sprite titaniumIcon = ResourcesManager.LoadAsepriteData("titanium_icon.aseprite").Sprites.First();
            CreateResourceIron(titaniumIcon, x);
            resourcesTexts[Resources.Titanium] = CreateResourceText("Titanium", x);
            x += ICON_SIZE + ICON_OFFSET;
            Sprite uraniumIcon = ResourcesManager.LoadAsepriteData("uranium_icon.aseprite").Sprites.First();
            CreateResourceIron(uraniumIcon, x);
            resourcesTexts[Resources.Uranium] = CreateResourceText("Uranium", x);
            x += ICON_SIZE + ICON_OFFSET;
            Sprite diamondIcon = ResourcesManager.LoadAsepriteData("diamond_icon.aseprite").Sprites.First();
            CreateResourceIron(diamondIcon, x);
            resourcesTexts[Resources.Diamond] = CreateResourceText("Diamonds", x);
            x += ICON_SIZE + ICON_OFFSET;
        }

        private void CreateConstructionUI()
        {
            buildIconSprite = ResourcesManager.LoadAsepriteData("build_button.aseprite").Sprites.First();
            selectedIconSprite = ResourcesManager.LoadAsepriteData("build_selected.aseprite").Sprites.First();
            smallButtonSprite = ResourcesManager.LoadAsepriteData("small_button.aseprite").Sprites.First();
            helpButtonSprite = ResourcesManager.LoadImage("help_button.png");
            closeButtonSprite = ResourcesManager.LoadImage("close_button.png");

            Sprite buildingBlockIcon = ResourcesManager.LoadAsepriteData("block.aseprite").Sprites.First();
            Sprite minerBlockIcon = ResourcesManager.LoadAsepriteData("miner.aseprite").Sprites.First();
            Sprite drillBlockIcon = ResourcesManager.LoadAsepriteData("drill.aseprite").Sprites.First();

            float x = BUILD_ICON_OFFSET;
            buildIcons[BlockType.BuildingBlock] = CreateConstructionIcon(buildingBlockIcon, () => ShowHelp(BlockType.BuildingBlock), x);
            buildButtons[BlockType.BuildingBlock] = CreateConstructionButton("Block", x);
            buildButtons[BlockType.BuildingBlock].OnClick.AddCallback(() => SelectBlockToBuild(BlockType.BuildingBlock));
            x += BUILD_ICON_SIZE + BUILD_ICON_OFFSET;
            buildIcons[BlockType.Miner] = CreateConstructionIcon(minerBlockIcon, () => ShowHelp(BlockType.Miner), x);
            buildButtons[BlockType.Miner] = CreateConstructionButton("Miner", x);
            buildButtons[BlockType.Miner].OnClick.AddCallback(() => SelectBlockToBuild(BlockType.Miner));
            x += BUILD_ICON_SIZE + BUILD_ICON_OFFSET;
            buildIcons[BlockType.Drill] = CreateConstructionIcon(drillBlockIcon, () => ShowHelp(BlockType.Drill), x);
            buildButtons[BlockType.Drill] = CreateConstructionButton("Drill", x);
            buildButtons[BlockType.Drill].OnClick.AddCallback(() => SelectBlockToBuild(BlockType.Drill));
            x += BUILD_ICON_SIZE + BUILD_ICON_OFFSET;
        }

        private void CreateUpgradesUI()
        {
            float x = BUILD_ICON_OFFSET;
            upgradesButtons[Upgrades.DrillSpeed] = CreateUpgradeButton("Upgrade 3", x, () => ShowHelp(Upgrades.DrillSpeed));
            upgradesButtons[Upgrades.DrillSpeed].OnClick.AddCallback(() => Upgrade(Upgrades.DrillSpeed));
            x += BUILD_ICON_SIZE + BUILD_ICON_OFFSET;
            upgradesButtons[Upgrades.MinerSpeed] = CreateUpgradeButton("Upgrade 2", x, () => ShowHelp(Upgrades.MinerSpeed));
            upgradesButtons[Upgrades.MinerSpeed].OnClick.AddCallback(() => Upgrade(Upgrades.MinerSpeed));
            x += BUILD_ICON_SIZE + BUILD_ICON_OFFSET;
            upgradesButtons[Upgrades.BlockStrength] = CreateUpgradeButton("Upgrade 1", x, () => ShowHelp(Upgrades.BlockStrength));
            upgradesButtons[Upgrades.BlockStrength].OnClick.AddCallback(() => Upgrade(Upgrades.BlockStrength));
            x += BUILD_ICON_SIZE + BUILD_ICON_OFFSET;
        }

        private void ShowHelp(BlockType type)
        {
            List<string> info = null;
            switch (type)
            {
                case BlockType.BuildingBlock:
                    info = new List<string>
                    {
                        "Building block",
                        "Base construction element",
                        "Max pressure: 5",
                        "Max mount strength: 2",
                        "Mass: 1",
                        "Cost: 1 Iron",
                    };
                    break;
                case BlockType.Miner:
                    info = new List<string>
                    {
                        "Miner",
                        "Mines ore in range of 2 tiles",
                        "Max pressure: 3",
                        "Max mount strength: 2",
                        "Mass: 2",
                        "Cost: 3 Iron",
                    };
                    break;
                case BlockType.Drill:
                    info = new List<string>
                    {
                        "Drill",
                        "Removes blocks 4 tiles underneath",
                        "Max pressure: 3",
                        "Max mount strength: 2",
                        "Mass: 2",
                        "Cost: 3 Iron",
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (buttonInfo != null)
                buttonInfo.Destroy();
            buttonInfo = CreateButtonInfoUI(info);
        }

        private void ShowHelp(Upgrades upgrade)
        {
            List<string> info = null;
            switch (upgrade)
            {
                case Upgrades.BlockStrength:
                    info = new List<string>
                    {
                        "Upgrade block strength",
                        "Increase blocks mount and pressure strength",
                        "Max pressure: +2",
                        "Max mount strength: +1",
                        "Cost: 5 Titanium",
                    };
                    break;
                case Upgrades.MinerSpeed:
                    info = new List<string>
                    {
                        "Upgrade miner",
                        "Max range: +1",
                        "Speed: x2",
                        "Cost: 3 Uranium",
                    };
                    break;
                case Upgrades.DrillSpeed:
                    info = new List<string>
                    {
                        "Upgrade drill",
                        "Max range: +2",
                        "Speed: x2",
                        "Cost: 2 Diamond",
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, null);
            }

            if (buttonInfo != null)
                buttonInfo.Destroy();
            buttonInfo = CreateButtonInfoUI(info);
        }

        private void SelectBlockToBuild(BlockType type)
        {
            placedBlockType = type;
            UpdateConstructionUI();
        }

        private void Upgrade(Upgrades upgrade)
        {
            switch (upgrade)
            {
                case Upgrades.BlockStrength:
                    if (!AddResource(Resources.Titanium, -5))
                        break;
                    Map.BlocksStats[BlockType.BuildingBlock].PressureStrength += 2;
                    Map.BlocksStats[BlockType.BuildingBlock].MountStrength += 1;
                    break;
                case Upgrades.MinerSpeed:
                    if (!AddResource(Resources.Uranium, -3))
                        break;
                    MiningRange += 1;
                    MiningSpeed *= 2;
                    break;
                case Upgrades.DrillSpeed:
                    if (!AddResource(Resources.Diamond, -2))
                        break;
                    DrillingRange += 2;
                    DrillingSpeed *= 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(upgrade), upgrade, null);
            }
            
            UpdateUpgradesUI();
        }

        private UIImage CreateConstructionIcon(Sprite sprite, Action helpAction, float x)
        {
            Entity entity = UI.CreateUIElement("Build icon");
            UIImage image = entity.AddComponent<UIImage>();
            image.Sprite = buildIconSprite;
            image.RectTransform.AnchorMin = new Vector2(0, 0);
            image.RectTransform.AnchorMax = new Vector2(0, 0);
            image.RectTransform.Size = new Vector2(BUILD_ICON_SIZE, BUILD_ICON_SIZE);
            image.RectTransform.AnchoredPosition = new Vector2(BUILD_ICON_SIZE / 2 + x, BUILD_ICON_OFFSET + BUILD_ICON_SIZE / 2 + SMALL_BUTTON_SIZE_Y + 5);
            
            Entity entity2 = UI.CreateUIElement("Build icon2", entity);
            UIImage image2 = entity2.AddComponent<UIImage>();
            image2.Sprite = sprite;
            image2.RectTransform.AnchorMin = new Vector2(0, 0);
            image2.RectTransform.AnchorMax = new Vector2(1, 1);
            image2.RectTransform.OffsetMin = new Vector2(BUILD_ICON_IMAGE_OFFSET, BUILD_ICON_IMAGE_OFFSET);
            image2.RectTransform.OffsetMax = new Vector2(BUILD_ICON_IMAGE_OFFSET, BUILD_ICON_IMAGE_OFFSET);
            image2.RectTransform.SortingOrder = -1;
            
            Entity entity3 = UI.CreateUIElement("Help", entity);
            UIButton button = entity3.AddComponent<UIButton>();
            button.Sprite = helpButtonSprite;
            button.RectTransform.AnchorMin = new Vector2(1, 0);
            button.RectTransform.AnchorMax = new Vector2(1, 0);
            button.RectTransform.Size = new Vector2(HELP_BUTTON_SIZE, HELP_BUTTON_SIZE);
            button.RectTransform.AnchoredPosition = new Vector2(-HELP_BUTTON_SIZE / 2 - 2, HELP_BUTTON_SIZE / 2 + 2);
            button.RectTransform.SortingOrder = -2;
            button.OnClick.AddCallback(() => helpAction?.Invoke());

            return image;
        }

        private UIButton CreateConstructionButton(string text, float x)
        {
            Entity entity = UI.CreateUIElement("Build button");
            UIButton button = entity.AddComponent<UIButton>();
            button.Sprite = smallButtonSprite;
            button.RectTransform.AnchorMin = new Vector2(0, 0);
            button.RectTransform.AnchorMax = new Vector2(0, 0);
            button.RectTransform.Size = new Vector2(SMALL_BUTTON_SIZE_X, SMALL_BUTTON_SIZE_Y);
            button.RectTransform.AnchoredPosition = new Vector2(BUILD_ICON_SIZE / 2 + x, BUILD_ICON_OFFSET + SMALL_BUTTON_SIZE_Y / 2);
            
            Entity entity2 = UI.CreateUIElement("Build text", entity);
            UIText uiText = entity2.AddComponent<UIText>();
            uiText.Text = text;
            uiText.TextSize = 32;
            uiText.Color = Color.Black;
            uiText.TextAlignment = AlignmentType.CenterMiddle;
            uiText.RectTransform.AnchorMin = new Vector2(0, 0);
            uiText.RectTransform.AnchorMax = new Vector2(1, 1);
            uiText.RectTransform.SortingOrder = -1;

            return button;
        }

        private UIButton CreateUpgradeButton(string text, float x, Action helpAction)
        {
            Entity entity = UI.CreateUIElement("Upgrade button");
            UIButton button = entity.AddComponent<UIButton>();
            button.Sprite = smallButtonSprite;
            button.RectTransform.AnchorMin = new Vector2(1, 0);
            button.RectTransform.AnchorMax = new Vector2(1, 0);
            button.RectTransform.Size = new Vector2(SMALL_BUTTON_SIZE_X, SMALL_BUTTON_SIZE_Y);
            button.RectTransform.AnchoredPosition = new Vector2(-BUILD_ICON_SIZE / 2 - x, BUILD_ICON_OFFSET + SMALL_BUTTON_SIZE_Y / 2);
            
            Entity entity2 = UI.CreateUIElement("Upgrade text", entity);
            UIText uiText = entity2.AddComponent<UIText>();
            uiText.Text = text;
            uiText.TextSize = 32;
            uiText.Color = Color.Black;
            uiText.TextAlignment = AlignmentType.CenterMiddle;
            uiText.RectTransform.AnchorMin = new Vector2(0, 0);
            uiText.RectTransform.AnchorMax = new Vector2(1, 1);
            uiText.RectTransform.SortingOrder = -1;
            
            Entity entity3 = UI.CreateUIElement("Help", entity);
            UIButton button2 = entity3.AddComponent<UIButton>();
            button2.Sprite = helpButtonSprite;
            button2.RectTransform.AnchorMin = new Vector2(1, 1);
            button2.RectTransform.AnchorMax = new Vector2(1, 1);
            button2.RectTransform.Size = new Vector2(HELP_BUTTON_SIZE, HELP_BUTTON_SIZE);
            button2.RectTransform.AnchoredPosition = new Vector2(-HELP_BUTTON_SIZE / 2 - 2, 10);
            button2.RectTransform.SortingOrder = -2;
            button2.OnClick.AddCallback(() => helpAction?.Invoke());

            return button;
        }

        private void UpdateResourcesText()
        {
            resourcesTexts[Resources.Iron].Text = resourcesAmount[Resources.Iron].ToString();
            resourcesTexts[Resources.Titanium].Text = resourcesAmount[Resources.Titanium].ToString();
            resourcesTexts[Resources.Uranium].Text = resourcesAmount[Resources.Uranium].ToString();
            resourcesTexts[Resources.Diamond].Text = resourcesAmount[Resources.Diamond].ToString();
        }

        private void UpdateConstructionUI()
        {
            buildButtons[BlockType.BuildingBlock].Interactable =
                resourcesAmount[Resources.Iron] >= buildIronCost[BlockType.BuildingBlock];
            buildButtons[BlockType.Miner].Interactable =
                resourcesAmount[Resources.Iron] >= buildIronCost[BlockType.Miner];
            buildButtons[BlockType.Drill].Interactable =
                resourcesAmount[Resources.Iron] >= buildIronCost[BlockType.Drill];
            
            buildIcons[BlockType.BuildingBlock].Sprite = placedBlockType == BlockType.BuildingBlock
                ? selectedIconSprite : buildIconSprite;
            buildIcons[BlockType.Miner].Sprite = placedBlockType == BlockType.Miner
                ? selectedIconSprite : buildIconSprite;
            buildIcons[BlockType.Drill].Sprite = placedBlockType == BlockType.Drill
                ? selectedIconSprite : buildIconSprite;
        }

        private void UpdateUpgradesUI()
        {
            if (resourcesAmount.Count == 0)
                return;
            
            upgradesButtons[Upgrades.BlockStrength].Interactable =
                resourcesAmount[Resources.Titanium] >= 5;
            upgradesButtons[Upgrades.MinerSpeed].Interactable =
                resourcesAmount[Resources.Uranium] >= 3;
            upgradesButtons[Upgrades.DrillSpeed].Interactable =
                resourcesAmount[Resources.Diamond] >= 2;
        }

        private void CreateResourceIron(Sprite sprite, float x)
        {
            Entity entity = UI.CreateUIElement("Resource icon");
            UIImage image = entity.AddComponent<UIImage>();
            image.Sprite = sprite;
            image.RectTransform.AnchorMin = new Vector2(0, 1);
            image.RectTransform.AnchorMax = new Vector2(0, 1);
            image.RectTransform.Size = new Vector2(ICON_SIZE, ICON_SIZE);
            image.RectTransform.AnchoredPosition = new Vector2(ICON_SIZE / 2 + x, - ICON_OFFSET - ICON_SIZE / 2);
        }

        private UIText CreateResourceText(string name, float x)
        {
            Entity entity = UI.CreateUIElement("Resource text");
            UIText text = entity.AddComponent<UIText>();
            text.Text = name;
            text.Color = Color.Black;
            text.TextSize = TEXT_SIZE;
            text.TextAlignment = AlignmentType.CenterMiddle;
            text.RectTransform.AnchorMin = new Vector2(0, 1);
            text.RectTransform.AnchorMax = new Vector2(0, 1);
            text.RectTransform.Size = new Vector2(ICON_SIZE, TEXT_SIZE + ICON_OFFSET);
            text.RectTransform.AnchoredPosition = new Vector2(ICON_SIZE / 2 + x, - ICON_OFFSET - ICON_SIZE - TEXT_SIZE / 2);
            
            entity = UI.CreateUIElement("Resource amount text");
            UIText amountText = entity.AddComponent<UIText>();
            amountText.Text = "0";
            amountText.Color = Color.Black;
            amountText.TextSize = TEXT_SIZE;
            amountText.TextAlignment = AlignmentType.CenterMiddle;
            amountText.RectTransform.AnchorMin = new Vector2(0, 1);
            amountText.RectTransform.AnchorMax = new Vector2(0, 1);
            amountText.RectTransform.Size = new Vector2(ICON_SIZE, TEXT_SIZE + ICON_OFFSET);
            amountText.RectTransform.AnchoredPosition = new Vector2(ICON_SIZE / 2 + x, - ICON_OFFSET - ICON_SIZE - TEXT_SIZE * 1.5f);

            return amountText;
        }

        public override void OnUpdate()
        {
            if (Input.IsMouseJustPressed(MouseCodes.ButtonLeft) && !UI.IsPointerOverUI())
            {
                if (buildIronCost[placedBlockType] <= resourcesAmount[Resources.Iron])
                {
                    if (Map.TryPlacePlayersBlock(Camera.Main.ScreenToWorldPoint(Input.MousePosition), placedBlockType))
                        AddResource(Resources.Iron, -buildIronCost[placedBlockType]);
                }
            }
            
            if (Input.IsMouseJustPressed(MouseCodes.ButtonRight) && !UI.IsPointerOverUI())
            {
                //Map.TryPlaceBlock(Camera.Main.ScreenToWorldPoint(Input.MousePosition), BlockType.Stone);
            }
        }
    }
}