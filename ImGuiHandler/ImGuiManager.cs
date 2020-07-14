using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;

namespace ImGuiHandler
{
    public class ImGuiManager
    {
        private readonly ImGuiRenderer _renderer;
        private readonly HashSet<ImGuiElement> _elements = new HashSet<ImGuiElement>();
        private readonly Queue<ImGuiElement> _elementsToAdd = new Queue<ImGuiElement>();
        private readonly Queue<ImGuiElement> _elementsToRemove = new Queue<ImGuiElement>();
        
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
            _elementsToAdd.Enqueue(element);
        }

        /// <summary>
        /// Removes a UI element from the manager, and will cause that element to no longer be rendered.
        /// </summary>
        public void RemoveElement(ImGuiElement element)
        {
            _elementsToRemove.Enqueue(element);
        }

        /// <summary>
        /// Renders all elements that have been added to the manager
        /// </summary>
        public void RenderElements(TimeSpan timeSinceLastFrame)
        {
            // Add and remove all elements waiting to be worked on.  We can't do this when `AddElement` or
            // `RemoveElement` is called as events might trigger them, and that will cause the collection to be 
            // modified during iteration
            while (_elementsToAdd.Any())
            {
                var element = _elementsToAdd.Dequeue();
                _elements.Add(element);
            }

            while (_elementsToRemove.Any())
            {
                var element = _elementsToRemove.Dequeue();
                _elements.Remove(element);
            }
            
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