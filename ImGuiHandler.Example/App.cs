using System.Collections.Generic;
using ImGuiHandler.Example.Elements;
using ImGuiHandler.MonoGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ImGuiHandler.Example
{
    public class App : Game
    {
        private static readonly EntityData[] Entities = FormTestEntities();
        private readonly DemoWindowElement _demoWindowElement;
        private readonly EntityDataWindow _entityDataWindow;
        private ImGuiManager _imGuiManager;
        private KeyboardState _previousKeyState, _currentKeyState;
        
        public App()
        {
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024, 
                PreferredBackBufferHeight = 768,
                PreferMultiSampling = true
            };

            IsMouseVisible = true;
            
            _demoWindowElement = new DemoWindowElement();

            var entityEditors = CreateEntityDataEditors();
            _entityDataWindow = new EntityDataWindow(entityEditors);
        }

        protected override void Initialize()
        {
            var imGuiRenderer = new MonoGameImGuiRenderer(this);
            imGuiRenderer.Initialize();
            
            _imGuiManager = new ImGuiManager(imGuiRenderer);
            _imGuiManager.AddElement(_demoWindowElement);
            _imGuiManager.AddElement(_entityDataWindow);

            _entityDataWindow.IsVisible = true;
            
            base.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKeyState = _currentKeyState;
            _currentKeyState = Keyboard.GetState();

            if (HasBeenPressed(Keys.F12))
            {
                _demoWindowElement.IsVisible = !_demoWindowElement.IsVisible;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _imGuiManager.RenderElements(gameTime.ElapsedGameTime);
            
            base.Draw(gameTime);
        }

        private static IEnumerable<EntityDataEditor> CreateEntityDataEditors()
        {
            var entityEditors = new List<EntityDataEditor>();
            foreach (var entity in Entities)
            {
                var editor = new EntityDataEditor(entity);
                editor.PropertyChanged += (sender, args) =>
                {
                    switch (args.PropertyName)
                    {
                        case nameof(EntityDataEditor.DisplayName):
                            entity.DisplayName = editor.DisplayName;
                            break;

                        case nameof(EntityDataEditor.Health):
                            entity.Health = editor.Health;
                            break;

                        case nameof(EntityDataEditor.GoldCost):
                            entity.GoldCost = editor.GoldCost;
                            break;

                        case nameof(EntityDataEditor.MovementSpeed):
                            entity.MovementSpeed = editor.MovementSpeed;
                            break;
                    }
                };

                entityEditors.Add(editor);
            }

            return entityEditors;
        }

        private static EntityData[] FormTestEntities()
        {
            var first = new EntityData
            {
                Id = 1,
                DisplayName = "First",
                Health = 123,
                GoldCost = 13,
                MovementSpeed = 123.4f
            };
            
            var second = new EntityData
            {
                Id = 2,
                DisplayName = "Second",
                Health = 300,
                GoldCost = 44,
                MovementSpeed = 123.4f
            };
            
            var third = new EntityData
            {
                Id = 3,
                DisplayName = "Third",
                Health = 432,
                GoldCost = 66,
                MovementSpeed = 123.4f
            };
            
            var fourth = new EntityData
            {
                Id = 4,
                DisplayName = "Fourth",
                Health = 11,
                GoldCost = 77,
                MovementSpeed = 123.4f
            };
            
            var fifth = new EntityData
            {
                Id = 5,
                DisplayName = "Fifth",
                Health = 23,
                GoldCost = 122,
                MovementSpeed = 123.4f
            };

            return new[] {first, second, third, fourth, fifth};
        }

        private bool HasBeenPressed(Keys key) => _previousKeyState.IsKeyDown(key) && _currentKeyState.IsKeyUp(key);
    }
}