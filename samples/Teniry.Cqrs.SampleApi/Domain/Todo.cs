using System.ComponentModel.DataAnnotations;

namespace Teniry.Cqrs.SampleApi.Domain;

public class Todo(string description, bool completed) {
    [Key]
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Description { get; set; } = description;

    public bool Completed { get; set; } = completed;
    public ICollection<Tag> Tags { get; set; } = [];
}