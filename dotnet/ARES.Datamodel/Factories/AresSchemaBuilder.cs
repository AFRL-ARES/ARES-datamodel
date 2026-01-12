using Ares.Datamodel.Extensions;

namespace Ares.Datamodel.Factories;

public static class AresSchemaBuilder
{
  public static RootSchemaBuilder Create(string name, AresDataType type) => new(name, type);
  public static RootSchemaBuilder Create(AresDataType type) => new(type);
  public static RootSchemaBuilder Empty() => new();
  public static EntryBuilder StringEntry() => new(AresDataType.String);
  public static EntryBuilder NumberEntry() => new(AresDataType.Number);
  public static EntryBuilder Entry(AresDataType type) => new(type);
}

public class RootSchemaBuilder
{
  private readonly AresDataSchema _schema = new();
  private readonly string _rootName = "";
  private readonly AresDataType _rootType;

  private string _rootDescription = "";
  private string _rootUnit = "";
  private bool _rootOptional = false; // Added back

  public RootSchemaBuilder() { }

  public RootSchemaBuilder(AresDataType type)
  {
    _rootType = type;
  }

  public RootSchemaBuilder(string name, AresDataType type)
  {
    _rootName = name;
    _rootType = type;
  }

  public RootSchemaBuilder AsOptional(bool isOptional = true)
  {
    _rootOptional = isOptional;
    return this;
  }

  public RootSchemaBuilder WithDescription(string description)
  {
    _rootDescription = description;
    return this;
  }

  public RootSchemaBuilder WithUnit(string unit)
  {
    _rootUnit = unit;
    return this;
  }

  public RootSchemaBuilder AddEntry(string name, SchemaEntry entry)
  {
    _schema.Fields.Add(name, entry);
    return this;
  }

  public AresDataSchema Build()
  {
    if (!string.IsNullOrEmpty(_rootName))
    {
      // Now using the _rootOptional flag
      _schema.AddEntry(_rootName, _rootType, _rootOptional, _rootDescription, _rootUnit);
    }
    return _schema;
  }
}

public class EntryBuilder
{
  private readonly SchemaEntry _entry;

  public EntryBuilder(AresDataType type)
  {
    _entry = new SchemaEntry { Type = type };
  }

  public EntryBuilder AsOptional(bool isOptional = true)
  {
    _entry.Optional = isOptional;
    return this;
  }

  public EntryBuilder WithDescription(string description)
  {
    _entry.Description = description;
    return this;
  }

  public EntryBuilder WithUnit(string unit)
  {
    _entry.Unit = unit;
    return this;
  }

  public EntryBuilder WithChoices(params string[] choices)
  {
    if (_entry.Type != AresDataType.String)
      throw new InvalidOperationException("Cannot add string choices to a non-string entry.");

    _entry.StringChoices.Strings.AddRange(choices);
    return this;
  }

  public EntryBuilder WithChoices(params double[] choices)
  {
    if (_entry.Type != AresDataType.Number)
      throw new InvalidOperationException("Cannot add number choices to a non-number entry.");

    _entry.NumberChoices.Numbers.AddRange(choices);
    return this;
  }

  // Overloads for int/float for convenience
  public EntryBuilder WithChoices(IEnumerable<int> choices) => WithChoices(choices.Select(Convert.ToDouble).ToArray());
  public EntryBuilder WithChoices(IEnumerable<float> choices) => WithChoices(choices.Select(Convert.ToDouble).ToArray());

  public SchemaEntry Build() => _entry;
}