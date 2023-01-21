using EcuDiagSim.App.Interfaces;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace EcuDiagSim.App.Services
{
    public class LocalSettingsService : ISettingsService
    {
        private readonly ApplicationDataContainer _rootContainer = ApplicationData.Current.LocalSettings;

        public T? Load<T>(string containerName, string key)
        {
            Validate(containerName, key);

            if (_rootContainer.Containers[containerName].Values[key] is string jsonString)
            {
                return JsonSerializer.Deserialize<T>(jsonString);
            }

            return default;
        }

        public bool TryLoad<T>(string containerName, string key, out T? value)
        {
            try
            {
                value = Load<T>(containerName, key);
                return true;
            }
            catch
            {
            }

            value = default;

            return false;
        }

        public void Save<T>(string containerName, string key, T value)
        {
            ApplicationDataContainer? container = _rootContainer.Containers.GetValueOrDefault(containerName);
            container ??= _rootContainer.CreateContainer(containerName, ApplicationDataCreateDisposition.Always);
            container.Values[key] = JsonSerializer.Serialize(value);
        }

        public bool TrySave<T>(string containerName, string key, T value)
        {
            try
            {
                Save<T>(containerName, key, value);
                return true;
            }
            catch
            {
            }

            return false;
        }

        public void RemoveContainer(string containerName)
        {
            _rootContainer.DeleteContainer(containerName);
        }

        public void RemoveAllSettings()
        {
            foreach (KeyValuePair<string, ApplicationDataContainer> container in _rootContainer.Containers)
            {
                _rootContainer.DeleteContainer(container.Key);
            }

            foreach (KeyValuePair<string, object> value in _rootContainer.Values)
            {
                _rootContainer.Values.Clear();
            }
        }

        private void Validate(string containerName, string key)
        {
            if (string.IsNullOrEmpty(containerName) is true)
            {
                throw new ContainerNameIsNullOrEmptyException(containerName);
            }

            if (string.IsNullOrEmpty(key) is true)
            {
                throw new KeyIsNullOrEmptyException(containerName, key);
            }

            if (_rootContainer.Containers.ContainsKey(containerName) is false)
            {
                throw new ContainerNameDoesNotExistException(containerName);
            }

            if (_rootContainer.Containers[containerName].Values.ContainsKey(key) is false)
            {
                throw new KeyDoesNotExistException(containerName, key);
            }
        }

        public class LocalSettingsException : Exception
        {
            public LocalSettingsException(string containerName, string key = "")
            {
                ContainerName = containerName;
                Key = key;
            }

            public string ContainerName { get; }

            public string Key { get; }
        }

        public class ContainerNameIsNullOrEmptyException : LocalSettingsException
        {
            public ContainerNameIsNullOrEmptyException(string containerName) : base(containerName)
            {
            }
        }

        public class KeyIsNullOrEmptyException : LocalSettingsException
        {
            public KeyIsNullOrEmptyException(string containerName, string key) : base(containerName, key)
            {
            }
        }

        public class ContainerNameDoesNotExistException : LocalSettingsException
        {
            public ContainerNameDoesNotExistException(string containerName) : base(containerName)
            {
            }
        }

        public class KeyDoesNotExistException : LocalSettingsException
        {
            public KeyDoesNotExistException(string containerName, string key) : base(containerName, key)
            {
            }
        }
    }
}