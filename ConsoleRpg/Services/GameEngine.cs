﻿using ConsoleRpg.Helpers;
using ConsoleRpgEntities.Data;
using ConsoleRpgEntities.Models.Attributes;
using ConsoleRpgEntities.Models.Characters;
using ConsoleRpgEntities.Models.Characters.Monsters;
using ConsoleRpgEntities.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsoleRpg.Services;

public class GameEngine
{
    private readonly GameContext _context;
    private readonly MenuManager _menuManager;
    private readonly OutputManager _outputManager;

    private IPlayer _player;
    private IMonster _goblin;

    public GameEngine(GameContext context, MenuManager menuManager, OutputManager outputManager)
    {
        _menuManager = menuManager;
        _outputManager = outputManager;
        _context = context;
    }

    public void Run()
    {
        if (_menuManager.ShowMainMenu())
        {
            SetupGame();
        }
    }

    private void GameLoop()
    {
        _outputManager.Clear();

        while (true)
        {
            _outputManager.WriteLine("Choose an action:", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Attack");
            _outputManager.WriteLine("2. Search Inventory");
            _outputManager.WriteLine("3. Search Inventory by Type");
            _outputManager.WriteLine("4. Sort Items");
            _outputManager.WriteLine("5. Choose Items");
            _outputManager.WriteLine("0. Quit");

            _outputManager.Display();

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    AttackCharacter();
                    break;
                case "2":
                    SearchInventory();
                    break;
                case "3":
                    ListItemsByType();
                    break;
                case "4":
                    SortItems();
                    break;
                case "5":
                    ChooseItems();
                    break;
                case "0":
                    _outputManager.WriteLine("Exiting game...", ConsoleColor.Red);
                    _outputManager.Display();
                    Environment.Exit(0);
                    break;
                default:
                    _outputManager.WriteLine("Invalid selection. Please choose 1.", ConsoleColor.Red);
                    break;
            }
        }
    }

    public void SearchInventory()
    {
    Console.WriteLine("Search by Name: ");
    string input = Console.ReadLine();
     
    var items = _context.Items.Where(item => item.Name.Contains(input));    
               
        foreach (var item in items)
        {
            Console.WriteLine($"Item: {item.Name}\tType:{item.Type}");
        }
    }

    public void ListItemsByType()
    {
        var itemsByType = _context.Items
            .GroupBy(t => t.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToList();
        foreach (var item in itemsByType)
        {
            Console.WriteLine($"{item.Type}\t{item.Count}");
        }
    }

    public void SortItems()      
    {
        Console.WriteLine("\nSort Options:");
        Console.WriteLine("1. Sort by Name");
        Console.WriteLine("2. Sort by Attack Value");
        Console.WriteLine("3. Sort by Defense Value");

        var input = Console.ReadLine();

        switch (input)
        {
            case "1":
                Console.WriteLine("Sort by Name");
                SortByName();
                break;
            case "2":
                Console.WriteLine("Sort by Attack Value");
                SortByAttackValue();
                break;
            case "3":
                Console.WriteLine("Sort by Defense Vaue");
                SortByDefenseValue();
                break;
            default:
                Console.WriteLine("Invalid Selection");
                break;
        }
    }
    private void SortByName()
    {
        var allItems = _context.Items.OrderBy(i => i.Name);
        foreach (var item in allItems)
        {
            Console.WriteLine($"\n{item.Name}");
        }
    }
    private void SortByAttackValue()
    {
        var allItems = _context.Items.OrderBy(i => i.Attack);
        foreach (var item in allItems)
        {
            Console.WriteLine($"\n{item.Name}\t{item.Attack}");
        }
    }
    private void SortByDefenseValue()
    {
        var allItems = _context.Items.OrderBy(i => i.Defense);
        foreach (var item in allItems)
        {
            Console.WriteLine($"\n{item.Name}\t{item.Defense}");
        }
    }

    public void ChooseItems()
    {
        AddItem("Weapon");
        AddItem("Armor");

    }

    public void AddItem(string itemType)
    {
        bool addItem;

        var query = from p in _context.Players
            join i in _context.Items on p.Id equals i.PlayerId into itemGroup
            from i in itemGroup.DefaultIfEmpty()
            select new { player = p, item = i };
        
        var itemLookup = from q in query
                   where q.item.Type == itemType && q.item.PlayerId == 1
                   select q;

        
        if (!itemLookup.Any()) // Check if itemLookup is empty
        {
            Console.WriteLine($"You have no items of type {itemType}");
            addItem = true;
        }
        else
        {
            Console.WriteLine($"Do you want to add another item of type {itemType}? (Y/N)");
            var input = Console.ReadLine().ToUpper()[0];

            if (input == 'Y')
            {
                Console.WriteLine($"You've chosen to add an additional {itemType}.");
                addItem = true;
            }
            else
            {
                Console.WriteLine($"You've chosen not to add an additional {itemType}.");
                addItem = false;
            }

        }     

        if (addItem)
        {
            Console.WriteLine($"Let's add an item of type {itemType}");
            Console.WriteLine($"Name the {itemType} you'd like to add: ");
            var itemToAdd = Console.ReadLine();

            var finditem = from i in _context.Items
                   where i.Type == itemType && i.Name == itemToAdd
                   select i;
            
            var itemHasNoPlayer = from f in finditem
                    where f.PlayerId == null
                    select f;
                    
            bool itemNotAdded = true;

            while (itemNotAdded)
            {
                // Check the item exists
                if (finditem.Any())
                {
                    System.Console.WriteLine($"This is a valid {itemType} name.");
                    if (itemHasNoPlayer.Any())
                    {
                        System.Console.WriteLine($"You can add this {itemType}.");
                        
                        var itemHasNoPlayerFirst = itemHasNoPlayer.FirstOrDefault();
                        itemHasNoPlayerFirst.PlayerId = 1;

                        UpdateItem(itemHasNoPlayerFirst);
                        itemNotAdded = false;
                    }
                    else
                    {
                        System.Console.WriteLine($"You already have this {itemType}.");
                        break;
                    }

                }
                else
                {
                    System.Console.WriteLine($"That is not a valid {itemType} name.");
                    break;
                }
            }
        }
    }
    
    private void AttackCharacter()
    {
        if (_goblin is ITargetable targetableGoblin)
        {
            int itemIdForAttack = _player.Attack(targetableGoblin);
            _player.UseAbility(_player.Abilities.First(), targetableGoblin);

            if (itemIdForAttack == 0)
            {
                Console.WriteLine("There was no attack, so no items to remove.");
            }
            else
            {
                var itemForAttack = getItem(itemIdForAttack);
                itemForAttack.PlayerId = null;
                UpdateItem(itemForAttack);
            }

        }
    }

    private void SetupGame()
    {
        _player = _context.Players.FirstOrDefault();
        _outputManager.WriteLine($"{_player.Name} has entered the game.", ConsoleColor.Green);

        // Load monsters into random rooms 
        LoadMonsters();

        // Pause before starting the game loop
        Thread.Sleep(500);
        GameLoop();
    }

    private void LoadMonsters()
    {
        _goblin = _context.Monsters.OfType<Goblin>().FirstOrDefault();
    }

    public void UpdateItem(Item item)
    {
        _context.Items.Update(item);
        _context.SaveChanges();
    }

    public void DeleteItem(Item item)
    {
        _context.Items.Remove(item);
        _context.SaveChanges();
    }

    public Item getItem(int id)
    {
        var retrievedItem = _context.Items.Where(i=> i.Id == id).FirstOrDefault();
        return retrievedItem;
    }

}