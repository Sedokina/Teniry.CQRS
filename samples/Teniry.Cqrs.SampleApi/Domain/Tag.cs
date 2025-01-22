using System.ComponentModel.DataAnnotations;

namespace Teniry.Cqrs.SampleApi.Domain;

public class Tag(string title) {
    [Key]
    public Guid Id { get; set; }

    public string Title { get; set; } = title;
}