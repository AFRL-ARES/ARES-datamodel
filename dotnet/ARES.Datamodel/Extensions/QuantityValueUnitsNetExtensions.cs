using System.Globalization;
using UnitsNet;

namespace Ares.Datamodel.Extensions;

public static class QuantityValueUnitsNetExtensions
{
  public static bool TryToUnitsNetQuantity(this QuantityValue value, out IQuantity? quantity)
  {
    quantity = null;

    if(value.Type == QuantityType.Unspecified || string.IsNullOrWhiteSpace(value.Unit))
      return false;

    var unitsNetQuantityName = value.Type.ToUnitsNetQuantityName();
    var quantityInfo = Quantity.Infos.FirstOrDefault(info => info.Name.Equals(unitsNetQuantityName, StringComparison.OrdinalIgnoreCase));
    if(quantityInfo is null)
      return false;

    var enumUnit = quantityInfo.UnitInfos
      .Select(unitInfo => unitInfo.Value)
      .OfType<Enum>()
      .FirstOrDefault(u => u.ToString().Equals(value.Unit, StringComparison.OrdinalIgnoreCase));

    if(enumUnit is not null)
    {
      quantity = Quantity.From(value.Scalar, enumUnit);
      return true;
    }

    if(!UnitParser.Default.TryParse(value.Unit, quantityInfo.UnitType, out Enum? unitEnum) || unitEnum is null)
      return false;

    quantity = Quantity.From(value.Scalar, unitEnum);
    return true;
  }

  public static IQuantity ToUnitsNetQuantity(this QuantityValue value)
  {
    if(value.TryToUnitsNetQuantity(out var quantity) && quantity is not null)
      return quantity;

    throw new InvalidOperationException($"Failed to convert QuantityValue(type={value.Type}, scalar={value.Scalar.ToString(CultureInfo.InvariantCulture)}, unit='{value.Unit}') to a UnitsNet quantity.");
  }

  public static QuantityValue ToQuantityValue(this IQuantity quantity, QuantityType? quantityTypeOverride = null)
  {
    ArgumentNullException.ThrowIfNull(quantity);

    var resolvedType = quantityTypeOverride ?? quantity.ToQuantityType();
    return new QuantityValue
    {
      Scalar = (double)quantity.Value,
      Type = resolvedType,
      Unit = quantity.Unit.ToString()
    };
  }

  public static QuantityType ToQuantityType(this IQuantity quantity)
  {
    ArgumentNullException.ThrowIfNull(quantity);

    if(Enum.TryParse<QuantityType>(quantity.QuantityInfo.Name, true, out var parsed))
      return parsed;

    throw new InvalidOperationException($"No QuantityType mapping exists for UnitsNet quantity '{quantity.QuantityInfo.Name}'.");
  }

  public static string ToUnitsNetQuantityName(this QuantityType quantityType)
  {
    if(quantityType == QuantityType.Unspecified)
      throw new InvalidOperationException("Cannot map QuantityType.Unspecified to a UnitsNet quantity.");

    var name = quantityType.ToString();
    if(Quantity.Infos.Any(info => info.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
      return name;

    throw new InvalidOperationException($"No UnitsNet quantity mapping exists for QuantityType '{quantityType}'.");
  }
}
