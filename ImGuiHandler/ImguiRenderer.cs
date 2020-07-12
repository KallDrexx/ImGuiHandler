using System;
using ImGuiNET;

namespace ImGuiHandler
{
    /// <summary>
    /// Standardized interface for hooking Dear ImGui into different rendering systems.
    /// </summary>
    public abstract class ImguiRenderer
    {
        private bool _hasBeenInitialized;

        /// <summary>
        /// Initializes the Dear ImGui system for use
        /// </summary>
        public void Initialize()
        {
            CustomInitialize();
            _hasBeenInitialized = true;
        }

        /// <summary>
        /// Gets ImGui prepared for a new frame
        /// </summary>
        /// <param name="timeSinceLastFrame"></param>
        public void BeforeLayout(TimeSpan timeSinceLastFrame)
        {
            if (!_hasBeenInitialized)
            {
                throw new InvalidOperationException("Renderer has not yet had Initialize() called");
            }
            
            ImGui.GetIO().DeltaTime = (float) timeSinceLastFrame.TotalSeconds;
            UpdateInput();
            ImGui.NewFrame();
        }

        /// <summary>
        /// Asks ImGui to generate the geometry data and sends it to the graphics pipeline.  This should be called
        /// after all ImGui calls have been finished.
        /// </summary>
        public void AfterLayout()
        {
            if (!_hasBeenInitialized)
            {
                throw new InvalidOperationException("Renderer has not yet had Initialize() called");
            }
            
            ImGui.Render();

            var drawData = ImGui.GetDrawData();
            RenderImGui(drawData);
        }

        /// <summary>
        /// Custom initialization that's required depending on each rendering system.
        /// </summary>
        protected abstract void CustomInitialize();

        /// <summary>
        /// Application/Renderer specific logic to send input data (such as mouse/keyboard input to the ImGui system
        /// </summary>
        protected abstract void UpdateInput();

        /// <summary>
        /// Takes the geometry data from ImGui and sends it to the graphics device 
        /// </summary>
        protected abstract void RenderImGui(ImDrawDataPtr drawData);
    }
}