using System.Text;

namespace Ares.Datamodel.Extensions;

public static class AresSchemaHelper
{
  // ------------------------------------------------------------------------
  // BASIC ENTRIES (Primitives)
  // ------------------------------------------------------------------------

  public static AresStructSchema AddEntry(
    this AresStructSchema schema,
    string name,
    AresDataType type,
    bool optional = true,
    string description = "",
    QuantitySchema? quantitySchema = null,
    double? minNumberValue = null,
    double? maxNumberValue = null)
  {
    var entry = new AresValueSchema
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

  public static AresStructSchema AddEntry(this AresStructSchema schema, string name, AresDataType type, bool optional, IEnumerable<string> stringOptions, string description = "")
  {
    if (type != AresDataType.String && type != AresDataType.StringArray)
    {
      throw new InvalidOperationException($"Cannot provide string options to a datatype that is {type}");
    }

    var entry = new AresValueSchema
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

  public static AresStructSchema AddEntry(this AresStructSchema schema, string name, AresDataType type, bool optional, IEnumerable<double> numOptions, string description = "", double? minNumberValue = null, double? maxNumberValue = null)
  {
    if (type != AresDataType.Number && type != AresDataType.NumberArray)
    {
      throw new InvalidOperationException($"Cannot provide number options to a datatype that is {type}");
    }

    var entry = new AresValueSchema
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
  public static AresStructSchema AddEntry(this AresStructSchema schema, string name, AresDataType type, bool optional, IEnumerable<int> numOptions, string description = "", double? minNumberValue = null, double? maxNumberValue = null)
  {
    return schema.AddEntry(name, type, optional, numOptions.Select(n => (double)n), description, minNumberValue, maxNumberValue);
  }

  // Overload for Float options
  public static AresStructSchema AddEntry(this AresStructSchema schema, string name, AresDataType type, bool optional, IEnumerable<float> numOptions, string description = "", double? minNumberValue = null, double? maxNumberValue = null)
  {
    return schema.AddEntry(name, type, optional, numOptions.Select(n => (double)n), description, minNumberValue, maxNumberValue);
  }

  // ------------------------------------------------------------------------
  // RECURSIVE ENTRIES (Structs and Lists)
  // ------------------------------------------------------------------------

  /// <summary>
  /// Adds a nested Struct schema.
  /// </summary>
  public static AresStructSchema AddStructEntry(this AresStructSchema schema, string name, bool optional, AresStructSchema innerStructSchema, string description = "")
  {
    var entry = new AresValueSchema
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
  public static AresStructSchema AddListEntry(this AresStructSchema schema, string name, bool optional, AresValueSchema listElementSchema, string description = "")
  {
    var entry = new AresValueSchema
    {
      Type = AresDataType.List,
      Optional = optional,
      Description = description,
      ListElementSchema = listElementSchema // Populates the recursive element definition
    };
    schema.Fields[name] = entry;

    return schema;
  }

  public static string Stringify(this AresValueSchema schema)
  {
    return schema.Type switch
    {
      AresDataType.UnspecifiedType => "Unspecified",
      AresDataType.Null => "Null",
      AresDataType.Boolean => "Boolean",
      AresDataType.String => schema.StringifyString(),
      AresDataType.Number => schema.StringifyNumber(),
      AresDataType.StringArray => schema.StringifyStringArray(),
      AresDataType.NumberArray => schema.StringifyNumArray(),
      AresDataType.List => schema.StringifyList(),
      AresDataType.Struct => schema.StringifyStruct(),
      AresDataType.ByteArray => "Byte Array",
      AresDataType.Any => "Any",
      AresDataType.Unit => "Unit",
      AresDataType.Function => "Function Pointer",
      AresDataType.Quantity => schema.StringifyQuantity(),
      _ => $"{schema.Type}"
    };
  }

  private static string StringifyString(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.String)
    {
      throw new InvalidOperationException($"Tried to stringify string, but it's actually {entry.Type}");
    }

    var sb = new StringBuilder("String");
    if (entry.AvailableChoicesCase == AresValueSchema.AvailableChoicesOneofCase.StringChoices)
    {
      sb.AppendLine();
      sb.Append("Available Choices: ");
      sb.Append(string.Join(", ", entry.StringChoices.Strings));
    }

    return sb.ToString();
  }

  private static string StringifyNumber(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.Number)
    {
      throw new InvalidOperationException($"Tried to stringify number, but it's actually {entry.Type}");
    }

    var sb = new StringBuilder("Number");
    if (entry.HasMinNumberValue)
    {
      sb.AppendLine($" (Min: {entry.MinNumberValue})");
    }
    if (entry.HasMaxNumberValue)
    {
      sb.AppendLine($" (Max: {entry.MaxNumberValue})");
    }
    if (entry.AvailableChoicesCase == AresValueSchema.AvailableChoicesOneofCase.NumberChoices)
    {
      sb.AppendLine("Available Choices: ");
      sb.Append(string.Join(", ", entry.NumberChoices.Numbers));
    }

    return sb.ToString();
  }

  private static string StringifyStringArray(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.StringArray)
    {
      throw new InvalidOperationException($"Tried to stringify string array, but it's actually {entry.Type}");
    }

    var sb = new StringBuilder("String Array");
    if (entry.AvailableChoicesCase == AresValueSchema.AvailableChoicesOneofCase.StringChoices)
    {
      sb.AppendLine("Available Choices: ");
      var choices = string.Join(", ", entry.StringChoices.Strings);
      sb.Append(choices);
    }

    return sb.ToString();
  }

  private static string StringifyNumArray(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.NumberArray)
    {
      throw new InvalidOperationException($"Tried to stringify number array, but it's actually {entry.Type}");
    }

    var sb = new StringBuilder("Number Array");
    if (entry.HasMinNumberValue)
    {
      sb.AppendLine($" (Min: {entry.MinNumberValue})");
    }
    if (entry.HasMaxNumberValue)
    {
      sb.AppendLine($" (Max: {entry.MaxNumberValue})");
    }
    if (entry.AvailableChoicesCase == AresValueSchema.AvailableChoicesOneofCase.NumberChoices)
    {
      sb.AppendLine("Available Choices: ");
      sb.Append(string.Join(", ", entry.NumberChoices.Numbers));
    }

    return sb.ToString();
  }

  private static string StringifyList(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.List)
    {
      throw new InvalidOperationException($"Tried to stringify list, but it's actually {entry.Type}");
    }

    var element = entry.ListElementSchema?.Stringify() ?? "Any";
    return $"List<{element}>";
  }

  private static string StringifyStruct(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.Struct)
    {
      throw new InvalidOperationException($"Tried to stringify struct, but it's actually {entry.Type}");
    }

    if (entry.StructSchema is null || entry.StructSchema.Fields.Count == 0)
      return "Struct {}";

    var fields = entry.StructSchema.Fields
      .Select(field => $"{field.Key}: {field.Value.Stringify()}");
    return $"Struct {{ {string.Join("; ", fields)} }}";
  }

  private static string StringifyQuantity(this AresValueSchema entry)
  {
    if (entry.Type != AresDataType.Quantity)
    {
      throw new InvalidOperationException($"Tried to stringify quantity, but it's actually {entry.Type}");
    }

    var quantitySchema = entry.QuantitySchema;
    if (quantitySchema is null)
      return "Quantity";

    var sb = new StringBuilder($"Quantity ({quantitySchema.QuantityType})");
    if (!string.IsNullOrWhiteSpace(quantitySchema.BoundsUnit))
    {
      sb.Append($" [{quantitySchema.BoundsUnit}]");
    }
    if (quantitySchema.HasMinScalarValue)
    {
      sb.Append($" (Min: {quantitySchema.MinScalarValue})");
    }
    if (quantitySchema.HasMaxScalarValue)
    {
      sb.Append($" (Max: {quantitySchema.MaxScalarValue})");
    }

    return sb.ToString();
  }
}
