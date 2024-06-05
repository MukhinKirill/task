using System.ComponentModel.DataAnnotations;

namespace Task.Common.EntityFrameWork;

public abstract class Entity <TKey> : IAggregateRoot
{
    [Key]
    public TKey id { get; set; }
}

public abstract class Entity : Entity<int>
{
}