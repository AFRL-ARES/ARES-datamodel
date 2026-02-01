using Ares.Datamodel.Factories;

namespace Ares.Datamodel.Extensions;

public static class AresValueSchemaExtensions
{
  public static SchemaEntry ToSchemaEntry(this AresValue value)
  {
    return value.KindCase switch
    {
      AresValue.KindOneofCase.None => AresSchemaBuilder.Entry(AresDataType.UnspecifiedType).Build(),
      AresValue.KindOneofCase.NullValue => AresSchemaBuilder.Entry(AresDataType.Null).Build(),
      AresValue.KindOneofCase.BoolValue => AresSchemaBuilder.Entry(AresDataType.Boolean).Build(),
      AresValue.KindOneofCase.StringValue => AresSchemaBuilder.Entry(AresDataType.String).Build(),
      AresValue.KindOneofCase.NumberValue => AresSchemaBuilder.Entry(AresDataType.Number).Build(),
      AresValue.KindOneofCase.StringArrayValue => AresSchemaBuilder.Entry(AresDataType.StringArray).Build(),
      AresValue.KindOneofCase.NumberArrayValue => AresSchemaBuilder.Entry(AresDataType.NumberArray).Build(),
      AresValue.KindOneofCase.BytesValue => AresSchemaBuilder.Entry(AresDataType.ByteArray).Build(),
      AresValue.KindOneofCase.UnitValue => AresSchemaBuilder.Entry(AresDataType.Unit).Build(),
      AresValue.KindOneofCase.FunctionValue => AresSchemaBuilder.Entry(AresDataType.Function).Build(),
      AresValue.KindOneofCase.ListValue => CreateListEntry(value.ListValue.Values),
      AresValue.KindOneofCase.StructValue => CreateStructEntry(value.StructValue),
      _ => AresSchemaBuilder.Entry(AresDataType.Any).Build()
    };
  }

  private static SchemaEntry CreateStructEntry(AresStruct structValue)
  {
    var schema = new AresDataSchema();
    foreach(var field in structValue.Fields)
    {
      schema.Fields[field.Key] = field.Value.ToSchemaEntry();
    }

    var entry = AresSchemaBuilder.Entry(AresDataType.Struct).Build();
    entry.StructSchema = schema;
    return entry;
  }

  private static SchemaEntry CreateListEntry(IEnumerable<AresValue> values)
  {
    var list = values.ToArray();
    if(list.Length == 0)
    {
      return CreateListEntry(AresSchemaBuilder.Entry(AresDataType.Any).Build());
    }

    var first = list[0].ToSchemaEntry();
    var allSameType = list.All(val => val.ToSchemaEntry().Type == first.Type);
    if(allSameType)
    {
      return CreateListEntry(first);
    }

    return CreateListEntry(AresSchemaBuilder.Entry(AresDataType.Any).Build());
  }

  private static SchemaEntry CreateListEntry(SchemaEntry elementSchema)
  {
    var entry = AresSchemaBuilder.Entry(AresDataType.List).Build();
    entry.ListElementSchema = elementSchema;
    return entry;
  }
}
