using Ares.Datamodel.Extensions;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;

namespace Ares.Datamodel.Tests;

public class AresValueHelperTests
{
  [Test]
  public void CreateTimestamp_FromTimestamp_UsesTimestampValueKind()
  {
    var timestamp = Timestamp.FromDateTime(new DateTime(2026, 5, 10, 12, 30, 0, DateTimeKind.Utc));

    var value = AresValueHelper.CreateTimestamp(timestamp);

    Assert.That(value.KindCase, Is.EqualTo(AresValue.KindOneofCase.TimestampValue));
    Assert.That(value.TimestampValue, Is.EqualTo(timestamp));
  }

  [Test]
  public void CreateFloat_UsesFloatValueKind()
  {
    var value = AresValueHelper.CreateFloat(1.25d);

    Assert.That(value.KindCase, Is.EqualTo(AresValue.KindOneofCase.FloatValue));
    Assert.That(value.FloatValue, Is.EqualTo(1.25d));
  }

  [Test]
  public void CreateInt_UsesIntValueKind()
  {
    var value = AresValueHelper.CreateInt(1234567890123L);

    Assert.That(value.KindCase, Is.EqualTo(AresValue.KindOneofCase.IntValue));
    Assert.That(value.IntValue, Is.EqualTo(1234567890123L));
  }

  [Test]
  public void CreateIntArray_UsesIntArrayValueKind()
  {
    var value = AresValueHelper.CreateIntArray([1, 2, 3]);

    Assert.That(value.KindCase, Is.EqualTo(AresValue.KindOneofCase.IntArrayValue));
    Assert.That(value.IntArrayValue.Ints, Is.EqualTo(new long[] { 1, 2, 3 }));
  }

  [Test]
  public void CreateFloatArray_UsesFloatArrayValueKind()
  {
    var value = AresValueHelper.CreateFloatArray([1.25d, 2.5d]);

    Assert.That(value.KindCase, Is.EqualTo(AresValue.KindOneofCase.FloatArrayValue));
    Assert.That(value.FloatArrayValue.Floats, Is.EqualTo(new[] { 1.25d, 2.5d }));
  }

  [TestCase(AresDataType.Timestamp, AresValue.KindOneofCase.TimestampValue)]
  [TestCase(AresDataType.Float, AresValue.KindOneofCase.FloatValue)]
  [TestCase(AresDataType.Int, AresValue.KindOneofCase.IntValue)]
  [TestCase(AresDataType.IntArray, AresValue.KindOneofCase.IntArrayValue)]
  [TestCase(AresDataType.FloatArray, AresValue.KindOneofCase.FloatArrayValue)]
  public void CreateDefault_NewTypes_ReturnsExpectedKind(AresDataType dataType, AresValue.KindOneofCase expectedKind)
  {
    var value = AresValueHelper.CreateDefault(dataType);

    Assert.That(value.KindCase, Is.EqualTo(expectedKind));
  }

  [TestCase(AresDataType.Timestamp, AresValue.KindOneofCase.TimestampValue)]
  [TestCase(AresDataType.Float, AresValue.KindOneofCase.FloatValue)]
  [TestCase(AresDataType.Int, AresValue.KindOneofCase.IntValue)]
  [TestCase(AresDataType.IntArray, AresValue.KindOneofCase.IntArrayValue)]
  [TestCase(AresDataType.FloatArray, AresValue.KindOneofCase.FloatArrayValue)]
  public void GetAresDataType_NewTypes_ReturnsExpectedType(AresDataType expectedType, AresValue.KindOneofCase expectedKind)
  {
    var value = AresValueHelper.CreateDefault(expectedType);

    Assert.That(value.KindCase, Is.EqualTo(expectedKind));
    Assert.That(value.GetAresDataType(), Is.EqualTo(expectedType));
  }

  [TestCase(AresDataType.Timestamp, "1970-01-01T00:00:00.0000000Z")]
  [TestCase(AresDataType.Float, "0")]
  [TestCase(AresDataType.Int, "0")]
  [TestCase(AresDataType.IntArray, "")]
  [TestCase(AresDataType.FloatArray, "")]
  public void Stringify_NewTypes_ReturnsExpectedText(AresDataType dataType, string expectedText)
  {
    var value = AresValueHelper.CreateDefault(dataType);

    Assert.That(value.Stringify(), Is.EqualTo(expectedText));
  }

  [Test]
  public void TryGetNumericValue_AcceptsNumberFloatAndInt()
  {
    Assert.That(AresValueHelper.CreateNumber(1.5).TryGetNumericValue(out var number), Is.True);
    Assert.That(number, Is.EqualTo(1.5));

    Assert.That(AresValueHelper.CreateFloat(2.5d).TryGetNumericValue(out var floatValue), Is.True);
    Assert.That(floatValue, Is.EqualTo(2.5d));

    Assert.That(AresValueHelper.CreateInt(3).TryGetNumericValue(out var intValue), Is.True);
    Assert.That(intValue, Is.EqualTo(3));
  }

  [TestCase(AresDataType.Timestamp)]
  [TestCase(AresDataType.Float)]
  [TestCase(AresDataType.Int)]
  [TestCase(AresDataType.IntArray)]
  [TestCase(AresDataType.FloatArray)]
  public void ToAresValueSchema_NewTypes_ReturnsExpectedType(AresDataType dataType)
  {
    var value = AresValueHelper.CreateDefault(dataType);

    var schema = value.ToAresValueSchema();

    Assert.That(schema.Type, Is.EqualTo(dataType));
  }

  [TestCase(AresDataType.IntArray, "Int Array")]
  [TestCase(AresDataType.FloatArray, "Float Array")]
  public void StringifySchema_NewArrayTypes_ReturnsExpectedText(AresDataType dataType, string expectedText)
  {
    var schema = new AresValueSchema { Type = dataType };

    Assert.That(schema.Stringify(), Is.EqualTo(expectedText));
  }

  [TestCase(AresDataType.IntArray)]
  [TestCase(AresDataType.FloatArray)]
  public void AddEntry_NewArrayTypes_AcceptsNumberChoices(AresDataType dataType)
  {
    var schema = new AresStructSchema();

    schema.AddEntry("values", dataType, optional: false, new[] { 1.0, 2.0 });

    Assert.That(schema.Fields["values"].Type, Is.EqualTo(dataType));
    Assert.That(schema.Fields["values"].NumberChoices.Numbers, Is.EqualTo(new[] { 1.0, 2.0 }));
  }
}
