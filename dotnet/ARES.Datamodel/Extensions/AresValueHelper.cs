using Google.Protobuf;

namespace Ares.Datamodel.Extensions;

public static class AresValueHelper
{
  public static AresValue CreateNumber(int value)
  {
    return new AresValue { NumberValue = value, };
  }

  public static AresValue CreateNumber(double value)
  {
    return new AresValue { NumberValue = value, };
  }

  public static AresValue CreateNumber(float value)
  {
    return new AresValue { NumberValue = value, };
  }

  public static AresValue CreateString(string value)
  {
    return new AresValue { StringValue = value, };
  }

  public static AresValue CreateNumberArray(IEnumerable<int> values)
  {
    var val = new AresValue { NumberArrayValue = new NumberArray() };
    val.NumberArrayValue.Numbers.AddRange(values.Select(num => (double)num));

    return val;
  }

  public static AresValue CreateNumberArray(IEnumerable<double> values)
  {
    var val = new AresValue { NumberArrayValue = new NumberArray() };
    val.NumberArrayValue.Numbers.AddRange(values.Select(num => num));

    return val;
  }

  public static AresValue CreateNumberArray(IEnumerable<float> values)
  {
    var val = new AresValue { NumberArrayValue = new NumberArray() };
    val.NumberArrayValue.Numbers.AddRange(values.Select(num => (double)num));

    return val;
  }

  public static AresValue CreateStringArray(IEnumerable<string> values)
  {
    var val = new AresValue { StringArrayValue = new StringArray() };
    val.StringArrayValue.Strings.AddRange(values);

    return val;
  }

  public static AresValue CreateStruct(AresStruct value)
  {
    return new AresValue { StructValue = value };
  }

  public static AresValue CreateList(IEnumerable<AresValue> values)
  {
    var list = new AresValueList();
    list.Values.AddRange(values);
    return new AresValue { ListValue = list };
  }

  public static AresValue CreateNull()
  {
    return new AresValue { NullValue = NullValue.NullValue };
  }

  public static AresValue CreateUnit()
  {
    return new AresValue { UnitValue = UnitValue.UnitValue };
  }

  public static AresValue CreateFunction(string id)
  {
    return new AresValue { FunctionValue = new FunctionValue { FunctionId = id } };
  }

  public static AresValue CreateBool(bool value)
  {
    return new AresValue { BoolValue = value };
  }

  public static AresValue CreateBytes(byte[] bytes)
  {
    return new AresValue { BytesValue = ByteString.CopyFrom(bytes) };
  }

  public static AresValue CreateStruct()
  {
    return new AresValue { StructValue = new AresStruct() };
  }

  public static AresValue CreateList()
  {
    return new AresValue { ListValue = new AresValueList() };
  }

  public static AresValue CreateDefault(AresDataType dataType)
  {
    return dataType switch
    {
      AresDataType.UnspecifiedType => CreateNull(),
      AresDataType.Null => CreateNull(),
      AresDataType.Boolean => CreateBool(false),
      AresDataType.String => CreateString(string.Empty),
      AresDataType.Number => CreateNumber(0),
      AresDataType.StringArray => CreateStringArray(Array.Empty<string>()),
      AresDataType.NumberArray => CreateNumberArray(Array.Empty<double>()),
      AresDataType.ByteArray => CreateBytes(Array.Empty<byte>()),
      AresDataType.Struct => CreateStruct(),
      AresDataType.List => CreateList(),
      AresDataType.Unit => CreateUnit(),
      _ => CreateNull()
    };
  }

  public static AresValue CreateDefault(AresDataType dataType, IEnumerable<string>? choices = null)
  {
    return dataType switch
    {
      AresDataType.String => CreateString(choices?.FirstOrDefault() ?? string.Empty),
      _ => CreateDefault(dataType)
    };
  }

  public static AresValue CreateDefault(AresDataType dataType, IEnumerable<double>? choices = null)
  {
    return dataType switch
    {
      AresDataType.Number => CreateNumber(choices?.FirstOrDefault() ?? 0),
      _ => CreateDefault(dataType)
    };
  }

  public static bool IsPrimitiveType(AresValue value)
  {
    switch (value.KindCase)
    {
      case AresValue.KindOneofCase.None:
        return true;
      case AresValue.KindOneofCase.NullValue:
        return true;
      case AresValue.KindOneofCase.BoolValue:
        return true;
      case AresValue.KindOneofCase.StringValue:
        return true;
      case AresValue.KindOneofCase.NumberValue:
        return true;
      case AresValue.KindOneofCase.StringArrayValue:
        return false;
      case AresValue.KindOneofCase.NumberArrayValue:
        return false;
      case AresValue.KindOneofCase.BytesValue:
        return false;
      case AresValue.KindOneofCase.StructValue:
        return false;
      case AresValue.KindOneofCase.ListValue:
        return false;
      case AresValue.KindOneofCase.UnitValue:
        return true;
      case AresValue.KindOneofCase.FunctionValue:
        return false;
      default:
        return false;
    }
  }

  public static string Stringify(this AresValue value)
  {
    return value.KindCase switch
    {
      AresValue.KindOneofCase.NullValue => "None",
      AresValue.KindOneofCase.BoolValue => value.BoolValue.ToString(),
      AresValue.KindOneofCase.StringValue => value.StringValue,
      AresValue.KindOneofCase.NumberValue => value.NumberValue.ToString(),
      AresValue.KindOneofCase.BytesValue => BitConverter.ToString(value.BytesValue.ToByteArray()),
      AresValue.KindOneofCase.StringArrayValue => string.Join(", ", value.StringArrayValue.Strings),
      AresValue.KindOneofCase.NumberArrayValue => string.Join(", ", value.NumberArrayValue.Numbers),
      AresValue.KindOneofCase.ListValue => $"[{string.Join(", ", value.ListValue.Values.Select(v => v.Stringify()))}]",
      AresValue.KindOneofCase.StructValue => $"{{{string.Join(", ", value.StructValue.Fields.Select(kv => $"{kv.Key}: {kv.Value.Stringify()}"))}}}",
      AresValue.KindOneofCase.UnitValue => "()",
      AresValue.KindOneofCase.FunctionValue => $"Function pointer: {value.FunctionValue.FunctionId}",
      _ => "Unknown value"
    };
  }

  public static AresDataType GetAresDataType(this AresValue value)
  {
    return value.KindCase switch
    {
      AresValue.KindOneofCase.None => AresDataType.UnspecifiedType,
      AresValue.KindOneofCase.NullValue => AresDataType.Null,
      AresValue.KindOneofCase.BoolValue => AresDataType.Boolean,
      AresValue.KindOneofCase.StringValue => AresDataType.String,
      AresValue.KindOneofCase.NumberValue => AresDataType.Number,
      AresValue.KindOneofCase.StringArrayValue => AresDataType.StringArray,
      AresValue.KindOneofCase.NumberArrayValue => AresDataType.NumberArray,
      AresValue.KindOneofCase.BytesValue => AresDataType.ByteArray,
      AresValue.KindOneofCase.StructValue => AresDataType.Struct,
      AresValue.KindOneofCase.ListValue => AresDataType.List,
      AresValue.KindOneofCase.UnitValue => AresDataType.Unit,
      AresValue.KindOneofCase.FunctionValue => AresDataType.Function,
      _ => AresDataType.UnspecifiedType
    };
  }
}
