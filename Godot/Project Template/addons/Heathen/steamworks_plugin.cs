using Godot;
using System;

[Tool]
public class steamworks_plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        AddAutoloadSingleton("SteamworksBehaviour", "res://addons/Heathen/SteamworksBehaviour.cs");
    }

    public override void _ExitTree()
    {
        ProjectSettings.Clear("Heathen/Steamworks/App Id");
        RemoveAutoloadSingleton("SteamworksBehaviour");
    }
}