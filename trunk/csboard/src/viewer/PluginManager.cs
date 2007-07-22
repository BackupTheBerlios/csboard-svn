//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Library General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
//
// Copyright (C) 2006 Ravi Kiran UVS

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using Mono.Unix;

namespace CsBoard
{
	namespace Plugin
	{

		public abstract class CsPlugin
		{
			string id;
			string name;
			string description;

			public string Name
			{
				get
				{
					return name;
				}
			}
			public string Description
			{
				get
				{
					return description;
				}
			}
			public string Id
			{
				get
				{
					return id;
				}
			}

			public CsPlugin (string id, string name,
					 string description)
			{
				this.id = id;
				this.name = name;
				this.description = description;
			}

			public abstract bool Initialize ();
			public abstract bool Shutdown ();
		}

		public struct PluginInfo
		{
			Type pluginType;
			CsPlugin plugin;
			bool loaded;
			bool initialized;

			public Type PluginType
			{
				get
				{
					return pluginType;
				}
			}
			public CsPlugin Plugin
			{
				get
				{
					return plugin;
				}
				set
				{
					plugin = value;
				}
			}

			public bool Loaded
			{
				get
				{
					return loaded;
				}
				set
				{
					loaded = value;
				}
			}

			public bool Initialized
			{
				get
				{
					return initialized;
				}
				set
				{
					initialized = value;
				}
			}

			public PluginInfo (Type t)
			{
				pluginType = t;
				plugin = null;
				loaded = false;
				initialized = false;
			}
		}

		public class PluginManager
		{

			string plugins_dir;
			ArrayList plugins;

			public ArrayList Plugins
			{
				get
				{
					return plugins;
				}
			}
			// Plugins in the assembly, always loaded.
			static Type[] stock_plugins = new Type[]{
				typeof (CsBoard.Viewer.PGNFileLoader),
				typeof (CsBoard.Viewer.PGNBufferLoader)
			};

			private static PluginManager pluginManager =
				new PluginManager ("plugins");

			public static PluginManager Instance
			{
				get
				{
					return pluginManager;
				}
			}

			private PluginManager (string plugindir)
			{
				plugins_dir = plugindir;
				plugins = GetPlugins ();
			}

			public void StartPlugins ()
			{
				for (int i = 0; i < plugins.Count; i++)
				  {
					  PluginInfo info =
						  (PluginInfo) plugins[i];
					  if (!info.Loaded)
					    {
						    CsPlugin plugin =
							    (CsPlugin)
							    Activator.
							    CreateInstance
							    (info.PluginType);
						    info.Plugin = plugin;
					    }

					  if (!info.Initialized)
						  info.Initialized =
							  info.Plugin.
							  Initialize ();
					  plugins[i] = info;
				  }
			}

			public void ClosePlugins ()
			{
				for (int i = 0; i < plugins.Count; i++)
				  {
					  PluginInfo info =
						  (PluginInfo) plugins[i];
					  if (!info.Loaded
					      || !info.Initialized)
						  continue;
					  info.Plugin.Shutdown ();
					  info.Initialized = false;
				  }
			}

			ArrayList GetPlugins ()
			{
				ArrayList all_plugin_types = new ArrayList ();

				// Load the stock plugins
				foreach (Type type in stock_plugins)
				{
					all_plugin_types.
						Add (new PluginInfo (type));
				}

				string[]files = null;
				try
				{
					files = Directory.
						GetFiles (plugins_dir,
							  "*.dll");
				}
				catch (Exception e)
				{
					Console.WriteLine (Catalog.
							   GetString
							   ("Exception: \n") +
							   e);
					return all_plugin_types;
				}

				foreach (string file in files)
				{
					try
					{
						ArrayList asm_plugins =
							GetPluginTypesInFile
							(file);
						foreach (Type type in
							 asm_plugins)
						{
							all_plugin_types.
								Add (new
								     PluginInfo
								     (type));
						}
					}
					catch (Exception e)
					{
						Console.WriteLine
							(Catalog.
							 GetString
							 ("Failed to load plugin {0}: {1}"),
							 Path.
							 GetFileName (file),
							 e);
					}
				}

				return all_plugin_types;
			}

			static ArrayList GetPluginTypesInFile (string
							       filepath)
			{
				Assembly asm = Assembly.LoadFrom (filepath);
				return GetPluginTypesInAssembly (asm);
			}

			static ArrayList GetPluginTypesInAssembly (Assembly
								   asm)
			{
				Type[]types = asm.GetTypes ();
				ArrayList asm_plugins = new ArrayList ();

				foreach (Type type in types)
				{
					if (type.BaseType ==
					    typeof (CsPlugin))
					  {
						  asm_plugins.Add (type);
					  }
				}

				return asm_plugins;
			}
		}
	}
}
