using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.IO;

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

			public PluginInfo (Type t)
			{
				pluginType = t;
				plugin = null;
				loaded = false;
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
					  if (info.Loaded)
						  continue;
					  CsPlugin plugin =
						  (CsPlugin) Activator.
						  CreateInstance (info.
								  PluginType);
					  info.Plugin = plugin;
					  info.Loaded = plugin.Initialize ();
					  plugins[i] = info;
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
					Console.WriteLine ("Exception: \n" +
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
							("Failed to load plugin {0}: {1}",
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
				bool found_one = false;

				foreach (Type type in types)
				{
					if (type.BaseType ==
					    typeof (CsPlugin))
					  {
						  asm_plugins.Add (type);
						  found_one = true;
					  }
				}

				return asm_plugins;
			}
		}
	}
}
