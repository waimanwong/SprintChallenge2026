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
    }

    private void InitGrassCellsAroundMyShack()
    {
        var cellsAroundShack = new List<(int, int)>
        {
            (myShack.x - 1, myShack.y - 1),
            (myShack.x - 1, myShack.y + 1),
            (myShack.x + 1, myShack.y - 1),
            (myShack.x + 1, myShack.y + 1),

            (myShack.x - 1, myShack.y),
            (myShack.x + 1, myShack.y),
            (myShack.x, myShack.y - 1),
            (myShack.x, myShack.y + 1),

        };
        foreach (var (x, y) in cellsAroundShack)
        {
            if (0 <= x && x < width && 0 <= y && y < height
                && cells[y][x] == Map.GRASS)
            {
                grassCellsAroundMyShack.Add(new Position(x, y));
            }
        }
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

    State state;
    public Position targetTreePosition;

    public HarvestTask(int trollId, Position treePosition) : base(trollId)
    {
        this.state = State.MovingToTree;
        this.targetTreePosition = treePosition;
    }

    public override string ToString()
    {
        return $"[{id}] Harvest at {targetTreePosition}";
    }

    public override (Command, bool isCompleted) Run(GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);

        switch (state)
        {
            case State.MovingToTree:
                if (me.DistanceTo(targetTreePosition) == 0)
                {
                    state = State.Harvesting;
                    goto case State.Harvesting;
                }

                // Move towards the tree
                return (new MoveCommand(me.id, targetTreePosition), isCompleted: false);

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

        var basicTroll = new Characteristics(1, 1, 1, 0);

        if (CanTrainTroll(gameState, basicTroll, out Cost requiredResources))
        {
            commands.Add(new TrainCommand(basicTroll));
        }

        foreach (var (trollId, task) in tasks)
        {
            if (task is null)
            {
                //Troll is idle, need to assign a task
                tasks[trollId] = TaskSelector.SelectTask(gameState, trollId, tasks);
            }

            Program.Debug($"Running task {task}");
            var (command, isCompleted) = tasks[trollId]!.Run(gameState);

            if (isCompleted)
            {
                tasks[trollId] = null;
            }

            commands.Add(command);
        }

        return commands;
    }

    private static bool CanTrainTroll(GameState gameState, Characteristics desiredCharacteristics,
        out Cost requiredResources)
    {
        var inventory = gameState.inventories[0];
        var cost = desiredCharacteristics.GetCost(gameState);

        var canPay = inventory.CanPay(cost, out Cost missing);
        requiredResources = missing;

        Program.Debug($"Missing resources: {missing.plum} PLUM, {missing.lemon} LEMON, {missing.apple} APPLE, {missing.iron} IRON");

        return canPay;
    }
}

static class ResourceManager
{
    public static TrollTask? SelectTask(GameState gameState, int trollId, Dictionary<int, TrollTask?> tasksInProgress)
    {
        var troll = gameState.GetMyTroll(trollId);
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
                    var closestMine = gameState.map.ironMines
                        .OrderBy(m => m.DistanceTo(troll))
                        .First();

                    return new MineTask(trollId, closestMine);
                }
            }
            else
            {
                var treeWithFruitsOfType = gameState.trees
                    .Where(t => t.type == type && t.fruits > 0)
                    .OrderBy(t => t.DistanceTo(troll))
                    .ToList();

                foreach (var tree in treeWithFruitsOfType)
                {
                    var treeIsFree = false == tasksInProgress.Values
                        .OfType<HarvestTask>()
                        .Any(t => t.targetTreePosition.x == tree.x && t.targetTreePosition.y == tree.y);
                    if (treeIsFree)
                    {
                        return new HarvestTask(trollId, tree);
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

        // For now, only harvest task is implemented, so we directly return a harvest task
        var task = ResourceManager.SelectTask(gameState, trollId, tasksInProgress);
        if (task is not null)
        {
            return task;
        }

        //Default task when no tree with fruits is available is to mine, we select the closest mine
        var closestMine = gameState.map.ironMines
            .OrderBy(m => m.DistanceTo(troll))
            .First();

        return new MineTask(trollId, closestMine);
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

        // game loop
        while (true)
        {
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