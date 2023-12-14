using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Digger.Architecture;

namespace Digger;

public class Terrain : ICreature
{
    public string GetImageFileName()
    {
        return "Terrain.png";
    }

    public int GetDrawingPriority()
    {
        return 1;
    }

    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand { DeltaX = 0, DeltaY = 0 };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return true;
    }
}

public class Player : ICreature
{
    public string GetImageFileName()
    {
        return "Digger.png";
    }

    public int GetDrawingPriority()
    {
        return 0;
    }

    public CreatureCommand Act(int x, int y)
    {
        return Game.KeyPressed switch
        {
            Key.Up when y - 1 >= 0 && Game.Map[x, y - 1] is not Sack =>
                new CreatureCommand { DeltaX = 0, DeltaY = -1 },
            Key.Right when x + 1 < Game.MapWidth && Game.Map[x + 1, y] is not Sack =>
                new CreatureCommand { DeltaX = 1, DeltaY = 0 },
            Key.Down when y + 1 < Game.MapHeight && Game.Map[x, y + 1] is not Sack =>
                new CreatureCommand { DeltaX = 0, DeltaY = 1 },
            Key.Left when x - 1 >= 0 && Game.Map[x - 1, y] is not Sack =>
                new CreatureCommand { DeltaX = -1, DeltaY = 0 },
            _ => new CreatureCommand()
        };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return conflictedObject is Sack or Monster;
    }
}

public class Sack : ICreature
{
    private int countSteps;

    public string GetImageFileName()
    {
        return "Sack.png";
    }

    public int GetDrawingPriority()
    {
        return 2;
    }

    public CreatureCommand Act(int x, int y)
    {
        if (y + 1 < Game.MapHeight)
        {
            var next = Game.Map[x, y + 1];
            if (next == null || countSteps > 0 && next is Player or Monster)
            {
                countSteps++;
                return new CreatureCommand { DeltaX = 0, DeltaY = 1 };
            }
        }

        if (y == Game.MapHeight || countSteps > 1)
            return new CreatureCommand { TransformTo = new Gold() };
        countSteps = 0;
        return new CreatureCommand();
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return false;
    }
}

public class Gold : ICreature
{
    public string GetImageFileName()
    {
        return "Gold.png";
    }

    public int GetDrawingPriority()
    {
        return 3;
    }

    public CreatureCommand Act(int x, int y)
    {
        return new CreatureCommand();
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        if (conflictedObject is Player)
            Game.Scores += 10;

        return true;
    }
}

class Monster : ICreature
{
    public string GetImageFileName()
    {
        return "Monster.png";
    }

    public int GetDrawingPriority()
    {
        return 4;
    }

    public CreatureCommand Act(int x, int y)
    {
        if (!TryFindPlayerOnMap(out var playerX, out var playerY))
            return new CreatureCommand();

        var stepVariants = CheckStepVariants(x, y);
        if (stepVariants.Count <= 1)
            return new CreatureCommand { DeltaX = stepVariants[0].Item1, DeltaY = stepVariants[0].Item2 };
        var bestStep = stepVariants[0];
        var bestWayLengthToPlayer = int.MaxValue;
        foreach (var (deltaX, deltaY) in stepVariants)
        {
            var wayLengthToPlayer = Math.Abs(playerX - (x + deltaX)) + Math.Abs(playerY - (y + deltaY));
            if (wayLengthToPlayer < bestWayLengthToPlayer)
            {
                bestStep = (deltaX, deltaY);
                bestWayLengthToPlayer = wayLengthToPlayer;
            }
        }

        return new CreatureCommand { DeltaX = bestStep.Item1, DeltaY = bestStep.Item2 };
    }

    public bool DeadInConflict(ICreature conflictedObject)
    {
        return conflictedObject is Sack or Monster;
    }

    private static bool TryFindPlayerOnMap(out int x, out int y)
    {
        for (var i = 0; i < Game.MapWidth; i++)
        for (var j = 0; j < Game.MapHeight; j++)
            if (Game.Map[i, j] is Player)
            {
                (x, y) = (i, j);
                return true;
            }

        (x, y) = (0, 0);
        return false;
    }

    private static List<(int, int)> CheckStepVariants(int x, int y)
    {
        var result = new List<(int, int)> { (0, 0) };
        if (x - 1 >= 0 && (Game.Map[x - 1, y] == null || Game.Map[x - 1, y] is Gold or Player))
            result.Add((-1, 0));
        if (x + 1 < Game.MapWidth && (Game.Map[x + 1, y] == null || Game.Map[x + 1, y] is Gold or Player))
            result.Add((1, 0));
        if (y - 1 >= 0 && (Game.Map[x, y - 1] == null || Game.Map[x, y - 1] is Gold or Player))
            result.Add((0, -1));
        if (y + 1 < Game.MapHeight && (Game.Map[x, y + 1] == null || Game.Map[x, y + 1] is Gold or Player))
            result.Add((0, 1));
        return result;
    }
}