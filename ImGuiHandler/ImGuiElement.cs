using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ImGuiNET;

namespace ImGuiHandler
{
    /// <summary>
    /// Represents one or more ImGUi controls that can be manipulated or rendered
    /// </summary>
    public abstract class ImGuiElement : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> _notifyPropertyChangedObjects = new Dictionary<string, object>();
        private readonly Dictionary<string, byte[]> _propertyTextBuffers = new Dictionary<string, byte[]>();
        private bool _disablePropertyNotificationEvents;

        /// <summary>
        /// When true this component is expected to be rendered
        /// </summary>
        public bool IsVisible { get; set; } = true;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public ImGuiElement()
        {
            var propertiesRequiringTextBuffers = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => (Property: x, TextBufferAttribute: x.GetCustomAttribute<HasTextBufferAttribute>()))
                .Where(x => x.TextBufferAttribute != null);

            foreach (var (property, attribute) in propertiesRequiringTextBuffers)
            {
                if (property.PropertyType != typeof(string))
                {
                    var message = $"{GetType().Name}'s {property.Name} property has the HasTextBufferAttribute but is not a string.  " +
                                  $"This attribute is only supported on string properties.";
                    
                    throw new InvalidOperationException(message);
                }

                if (attribute.MaxLength <= 1)
                {
                    var message = $"Text buffer max length for property {GetType().Name}'s {property.Name} property must be at least 1";
                    throw new InvalidOperationException(message);
                }

                var textBuffer = new byte[attribute.MaxLength];
                _propertyTextBuffers[property.Name] = textBuffer;
            }
        }

        /// <summary>
        /// Render this element
        /// </summary>
        public void Render()
        {
            if (!IsVisible)
            {
                return;
            }
            
            CustomRender();
        }

        /// <summary>
        /// Rendering code specific to this element.  ImGui rendering goes in here.
        /// </summary>
        protected abstract void CustomRender();
        
        /// <summary>
        /// Temporarily disables property changed events from being dispatched until the returned IDisposable
        /// is disposed.  Mostly useful for initial values being set up, when event propagation isn't needed.
        /// </summary>
        /// <returns></returns>
        protected IDisposable DisablePropertyChangedNotifications()
        {
            return new EventNotificationDisabler(this);
        }
        
        /// <summary>
        /// Retrieves the current value of the specified property from the backing store, or the `default(T)` if no
        /// value has been set yet. Cuts down on boilerplate on MVVM style properties.
        /// </summary>
        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            
            if (typeof(T) == typeof(string) && _propertyTextBuffers.TryGetValue(propertyName, out var textBuffer))
            {
                // If this is a string type backed by a text buffer, then _notifyPropertyChangedObjects will *NOT* have
                // the value.  This is to keep the number of times we have to convert between a byte buffer and string
                // down.  Therefore, strings will be lazily materialized.
                
                // When we pass a string to ImGui of length X, but the user sets the value to a string that's less
                // than X - 1, we end up getting null codes in the string, and in some cases this may put null values
                // in the middle of a string, thus making wrong in serialization and I/O operations.  This tends to
                // happen when select-all + paste occurs in a text box.  To prevent errors we only want the string
                // up to the first null character

                var rawString = Encoding.Default.GetString(textBuffer);
                return (T) (object) rawString.Split('\0')[0];
            }

            return _notifyPropertyChangedObjects.TryGetValue(propertyName, out var value)
                ? (T) value
                : default;
        }
        
        /// <summary>
        /// Sets the raw value of a property to the specified value.  The object must be the same type (or castable
        /// to the same type) as the corresponding `Get` type, or else an exception will occur at runtime. Cuts down on
        /// boilerplate on MVVM style properties.
        /// </summary>
        protected void Set(object value, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            var hasExistingValue = _notifyPropertyChangedObjects.TryGetValue(propertyName, out var existingValue);
            if (hasExistingValue && existingValue == value)
            {
                return;
            }

            _notifyPropertyChangedObjects[propertyName] = value;
            
            // If this field is backed by a text buffer, we need to update the text buffer with the new value, otherwise
            // ImGui controls will display the wrong value.
            if (_propertyTextBuffers.TryGetValue(propertyName, out var textBuffer))
            {
                var valueBytes = value != null
                    ? Encoding.Default.GetBytes((string) value)
                    : Array.Empty<byte>();
                
                Array.Clear(textBuffer, 0, textBuffer.Length);
                
                // Not sure if we should truncate or exception if the value is larger than the specified max length.
                // For now we'll truncate, but this should probably be revisited.
                var length = Math.Min(valueBytes.Length, textBuffer.Length);
                Array.Copy(valueBytes, textBuffer, length);
            }

            CallPropertyChangedNotification(propertyName);
        }

        /// <summary>
        /// Creates a standard ImGui text editor for a property
        /// </summary>
        protected void InputText(string property, string label)
        {
            var buffer = GetTextBuffer(property);
            var hasBeenChanged = ImGui.InputText(label, buffer, (uint) buffer.Length);

            if (hasBeenChanged)
            {
                UpdatePropertyFromTextBuffer(property);
            }
        }

        /// <summary>
        /// Creates a standard ImGui integer editor for a property
        /// </summary>
        protected void InputInt(string property, string label)
        {
            var intVal = Get<int>(property);
            var hasBeenChanged = ImGui.InputInt(label, ref intVal);

            if (hasBeenChanged)
            {
                Set(intVal, property);
            }
        }

        /// <summary>
        /// Creates a standard ImGui double editor for a property
        /// </summary>
        protected void InputDouble(string property, string label)
        {
            var value = Get<double>(property);
            var hasBeenChanged = ImGui.InputDouble(label, ref value);
            
            if (hasBeenChanged)
            {
                Set(value, property);
            }
        }

        /// <summary>
        /// Creates a standard ImGui float editor for a property
        /// </summary>
        protected void InputFloat(string property, string label)
        {
            var value = Get<float>(property);
            var hasBeenChanged = ImGui.InputFloat(label, ref value);
            
            if (hasBeenChanged)
            {
                Set(value, property);
            }
        }

        /// <summary>
        /// Creates a standard ImGui checkbox control for a property
        /// </summary>
        protected void Checkbox(string property, string label)
        {
            var value = Get<bool>(property);
            var hasBeenChanged = ImGui.Checkbox(label, ref value);
            
            if (hasBeenChanged)
            {
                Set(value, property);
            }
        }

        /// <summary>
        /// Retrieves the text buffer for the specified property.  Properties must be tagged with a
        /// [HasTextBuffer] attribute in order for a text buffer to be retrieved
        /// </summary>
        private byte[] GetTextBuffer(string property)
        {
            if (!_propertyTextBuffers.TryGetValue(property, out var buffer))
            {
                var message = $"Property {property} does not have a text buffer available.  " +
                              "Make sure it's marked with the [HasTextBuffer] attribute";
                
                throw new InvalidOperationException(message);
            }

            return buffer;
        }

        /// <summary>
        /// Updates a string property's value from the value being stored in an underlying text buffer
        /// </summary>
        private void UpdatePropertyFromTextBuffer(string propertyName)
        {
            if (!_propertyTextBuffers.ContainsKey(propertyName))
            {
                var message = $"Property {propertyName} does not have a text buffer available.  " +
                              "Make sure it's marked with the [HasTextBuffer] attribute";
                
                throw new InvalidOperationException(message);
            }

            // We don't want to actually call `Set<string>()` here, because we don't want to materialize the 
            // string from the byte data.  This will be called every frame that an ImGui text input is visible (and
            // for each one), which will cause a lot of garbage.  However, we only care about materializing the
            // final string when the property's getter is being called, which will be sometime later.  Therefore,
            // we will just call notifications and let the Getter materialize the string.

            CallPropertyChangedNotification(propertyName);
        }

        private void CallPropertyChangedNotification(string propertyName)
        {
            if (!_disablePropertyNotificationEvents)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private class EventNotificationDisabler : IDisposable
        {
            private readonly ImGuiElement _element;

            public EventNotificationDisabler(ImGuiElement element)
            {
                _element = element;
                _element._disablePropertyNotificationEvents = true;
            }
            
            public void Dispose()
            {
                _element._disablePropertyNotificationEvents = false;
            }
        }
    }
}