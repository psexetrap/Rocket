﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Rocket.API;
using Rocket.Core;
using Rocket.API.Commands;
using Rocket.API.Plugins;
using System.Collections.ObjectModel;

namespace Rocket.Unturned.Commands
{
    public class CommandRocket : IRocketCommand
    {
        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public string Name
        {
            get { return "rocket"; }
        }

        public string Help
        {
            get { return "Reloading Rocket or individual plugins"; }
        }

        public string Syntax
        {
            get { return "<plugins | reload> | <reload | unload | load> <plugin>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "rocket.info", "rocket.rocket" }; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length == 0)
            {
                R.Instance.Implementation.Chat.Say(caller, "Rocket v" + Assembly.GetExecutingAssembly().GetName().Version + " for "+R.Instance.Implementation.Name);
                R.Instance.Implementation.Chat.Say(caller, "https://rocketmod.net - 2016");
                return;
            }

            if (command.Length == 1)
            {
                switch (command[0].ToLower()) {
                    case "plugins":
                        if (caller != null && !caller.HasPermission("rocket.plugins")) return;
                        ReadOnlyCollection<IRocketPlugin> plugins = R.Instance.GetPlugins();
                        R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_plugins_loaded", String.Join(", ", plugins.Where(p => p.State == PluginState.Loaded).Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_plugins_unloaded", String.Join(", ", plugins.Where(p => p.State == PluginState.Unloaded).Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_plugins_failure", String.Join(", ", plugins.Where(p => p.State == PluginState.Failure).Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_plugins_cancelled", String.Join(", ", plugins.Where(p => p.State == PluginState.Cancelled).Select(p => p.GetType().Assembly.GetName().Name).ToArray())));
                        break;
                    case "reload":
                        if (caller!=null && !caller.HasPermission("rocket.reload")) return;
                            R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_reload"));
                            R.Instance.Reload();
                        break;
                }
            }

            if (command.Length == 2)
            {
                IRocketPlugin p = R.Instance.GetPlugins().Where(pl => pl.Name.ToLower().Contains(command[1].ToLower())).FirstOrDefault();
                if (p != null)
                {
                    switch (command[0].ToLower())
                    {
                        case "reload":
                            if (caller != null && !caller.HasPermission("rocket.reloadplugin")) return;
                            if (p.State == PluginState.Loaded)
                            {
                                R.Instance.Implementation.Chat.Say(caller,R.Translate("command_rocket_reload_plugin", p.GetType().Assembly.GetName().Name));
                                p.ReloadPlugin();
                            }
                            else
                            {
                                R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_not_loaded", p.GetType().Assembly.GetName().Name));
                            }
                            break;
                        case "unload":
                            if (caller != null && !caller.HasPermission("rocket.unloadplugin")) return;
                            if (p.State == PluginState.Loaded)
                            {
                                R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_unload_plugin", p.GetType().Assembly.GetName().Name));
                                p.UnloadPlugin();
                            }
                            else
                            {
                                R.Instance.Implementation.Chat.Say(caller,R.Translate("command_rocket_not_loaded", p.GetType().Assembly.GetName().Name));
                            }
                            break;
                        case "load":
                            if (caller != null && !caller.HasPermission("rocket.loadplugin")) return;
                            if (p.State != PluginState.Loaded)
                            {
                                R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_load_plugin", p.GetType().Assembly.GetName().Name));
                                p.LoadPlugin();
                            }
                            else
                            {
                                R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_already_loaded", p.GetType().Assembly.GetName().Name));
                            }
                            break;
                    }
                }
                else
                {
                    R.Instance.Implementation.Chat.Say(caller, R.Translate("command_rocket_plugin_not_found", command[1]));
                }
            }


        }
    }
}