using System.Collections.Generic;
using ImGuiNET;

namespace ImGuiHandler.Example.Elements
{
    public class EntityDataWindow : ImGuiElement
    {
        private readonly List<EntityDataEditor> _entityEditors;

        public bool ShowEntities
        {
            get => Get<bool>();
            set => Set(value);
        }

        public EntityDataWindow(IEnumerable<EntityDataEditor> entities)
        {
            _entityEditors = new List<EntityDataEditor>(entities);
        }
        
        protected override void CustomRender()
        {
            if (ImGui.Begin("Entity Data Editor"))
            {
                var framerate = ImGui.GetIO().Framerate;
                ImGui.Text($"Frame time: {(1000f / framerate):F3} ms/frame");
                ImGui.Text($"Framerate: {framerate:F1} FPS");
                
                ImGui.NewLine();
                
                ImGui.Text($"Entity Types: {_entityEditors.Count}");
                Checkbox(nameof(ShowEntities), "Show Entities");

                if (ShowEntities)
                {
                    if (ImGui.TreeNode("Entities"))
                    {
                        foreach (var editor in _entityEditors)
                        {
                            if (ImGui.TreeNode(editor.EntityId.ToString(), editor.DisplayName ?? "<No Name>"))
                            {
                                editor.IsVisible = true;
                                editor.Render();
                                
                                ImGui.TreePop();
                            }
                        }
                        
                        ImGui.TreePop();
                    }
                }
            }
            
            ImGui.End();
        }
    }
}