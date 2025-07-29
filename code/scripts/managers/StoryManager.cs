using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public record struct DialogueCard(int SpeakerImageId, string SpeakerName, string DialogueText)
{
}

public partial class StoryManager : Node
{
    private readonly List<StoryPoint> _story = new()
    {
        new StoryPoint(() => Task.CompletedTask, () =>
        {
            World.Instance.Ui.DisplayLongDialogue(new List<DialogueCard>
                {
                    new(TileLookUp.PlayerId, "Scientist",
                        "Finally my greatest Invention is complete: The Time Machine. Now to test if it works.\n" +
                        "(Scroll the mouse wheel to first preview and then move to another time)")
                }
            );
            return Task.CompletedTask;
        }),
        new StoryPoint(async () => { await World.Instance.Entities.ToSignal(World.Instance.Entities, "LayerChanged"); },
            () =>
            {
                World.Instance.Entities.Player.CanTimeTravel = false;
                World.Instance.Entities.Player.CanMove = true;
                World.Instance.Ui.DisplayLongDialogue(new List<DialogueCard>
                    {
                        new(TileLookUp.PlayerId, "Scientist",
                            "Wow! It really worked. Let's have a look around in the past.\n" +
                            "(Move to one of the marked tiles, by clicking it)")
                    }
                );
                return Task.CompletedTask;
            }),
        new StoryPoint(async () => { await World.Instance.Entities.ToSignal(World.Instance.Entities, "PlayerMoved"); },
            async () =>
            {
                HexCoords spawn = new(0, -2);
                if (World.Instance.Entities.GetCharacterIdAtPosition(spawn) != -1)
                    await World.Instance.Entities.AnimatedMove(spawn, new HexCoords(0, -1),
                        new List<HexCoords> { spawn, new(0, -1) });
                World.Instance.Entities.Player.CanMove = false;
                await World.Instance.Entities.SpawnEnemy(spawn, TileLookUp.GenericEnemyId);
                World.Instance.Entities.Player.CanTimeTravel = true;
                World.Instance.Ui.DisplayLongDialogue(new List<DialogueCard>
                    {
                        new(TileLookUp.GenericEnemyId, "???",
                            "Stop right there. I am from the Agency for Time Travel."),
                        new(TileLookUp.GenericEnemyId, "ATT Officer",
                            "We noticed you completed a not authorized jump in time. You should not be doing that, or you may cause a some temporal anomalies."),
                        new(TileLookUp.PlayerId, "Scientist", "I should probably get away from here.\n" +
                                                              "(Scroll to return to the present)")
                    }
                );
            }),
        new StoryPoint(
            async () => { await World.Instance.Entities.ToSignal(World.Instance.Entities, "AllEnemiesDefeated"); },
            async () =>
            {
                World.Instance.Entities.Player.CanMove = true;
                World.Instance.Entities.SpawnEnemy(new HexCoords(-1, -2), TileLookUp.GenericEnemyId);
                await World.Instance.Entities.SpawnEnemy(new HexCoords(1, -3), TileLookUp.GenericEnemyId);
                World.Instance.Ui.DisplayLongDialogue(new List<DialogueCard>
                    {
                        new(TileLookUp.GenericEnemyId, "Other ATT Officer",
                            "Hey Bob, sorry we are late. We ... lost track of time."),
                        new(TileLookUp.GenericEnemyId, "Other ATT Officer",
                            "Oh my god, he's dead. You killed him. You will pay for that!")
                    }
                );
            }),
        new StoryPoint(
            async () => { await World.Instance.Entities.ToSignal(World.Instance.Entities, "AllEnemiesDefeated"); },
            () =>
            {
                World.Instance.Entities.DoorsEnabled = true;
                World.Instance.Ui.DisplayLongDialogue(new List<DialogueCard>
                    {
                        new(TileLookUp.PlayerId, "Scientist",
                            "That doesn't look good for me. I should probably leave."),
                    }
                );
                return Task.CompletedTask;
            }),
        new StoryPoint(
            async () => { await World.Instance.ToSignal(World.Instance, "DoorEntered"); },
            async () =>
            {
                await World.Instance.Entities.SpawnEnemy(new(0, 0), TileLookUp.GenericEnemyId);
            })
    };
    
    private readonly List<StoryPoint> _endlessStory = new()
    {
        new StoryPoint(() => Task.CompletedTask, () =>
        {
            World.Instance.Entities.Player.CanMove = true;
            World.Instance.Ui.DisplayLongDialogue(new List<DialogueCard>
                {
                    new(TileLookUp.PlayerId, "Scientist",
                        "I am under constant attack by the ATT. Surely they will give up at some point?\n"),
                    new(-1, "Endless Mode",
                    "In Endless Mode your goal is to survive as long as possible."),
                    new(-1, "Randomized Levels",
                    "Your tampering in the timeline causes the times you are not currently in to change.")
                }
            );
            return Task.CompletedTask;
        })
    };
    

    public async void Start()
    {
        if (World.Instance.IsEndlessWorld == false)
        {
            foreach (var storyPoint in _story)
            {
                await storyPoint.WaitFor();
                await storyPoint.ThenDo();
            }
        }
        else
        {
            foreach (var storyPoint in _endlessStory)
            {
                await storyPoint.WaitFor();
                await storyPoint.ThenDo();
            }
        }
        
    }

    public record struct StoryPoint(Func<Task> WaitFor, Func<Task> ThenDo)
    {
    }
}