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
  private readonly AresStructSchema _schema = new();
  private readonly string _rootName = "";
  private readonly AresDataType _rootType;

  private string _rootDescription = "";
  private QuantitySchema? _rootQuantitySchema;
  private double? _rootMinNumberValue;
  private double? _rootMaxNumberValue;
  private bool _rootOptional = false;

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

  public RootSchemaBuilder WithQuantitySchema(QuantitySchema quantitySchema)
  {
    _rootQuantitySchema = quantitySchema;
    return this;
  }

  public RootSchemaBuilder WithQuantity(QuantityType quantityType)
  {
    _rootQuantitySchema ??= new QuantitySchema();
    _rootQuantitySchema.QuantityType = quantityType;
    return this;
  }

  public RootSchemaBuilder WithQuantityRange(QuantityType quantityType, string boundsUnit, double? minScalarValue = null, double? maxScalarValue = null)
  {
    var schema = _rootQuantitySchema ??= new QuantitySchema();
    schema.QuantityType = quantityType;
    schema.BoundsUnit = boundsUnit;
    if(minScalarValue is not null)
      schema.MinScalarValue = minScalarValue.Value;
    if(maxScalarValue is not null)
      schema.MaxScalarValue = maxScalarValue.Value;
    return this;
  }

  public RootSchemaBuilder WithMinNumberValue(double value)
  {
    _rootMinNumberValue = value;
    return this;
  }

  public RootSchemaBuilder WithMaxNumberValue(double value)
  {
    _rootMaxNumberValue = value;
    return this;
  }

  public RootSchemaBuilder WithNumberRange(double minValue, double maxValue)
  {
    _rootMinNumberValue = minValue;
    _rootMaxNumberValue = maxValue;
    return this;
  }

  public RootSchemaBuilder AddEntry(string name, AresValueSchema entry)
  {
    _schema.Fields.Add(name, entry);
    return this;
  }

  public AresStructSchema Build()
  {
    if (!string.IsNullOrEmpty(_rootName))
      _schema.AddEntry(_rootName, _rootType, _rootOptional, _rootDescription, _rootQuantitySchema, _rootMinNumberValue, _rootMaxNumberValue);

    return _schema;
  }
}

public class EntryBuilder
{
  private readonly AresValueSchema _entry;

  public EntryBuilder(AresDataType type)
  {
    _entry = new AresValueSchema { Type = type };
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

  public EntryBuilder WithQuantitySchema(QuantitySchema quantitySchema)
  {
    _entry.QuantitySchema = quantitySchema;
    return this;
  }

  public EntryBuilder WithQuantity(QuantityType quantityType)
  {
    _entry.QuantitySchema ??= new QuantitySchema();
    _entry.QuantitySchema.QuantityType = quantityType;
    return this;
  }

  public EntryBuilder WithQuantityRange(QuantityType quantityType, string boundsUnit, double? minScalarValue = null, double? maxScalarValue = null)
  {
    var schema = _entry.QuantitySchema ??= new QuantitySchema();
    schema.QuantityType = quantityType;
    schema.BoundsUnit = boundsUnit;
    if(minScalarValue is not null)
      schema.MinScalarValue = minScalarValue.Value;
    if(maxScalarValue is not null)
      schema.MaxScalarValue = maxScalarValue.Value;
    return this;
  }

  public EntryBuilder WithMinNumberValue(double value)
  {
    _entry.MinNumberValue = value;
    return this;
  }

  public EntryBuilder WithMaxNumberValue(double value)
  {
    _entry.MaxNumberValue = value;
    return this;
  }

  public EntryBuilder WithNumberRange(double minValue, double maxValue)
  {
    _entry.MinNumberValue = minValue;
    _entry.MaxNumberValue = maxValue;
    return this;
  }

  public EntryBuilder WithChoices(params string[] choices)
  {
    if(_entry.Type == AresDataType.Any) 
      _entry.Type = AresDataType.String;

    if(_entry.Type != AresDataType.String)
      throw new InvalidOperationException("Cannot add string choices to a non-string entry.");

    _entry.StringChoices.Strings.AddRange(choices);
    return this;
  }

  public EntryBuilder WithChoices(params double[] choices)
  {
    if(_entry.Type != AresDataType.Number)
      throw new InvalidOperationException("Cannot add number choices to a non-number entry.");

    _entry.NumberChoices.Numbers.AddRange(choices);
    return this;
  }

  public EntryBuilder WithChoices(IEnumerable<int> choices) => WithChoices(choices.Select(Convert.ToDouble).ToArray());
  public EntryBuilder WithChoices(IEnumerable<float> choices) => WithChoices(choices.Select(Convert.ToDouble).ToArray());

  public EntryBuilder WithStructSchema(AresStructSchema schema)
  {
      if(_entry.Type == AresDataType.Any) 
        _entry.Type = AresDataType.Struct;

      if(_entry.Type != AresDataType.Struct)
        throw new InvalidOperationException($"Cannot add struct schema to a {_entry.Type} entry.");

      _entry.StructSchema = schema;
      return this;
  }

  public EntryBuilder WithStructSchema(Action<AresStructSchema> configure)
  {
    if(configure is null)
      throw new ArgumentNullException(nameof(configure));

    var schema = new AresStructSchema();
    configure(schema);
    return WithStructSchema(schema);
  }

  public EntryBuilder WithListElementSchema(AresValueSchema elementSchema)
  {
      if(_entry.Type == AresDataType.Any) 
        _entry.Type = AresDataType.List;

      if(_entry.Type != AresDataType.List)
        throw new InvalidOperationException($"Cannot add list element schema to a {_entry.Type} entry.");

      _entry.ListElementSchema = elementSchema;
      return this;
  }

  public EntryBuilder WithListElementSchema(AresDataType elementType)
  {
    return WithListElementSchema(AresSchemaBuilder.Entry(elementType).Build());
  }

  public EntryBuilder WithListElementSchema(Action<EntryBuilder> configure)
  {
    if(configure is null)
      throw new ArgumentNullException(nameof(configure));

    var builder = new EntryBuilder(AresDataType.Any);
    configure(builder);
    return WithListElementSchema(builder.Build());
  }

  public AresValueSchema Build() => _entry;
}
