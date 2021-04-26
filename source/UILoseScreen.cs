using Iron;

namespace IronCustom
{
    public class UILoseScreen : ScriptComponent
    {
        private static Sprite _panelSprite = null;
        private static Sprite panelSprite => _panelSprite ?? (_panelSprite = ResourcesManager.LoadImage("lose_screen.png"));
        private static Sprite _smallButtonSprite = null;
        private static Sprite smallButtonSprite => _smallButtonSprite ?? (_smallButtonSprite = ResourcesManager.LoadImage("small_button.aseprite"));
        
        private const float SMALL_BUTTON_SIZE_X = 150;
        private const float SMALL_BUTTON_SIZE_Y = 45;
        
        public static Entity Create()
        {
            Entity entity = UI.CreateUIElement("Lose screen");
            RectTransformation rt = entity.GetComponent<RectTransformation>();
            UIImage image = entity.AddComponent<UIImage>();
            image.Sprite = panelSprite;
            rt.AnchorMin = new Vector2(0, 0);
            rt.AnchorMax = new Vector2(1, 1);
            rt.SortingOrder = -3;

            UILoseScreen element = entity.AddComponent<UILoseScreen>();
            element.CreateInner();
            
            return entity;
        }

        private void CreateInner()
        {
            Entity entity0 = UI.CreateUIElement("Text", Entity);
            UIText uiText0 = entity0.AddComponent<UIText>();
            uiText0.Text = "You lost!";
            uiText0.TextSize = 96;
            uiText0.Color = Color.Black;
            uiText0.TextAlignment = AlignmentType.CenterMiddle;
            uiText0.RectTransform.AnchorMin = new Vector2(0.5f, 0.5f);
            uiText0.RectTransform.AnchorMax = new Vector2(0.5f, 0.5f);
            uiText0.RectTransform.Size = new Vector2(Screen.Width, 100);
            uiText0.RectTransform.AnchoredPosition = new Vector2(0, -80);
            uiText0.RectTransform.SortingOrder = -1;
            
            Entity entity = UI.CreateUIElement("Text", Entity);
            UIText uiText1 = entity.AddComponent<UIText>();
            uiText1.Text = "All of your blocks are frozen forever..";
            uiText1.TextSize = 64;
            uiText1.Color = Color.Black;
            uiText1.TextAlignment = AlignmentType.CenterMiddle;
            uiText1.RectTransform.AnchorMin = new Vector2(0.5f, 0.5f);
            uiText1.RectTransform.AnchorMax = new Vector2(0.5f, 0.5f);
            uiText1.RectTransform.Size = new Vector2(Screen.Width, 70);
            uiText1.RectTransform.AnchoredPosition = new Vector2(0, -140);
            uiText1.RectTransform.SortingOrder = -1;
            
            Entity buttonEntity = UI.CreateUIElement("Button", Entity);
            UIButton button = buttonEntity.AddComponent<UIButton>();
            button.Sprite = smallButtonSprite;
            button.RectTransform.AnchorMin = new Vector2(0.5f, 0.5f);
            button.RectTransform.AnchorMax = new Vector2(0.5f, 0.5f);
            button.RectTransform.Size = new Vector2(SMALL_BUTTON_SIZE_X, SMALL_BUTTON_SIZE_Y);
            button.RectTransform.AnchoredPosition = new Vector2(0, 200);
            button.RectTransform.SortingOrder = -1;
            button.OnClick.AddCallback(GameManager.Restart);
            
            Entity textEntity = UI.CreateUIElement("Menu text", buttonEntity);
            UIText uiText = textEntity.AddComponent<UIText>();
            uiText.Text = "Restart";
            uiText.TextSize = 32;
            uiText.Color = Color.Black;
            uiText.TextAlignment = AlignmentType.CenterMiddle;
            uiText.RectTransform.AnchorMax = Vector2.One;
            uiText.RectTransform.SortingOrder = -1;
        }
    }
}