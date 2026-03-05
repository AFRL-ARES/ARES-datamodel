using Ares.Datamodel.Extensions;
using UnitsNet;
using NUnit.Framework;

namespace Ares.Datamodel.Tests;

public class QuantityValueUnitsNetExtensionsTests
{
  [Test]
  public void QuantityType_AllDefinedValuesExistInUnitsNet()
  {
    var unitsNetNames = Quantity.Infos
      .Select(info => info.Name)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var missing = Enum.GetValues<QuantityType>()
      .Where(type => type != QuantityType.Unspecified)
      .Where(type => !unitsNetNames.Contains(type.ToString()))
      .Select(type => type.ToString())
      .OrderBy(name => name)
      .ToArray();

    Assert.That(missing, Is.Empty, $"QuantityType values missing in UnitsNet: {string.Join(", ", missing)}");
  }

  [Test]
  public void ToQuantityValue_FromLength_MapsTypeAndUnit()
  {
    var quantity = Length.FromMeters(1.25);

    var result = quantity.ToQuantityValue();

    Assert.That(result.Type, Is.EqualTo(QuantityType.Length));
    Assert.That(result.Unit, Is.EqualTo("Meter"));
    Assert.That(result.Scalar, Is.EqualTo(1.25).Within(1e-8));
  }

  [Test]
  public void ToUnitsNetQuantity_FromLengthQuantityValue_ProducesLength()
  {
    var value = new QuantityValue
    {
      Scalar = 250,
      Type = QuantityType.Length,
      Unit = "Millimeter"
    };

    var quantity = value.ToUnitsNetQuantity();

    Assert.That(quantity, Is.TypeOf<Length>());
    var length = (Length)quantity;
    Assert.That(length.Meters, Is.EqualTo(0.25).Within(1e-8));
  }

  [Test]
  public void ToUnitsNetQuantity_FromElectricPotentialQuantityValue_UsesElectricPotential()
  {
    var value = new QuantityValue
    {
      Scalar = 12,
      Type = QuantityType.ElectricPotential,
      Unit = "Volt"
    };

    var quantity = value.ToUnitsNetQuantity();

    Assert.That(quantity.QuantityInfo.Name, Is.EqualTo("ElectricPotential"));
    Assert.That((double)quantity.Value, Is.EqualTo(12d).Within(1e-8));
  }

  [Test]
  public void ToUnitsNetQuantity_FromVolumeFlowQuantityValue_UsesVolumeFlow()
  {
    var value = new QuantityValue
    {
      Scalar = 10,
      Type = QuantityType.VolumeFlow,
      Unit = "LiterPerMinute"
    };

    var quantity = value.ToUnitsNetQuantity();

    Assert.That(quantity.QuantityInfo.Name, Is.EqualTo("VolumeFlow"));
    Assert.That((double)quantity.Value, Is.EqualTo(10d).Within(1e-8));
  }

  [Test]
  public void ToUnitsNetQuantity_FromHeatFluxQuantityValue_UsesHeatFlux()
  {
    var value = new QuantityValue
    {
      Scalar = 500,
      Type = QuantityType.HeatFlux,
      Unit = "WattPerSquareMeter"
    };

    var quantity = value.ToUnitsNetQuantity();

    Assert.That(quantity.QuantityInfo.Name, Is.EqualTo("HeatFlux"));
    Assert.That((double)quantity.Value, Is.EqualTo(500d).Within(1e-8));
  }

  [Test]
  public void TryToUnitsNetQuantity_WithUnspecifiedType_ReturnsFalse()
  {
    var value = new QuantityValue
    {
      Scalar = 5,
      Type = QuantityType.Unspecified,
      Unit = "Meter"
    };

    var ok = value.TryToUnitsNetQuantity(out var quantity);

    Assert.That(ok, Is.False);
    Assert.That(quantity, Is.Null);
  }

  [Test]
  public void CreateDefault_Quantity_ReturnsQuantityAresValue()
  {
    var value = AresValueHelper.CreateDefault(AresDataType.Quantity);

    Assert.That(value.KindCase, Is.EqualTo(AresValue.KindOneofCase.QuantityValue));
    Assert.That(value.QuantityValue.Type, Is.EqualTo(QuantityType.Unspecified));
  }
}
