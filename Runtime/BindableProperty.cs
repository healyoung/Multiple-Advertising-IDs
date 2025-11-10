using System;
using UnityEngine;

namespace HealYoung
{
    #region BindableProperty

    
    public interface IUnRegister
    {
        void UnRegister();
    }
    public interface IReadonlyBindableProperty<T>
    {
        T Value { get; }
        IUnRegister RegisterWithInitValue(Action<T> action);
        void UnRegister(Action<T> onValueChanged);
        IUnRegister Register(Action<T> onValueChanged);
    }

    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutEvent(T newValue);
        void SaveValue();
        void Release();
    }

    public sealed class BindableProperty<T> : IBindableProperty<T>
    {
        public BindableProperty(T defaultValue = default)
        {
            _value = defaultValue;
        }

        private T _value;

        public T Value
        {
            get => GetValue();
            set
            {
                if (value == null && _value == null) return;
                if (value != null && value.Equals(_value)) return;

                SetValue(value);
                _onValueChanged?.Invoke(value);
            }
        }

        private void SetValue(T newValue)
        {
            _value = newValue;
        }

        private T GetValue()
        {
            return _value;
        }

        public void SetValueWithoutEvent(T newValue)
        {
            _value = newValue;
        }

        public void SaveValue()
        {
        }

        public void Release()
        {
        }

        private Action<T> _onValueChanged = _ => { };

        public IUnRegister Register(Action<T> onValueChanged)
        {
            _onValueChanged += onValueChanged;
            return new BindablePropertyUnRegister<T>()
            {
                BindableProperty = this,
                OnValueChanged = onValueChanged
            };
        }

        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(_value);
            return Register(onValueChanged);
        }

        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            _onValueChanged -= onValueChanged;
        }
    }

    /// <summary>
    /// 用于本地数据存储的类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class StorageProperty<T> : IBindableProperty<T>
    {
        public T Value
        {
            get => _value;
            set => SetValue(value);
        }

        public void SetValueWithoutEvent(T newValue)
        {
            _value = newValue;
            string value = JsonUtility.ToJson(_value);
            PlayerPrefs.SetString(_key, value);
        }

        public StorageProperty(string key, T defaultValue)
        {
            _key = key;
            GetValue(defaultValue);
        }

        private void SetValue(T newValue)
        {
            if (!newValue.Equals(_value))
            {
                _value = newValue;
                SaveValue();
            }
        }

        public void SaveValue()
        {
            string value = JsonUtility.ToJson(_value);
            PlayerPrefs.SetString(_key, value);
            OnValueChanged?.Invoke(_value);
        }

        public void Release()
        {
            if (PlayerPrefs.HasKey(_key))
            {
                PlayerPrefs.DeleteKey(_key);
            }
        }

        private void GetValue(T defaultValue)
        {
            if (PlayerPrefs.HasKey(_key))
            {
                string value = PlayerPrefs.GetString(_key);
                _value = JsonUtility.FromJson<T>(value);
            }
            else
            {
                try
                {
                    string value = JsonUtility.ToJson(defaultValue);
                    PlayerPrefs.SetString(_key, value);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    _value = defaultValue;
                }
            }

            if (typeof(T).IsClass) _value ??= Activator.CreateInstance<T>();
        }


        public IUnRegister RegisterWithInitValue(Action<T> action)
        {
            _value = default;
            return Register(action);
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            OnValueChanged -= onValueChanged;
        }

        public IUnRegister Register(Action<T> onValueChanged)
        {
            OnValueChanged += onValueChanged;
            return new BindablePropertyUnRegister<T>()
            {
                OnValueChanged = onValueChanged
            };
        }

        private event Action<T> OnValueChanged = delegate { };
        private readonly string _key;
        private T _value;
    }

    public class BindablePropertyUnRegister<T> : IUnRegister
    {
        public BindableProperty<T> BindableProperty { get; set; }

        public Action<T> OnValueChanged { get; set; }

        public void UnRegister()
        {
            BindableProperty.UnRegister(OnValueChanged);

            BindableProperty = null;
            OnValueChanged = null;
        }
    }

    #endregion
}