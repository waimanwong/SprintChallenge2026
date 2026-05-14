using System;
using System.Collections.Generic;
using System.Linq;

class Position(int x, int y)
{
    public int x = x;
    public int y = y;

    public int DistanceTo(Position other)
    {
        return Math.Abs(x - other.x) + Math.Abs(y - other.y);
    }
}

class Map
{
    public int width;
    public int height;

    public string[] cells;

    public Position myShack;
    public Position opponentShack;

    public List<Position> ironMines = [];

    public Map(int width, int height, string[] cells)
    {
        this.width = width;
        this.height = height;

        this.cells = cells;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cells[y][x] == '0')
                {
                    myShack = new Position(x, y);
                }
                else if (cells[y][x] == '1')
                {
                    opponentShack = new Position(x, y);
                }
                else if (cells[y][x] == '+')
                {
                    ironMines.Add(new Position(x, y));
                }
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

    public void Pay((int plum, int lemon, int apple, int iron) cost)
    {
        this.plum -= cost.plum;
        this.lemon -= cost.lemon;
        this.apple -= cost.apple;
        this.iron -= cost.iron;
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

    public bool HasFruits()
    {
        return carryPlum > 0 || carryLemon > 0 || carryApple > 0 || carryBanana > 0 || carryIron > 0 || carryWood > 0;
    }

    public bool HasCapacityLeft()
    {
        return (carryPlum + carryLemon + carryApple + carryBanana + carryIron + carryWood) < carryCapacity;
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

    public GameState(Map map, List<Inventory> inventories, List<Tree> trees,
        List<Troll> myTrolls, List<Troll> enemyTrolls)
    {
        this.map = map;
        this.inventories = inventories;
        this.trees = trees;
        this.myTrolls = myTrolls;
        this.enemyTrolls = enemyTrolls;
    }

    public Troll GetMyTroll(int trollId)
    {
        return myTrolls.First(t => t.id == trollId);
    }
}
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
        id = id;
        type = type;
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

    public TrainCommand(int moveSpeed, int carryCapacity, int harvestPower, int chopPower)
    {
        this.moveSpeed = moveSpeed;
        this.carryCapacity = carryCapacity;
        this.harvestPower = harvestPower;
        this.chopPower = chopPower;
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


static class TaskManager
{
    public static Queue<Task> pendingTasks = [];
    public static Queue<NoTrollTask> noTrollTasks = [];

    public static Dictionary<int, Task?> tasksPerTroll = [];

    public static void SetWorkers(IEnumerable<Troll> trolls)
    {
        foreach (var troll in trolls)
        {
            if (tasksPerTroll.ContainsKey(troll.id))
            {
                continue;
            }

            tasksPerTroll[troll.id] = null;
        }
    }

    public static void AddTaskForNoTroll(NoTrollTask task)
    {
        noTrollTasks.Enqueue(task);
    }

    public static void AddTaskForTrolls(Task task)
    {
        pendingTasks.Enqueue(task);
    }

    public static List<Command> GetCommands(GameState gameState)
    {
        var commands = new List<Command>();

        while (noTrollTasks.Count > 0)
        {
            var noTrollTask = noTrollTasks.Dequeue();
            var command = noTrollTask.Run();
            commands.Add(command);
        }

        foreach (var trollId in tasksPerTroll.Keys)
        {
            if (tasksPerTroll[trollId] == null)
            {
                if (pendingTasks.Count > 0)
                {
                    tasksPerTroll[trollId] = pendingTasks.Dequeue();
                }
                else
                {
                    tasksPerTroll[trollId] = new ChopTask();
                }
            }

            var (command, isCompleted) = tasksPerTroll[trollId]!.Run(trollId, gameState);
            commands.Add(command);

            if (isCompleted)
            {
                tasksPerTroll[trollId] = null;
            }
        }

        return commands;
    }
}

abstract class Task
{
    static int taskCounter = 1;

    public int id;

    public Task()
    {
        id = taskCounter++;
    }

    public abstract (Command, bool isCompleted) Run(int trollId, GameState gameState);
}

class MineTask : Task
{
    public int amount;

    public MineTask(int amount)
    {
        this.amount = amount;
    }

    public override (Command, bool isCompleted) Run(int trollId, GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);
        var hasCapacityLeft = me.HasCapacityLeft();

        if (hasCapacityLeft)
        {
            //If adjacent to a mine, mine it
            var adjacentMine = gameState.map.ironMines.FirstOrDefault(t => t.DistanceTo(me) == 1);
            if (adjacentMine != null)
            {
                return (new MineCommand(me.id), isCompleted: false);
            }

            // Otherwise, move to the nearest mine
            var nearestMine = gameState.map.ironMines
                .OrderBy(t => t.DistanceTo(me))
                .FirstOrDefault();

            return (new MoveCommand(me.id, nearestMine), isCompleted: false);
        }
        else
        {
            // If we are next to the shack, drop items
            var myShack = gameState.map.myShack;

            if (me.DistanceTo(myShack) == 1)
            {
                return (new DropCommand(me.id), isCompleted: true);
            }

            // Otherwise, move to the shack
            return (new MoveCommand(me.id, myShack), isCompleted: false);
        }
    }
}

class HarvestTask : Task
{
    public string resourceType;
    public int amount;

    public HarvestTask(string resourceType, int amount)
    {
        this.resourceType = resourceType;
        this.amount = amount;
    }

    public override string ToString()
    {
        return $"[{id}] Harvest {amount} of {resourceType}";
    }

    public override (Command, bool isCompleted) Run(int trollId, GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);
        var hasCapacityLeft = me.HasCapacityLeft();

        if (hasCapacityLeft)
        {
            // If we are on a tree and still have capacity left, harvest it
            var matchingTree = gameState.trees.FirstOrDefault(t => t.type == resourceType && t.DistanceTo(me) == 0);
            if (matchingTree != null && matchingTree.fruits > 0)
            {
                return (new HarvestCommand(me.id), isCompleted: false);
            }

            // Otherwise, move to the nearest tree
            var nearestTreeWithFruits = gameState.trees
                .Where(t => t.type == resourceType)
                .OrderBy(t => t.DistanceTo(me))
                .FirstOrDefault();

            if (nearestTreeWithFruits == null)
            {
                return (new WaitCommand(), isCompleted: true);
            }

            return (new MoveCommand(me.id, nearestTreeWithFruits), isCompleted: false);
        }
        else
        {
            //Capacity full, need to drop items

            var myShack = gameState.map.myShack;
            if (me.DistanceTo(myShack) == 1)
            {
                return (new DropCommand(me.id), isCompleted: true);
            }

            // Otherwise, move to the shack
            return (new MoveCommand(me.id, myShack), isCompleted: false);
        }
    }
}

class ChopTask : Task
{
    public override string ToString()
    {
        return $"[{id}] Chop tree";
    }

    public override (Command, bool isCompleted) Run(int trollId, GameState gameState)
    {
        var me = gameState.GetMyTroll(trollId);
        var hasCapacityLeft = me.HasCapacityLeft();

        if (hasCapacityLeft)
        {
            // If we are on a tree and still have capacity left, chop it
            var matchingTree = gameState.trees.FirstOrDefault(t => t.DistanceTo(me) == 0);
            if (matchingTree != null)
            {
                return (new ChopCommand(me.id), isCompleted: false);
            }

            // Otherwise, move to the nearest tree
            var nearestTree = gameState.trees
                .OrderBy(t => t.DistanceTo(me))
                .FirstOrDefault();

            if (nearestTree == null)
            {
                return (new WaitCommand(), isCompleted: true);
            }

            return (new MoveCommand(me.id, nearestTree), isCompleted: false);
        }
        else
        {
            //Capacity full, need to drop items

            var myShack = gameState.map.myShack;
            if (me.DistanceTo(myShack) == 1)
            {
                return (new DropCommand(me.id), isCompleted: true);
            }

            // Otherwise, move to the shack
            return (new MoveCommand(me.id, myShack), isCompleted: false);
        }
    }
}

abstract class NoTrollTask
{
    public abstract Command Run();
}

class TrainTask : NoTrollTask
{
    public int moveSpeed;
    public int carryCapacity;
    public int harvestPower;
    public int chopPower;
    public TrainTask(int moveSpeed, int carryCapacity, int harvestPower, int chopPower)
    {
        this.moveSpeed = moveSpeed;
        this.carryCapacity = carryCapacity;
        this.harvestPower = harvestPower;
        this.chopPower = chopPower;
    }

    public (int plum, int lemon, int apple, int iron) GetCost(int existingTrolls)
    {
        return (
            existingTrolls + (moveSpeed * moveSpeed),
            existingTrolls + (carryCapacity * carryCapacity),
            existingTrolls + (harvestPower * harvestPower),
            existingTrolls + (chopPower * chopPower)
        );
    }

    public override Command Run()
    {
        return new TrainCommand(moveSpeed, carryCapacity, harvestPower, chopPower);
    }
}

abstract class Objective
{
    protected bool isInited = false;

    public void Start(GameState gameState)
    {
        if (!isInited)
        {
            Init(gameState);
            isInited = true;
        }
    }

    protected abstract void Init(GameState gameState);

    public abstract bool IsCompleted(GameState gameState);
}

class ReachInventory : Objective
{
    public int minPlum;
    public int minLemon;
    public int minApple;
    public int minIron;

    public ReachInventory(int minPlum, int minLemon, int minApple, int minIron)
    {
        this.minPlum = minPlum;
        this.minLemon = minLemon;
        this.minApple = minApple;
        this.minIron = minIron;
    }

    protected override void Init(GameState gameState)
    {
        var commands = new List<Command>();
        var myInventory = gameState.inventories[0];

        // Check if we need to reach for any resources
        if (myInventory.plum < minPlum)
        {
            for (int i = 0; i < minPlum - myInventory.plum; i++)
                TaskManager.AddTaskForTrolls(new HarvestTask("PLUM", 1));
        }
        if (myInventory.lemon < minLemon)
        {
            for (int i = 0; i < minLemon - myInventory.lemon; i++)
                TaskManager.AddTaskForTrolls(new HarvestTask("LEMON", 1));
        }
        if (myInventory.apple < minApple)
        {
            for (int i = 0; i < minApple - myInventory.apple; i++)
                TaskManager.AddTaskForTrolls(new HarvestTask("APPLE", 1));
        }
        if (myInventory.iron < minIron)
        {
            for (int i = 0; i < minIron - myInventory.iron; i++)
                TaskManager.AddTaskForTrolls(new MineTask(1));
        }
    }

    public override bool IsCompleted(GameState gameState)
    {
        var myInventory = gameState.inventories[0];

        var isCompleted = true;

        // Check if we need to reach for any resources
        if (myInventory.plum < minPlum)
        {
            isCompleted = false;
        }
        if (myInventory.lemon < minLemon)
        {
            isCompleted = false;
        }
        if (myInventory.apple < minApple)
        {
            isCompleted = false;
        }
        if (myInventory.iron < minIron)
        {
            isCompleted = false;
        }

        return isCompleted;
    }

    public override string ToString()
    {
        return "ReachInventory";
    }
}

class TrainTroll : Objective
{
    public int moveSpeed;
    public int carryCapacity;
    public int harvestPower;
    public int chopPower;

    public TrainTroll(int moveSpeed, int carryCapacity, int harvestPower, int chopPower)
    {
        this.moveSpeed = moveSpeed;
        this.carryCapacity = carryCapacity;
        this.harvestPower = harvestPower;
        this.chopPower = chopPower;
    }

    protected override void Init(GameState gameState)
    {
        var task = new TrainTask(moveSpeed, carryCapacity, harvestPower, chopPower);
        TaskManager.AddTaskForNoTroll(task);

        var cost = task.GetCost(gameState.myTrolls.Count);
        gameState.inventories[0].Pay(cost);
    }

    public override bool IsCompleted(GameState gameState)
    {
        return gameState.myTrolls.Any(t => t.movementSpeed == moveSpeed
            && t.carryCapacity == carryCapacity
            && t.harvestPower == harvestPower
            && t.chopPower == chopPower);
    }

    public override string ToString()
    {
        return "TrainTroll";
    }
}
/// <summary>
/// Plays a script of the game
/// </summary>
class Script
{
    private Queue<Objective> objectives = new Queue<Objective>();

    public Script()
    {
        objectives.Enqueue(new ReachInventory(2, 2, 2, 2));
        objectives.Enqueue(new TrainTroll(1, 1, 1, 1));
        objectives.Enqueue(new ReachInventory(3, 3, 3, 3));
        objectives.Enqueue(new TrainTroll(1, 1, 1, 1));
        objectives.Enqueue(new ReachInventory(4, 4, 4, 4));
        objectives.Enqueue(new TrainTroll(1, 1, 1, 1));
    }

    public void Play(GameState gameState)
    {
        while (objectives.Count > 0)
        {
            var objective = objectives.Peek();
            objective.Start(gameState);

            if (objective.IsCompleted(gameState) == false)
            {
                return;
            }
            else
            {
                objectives.Dequeue();
            }
        }
    }
}

public class Program
{
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

        var script = new Script();

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

            TaskManager.SetWorkers(myTrolls);

            var gameState = new GameState(map, inventories, trees, myTrolls, enemyTrolls);

            script.Play(gameState);

            var commands = TaskManager.GetCommands(gameState);

            var output = string.Join(";", commands.Select(c => c.ToString()));

            Console.WriteLine(output);
        }
    }
}