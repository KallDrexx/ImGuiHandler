using ImGuiNET;

namespace ImGuiHandler.Example.Elements
{
    public class EntityDataEditor : ImGuiElement
    {
        private readonly EntityDataDisplay _displayWindow; 
            
        public int EntityId { get; }

        [HasTextBuffer(100)]
        public string DisplayName
        {
            get => Get<string>();
            set => Set(value);
        }

        public int Health
        {
            get => Get<int>();
            set => Set(value);
        }

        public int GoldCost
        {
            get => Get<int>();
            set => Set(value);
        }

        public float MovementSpeed
        {
            get => Get<float>();
            set => Set(value);
        }

        public EntityDataEditor(EntityData entity)
        {
            using (DisablePropertyChangedNotifications())
            {
                EntityId = entity.Id;
                DisplayName = entity.DisplayName;
                Health = entity.Health;
                GoldCost = entity.GoldCost;
                MovementSpeed = entity.MovementSpeed;
            }
            
            _displayWindow = new EntityDataDisplay(entity);
        }

        protected override void CustomRender()
        {
            InputText(nameof(DisplayName), "Display Name");
            InputInt(nameof(Health), "Health");
            InputInt(nameof(GoldCost), "Gold Cost");
            InputFloat(nameof(MovementSpeed), "Movement Speed");

            if (ImGui.Button("Show Data Window"))
            {
                _displayWindow.IsVisible = !_displayWindow.IsVisible;
            }
            
            _displayWindow.Render();
        }
    }
}