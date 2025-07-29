using System.Collections.Generic;
using Godot;

public partial class UI : Node
{
    [Signal]
    public delegate void DialogueContinuedEventHandler();

    private DialogueHandler _dialogueHandler;
    private HealthUi _healthUi;
    private PanelContainer _pauseMenu;
    private QueueVisual _queue;
    private StoryManager _story;
    private TimeLine _timeLine;
    private PanelContainer _gameOverPanel;
    private PanelContainer _tutorialFinishedPanel;
    private Label _scoreLabel;


    public override void _Ready()
    {
        base._Ready();
        _dialogueHandler = GetNode<DialogueHandler>("Scaler/DialogueHandler");
        _pauseMenu = GetNode<PanelContainer>("Scaler/PauseMenu");
        _timeLine = GetNode<TimeLine>("Scaler/TimeLine");
        _queue = GetNode<QueueVisual>("Scaler/QueueVisual");
        _healthUi = GetNode<HealthUi>("Scaler/HealthUI");
        _gameOverPanel = GetNode<PanelContainer>("Scaler/GameOverMenu");
        _tutorialFinishedPanel = GetNode<PanelContainer>("Scaler/TutorialEndMenu");
        _scoreLabel = GetNode<Label>("Scaler/Score");
    }

    public void Initialize()
    {
        if (World.Instance.IsEndlessWorld)
        {
            _scoreLabel.Show();
        }
        _story = new StoryManager();
        _story.Start();
    }

    private void DisplayDialogue(DialogueCard dialogue)
    {
        _dialogueHandler.DisplayDialogue(dialogue);
    }

    public async void DisplayLongDialogue(List<DialogueCard> dialogue)
    {
        foreach (var dialogueCard in dialogue)
        {
            DisplayDialogue(dialogueCard);
            await ToSignal(this, "DialogueContinued");
        }

        _dialogueHandler.Hide();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventKey eventKey)
            if (eventKey.IsActionPressed("continue_dialogue"))
                EmitSignal("DialogueContinued");
    }

    private void OnPauseButtonPressed()
    {
        _pauseMenu.Show();
        Game.Instance.Paused = true;
    }

    public void OnTutorialEnd()
    {
        _tutorialFinishedPanel.Show();
        Game.Instance.Paused = true;
    }

    public void OnGameOver()
    {
        _gameOverPanel.Show();
        Game.Instance.Paused = true;
    }
    
    private void OnResumeButtonPressed()
    {
        _pauseMenu.Hide();
        Game.Instance.Paused = false;
    }

    private void OnExitButtonPressed()
    {
        World.Instance.Entities.SaveScore();
        Game.Instance.ToMainMenu();
    }

    public void UpdateTimeLine()
    {
        _timeLine.Update();
    }

    public void UpdateQueue()
    {
        _queue.Update();
    }

    public void UpdateHealthBar()
    {
        _healthUi.Update();
    }

    private void OnDialogueClicked()
    {
        EmitSignal("DialogueContinued");
    }

    public void UpdateScore(int score)
    {
        _scoreLabel.Text = "Score: " + score.ToString();
    }
}