using ImGuiNET;

namespace ImGuiHandler.Example.Elements
{
    public class EntityDataDisplay : ImGuiElement
    {
        private readonly EntityData _entity;

        public EntityDataDisplay(EntityData entity)
        {
            _entity = entity;
        }

        protected override void CustomRender()
        {
            if (ImGui.Begin($"Data For Entity {_entity.Id}"))
            {
                ImGui.Columns(2);
                ImGui.Text("Property"); ImGui.NextColumn();
                ImGui.Text("Value"); ImGui.NextColumn();
                ImGui.Separator();
                
                ImGui.Text("Name"); ImGui.NextColumn();
                ImGui.Text(_entity.DisplayName); ImGui.NextColumn();
                ImGui.Text("Health"); ImGui.NextColumn();
                ImGui.Text(_entity.Health.ToString()); ImGui.NextColumn();
                ImGui.Text("Gold Cost"); ImGui.NextColumn();
                ImGui.Text(_entity.GoldCost.ToString()); ImGui.NextColumn();
                ImGui.Text("Speed"); ImGui.NextColumn();
                ImGui.Text(_entity.MovementSpeed.ToString("N"));
                ImGui.Columns(1);
            }
            
            ImGui.End();
        }
    }
}