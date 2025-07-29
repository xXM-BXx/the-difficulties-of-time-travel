using System.Linq;
using Godot;

public partial class QueueVisual : Container
{
    [Export] public int MaxItemsInQueue = 10;

    public void Update()
    {
        var queue = World.Instance.Entities.Queue;
        ClearItems();
        var newItemScene = GD.Load<PackedScene>("scenes/ui/QueueItem.tscn");
        for (var i = 0; i < queue.Count; i++)
            if (newItemScene.Instantiate() is QueueItem newItemInstance)
            {
                newItemInstance.ZIndex = 0;
                var isFirst = i == 0;
                newItemInstance.Initialize(queue[i], isFirst);
                if (i >= MaxItemsInQueue) newItemInstance.Hide();
                AddChild(newItemInstance);
            }
    }

    private void ClearItems()
    {
        if (GetChildCount() > 0)
        {
            var children = GetChildren().OfType<QueueItem>();
            foreach (var child in children)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }
    }
}