using System.Reflection.Emit;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace ConsoleMacros;

// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

public class ModEntry : Mod
{
    public static void Log(string v, LogLevel logLevel = LogLevel.Debug)
    {
        _log.Log(v, logLevel);
    }
    
    public static IMonitor _log = null!;
    public static IManifest Manifest = null!;
    public static string assetName = null!;
    public static Dictionary<string, string> macros = new();
    
    public override void Entry(IModHelper helper)
    {
        _log = Monitor;
        Manifest = ModManifest;
        assetName = Manifest.UniqueID + "/Macros";

        Helper.Events.Content.AssetRequested += OnAssetRequested;
        Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

        helper.ConsoleCommands.Add("macro",
            "Base command for SMAPI macro execution and debugging. See: \"macro run\", \"macro list\", \"macro reload\".",
            Macro);
    }

    private static void RefreshMacros()
    {
        string[] files = Directory.GetFiles("Mods/ConsoleMacros/Macros", "*.*");

        foreach (var file in files)
        {
            string fileName = new FileInfo(file).Name.Split(".").SkipLast(1).Join(delimiter: ".");
            macros[fileName] = File.ReadAllText(file);
        }

        Dictionary<string, string> contentSource = Game1.content.Load<Dictionary<string, string>>(assetName);
        
        // https://stackoverflow.com/a/10559415
        macros = macros.Concat(contentSource)
            .GroupBy(v => v.Key)
            .ToDictionary(v => v.Key, v => v.First().Value);
        
        Log($"Loaded {macros.Count} macros:" +
            $"\n {contentSource.Count} added via Content Patcher" +
            $"\n {macros.Count - contentSource.Count} added via Macros folder."
        );
    }

    public void Macro(string command, string[] args)
    {
        switch (args.Length != 0 ? args[0] : "")
        {
            case "run": 
                MacroRun(args);
                break;
            case "list":
                MacroList();
                break;
            case "reload":
                RefreshMacros();
                break;
            default:
                Log(
                    "Base command for SMAPI macro execution and debugging.\n\nCommands:\n macro run <macro>\n macro list\n");
                break;
        }
    }

    public static void MacroRun(string[] args)
    {
        List<string> commands = new();

        foreach (var macro in args)
        {
            string? thisMacro = macros.GetValueOrDefault(macro);
            if (thisMacro is null)
            {
                thisMacro = macros.GetValueOrDefault(macro.ToLower());
            }
            if (thisMacro is null) continue;

            commands.AddRange(thisMacro.Split("\n"));
        }

        foreach (var cmd in commands)
        {
            try
            {
                ExecuteCommand(cmd);
            }
            catch
            {
                // Should never happen. Just in case.
                Log($"Failed to execute command: \"{cmd}\"", LogLevel.Error);
            }
        }
    }

    public static void MacroList()
    {
        Log($"Available macros:\n\n {macros.Keys.Join(delimiter:", ")}");
    }
    
    // https://gist.github.com/Shockah/ec111245868ee9b7dbf2ca2928dd2896
    private static readonly Lazy<Action<string>> AddToRawCommandQueue = new(() =>
    {
        var scoreType = AccessTools.TypeByName("StardewModdingAPI.Framework.SCore, StardewModdingAPI")!;
        var commandQueueType = AccessTools.TypeByName("StardewModdingAPI.Framework.CommandQueue, StardewModdingAPI")!;
        var scoreGetter = AccessTools.PropertyGetter(scoreType, "Instance")!;
        var rawCommandQueueField = AccessTools.Field(scoreType, "RawCommandQueue")!;
        var commandQueueAddMethod = AccessTools.Method(commandQueueType, "Add");
        var dynamicMethod = new DynamicMethod("AddToRawCommandQueue", null, new Type[] { typeof(string) });
        var il = dynamicMethod.GetILGenerator();
        il.Emit(OpCodes.Call, scoreGetter);
        il.Emit(OpCodes.Ldfld, rawCommandQueueField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, commandQueueAddMethod);
        il.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<Action<string>>();
    });
    
    // https://gist.github.com/Shockah/ec111245868ee9b7dbf2ca2928dd2896
    private static void ExecuteCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
            return;
        AddToRawCommandQueue.Value(command);
    }
    
    private static void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        RefreshMacros();
    }

    private static void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        RefreshMacros();
    }
    
    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(assetName))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }
}