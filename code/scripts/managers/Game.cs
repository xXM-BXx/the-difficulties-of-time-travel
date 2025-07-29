using System.Linq;
using DimensionJump.scripts.managers;
using Godot;

public partial class Game : Node
{
    public bool Paused;
    public static Game Instance { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
        AudioServer.SetBusVolumeDb(2, -18);
        ToMainMenu();
    }

    public void ToMainMenu()
    {
        Paused = false;
        ClearChildren();
        var mainMenuScene = GD.Load<PackedScene>(PathLookups.MainMenuPath);
        var mainMenuInstance = mainMenuScene.Instantiate() as MainMenu;
        AddChild(mainMenuInstance);
        mainMenuInstance?.Initialize();
    }

    public void StartTutorial()
    {
        ClearChildren();
        var worldScene = GD.Load<PackedScene>(PathLookups.WorldScenePath);
        var worldInstance = worldScene.Instantiate();
        AddChild(worldInstance);
    }
    
    public void StartEndless()
    {
        ClearChildren();
        var worldScene = GD.Load<PackedScene>(PathLookups.EndlessWorldScenePath);
        var worldInstance = worldScene.Instantiate();
        AddChild(worldInstance);
    }

    public void CloseGame()
    {
        GetTree().Quit();
    }

    private void ClearChildren()
    {
        var children = GetChildren().Where(child => child is not AudioStreamPlayer).ToList();
        if (children.Count > 0)
            foreach (var child in children)
            {
                RemoveChild(child);
                child.QueueFree();
            }
    }


    public static void SaveScore(int score)
    {
        var config = new ConfigFile();
        config.SetValue("SaveData", "HighScore", score);

        var err = config.Save("user://save_data.cfg");

        if (err != Error.Ok)
            GD.PrintErr("Failed to save score!");
    }
    
    public static int LoadScore()
    {
        var config = new ConfigFile();
        var err = config.Load("user://save_data.cfg");

        if (err != Error.Ok)
            return 0; // Default score if file doesn't exist

        return (int)config.GetValue("SaveData", "HighScore", 0);
    }
}