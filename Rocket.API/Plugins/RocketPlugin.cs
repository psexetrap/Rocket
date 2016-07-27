﻿using Rocket.API;
using Rocket.API.Assets;
using Rocket.API.Collections;
using Rocket.API.Extensions;
using Rocket.API.Plugins;
using Rocket.Core.Extensions;
using Rocket.Logging;
using System;
using System.Linq;
using UnityEngine;

namespace Rocket.API.Plugins
{
    public class RocketPluginBase<T> : RocketPluginBase, IRocketPlugin<T> where T : class, IRocketPluginConfiguration
    {
        public IAsset<T> Configuration { get; private set; }

        protected RocketPluginBase(IRocketPluginManager manager, string name) : base(manager, name) 
        {
            Configuration = (IAsset<T>)manager.GetPluginConfiguration(this,typeof(T));
        }

        public override void LoadPlugin()
        {
            Configuration.Load();
            base.LoadPlugin();
        }
    }

    public class RocketPluginBase : MonoBehaviour, IRocketPlugin
    {
        public string WorkingDirectory { get; internal set; }

        public event RocketPluginUnloading OnPluginUnloading;
        public event RocketPluginUnloaded OnPluginUnloaded;

        public event RocketPluginLoading OnPluginLoading;
        public event RocketPluginLoaded OnPluginLoaded;

        public IRocketPluginManager PluginManager { get; private set; }
        public IAsset<TranslationList> Translations { get ; private set; }
        public PluginState State { get; private set; } = PluginState.Unloaded;
        public string Name { get; private set; }



        public virtual TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList();
            }
        }

        protected RocketPluginBase(IRocketPluginManager manager, string name)
        {
            this.PluginManager = manager;
            this.name = name;

            WorkingDirectory = manager.GetPluginDirectory(name);

            if (!System.IO.Directory.Exists(WorkingDirectory))
                System.IO.Directory.CreateDirectory(WorkingDirectory);
        }
        
        public string Translate(string translationKey, params object[] placeholder)
        {
            return Translations.Instance.Translate(translationKey, placeholder);
        }

        public void ReloadPlugin()
        {
            UnloadPlugin();
            LoadPlugin();
        }

        public virtual void LoadPlugin()
        {
            Logger.Info("\n[loading] " + name);
            Translations.Load();

            try
            {
                Load();
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to load " + Name+ ", unloading now...", ex);
                try
                {
                    UnloadPlugin(PluginState.Failure);
                    return;
                }
                catch (Exception ex1)
                {
                    Logger.Fatal("Failed to unload " + Name ,ex1);
                }
            }

            bool cancelLoading = false;
            if (OnPluginLoading != null)
            {
                foreach (var handler in OnPluginLoading.GetInvocationList().Cast<RocketPluginLoading>())
                {
                    try
                    {
                        handler(this, ref cancelLoading);
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex);
                    }
                    if (cancelLoading)
                    {
                        try
                        {
                            UnloadPlugin(PluginState.Cancelled);
                            return;
                        }
                        catch (Exception ex1)
                        {
                            Logger.Fatal("Failed to unload " + Name , ex1);
                        }
                    }
                }
            }
            State = PluginState.Loaded;
            OnPluginLoaded.TryInvoke();
        }

        public virtual void UnloadPlugin(PluginState state = PluginState.Unloaded)
        {
            Logger.Info("\n[unloading] " + Name);
            OnPluginUnloading.TryInvoke(this);
            Unload();
            State = state;
            OnPluginUnloaded.TryInvoke(this);
        }

        private void OnEnable()
        {
            LoadPlugin();
        }

        private void OnDisable()
        {
            UnloadPlugin();
        }

        protected virtual void Load()
        {
        }

        protected virtual void Unload()
        {
        }
    }
}