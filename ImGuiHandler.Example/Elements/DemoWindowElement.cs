using ImGuiNET;

namespace ImGuiHandler.Example.Elements
{
    public class DemoWindowElement : ImGuiElement
    {
        protected override void CustomRender()
        {
            ImGui.ShowDemoWindow();
        }
    }
}