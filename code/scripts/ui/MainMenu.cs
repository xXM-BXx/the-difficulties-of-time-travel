using Godot;

public partial class MainMenu : Node2D
{
    private void OnStartTutorial()
    {
        Game.Instance.StartTutorial();
    }

    private void OnStartEndless()
    {
        Game.Instance.StartEndless();
    }
    
    private void OnCloseGame()
    {
        Game.Instance.CloseGame();
    }

    public void Initialize()
    {
        var score = Game.LoadScore();
        if (score != 0)
        {
            GetNode<Label>("VBoxContainer/Button3/HighScore").Show();
            GetNode<Label>("VBoxContainer/Button3/HighScore").Text = "Highscore:" + score;
        }
    }
}