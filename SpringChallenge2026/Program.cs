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

    public List<Troll> trolls;
    public GameState(Map map, List<Inventory> inventories, List<Tree> trees, List<Troll> trolls)
    {
        this.map = map;
        this.inventories = inventories;
        this.trees = trees;
        this.trolls = trolls;
    }

    public Troll GetMe()
    {
        return trolls[me];
    }
}

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

class WaitCommand : Command
{
    public override string ToString()
    {
        return "WAIT";
    }
}


/// <summary>
/// Go to the nearest tree and harvest it, then go to the shack and drop the fruits, repeat.
/// </summary>
class BasicBot
{
    private GameState gameState;

    public BasicBot(GameState gameState)
    {
        this.gameState = gameState;
    }

    public Command GetCommand(GameState gameState)
    {
        var me = gameState.GetMe();
        var hasCapacityLeft = me.HasCapacityLeft();

        if (hasCapacityLeft)
        {

            // If we are on a tree and still have capacity left, harvest it
            var matchingTree = gameState.trees.FirstOrDefault(t => t.DistanceTo(me) == 0);
            if (matchingTree != null && matchingTree.fruits > 0 && hasCapacityLeft)
            {
                return new HarvestCommand(me.id);
            }

            // Otherwise, move to the nearest tree
            var nearestTreeWithFruits = gameState.trees
                .Where(t => t.fruits > 0)
                .OrderBy(t => t.DistanceTo(me))
                .FirstOrDefault();

            if (nearestTreeWithFruits == null)
            {
                return new WaitCommand();
            }

            return new MoveCommand(me.id, nearestTreeWithFruits);
        }
        else
        {

            // If we are next to the shack and we have fruits, drop them
            var myShack = gameState.map.myShack;

            if (me.DistanceTo(myShack) == 1 && me.HasFruits())
            {
                return new DropCommand(me.id);
            }

            // Otherwise, move to the shack
            return new MoveCommand(me.id, myShack);
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

            var trolls = new List<Troll>(trollsCount);
            for (int i = 0; i < trollsCount; i++)
            {
                trolls.Add(Troll.Parse(ReadLine()));
            }

            var gameState = new GameState(map, inventories, trees, trolls);

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // valid actions:
            // MOVE <id> <x> <y>
            // HARVEST <id> - when you are on the same cell as a tree
            // DROP <id> - when you are next to your shack and carry items
            var basicBot = new BasicBot(gameState);

            var command = basicBot.GetCommand(gameState);

            Console.WriteLine(command.ToString());
        }
    }
}