using System;
using System.Collections.Generic;
using System.Linq;
#region entities
class Position(int x, int y)
{
    public int x = x;
    public int y = y;

    public int DistanceTo(Position other)
    {
        return Math.Abs(x - other.x) + Math.Abs(y - other.y);
    }

    public void Deconstruct(out int x, out int y)
    {
        x = this.x;
        y = this.y;
    }

    override public string ToString()
    {
        return $"({x}, {y})";
    }
}

class Map
{
    public const char GRASS = '.';
    public const char MY_SHACK = '0';
    public const char OPPONENT_SHACK = '1';
    public const char IRON = '+';
    public const char WATER = '-';
    public const char ROCK = '#';

    public int width;
    public int height;

    public string[] cells;

    public Position myShack;
    public Position opponentShack;

    public List<Position> ironMines = [];

    public List<Position> grassCellsAroundMyShack = [];
    public List<Position> nearWaterCells = [];

    public Map(int width, int height, string[] cells)
    {
        this.width = width;
        this.height = height;

        this.cells = cells;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cells[y][x] == Map.MY_SHACK)
                {
                    myShack = new Position(x, y);
                }
                else if (cells[y][x] == Map.OPPONENT_SHACK)
                {
                    opponentShack = new Position(x, y);
                }
                else if (cells[y][x] == Map.IRON)
                {
                    ironMines.Add(new Position(x, y));
                }
            }
        }

        InitGrassCellsAroundMyShack();
        InitNearWaterCells();
    }

    private void InitGrassCellsAroundMyShack()
    {
        var cellsAroundShack = new List<(int, int)>
        {
            (myShack.x - 1, myShack.y),
            (myShack.x + 1, myShack.y),
            (myShack.x, myShack.y - 1),
            (myShack.x, myShack.y + 1)
        };

        var prioritizedCellsAroundShack = new List<(Position p, int rank)>();
        foreach (var (x, y) in cellsAroundShack)
        {
            if (0 <= x && x < width && 0 <= y && y < height
                && cells[y][x] == Map.GRASS)
            {
                var rank = IsAdjacentToWater(x, y) ? 10 : 0;

                prioritizedCellsAroundShack.Add((new Position(x, y), rank));
            }
        }

        this.grassCellsAroundMyShack = prioritizedCellsAroundShack
            .OrderByDescending(c => c.rank)
            .Select(c => c.p)
            .ToList();
    }

    /// <summary>
    /// Grass cells within 5 cells from my shack and adjacent to water.
    /// </summary>
    private void InitNearWaterCells()
    {
        for (var dx = -5; dx <= 5; dx++)
        {
            for (var dy = -5; dy <= 5; dy++)
            {
                if (dx == 0 && dy == 0)
                {
                    //position of my shack
                    continue;
                }

                var x = myShack.x + dx;
                var y = myShack.y + dy;

                if (0 <= x && x < width && 0 <= y && y < height
                    && cells[y][x] == Map.GRASS
                    && IsAdjacentToWater(x, y))
                {
                    nearWaterCells.Add(new Position(x, y));
                }
            }
        }
    }

    private bool IsAdjacentToWater(int x, int y)
    {
        var adjacentCells = new List<(int, int)>
        {
            (x - 1, y),
            (x + 1, y),
            (x, y - 1),
            (x, y + 1),
        };
        foreach (var (adjX, adjY) in adjacentCells)
        {
            if (0 <= adjX && adjX < width && 0 <= adjY && adjY < height
                && cells[adjY][adjX] == Map.WATER)
            {
                return true;
            }
        }
        return false;
    }
}

class Inventory(int plum, int lemon, int apple, int banana, int iron, int wood)
{
    public int plum = plum;
    public int lemon = lemon;
    public int apple = apple;
    public int banana = banana;
    public int iron = iron;
    public int wood = wood;

    public static Inventory Parse(string description)
    {
        var inputs = description.Split(' ');
        int plum = int.Parse(inputs[0]);
        int lemon = int.Parse(inputs[1]);
        int apple = int.Parse(inputs[2]);
        int banana = int.Parse(inputs[3]);
        int iron = int.Parse(inputs[4]);
        int wood = int.Parse(inputs[5]);
        return new Inventory(plum, lemon, apple, banana, iron, wood);
    }

    public void Pay(Cost cost)
    {
        this.plum -= cost.plum;
        this.lemon -= cost.lemon;
        this.apple -= cost.apple;
        this.iron -= cost.iron;
    }

    internal bool CanPay(Cost cost, out Cost missing)
    {
        missing = new Cost(cost.plum - plum, cost.lemon - lemon, cost.apple - apple, cost.iron - iron);

        return plum >= cost.plum
            && lemon >= cost.lemon
            && apple >= cost.apple
            && iron >= cost.iron;
    }

    internal IDictionary<string, int> ToMap()
    {
        return new Dictionary<string, int>
        {
            { Program.PLUM, plum },
            { Program.LEMON, lemon },
            { Program.APPLE, apple },
            { Program.BANANA, banana },
            { Program.IRON, iron },
            { Program.WOOD, wood }
        };
    }
}

class Troll(int id, int player,
    int x, int y,
    int movementSpeed, int carryCapacity,
    int harvestPower, int chopPower,
    int carryPlum, int carryLemon, int carryApple, int carryBanana, int carryIron, int carryWood) : Position(x, y)
{
    public int id = id;
    public int player = player;
    public int movementSpeed = movementSpeed;
    public int carryCapacity = carryCapacity;

    public int harvestPower = harvestPower;
    public int chopPower = chopPower;

    public int carryPlum = carryPlum;
    public int carryLemon = carryLemon;
    public int carryApple = carryApple;
    public int carryBanana = carryBanana;
    public int carryIron = carryIron;
    public int carryWood = carryWood;

    public int this[string resourceType]
    {
        get
        {
            return resourceType switch
            {
                Program.PLUM => carryPlum,
                Program.LEMON => carryLemon,
                Program.APPLE => carryApple,
                Program.BANANA => carryBanana,
                Program.IRON => carryIron,
                Program.WOOD => carryWood,
                _ => throw new NotSupportedException($"Resource type {resourceType} is not supported")
            };
        }
    }

    public static Troll Parse(string description)
    {
        var inputs = description.Split(' ');

        int id = int.Parse(inputs[0]);
        int player = int.Parse(inputs[1]);
        int x = int.Parse(inputs[2]);
        int y = int.Parse(inputs[3]);
        int movementSpeed = int.Parse(inputs[4]);
        int carryCapacity = int.Parse(inputs[5]);
        int harvestPower = int.Parse(inputs[6]);
        int chopPower = int.Parse(inputs[7]);

        int carryPlum = int.Parse(inputs[8]);
        int carryLemon = int.Parse(inputs[9]);
        int carryApple = int.Parse(inputs[10]);
        int carryBanana = int.Parse(inputs[11]);
        int carryIron = int.Parse(inputs[12]);
        int carryWood = int.Parse(inputs[13]);

        return new Troll(id, player, x, y, movementSpeed, carryCapacity, harvestPower, chopPower,
            carryPlum, carryLemon, carryApple, carryBanana, carryIron, carryWood);
    }

    public bool HasCapacityLeft()
    {
        return (carryPlum + carryLemon + carryApple + carryBanana + carryIron + carryWood) < carryCapacity;
    }
}

class Cost
{
    public int plum;
    public int lemon;
    public int apple;
    public int iron;
    public Cost(int plum, int lemon, int apple, int iron)
    {
        this.plum = plum;
        this.lemon = lemon;
        this.apple = apple;
        this.iron = iron;
    }

    public List<(string type, int qty)> SortByMostRequired()
    {
        var resources = new List<(string type, int qty)>
        {
            (Program.PLUM, plum),
            (Program.LEMON, lemon),
            (Program.APPLE, apple),
            (Program.IRON, iron)
        };

        return resources
            .Where(r => r.qty > 0)
            .OrderByDescending(r => r.qty)
            .ToList();
    }

    public override string ToString()
    {
        return $"plum={plum}, lemon={lemon}, apple={apple}, iron={iron}";
    }
}

class Characteristics
{
    public int moveSpeed;
    public int carryCapacity;
    public int harvestPower;
    public int chopPower;
    public Characteristics(int moveSpeed, int carryCapacity, int harvestPower, int chopPower)
    {
        this.moveSpeed = moveSpeed;
        this.carryCapacity = carryCapacity;
        this.harvestPower = harvestPower;
        this.chopPower = chopPower;
    }

    /// <summary>
    /// PLUM for movementSpeed
    /// LEMON for carryCapacity
    /// APPLE for harvestPower
    /// IRON for chopPower
    /// </summary>
    /// <param name="gameState"></param>
    /// <returns></returns>
    public Cost GetCost(GameState gameState)
    {
        var nbTrolls = gameState.myTrolls.Count;

        return new Cost(
            ComputeCost(nbTrolls, moveSpeed),
            ComputeCost(nbTrolls, carryCapacity),
            ComputeCost(nbTrolls, harvestPower),
            ComputeCost(nbTrolls, chopPower)
        );
    }

    public static int GetHighestCaracteristicPointsForTroll(int nbTroll, int resource)
    {
        return (int)Math.Sqrt(resource - nbTroll);
    }

    private int ComputeCost(int nbTroll, int characteristic)
    {
        return nbTroll + (characteristic * characteristic);
    }
}

class Tree(string type, int x, int y, int size, int health, int fruits, int cooldown) : Position(x, y)
{
    public string type = type;
    public int size = size;
    public int health = health;
    public int fruits = fruits;
    public int cooldown = cooldown;

    public static Tree Parse(string description)
    {
        var inputs = description.Split(' ');
        string type = inputs[0];
        int x = int.Parse(inputs[1]);
        int y = int.Parse(inputs[2]);
        int size = int.Parse(inputs[3]);
        int health = int.Parse(inputs[4]);
        int fruits = int.Parse(inputs[5]);
        int cooldown = int.Parse(inputs[6]);
        return new Tree(type, x, y, size, health, fruits, cooldown);
    }
}

class GameState
{
    const int me = 0;
    const int opponent = 1;

    public Map map;
    public List<Inventory> inventories;
    public List<Tree> trees;

    public List<Troll> myTrolls;
    public List<Troll> enemyTrolls;

    public Inventory myInventory;

    public GameState(Map map, List<Inventory> inventories, List<Tree> trees,
        List<Troll> myTrolls, List<Troll> enemyTrolls)
    {
        this.map = map;
        this.inventories = inventories;
        this.trees = trees;
        this.myTrolls = myTrolls;
        this.enemyTrolls = enemyTrolls;

        this.myInventory = inventories[0];
    }

    public Troll GetMyTroll(int trollId)
    {
        return myTrolls.First(t => t.id == trollId);
    }

}
#endregion

#region Commands
abstract class Command
{
}

class MoveCommand : Command
{
    public int id;
    public int x;
    public int y;

    public MoveCommand(int id, Position p)
    {
        this.id = id;
        this.x = p.x;
        this.y = p.y;
    }

    public override string ToString()
    {
        return $"MOVE {id} {x} {y}";
    }
}

class HarvestCommand : Command
{
    public int Id { get; set; }
    public HarvestCommand(int id)
    {
        Id = id;
    }
    public override string ToString()
    {
        return $"HARVEST {Id}";
    }
}

class PlantCommand : Command
{
    public int id;
    public string type;

    /// <summary>
    /// type = "PLUM", "LEMON", "APPLE", "BANANA"
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    public PlantCommand(int id, string type)
    {
        this.id = id;
        this.type = type;
    }

    public override string ToString()
    {
        return $"PLANT {id} {type}";
    }
}

class ChopCommand : Command
{
    public int id;
    public ChopCommand(int id)
    {
        this.id = id;
    }
    public override string ToString()
    {
        return $"CHOP {id}";
    }
}

class PickCommand : Command
{
    public int id;
    public string type;
    /// <summary>
    public PickCommand(int id, string type)
    {
        this.id = id;
        this.type = type;
    }
    public override string ToString()
    {
        return $"PICK {id} {type}";
    }
}

class DropCommand : Command
{
    public int Id { get; set; }
    public DropCommand(int id)
    {
        Id = id;
    }
    public override string ToString()
    {
        return $"DROP {Id}";
    }
}

class TrainCommand : Command
{
    public int moveSpeed;
    public int carryCapacity;
    public int harvestPower;
    public int chopPower;

    public TrainCommand(Characteristics characteristics)
    {
        this.moveSpeed = characteristics.moveSpeed;
        this.carryCapacity = characteristics.carryCapacity;
        this.harvestPower = characteristics.harvestPower;
        this.chopPower = characteristics.chopPower;
    }

    public override string ToString()
    {
        return $"TRAIN {moveSpeed} {carryCapacity} {harvestPower} {chopPower}";
    }
}

class MineCommand : Command
{
    public int id;
    public MineCommand(int id)
    {
        this.id = id;
    }
    public override string ToString()
    {
        return $"MINE {id}";
    }
}

class WaitCommand : Command
{
    public override string ToString()
    {
        return "WAIT";
    }
}

#endregion

abstract class TrollTask
{
    static int taskCounter = 1;

    public int id;
    public int trollId;

    public TrollTask(int trollId)
    {
        id = taskCounter++;
        this.trollId = trollId;
    }

    public abstract (Command, bool isCompleted) Run(GameState gameState);
}

class HarvestTask : TrollTask
{
    enum State
    {
        MovingToTree,
        Harvesting,
        MovingToShack,
    }

    public string type;
    State state;
    public Position treePosition;

    public HarvestTask(int trollId, string type, Position treePosition) : base(trollId)
    {
        this.state = State.MovingToTree;
        this.treePosition = treePosition;
        this.type = type;
    }

    public override (Command, bool isCompleted) Run(GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);

        Program.Debug($"Running HarvestTask for troll {trollId} at tree position {treePosition}, current state: {state}");

        switch (state)
        {
            case State.MovingToTree:
                if (me.DistanceTo(treePosition) == 0)
                {
                    state = State.Harvesting;
                    goto case State.Harvesting;
                }

                // Move towards the tree
                return (new MoveCommand(me.id, treePosition), isCompleted: false);

            case State.Harvesting:
                var hasCapacityLeft = me.HasCapacityLeft();
                if (!hasCapacityLeft)
                {
                    // Troll is full, need to go back to the shack to drop resources
                    state = State.MovingToShack;
                    goto case State.MovingToShack;
                }

                var matchingTree = gameState.trees.FirstOrDefault(t => t.DistanceTo(me) == 0);
                if (matchingTree is null || matchingTree.fruits == 0)
                {
                    //The tree is gone or has no fruits, task is completed
                    return (new MoveCommand(me.id, gameState.map.myShack), isCompleted: true);
                }

                // Harvest fruits from the tree
                return (new HarvestCommand(me.id), isCompleted: false);

            case State.MovingToShack:
                var myShack = gameState.map.myShack;
                if (me.DistanceTo(myShack) == 1)
                {
                    // Drop items at the shack
                    return (new DropCommand(me.id), isCompleted: true);
                }

                // Otherwise, move towards the shack
                return (new MoveCommand(me.id, myShack), isCompleted: false);
        }

        throw new InvalidOperationException("Invalid state");
    }

    public override string ToString()
    {
        return $"[HarvestTask] ({trollId}) Type: {type} Position: {treePosition}";
    }

}

class MineTask : TrollTask
{
    enum State
    {
        MovingToMine,
        Mining,
        MovingToShack,
    }

    Position minePosition;
    State state;

    public MineTask(int trollId, Position minePosition) : base(trollId)
    {
        this.state = State.MovingToMine;
        this.minePosition = minePosition;
    }

    public override (Command, bool isCompleted) Run(GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);

        Program.Debug($"Running MineTask for troll {trollId} at position {minePosition}, current state: {state}");

        switch (state)
        {
            case State.MovingToMine:
                if (me.DistanceTo(minePosition) > 1)
                {
                    // Move towards the mine
                    return (new MoveCommand(me.id, minePosition), isCompleted: false);
                }
                else
                {
                    state = State.Mining;
                    goto case State.Mining;
                }
            case State.Mining:
                var hasCapacityLeft = me.HasCapacityLeft();
                if (hasCapacityLeft)
                {
                    // Keep mining
                    return (new MineCommand(me.id), isCompleted: false);
                }
                else
                {
                    state = State.MovingToShack;
                    goto case State.MovingToShack;
                }
            case State.MovingToShack:
                var myShack = gameState.map.myShack;
                if (me.DistanceTo(myShack) == 1)
                {
                    // Drop items at the shack
                    return (new DropCommand(me.id), isCompleted: true);
                }
                else
                {
                    // Move towards the shack
                    return (new MoveCommand(me.id, myShack), isCompleted: false);
                }
        }

        throw new InvalidOperationException("Invalid state");
    }

    public override string ToString()
    {
        return $"[MineTask] ({trollId}), Position: {minePosition}";
    }
}

class PlantTask : TrollTask
{
    enum State
    {
        MovingToShack,
        PickingFruit,
        MovingToPlantingPosition,
        Planting
    }

    public string type;
    public Position position;
    private State state;

    public PlantTask(int trollId, string type, Position p) : base(trollId)
    {
        this.type = type;
        this.position = p;
        this.state = State.MovingToShack;
    }

    public override (Command, bool isCompleted) Run(GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);

        Program.Debug($"Running PlantTask for troll {trollId} at position {position}, current state: {state}");

        switch (state)
        {
            case State.MovingToShack:
                if (me.DistanceTo(gameState.map.myShack) == 1)
                {
                    state = State.PickingFruit;
                    goto case State.PickingFruit;
                }
                else if (me.DistanceTo(gameState.map.myShack) == 0)
                {
                    //Case the troll is just trained and is on the shack.
                    // Need to move him out of the shack to be able to pick the fruit
                    var grassAroundShack = gameState.map.grassCellsAroundMyShack;
                    var random = new Random();
                    return (new MoveCommand(me.id, grassAroundShack[random.Next(grassAroundShack.Count)]), isCompleted: false);
                }
                else
                {
                    // Move to the shack to pick the fruit
                    return (new MoveCommand(me.id, gameState.map.myShack), isCompleted: false);
                }

            case State.PickingFruit:
                state = State.MovingToPlantingPosition;
                return (new PickCommand(me.id, type), isCompleted: false);

            case State.MovingToPlantingPosition:
                if (me.DistanceTo(position) == 0)
                {
                    return (new PlantCommand(me.id, type), isCompleted: true);
                }
                else
                {
                    return (new MoveCommand(me.id, position), isCompleted: false);
                }
        }

        throw new InvalidOperationException("Invalid state");
    }

    public override string ToString()
    {
        return $"[PlantTask] ({trollId}) Type: {type} Position: {position}";
    }
}

class ChopTask : TrollTask
{
    enum State
    {
        MovingToTree,
        Chopping,
        MovingToShack,
    }
    State state;

    public Position treePosition;

    public ChopTask(int trollId, Position treePosition) : base(trollId)
    {
        this.treePosition = treePosition;
        state = State.MovingToTree;
    }
    public override (Command, bool isCompleted) Run(GameState gameState)
    {
        Program.Debug($"Running ChopTask for troll {trollId} at tree position {treePosition}, current state: {state}");

        var me = gameState.GetMyTroll(trollId);
        switch (state)
        {
            case State.MovingToTree:
                if (me.DistanceTo(treePosition) == 0)
                {
                    state = State.Chopping;
                    goto case State.Chopping;
                }
                else
                {
                    return (new MoveCommand(me.id, treePosition), isCompleted: false);
                }

            case State.Chopping:
                var matchingTree = gameState.trees.FirstOrDefault(t => t.DistanceTo(me) == 0);
                if (matchingTree is null || matchingTree.size == 0)
                {
                    //The tree is gone, task is completed
                    state = State.MovingToShack;
                    goto case State.MovingToShack;
                }
                // Chop the tree
                return (new ChopCommand(me.id), isCompleted: false);

            case State.MovingToShack:
                if (me.DistanceTo(gameState.map.myShack) == 1)
                {
                    return (new DropCommand(me.id), isCompleted: true);
                }
                else
                {
                    return (new MoveCommand(me.id, gameState.map.myShack), isCompleted: false);
                }
        }

        throw new InvalidOperationException("Invalid state");
    }
    public override string ToString()
    {
        return $"[ChopTask] ({trollId}) Position: {treePosition}";
    }
}

static class Player
{
    public static Dictionary<int, TrollTask?> tasks = [];

    public static void SetWorkers(List<Troll> myTrolls)
    {
        foreach (var troll in myTrolls)
        {
            if (!tasks.ContainsKey(troll.id))
            {
                tasks[troll.id] = null;
            }
        }
    }

    public static List<Command> Play(GameState gameState)
    {
        var commands = new List<Command>();

        var trainCommand = TrainManager.TrainCommand(gameState);
        if (trainCommand is not null)
        {
            commands.Add(trainCommand);
        }

        foreach (var (trollId, task) in tasks)
        {
            if (task is null)
            {
                //Troll is idle, need to assign a task
                tasks[trollId] = TaskSelector.SelectTask(gameState, trollId, tasks);
                Program.Debug($"Assigned task {tasks[trollId]}");
            }

            //Program.Debug($"Running task {tasks[trollId]}");
            var (command, isCompleted) = tasks[trollId]!.Run(gameState);

            if (isCompleted)
            {
                tasks[trollId] = null;
            }

            commands.Add(command);
        }

        return commands;
    }
}

static class BananaPlantManager
{
    //Ensure we have 2 bananas around the shack
    public static TrollTask? GetPlantTask(GameState gameState, int trollId, Dictionary<int, TrollTask?> tasksInProgress)
    {
        var inventory = gameState.myInventory;
        if (inventory.banana <= 0)
            return null;

        var troll = gameState.GetMyTroll(trollId);
        var myShack = gameState.map.myShack;

        var nearBananaTrees = gameState.trees
            .Count(t => t.type == Program.BANANA && t.DistanceTo(myShack) <= 2);

        if (nearBananaTrees >= 2)
            return null;

        foreach (var candidatePosition in gameState.map.grassCellsAroundMyShack)
        {
            var tree = gameState.trees.FirstOrDefault(t => t.DistanceTo(candidatePosition) == 0);
            if (tree is null)
            {
                var alreadyPlannedToPlantThere = tasksInProgress.Values.OfType<PlantTask>()
                    .Any(t => t.position.DistanceTo(candidatePosition) == 0);

                if (alreadyPlannedToPlantThere == false)
                {
                    inventory.banana--;
                    return new PlantTask(trollId, Program.BANANA, candidatePosition);
                }
            }
        }

        return null;
    }
}

static class PlantManager
{
    //Ensure we have a APPLE, PLUM and LEMON around the shack.
    public static PlantTask? GetPlantTask(GameState gameState, int trollId, Dictionary<int, TrollTask> tasksInProgress)
    {
        var troll = gameState.GetMyTroll(trollId);

        var hasApple = false;
        var hasPlum = false;
        var hasLemon = false;

        List<Position> availablePositions = [];

        Program.Debug($"[PlantManager] Checking for plant task around the shack {gameState.map.grassCellsAroundMyShack.Count}");

        foreach (var position in gameState.map.grassCellsAroundMyShack)
        {
            if (tasksInProgress.Values.OfType<PlantTask>().Any(t => t.position.DistanceTo(position) == 0))
            {
                //There is already a task to plant on this position, we consider it unavailable for planting other trees
                continue;
            }

            var tree = gameState.trees.FirstOrDefault(t => t.DistanceTo(position) == 0);
            if (tree is not null)
            {
                switch (tree.type)
                {
                    case Program.APPLE:
                        hasApple = true;
                        break;
                    case Program.PLUM:
                        hasPlum = true;
                        break;
                    case Program.LEMON:
                        hasLemon = true;
                        break;
                }
            }
            else
            {
                availablePositions.Add(position);
            }
        }

        if (availablePositions.Count == 0)
        {
            Program.Debug("[PlantManager] No available position to plant new tree");
            return null;
        }

        var nextPlantPosition = availablePositions[0];
        var myInventory = gameState.myInventory;

        if (hasApple == false)
        {
            var alreadyPlannedToPlantApple = tasksInProgress.Values
                .OfType<PlantTask>()
                .Any(t => t.type == Program.APPLE);

            if (alreadyPlannedToPlantApple == false && myInventory.apple > 0)
            {
                myInventory.apple--;
                return new PlantTask(trollId, Program.APPLE, nextPlantPosition);
            }
        }

        if (hasPlum == false)
        {
            var alreadyPlannedToPlantPlum = tasksInProgress.Values
                .OfType<PlantTask>()
                .Any(t => t.type == Program.PLUM);

            if (alreadyPlannedToPlantPlum == false && myInventory.plum > 0)
            {
                myInventory.plum--;
                return new PlantTask(trollId, Program.PLUM, nextPlantPosition);
            }
        }

        if (hasLemon == false)
        {
            var alreadyPlannedToPlantPlum = tasksInProgress.Values
                .OfType<PlantTask>()
                .Any(t => t.type == Program.LEMON);

            if (alreadyPlannedToPlantPlum == false && myInventory.lemon > 0)
            {
                myInventory.lemon--;
                return new PlantTask(trollId, Program.LEMON, nextPlantPosition);
            }
        }

        Program.Debug("[PlantManager] Have all trees around the shack or no inventory to plant missing trees");

        return null;
    }
}

static class TreeChoper
{
    // Chop banana trees of size 4
    public static TrollTask? GetTreeChopTask(GameState gameState, int trollId, Dictionary<int, TrollTask> tasksInProgress)
    {
        var troll = gameState.GetMyTroll(trollId);
        var myshack = gameState.map.myShack;

        if (troll.chopPower == 0)
        {
            //Can't chop trees, no need to assign a chopping task
            return null;
        }

        if (troll.chopPower > 1)
        {
            // chop at enemy shack
            var enemyShack = gameState.map.opponentShack;
            var treesAroundEnemyShack = gameState.trees
                .Where(t => t.DistanceTo(enemyShack) <= 3)
                .OrderBy(t => t.health)
                .FirstOrDefault();
            if (treesAroundEnemyShack is not null)
            {
                Program.Debug($"[TreeChoper] Assigning ChopTask to troll {trollId} to chop tree around enemy shack at position {treesAroundEnemyShack}");
                return new ChopTask(trollId, treesAroundEnemyShack);
            }
        }

        var bananaTreesToChop = gameState.trees
            .Where(t => t.type == Program.BANANA && t.size == 4 && t.DistanceTo(myshack) < 3)
            .OrderBy(t => t.DistanceTo(troll))
            .FirstOrDefault();

        if (bananaTreesToChop is not null)
        {
            return new ChopTask(trollId, bananaTreesToChop);
        }

        //try other trees if there is no banana tree to chop
        var otherTreesToChop = gameState.trees
            .Where(t => t.size == 4)
            .OrderBy(t => t.DistanceTo(troll))
            .FirstOrDefault();

        if (otherTreesToChop is not null)
        {
            return new ChopTask(trollId, otherTreesToChop);
        }

        return null;
    }
}

static class TrainManager
{
    public static TrainCommand? TrainCommand(GameState gameState)
    {
        var trollToTrain = GetCharacteristics(gameState);
        if (trollToTrain is null)
        {
            return null;
        }

        var myInventory = gameState.myInventory;
        var cost = trollToTrain.GetCost(gameState);

        var canPay = myInventory.CanPay(cost, out Cost missing);
        if (canPay == false)
        {
            return null;
        }

        var trainCommand = new TrainCommand(trollToTrain);
        myInventory.Pay(cost);
        return trainCommand;
    }

    private static Characteristics? GetCharacteristics(GameState gameState)
    {
        var trollsCount = gameState.myTrolls.Count;

        var nextTroll = trollsCount + 1;

        switch (nextTroll)
        {
            case 2:
                //return highest possible troll
                var highestMoveSpeed = Characteristics.GetHighestCaracteristicPointsForTroll(1, gameState.myInventory.plum);
                var highestCarryCapacity = Characteristics.GetHighestCaracteristicPointsForTroll(1, gameState.myInventory.lemon);
                var highestChopPower = Characteristics.GetHighestCaracteristicPointsForTroll(1, gameState.myInventory.iron);
                var highestHarvestPower = Characteristics.GetHighestCaracteristicPointsForTroll(1, gameState.myInventory.apple);

                return new Characteristics(highestMoveSpeed, highestCarryCapacity, highestHarvestPower, highestChopPower);
            case 3:
                // troll count = 2
                // move speed = 2:      cost = 2 + (2*2) = 6 plums
                // carry capacity = 4:  cost = 2 + (4*4) = 18 lemons
                // chop power = 2:      cost = 2 + (2*2) = 6 irons
                // harvest power = 2:   cost = 2 + (2*2) = 6 apples
                return new Characteristics(2, 4, 2, 2);  // 3rd troll
            default:
                return null;
        }
    }

    public static Cost? GetRequiredCost(GameState gameState)
    {
        var characteristics = GetCharacteristics(gameState);

        if (characteristics is null)
        {
            Program.Debug("[TrainManager] No more troll to train");
            return null;
        }
        return characteristics.GetCost(gameState);
    }
}

static class ResourceManager
{
    public static TrollTask? SelectTask(GameState gameState, int trollId, Dictionary<int, TrollTask?> tasksInProgress)
    {
        // Which resource ?
        // Ask train manager which troll to train next, and based on that, we know which resource we need the most
        var trainCost = TrainManager.GetRequiredCost(gameState);

        Program.Debug($"[ResourceManager] Train cost: plum={trainCost?.plum}, lemon={trainCost?.lemon}, apple={trainCost?.apple}, iron={trainCost?.iron}");
        if (trainCost is null)
        {
            return GetDefaultHarvestTask(gameState, trollId, tasksInProgress);
        }

        if (gameState.myInventory.CanPay(trainCost, out Cost missingResources) == false)
        {
            Program.Debug($"[ResourceManager] missing resources: {missingResources}");
            var myShack = gameState.map.myShack;
            var requiredTypes = missingResources.SortByMostRequired();
            var troll = gameState.GetMyTroll(trollId);

            foreach (var (type, qty) in requiredTypes)
            {
                if (type == Program.IRON && troll.chopPower > 0)
                {
                    if (tasksInProgress.Values.OfType<MineTask>().Any() == false)
                    {
                        // No troll is currently mining, we can assign a mining task to this troll
                        var closestMineToMyShack = gameState.map.ironMines
                            .OrderBy(m => m.DistanceTo(myShack))
                            .First();

                        return new MineTask(trollId, closestMineToMyShack);
                    }
                    Program.Debug($"[ResourceManager] Can't assign MineTask to troll {trollId} because another troll is already mining");
                }
                else
                {
                    if (tasksInProgress.Values.OfType<HarvestTask>().Any(t => t.type == type) == true)
                    {
                        Program.Debug($"[ResourceManager] Can't assign HarvestTask of type {type} to troll {trollId} because another troll is already harvesting this type");
                        continue;
                    }

                    //No task on this type in progress
                    var treeWithFruitsOfType = gameState.trees
                        .Where(t => t.type == type && t.fruits > 0)
                        .OrderBy(t => t.DistanceTo(myShack))
                        .ToList();

                    Program.Debug($"[ResourceManager] Found {treeWithFruitsOfType.Count} trees with fruits of type {type} for troll {trollId} to harvest");

                    foreach (var tree in treeWithFruitsOfType)
                    {
                        var treeIsFree = false == tasksInProgress.Values
                            .OfType<HarvestTask>()
                            .Any(t => t.treePosition.x == tree.x && t.treePosition.y == tree.y);
                        if (treeIsFree)
                        {
                            return new HarvestTask(trollId, type, tree);
                        }
                    }

                    Program.Debug($"[ResourceManager] No available tree with fruits of type {type} to assign HarvestTask to troll {trollId}");

                    var treeWithUpcomingFruitsOfType = gameState.trees
                        .Where(t => t.type == type)
                        .OrderBy(t => t.cooldown)
                        .FirstOrDefault();
                    if (treeWithUpcomingFruitsOfType is not null)
                    {
                        return new HarvestTask(trollId, type, treeWithUpcomingFruitsOfType);
                    }

                    Program.Debug($"[ResourceManager] No tree with fruits of type {type} or upcoming fruits to assign HarvestTask to troll {trollId}");
                }
            }
        }

        return null;
    }
    private static TrollTask? GetDefaultHarvestTask(GameState gameState, int trollId, Dictionary<int, TrollTask?> tasksInProgress)
    {
        var troll = gameState.GetMyTroll(trollId);
        var myShack = gameState.map.myShack;
        var myResources = gameState.myInventory.ToMap()
                .Where(x => x.Key == Program.PLUM || x.Key == Program.LEMON || x.Key == Program.APPLE || x.Key == Program.IRON)
                .OrderBy(x => x.Value);

        foreach (var (type, amount) in myResources)
        {
            if (type == Program.IRON && troll.chopPower > 0)
            {
                if (tasksInProgress.Values.OfType<MineTask>().Any() == false)
                {
                    // No troll is currently mining, we can assign a mining task to this troll
                    var closestMineToMyShack = gameState.map.ironMines
                        .OrderBy(m => m.DistanceTo(myShack))
                        .First();

                    return new MineTask(trollId, closestMineToMyShack);
                }
            }
            else
            {
                if (tasksInProgress.Values.OfType<HarvestTask>().Any(t => t.type == type) == false)
                {
                    //No task on this type in progress
                    var treeWithFruitsOfType = gameState.trees
                        .Where(t => t.type == type && t.fruits > 0)
                        .OrderBy(t => t.DistanceTo(myShack))
                        .ToList();

                    foreach (var tree in treeWithFruitsOfType)
                    {
                        var treeIsFree = false == tasksInProgress.Values
                            .OfType<HarvestTask>()
                            .Any(t => t.treePosition.x == tree.x && t.treePosition.y == tree.y);
                        if (treeIsFree)
                        {
                            return new HarvestTask(trollId, type, tree);
                        }
                    }
                }
            }
        }

        return null;
    }
}

static class TaskSelector
{
    public static TrollTask SelectTask(GameState gameState, int trollId, Dictionary<int, TrollTask?> tasksInProgress)
    {
        var troll = gameState.GetMyTroll(trollId);

        var plantTask = PlantManager.GetPlantTask(gameState, trollId, tasksInProgress);
        if (plantTask is not null)
        {
            Program.Debug("PlantManager assigns task");
            return plantTask;
        }

        if (gameState.myTrolls.Count < 3)
        {
            var resourceTask = ResourceManager.SelectTask(gameState, trollId, tasksInProgress);
            if (resourceTask is not null)
            {
                Program.Debug("ResourceManager assigns task");
                return resourceTask;
            }
        }

        var bananaPlantTask = BananaPlantManager.GetPlantTask(gameState, trollId, tasksInProgress);
        if (bananaPlantTask is not null)
        {
            Program.Debug("BananaPlantManager assigns task");
            return bananaPlantTask;
        }


        var chopTask = TreeChoper.GetTreeChopTask(gameState, trollId, tasksInProgress);
        if (chopTask is not null)
        {
            Program.Debug("TreeChoper assigns task");
            return chopTask;
        }

        Program.Debug("Default task");
        var closestTree = gameState.trees
            .Where(t => t.fruits > 0)
            .OrderBy(m => m.DistanceTo(troll))
            .FirstOrDefault();


        return new HarvestTask(trollId, closestTree.type, closestTree);
    }
}

public class Program
{
    public const string PLUM = "PLUM";
    public const string LEMON = "LEMON";
    public const string APPLE = "APPLE";
    public const string BANANA = "BANANA";
    public const string IRON = "IRON";
    public const string WOOD = "WOOD";

    public static void Debug(string text)
    {
        Console.Error.WriteLine(text);
    }

    public static string ReadLine()
    {
        string line = Console.ReadLine();
        //Debug(line);
        return line;
    }

    public static void Main(string[] args)
    {
        string[] inputs;
        inputs = ReadLine().Split(' ');
        int width = int.Parse(inputs[0]);
        int height = int.Parse(inputs[1]);
        string[] cells = new string[height];
        for (int i = 0; i < height; i++)
        {
            string line = ReadLine();
            cells[i] = line;
        }
        var map = new Map(width, height, cells);
        var turn = 0;

        // game loop
        while (true)
        {
            turn++;

            var inventories = new List<Inventory>(2);
            for (int i = 0; i < 2; i++)
            {
                inventories.Add(Inventory.Parse(ReadLine()));
            }
            int treesCount = int.Parse(ReadLine());
            var trees = new List<Tree>(treesCount);

            for (int i = 0; i < treesCount; i++)
            {
                trees.Add(Tree.Parse(ReadLine()));
            }
            int trollsCount = int.Parse(ReadLine());

            var myTrolls = new List<Troll>(trollsCount);
            var enemyTrolls = new List<Troll>(trollsCount);

            for (int i = 0; i < trollsCount; i++)
            {
                var troll = Troll.Parse(ReadLine());
                if (troll.player == 0)
                    myTrolls.Add(troll);
                else
                    enemyTrolls.Add(troll);
            }

            Player.SetWorkers(myTrolls);

            var gameState = new GameState(map, inventories, trees, myTrolls, enemyTrolls);

            var commands = Player.Play(gameState);

            var output = string.Join(";", commands.Select(c => c.ToString()));

            Console.WriteLine(output);
        }
    }
}