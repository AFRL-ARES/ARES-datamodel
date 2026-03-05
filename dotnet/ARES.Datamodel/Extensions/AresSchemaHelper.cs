namespace Ares.Datamodel.Extensions;

public static class AresSchemaHelper
{
  // ------------------------------------------------------------------------
  // BASIC ENTRIES (Primitives)
  // ------------------------------------------------------------------------

  public static AresDataSchema AddEntry(
    this AresDataSchema schema,
    string name,
    AresDataType type,
    bool optional = true,
    string description = "",
    QuantitySchema? quantitySchema = null,
    double? minNumberValue = null,
    double? maxNumberValue = null)
  {
    var entry = new SchemaEntry
    {
      Type = type,
      Optional = optional,
      Description = description
    };
    if (quantitySchema is not null)
      entry.QuantitySchema = quantitySchema;
    if (minNumberValue is not null)
      entry.MinNumberValue = minNumberValue.Value;
    if (maxNumberValue is not null)
      entry.MaxNumberValue = maxNumberValue.Value;
    schema.Fields[name] = entry;
    return schema;
  }

  // ------------------------------------------------------------------------
  // CONSTRAINED ENTRIES (Strings with Choices)
  // ------------------------------------------------------------------------

  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<string> stringOptions, string description = "")
  {
    if (type != AresDataType.String && type != AresDataType.StringArray)
    {
      throw new InvalidOperationException($"Cannot provide string options to a datatype that is {type}");
    }

    var entry = new SchemaEntry
    {
      Type = type,
      Optional = optional,
      Description = description,
      StringChoices = new StringArray()
    };

    // Note: 'Strings' comes from 'repeated string strings = 1' in ares_struct.proto
    entry.StringChoices.Strings.AddRange(stringOptions);
    schema.Fields[name] = entry;

    return schema;
  }

  // ------------------------------------------------------------------------
  // CONSTRAINED ENTRIES (Numbers with Choices)
  // ------------------------------------------------------------------------

  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<double> numOptions, string description = "", double? minNumberValue = null, double? maxNumberValue = null)
  {
    if (type != AresDataType.Number && type != AresDataType.NumberArray)
    {
      throw new InvalidOperationException($"Cannot provide number options to a datatype that is {type}");
    }

    var entry = new SchemaEntry
    {
      Type = type,
      Optional = optional,
      Description = description,
      NumberChoices = new NumberArray()
    };
    if (minNumberValue is not null)
      entry.MinNumberValue = minNumberValue.Value;
    if (maxNumberValue is not null)
      entry.MaxNumberValue = maxNumberValue.Value;

    // Note: 'Numbers' comes from 'repeated double numbers = 2' in ares_struct.proto
    entry.NumberChoices.Numbers.AddRange(numOptions);
    schema.Fields[name] = entry;

    return schema;
  }

  // Overload for Int options
  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<int> numOptions, string description = "", double? minNumberValue = null, double? maxNumberValue = null)
  {
    return schema.AddEntry(name, type, optional, numOptions.Select(n => (double)n), description, minNumberValue, maxNumberValue);
  }

  // Overload for Float options
  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<float> numOptions, string description = "", double? minNumberValue = null, double? maxNumberValue = null)
  {
    return schema.AddEntry(name, type, optional, numOptions.Select(n => (double)n), description, minNumberValue, maxNumberValue);
  }

  // ------------------------------------------------------------------------
  // RECURSIVE ENTRIES (Structs and Lists)
  // ------------------------------------------------------------------------

  /// <summary>
  /// Adds a nested Struct schema.
  /// </summary>
  public static AresDataSchema AddStructEntry(this AresDataSchema schema, string name, bool optional, AresDataSchema innerStructSchema, string description = "")
  {
    var entry = new SchemaEntry
    {
      Type = AresDataType.Struct,
      Optional = optional,
      Description = description,
      StructSchema = innerStructSchema // Populates the recursive schema field
    };
    schema.Fields[name] = entry;

    return schema;
  }

  /// <summary>
  /// Adds a List schema. You must define what a single element of the list looks like.
  /// </summary>
  public static AresDataSchema AddListEntry(this AresDataSchema schema, string name, bool optional, SchemaEntry listElementSchema, string description = "")
  {
    var entry = new SchemaEntry
    {
      Type = AresDataType.List,
      Optional = optional,
      Description = description,
      ListElementSchema = listElementSchema // Populates the recursive element definition
    };
    schema.Fields[name] = entry;

    return schema;
  }
}
