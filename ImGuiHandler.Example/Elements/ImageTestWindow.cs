using System;
using System.Numerics;
using ImGuiHandler.MonoGame;
using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace ImGuiHandler.Example.Elements
{
    public class ImageTestWindow : ImGuiElement
    {
        private readonly Texture2D _texture;
        private readonly IntPtr _textureId;
        private float _zoomFactor = 1;

        public ImageTestWindow(GraphicsDevice graphicsDevice, MonoGameImGuiRenderer monoGameImGuiRenderer)
        {
            _texture = Texture2D.FromFile(graphicsDevice, "SampleParticles.png");
            _textureId = monoGameImGuiRenderer.BindTexture(_texture);
        }

        protected override void CustomRender()
        {
            const int xBuffer = 20;
            const int yBuffer = 90;

            if (ImGui.Begin("Image Test"))
            {
                var windowSize = ImGui.GetWindowSize();
                var childSize = new Vector2(windowSize.X - xBuffer, windowSize.Y - yBuffer);
                ImGui.BeginChild("fullImage", childSize, true, ImGuiWindowFlags.HorizontalScrollbar);

                var scaleHeight = (childSize.Y - 20) / _texture.Height;
                var scaleWidth = (childSize.X - 20) / _texture.Width;
                var scale = Math.Min(scaleWidth, scaleHeight) * _zoomFactor;
                
                ImGui.Image(_textureId, new Vector2(_texture.Width * scale, _texture.Height * scale));
                
                ImGui.EndChild();

                var zoom = (int) (_zoomFactor * 100);
                if (ImGui.InputInt("% Zoom", ref zoom, 10) && zoom > 0)
                {
                    _zoomFactor = (float) zoom / 100;
                }
                
                if (ImGui.Button("Reset Zoom"))
                {
                    _zoomFactor = 1;
                }

                ImGui.End();
            }
        }
    }
}