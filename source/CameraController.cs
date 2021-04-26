using Iron;

namespace IronCustom
{
    public class CameraController : ScriptComponent
    {
        public static bool CanControl = true;
        private float topBound;
        private float bottomBound;
        private float targetY = 0;

        private const float EPS = 0.2f;
        private const float SENSITIVITY = 0.75f;
        private const float SPEED = 2f;

        public void Init(float topBound, float bottomBound)
        {
            this.topBound = topBound - Camera.Main.Height / 2;
            this.bottomBound = bottomBound + Camera.Main.Height / 2;
            targetY = Transformation.Position.Y;
        }

        public void Reset()
        {
            targetY = 0;
            Transformation.Position = Transformation.Position.SetY(0);
        }
        
        public override void OnUpdate()
        {
            if (GameManager.IsPaused)
                return;
            
            if (Math.Abs(targetY - Transformation.Position.Y) > EPS)
            {
                Transformation.Position = Transformation.Position.SetY(Math.Lerp(Transformation.Position.Y, targetY, SPEED * Time.DeltaTime));
            }
            
            if (!CanControl || UI.IsPointerOverUI())
                return;

            float y = Input.MouseScrollDelta.Y;
            if (!Math.Approximately(y, 0))
            {
                targetY += y * SENSITIVITY;

                if (targetY > topBound)
                    targetY = topBound;
                if (targetY < bottomBound)
                    targetY = bottomBound;
            }
        }
    }
}