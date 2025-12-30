namespace Ares.Datamodel.Extensions;

public static class AresSchemaHelper
{
  // ------------------------------------------------------------------------
  // BASIC ENTRIES (Primitives)
  // ------------------------------------------------------------------------

  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, string description = "", string unit = "")
  {
    var entry = new SchemaEntry
    {
      Type = type,
      Optional = optional,
      Description = description,
      Unit = unit
    };
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

  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<double> numOptions, string description = "", string unit = "")
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
      Unit = unit,
      NumberChoices = new NumberArray()
    };

    // Note: 'Numbers' comes from 'repeated double numbers = 2' in ares_struct.proto
    entry.NumberChoices.Numbers.AddRange(numOptions);
    schema.Fields[name] = entry;

    return schema;
  }

  // Overload for Int options
  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<int> numOptions, string description = "", string unit = "")
  {
    return schema.AddEntry(name, type, optional, numOptions.Select(n => (double)n), description, unit);
  }

  // Overload for Float options
  public static AresDataSchema AddEntry(this AresDataSchema schema, string name, AresDataType type, bool optional, IEnumerable<float> numOptions, string description = "", string unit = "")
  {
    return schema.AddEntry(name, type, optional, numOptions.Select(n => (double)n), description, unit);
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

  // ------------------------------------------------------------------------
  // FACTORY METHODS
  // ------------------------------------------------------------------------

  public static AresDataSchema CreateSchema(string name, AresDataType type)
  {
    var schema = new AresDataSchema();
    schema.AddEntry(name, type, false); // Defaulting optional to false for root creation
    return schema;
  }

  public static SchemaEntry CreateSchemaEntry(AresDataType dataType, bool optional, string description = "", string unit = "")
  {
    return new SchemaEntry
    {
      Type = dataType,
      Optional = optional,
      Description = description,
      Unit = unit
    };
  }
}