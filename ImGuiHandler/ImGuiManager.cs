using System;
using System.Collections.Generic;
using ImGuiNET;

namespace ImGuiHandler
{
    public class ImGuiManager
    {
        private readonly ImGuiRenderer _renderer;
        private readonly List<ImGuiElement> _elements = new List<ImGuiElement>();
        
        /// <summary>
        /// If true, ImGui elements are currently accepting mouse input.  This can be used to detect if the
        /// non-ImGui elements should ignore mouse inputs or not.  This can help prevent the mouse from clicking
        /// on application specific items behind the ImGui elements.
        /// </summary>
        public bool AcceptingMouseInput { get; private set; }
        
        /// <summary>
        /// If true, ImGui elements are currently accepting keyboard inputs.  This can be used to detect if
        /// non-ImGui elements should ignore keyboard input or not.  This can help prevent keyboard input meant for
        /// ImGui controls from impacting other parts of the application.
        /// </summary>
        public bool AcceptingKeyboardInput { get; private set; }

        public ImGuiManager(ImGuiRenderer renderer)
        {
            _renderer = renderer;
            
            _renderer.Initialize();
        }
        
        /// <summary>
        /// Adds a new UI element for the manager to render.  Only root level elements need to be added here, as
        /// any child elements will have their render method called by their parent UI Element.
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(ImGuiElement element)
        {
            _elements.Add(element);
        }

        /// <summary>
        /// Removes a UI element from the manager, and will cause that element to no longer be rendered.
        /// </summary>
        public void RemoveElement(ImGuiElement element)
        {
            _elements.Remove(element);
        }

        /// <summary>
        /// Renders all elements that have been added to the manager
        /// </summary>
        public void RenderElements(TimeSpan timeSinceLastFrame)
        {
            _renderer.BeforeLayout(timeSinceLastFrame);
            foreach (var element in _elements)
            {
                element.Render();
            }
            
            var io = ImGui.GetIO();
            AcceptingKeyboardInput = io.WantCaptureKeyboard;
            AcceptingMouseInput = io.WantCaptureMouse;

            _renderer.AfterLayout();
        }
    }
}