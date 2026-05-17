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

  [TestCase(AresDataType.Timestamp, AresValue.KindOneofCase.TimestampValue)]
  [TestCase(AresDataType.Float, AresValue.KindOneofCase.FloatValue)]
  [TestCase(AresDataType.Int, AresValue.KindOneofCase.IntValue)]
  public void CreateDefault_NewScalarTypes_ReturnsExpectedKind(AresDataType dataType, AresValue.KindOneofCase expectedKind)
  {
    var value = AresValueHelper.CreateDefault(dataType);

    Assert.That(value.KindCase, Is.EqualTo(expectedKind));
  }

  [TestCase(AresDataType.Timestamp, AresValue.KindOneofCase.TimestampValue)]
  [TestCase(AresDataType.Float, AresValue.KindOneofCase.FloatValue)]
  [TestCase(AresDataType.Int, AresValue.KindOneofCase.IntValue)]
  public void GetAresDataType_NewScalarTypes_ReturnsExpectedType(AresDataType expectedType, AresValue.KindOneofCase expectedKind)
  {
    var value = AresValueHelper.CreateDefault(expectedType);

    Assert.That(value.KindCase, Is.EqualTo(expectedKind));
    Assert.That(value.GetAresDataType(), Is.EqualTo(expectedType));
  }

  [TestCase(AresDataType.Timestamp, "1970-01-01T00:00:00.0000000Z")]
  [TestCase(AresDataType.Float, "0")]
  [TestCase(AresDataType.Int, "0")]
  public void Stringify_NewScalarTypes_ReturnsExpectedText(AresDataType dataType, string expectedText)
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
  public void ToAresValueSchema_NewScalarTypes_ReturnsExpectedType(AresDataType dataType)
  {
    var value = AresValueHelper.CreateDefault(dataType);

    var schema = value.ToAresValueSchema();

    Assert.That(schema.Type, Is.EqualTo(dataType));
  }
}
