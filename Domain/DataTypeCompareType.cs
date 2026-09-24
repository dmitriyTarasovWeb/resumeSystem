namespace resumeSystem.Domain;

public class DataTypeCompareType
{
    public int Id { get; set; }

    public int DataTypeId { get; set; }

    public int CompareTypeId { get; set; }

    public DataType DataType { get; set; } = null!;

    public CompareType CompareType { get; set; } = null!;
}