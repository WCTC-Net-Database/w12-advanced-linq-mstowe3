using System.ComponentModel.DataAnnotations.Schema;
using ConsoleRpgEntities.Models.Characters;

namespace ConsoleRpgEntities.Models;


// TODO note this model has been updated from the previous version so a migration will be needed
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Weight { get; set; }

    public int Value { get; set; }

    public virtual Player Player {get;set;}
    public virtual int? PlayerId {get;set;}
}