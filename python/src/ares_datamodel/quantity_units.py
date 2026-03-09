from __future__ import annotations

from typing import Any

from ares_datamodel.ares_quantity_type_pb2 import QuantityType
from ares_datamodel.ares_struct_pb2 import QuantityValue
from pint import Quantity, UnitRegistry
from pint.facets.plain import PlainQuantity, PlainUnit

_UNIT_REGISTRY = UnitRegistry()

_QUANTITY_TYPE_TO_CANONICAL_UNIT: dict[QuantityType, str] = {
    QuantityType.LENGTH: "meter",
    QuantityType.ACCELERATION: "meter/second**2",
    QuantityType.AREA: "meter**2",
    QuantityType.VOLUME: "meter**3",
    QuantityType.DURATION: "second",
    QuantityType.SPEED: "meter/second",
    QuantityType.FREQUENCY: "hertz",
    QuantityType.MASS: "kilogram",
    QuantityType.AMOUNT_OF_SUBSTANCE: "mole",
    QuantityType.MOLAR_MASS: "kilogram/mole",
    QuantityType.TEMPERATURE: "kelvin",
    QuantityType.TEMPERATURE_DELTA: "delta_degC",
    QuantityType.PRESSURE: "pascal",
    QuantityType.MOLARITY: "mole/liter",
    QuantityType.MASS_CONCENTRATION: "kilogram/meter**3",
    QuantityType.VOLUME_CONCENTRATION: "dimensionless",
    QuantityType.MASS_FRACTION: "dimensionless",
    QuantityType.DENSITY: "kilogram/meter**3",
    QuantityType.DYNAMIC_VISCOSITY: "pascal*second",
    QuantityType.KINEMATIC_VISCOSITY: "meter**2/second",
    QuantityType.VOLUME_FLOW: "meter**3/second",
    QuantityType.MASS_FLOW: "kilogram/second",
    QuantityType.MASS_FLUX: "kilogram/(meter**2*second)",
    QuantityType.ROTATIONAL_ACCELERATION: "radian/second**2",
    QuantityType.ROTATIONAL_SPEED: "radian/second",
    QuantityType.ENERGY: "joule",
    QuantityType.POWER: "watt",
    QuantityType.HEAT_FLUX: "watt/meter**2",
    QuantityType.SPECIFIC_ENERGY: "joule/kilogram",
    QuantityType.THERMAL_CONDUCTIVITY: "watt/(meter*kelvin)",
    QuantityType.FORCE: "newton",
    QuantityType.FORCE_PER_LENGTH: "newton/meter",
    QuantityType.RATIO: "dimensionless",
    QuantityType.IRRADIANCE: "watt/meter**2",
    QuantityType.ILLUMINANCE: "lux",
    QuantityType.TEMPERATURE_GRADIENT: "kelvin/meter",
    QuantityType.ELECTRIC_POTENTIAL: "volt",
    QuantityType.ELECTRIC_CURRENT: "ampere",
    QuantityType.ELECTRIC_RESISTANCE: "ohm",
    QuantityType.ELECTRIC_CONDUCTIVITY: "siemens/meter",
    QuantityType.ELECTRIC_CONDUCTANCE: "siemens",
    QuantityType.RATIO_CHANGE_RATE: "1/second",
    QuantityType.RELATIVE_HUMIDITY: "dimensionless",
}


def get_unit_registry() -> UnitRegistry:
    return _UNIT_REGISTRY


def to_pint_quantity(quantity_value: QuantityValue, registry: UnitRegistry | None = None) -> Quantity:
    """
    Convert an ARES QuantityValue to a Pint quantity.
    """
    ureg = registry or _UNIT_REGISTRY
    quantity = quantity_value.scalar * ureg.parse_units(quantity_value.unit)
    _validate_quantity_value(quantity_value, registry=ureg)
    return quantity


def validate_quantity_value(quantity_value: QuantityValue, registry: UnitRegistry | None = None) -> None:
    _validate_quantity_value(quantity_value, registry=registry)


def from_pint_quantity(
    quantity: Quantity,
    quantity_type: QuantityType | None = None,
    unit: str | None = None,
    registry: UnitRegistry | None = None,
) -> QuantityValue:
    ureg = registry or _UNIT_REGISTRY
    if quantity_type is None:
        root = quantity.to_root_units()
        inferred = _lookup_unit_type(root)
        if inferred is None:
            raise ValueError("Could not infer QuantityType from Pint quantity units. Pass quantity_type explicitly.")
        quantity_type = inferred

    if quantity_type == QuantityType.QUANTITY_TYPE_UNSPECIFIED:
        raise ValueError("Quantity type must be specified.")
    
    quantity_type_name = QuantityType.Name(quantity_type)

    expected_pint_unit = _QUANTITY_TYPE_TO_CANONICAL_UNIT.get(quantity_type)
    if expected_pint_unit is None:
        raise ValueError(f"No canonical Pint unit mapping for quantity type '{quantity_type_name}'.")

    # dummy value just to get dimension
    target_dimension = (1.0 * ureg.parse_units(expected_pint_unit)).dimensionality
    if quantity.dimensionality != target_dimension:
        raise ValueError(
            f"Pint quantity dimensionality is incompatible with quantity type "
            f"'{quantity_type_name}' (expected dimensionality of '{expected_pint_unit}')."
        )

    converted = quantity.to(ureg.parse_units(unit)) if unit else quantity
    resolved_unit = _format_units_name(converted.units)
    return QuantityValue(
        scalar=_magnitude_to_float(converted.magnitude),
        type=quantity_type,
        unit=resolved_unit,
    )


def _lookup_unit_type(quantity: PlainQuantity) -> QuantityType | None:
    units = "".join(str(quantity.units).split()) # Fancy way to remove spaces
    for (key, value) in _QUANTITY_TYPE_TO_CANONICAL_UNIT.items():
        if units == value:
            return key
    
    return None


def convert_quantity_value_unit(
    quantity_value: QuantityValue,
    target_unit: str,
    registry: UnitRegistry | None = None,
) -> QuantityValue:
    ureg = registry or _UNIT_REGISTRY
    source = to_pint_quantity(quantity_value, registry=ureg)
    return from_pint_quantity(source, quantity_type=quantity_value.type, unit=target_unit, registry=ureg)


def _format_units_name(unit: PlainUnit):
    unit_str = f"{unit:~}"
    unit_str = unit_str.replace("**", "^")
    unit_str = "".join(unit_str.split())
    return unit_str


def _validate_quantity_value(quantity_value: QuantityValue, registry: UnitRegistry | None = None) -> None:
    """
    Validate that a QuantityValue object's unit dimension matches the expected dimension as would
    be provided by Pint. Like PlainUnit can be of time length/time/etc and we just got to make sure
    that the unit provided by QuantityValue is the same type.
    """
    ureg = registry or _UNIT_REGISTRY
    if quantity_value.type == QuantityType.QUANTITY_TYPE_UNSPECIFIED:
        raise ValueError("Quantity type must be specified.")
    
    quantity_type_name = QuantityType.Name(quantity_value.type)

    expected_unit = _QUANTITY_TYPE_TO_CANONICAL_UNIT.get(quantity_value.type)
    if expected_unit is None:
        raise ValueError(f"No canonical Pint unit mapping for quantity type '{quantity_type_name}'.")

    source = float(quantity_value.scalar) * ureg.parse_units(quantity_value.unit)
    target = 1.0 * ureg.parse_units(expected_unit)
    if source.dimensionality != target.dimensionality:
        raise ValueError(
            f"Unit '{quantity_value.unit}' is incompatible with quantity type "
            f"'{quantity_type_name}' (expected dimensionality of '{expected_unit}')."
        )


def _magnitude_to_float(magnitude: Any) -> float:
    if isinstance(magnitude, complex):
        if magnitude.imag != 0:
            raise ValueError("Complex magnitude is not supported for QuantityValue scalar conversion.")
        return float(magnitude.real)
    return float(magnitude)
