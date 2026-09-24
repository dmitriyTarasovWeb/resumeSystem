namespace resumeSystem.Domain;

public class Attribute
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public int DataTypeId { get; set; }

    public bool IsDisplay { get; set; }

    public Category Category { get; set; } = null!;

    public DataType DataType { get; set; } = null!;
}