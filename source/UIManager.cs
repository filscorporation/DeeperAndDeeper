using System.Linq;
using Iron;

namespace IronCustom
{
    public class UIManager : ScriptComponent
    {
        private Sprite buildButtonSprite;
        private Sprite smallButtonSprite;
        private Sprite longButtonSprite;

        private Entity menu;
        private UIButton menuButton;
        private UIText modeText;

        private const float SMALL_BUTTON_SIZE_X = 150;
        private const float SMALL_BUTTON_SIZE_Y = 45;
        private const float LONG_BUTTON_SIZE_X = 300;
        private const float MENU_BUTTON_OFFSET = 20;
        
        public void CreateUI()
        {
            buildButtonSprite = ResourcesManager.LoadAsepriteData("build_button.aseprite").Sprites.First();
            smallButtonSprite = ResourcesManager.LoadAsepriteData("small_button.aseprite").Sprites.First();
            longButtonSprite = ResourcesManager.LoadAsepriteData("long_button.aseprite").Sprites.First();

            Entity menuButtonEntity = UI.CreateUIElement("Menu button");
            menuButton = menuButtonEntity.AddComponent<UIButton>();
            menuButton.Sprite = smallButtonSprite;
            menuButton.RectTransform.AnchorMin = new Vector2(1, 1);
            menuButton.RectTransform.AnchorMax = new Vector2(1, 1);
            menuButton.RectTransform.Size = new Vector2(SMALL_BUTTON_SIZE_X, SMALL_BUTTON_SIZE_Y);
            menuButton.RectTransform.AnchoredPosition = new Vector2(-SMALL_BUTTON_SIZE_X / 2 - 5, -SMALL_BUTTON_SIZE_Y / 2 - 5);
            menuButton.OnClick.AddCallback(OpenMenu);
            Entity menuTextEntity = UI.CreateUIElement("Menu text", menuButtonEntity);
            UIText menuText = menuTextEntity.AddComponent<UIText>();
            menuText.Text = "Menu";
            menuText.TextSize = 32;
            menuText.Color = Color.Black;
            menuText.TextAlignment = AlignmentType.CenterMiddle;
            menuText.RectTransform.AnchorMax = Vector2.One;

            menu = UI.CreateUIElement("Menu");
            menu.GetComponent<RectTransformation>().AnchorMax = Vector2.One;
            float y = 200;
            CreateMenuButton("Continue", y).OnClick.AddCallback(CloseMenu);
            y -= SMALL_BUTTON_SIZE_Y + MENU_BUTTON_OFFSET;
            CreateMenuButton("Restart", y).OnClick.AddCallback(GameManager.Restart);
            y -= SMALL_BUTTON_SIZE_Y + MENU_BUTTON_OFFSET;
            menu.IsActiveSelf = false;
            
            Entity modeTextEntity = UI.CreateUIElement("Mode text", menu);
            modeText = modeTextEntity.AddComponent<UIText>();
            modeText.TextSize = 32;
            modeText.Color = Color.Black;
            modeText.TextAlignment = AlignmentType.CenterMiddle;
            modeText.RectTransform.AnchorMin = new Vector2(0.5f, 0.5f);
            modeText.RectTransform.AnchorMax = new Vector2(0.5f, 0.5f);
            modeText.RectTransform.Size = new Vector2(400, SMALL_BUTTON_SIZE_Y);
            modeText.RectTransform.AnchoredPosition = new Vector2(0, y);
            UpdateGameModeText();
            y -= SMALL_BUTTON_SIZE_Y + MENU_BUTTON_OFFSET;
            
            CreateMenuButton("Restart as Tutorial", y, true).OnClick.AddCallback(() =>
            {
                GameManager.SelectedGameMode = GameMode.Tutorial;
                UpdateGameModeText();
                GameManager.Restart();
            });
            y -= SMALL_BUTTON_SIZE_Y + MENU_BUTTON_OFFSET;
            CreateMenuButton("Restart as Easy", y, true).OnClick.AddCallback(() =>
            {
                GameManager.SelectedGameMode = GameMode.Easy;
                UpdateGameModeText();
                GameManager.Restart();
            });
            y -= SMALL_BUTTON_SIZE_Y + MENU_BUTTON_OFFSET;
            CreateMenuButton("Restart as Hard", y, true).OnClick.AddCallback(() =>
            {
                GameManager.SelectedGameMode = GameMode.Hard;
                UpdateGameModeText();
                GameManager.Restart();
            });
            y -= SMALL_BUTTON_SIZE_Y + MENU_BUTTON_OFFSET;
            
            Entity creditsEntity = UI.CreateUIElement("Credits text", menu);
            UIText creditsText = creditsEntity.AddComponent<UIText>();
            creditsText.Text = "Created in 48 hours for LD48 using Iron Engine";
            creditsText.TextSize = 32;
            creditsText.Color = Color.Black;
            creditsText.RectTransform.AnchorMin = new Vector2(0.5f, 0.0f);
            creditsText.RectTransform.AnchorMax = new Vector2(0.5f, 0.0f);
            creditsText.RectTransform.Size = new Vector2(600, 40);
            creditsText.RectTransform.AnchoredPosition = new Vector2(0, 20);
        }

        public void OpenMenu()
        {
            GameManager.Pause();
            menuButton.Entity.IsActiveSelf = false;
            menu.IsActiveSelf = true;
        }

        public void CloseMenu()
        {
            GameManager.UnPause();
            menuButton.Entity.IsActiveSelf = true;
            menu.IsActiveSelf = false;
        }

        private UIButton CreateMenuButton(string text, float y, bool isLong = false)
        {
            Entity buttonEntity = UI.CreateUIElement("Button", menu);
            UIButton button = buttonEntity.AddComponent<UIButton>();
            button.Sprite = isLong ? longButtonSprite : smallButtonSprite;
            button.RectTransform.AnchorMin = new Vector2(0.5f, 0.5f);
            button.RectTransform.AnchorMax = new Vector2(0.5f, 0.5f);
            button.RectTransform.Size = new Vector2(isLong ? LONG_BUTTON_SIZE_X : SMALL_BUTTON_SIZE_X, SMALL_BUTTON_SIZE_Y);
            button.RectTransform.AnchoredPosition = new Vector2(0, y);
            Entity textEntity = UI.CreateUIElement("Menu text", buttonEntity);
            UIText uiText = textEntity.AddComponent<UIText>();
            uiText.Text = text;
            uiText.TextSize = 32;
            uiText.Color = Color.Black;
            uiText.TextAlignment = AlignmentType.CenterMiddle;
            uiText.RectTransform.AnchorMax = Vector2.One;

            return button;
        }

        private void UpdateGameModeText()
        {
            modeText.Text = "Game mode: " + GameManager.SelectedGameMode;
        }
    }
}