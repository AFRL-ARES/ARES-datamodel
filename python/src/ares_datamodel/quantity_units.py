from __future__ import annotations

from dataclasses import dataclass
from typing import Any
from typing import Protocol
from typing import TypeAlias

from pint import Quantity, UnitRegistry
from pint.facets.plain import PlainQuantity, PlainUnit

# Protobuf enum values are represented as ints at runtime.
# Accepting str supports fallback/test paths that use enum names.
QuantityTypeInput: TypeAlias = int | str

_UNIT_REGISTRY = UnitRegistry()


class QuantityValueLike(Protocol):
    scalar: float
    type: Any
    unit: str


@dataclass
class _QuantityValueRecord:
    scalar: float
    type: Any
    unit: str

_QUANTITY_TYPE_TO_CANONICAL_UNIT: dict[str, str] = {
    "LENGTH": "meter",
    "ACCELERATION": "meter/second**2",
    "AREA": "meter**2",
    "VOLUME": "meter**3",
    "DURATION": "second",
    "SPEED": "meter/second",
    "FREQUENCY": "hertz",
    "MASS": "kilogram",
    "AMOUNT_OF_SUBSTANCE": "mole",
    "MOLAR_MASS": "kilogram/mole",
    "TEMPERATURE": "kelvin",
    "TEMPERATURE_DELTA": "delta_degC",
    "PRESSURE": "pascal",
    "MOLARITY": "mole/liter",
    "MASS_CONCENTRATION": "kilogram/meter**3",
    "VOLUME_CONCENTRATION": "dimensionless",
    "MASS_FRACTION": "dimensionless",
    "DENSITY": "kilogram/meter**3",
    "DYNAMIC_VISCOSITY": "pascal*second",
    "KINEMATIC_VISCOSITY": "meter**2/second",
    "VOLUME_FLOW": "meter**3/second",
    "MASS_FLOW": "kilogram/second",
    "MASS_FLUX": "kilogram/(meter**2*second)",
    "ROTATIONAL_ACCELERATION": "radian/second**2",
    "ROTATIONAL_SPEED": "radian/second",
    "ENERGY": "joule",
    "POWER": "watt",
    "HEAT_FLUX": "watt/meter**2",
    "SPECIFIC_ENERGY": "joule/kilogram",
    "THERMAL_CONDUCTIVITY": "watt/(meter*kelvin)",
    "FORCE": "newton",
    "FORCE_PER_LENGTH": "newton/meter",
    "RATIO": "dimensionless",
    "IRRADIANCE": "watt/meter**2",
    "ILLUMINANCE": "lux",
    "TEMPERATURE_GRADIENT": "kelvin/meter",
    "ELECTRIC_POTENTIAL": "volt",
    "ELECTRIC_CURRENT": "ampere",
    "ELECTRIC_RESISTANCE": "ohm",
    "ELECTRIC_CONDUCTIVITY": "siemens/meter",
    "ELECTRIC_CONDUCTANCE": "siemens",
    "RATIO_CHANGE_RATE": "1/second",
    "RELATIVE_HUMIDITY": "dimensionless",
}


def get_unit_registry() -> UnitRegistry:
    return _UNIT_REGISTRY


def to_pint_quantity(quantity_value: QuantityValueLike, registry: UnitRegistry | None = None) -> Quantity:
    """
    Convert an ARES QuantityValue-like object to a Pint quantity.

    The object must expose: scalar, unit, type.
    """
    ureg = registry or _UNIT_REGISTRY
    quantity = quantity_value.scalar * ureg.parse_units(quantity_value.unit)
    _validate_quantity_value(quantity_value, registry=ureg)
    return quantity


def validate_quantity_value(quantity_value: QuantityValueLike, registry: UnitRegistry | None = None) -> None:
    _validate_quantity_value(quantity_value, registry=registry)


def from_pint_quantity(
    quantity: Quantity,
    quantity_type: QuantityTypeInput | None = None,
    unit: str | None = None,
    registry: UnitRegistry | None = None,
) -> QuantityValueLike:
    ureg = registry or _UNIT_REGISTRY
    if quantity_type is None:
        root = quantity.to_root_units()
        inferred = _lookup_unit_type(root)
        if inferred is None:
            raise ValueError("Could not infer QuantityType from Pint quantity units. Pass quantity_type explicitly.")
        quantity_type = inferred

    quantity_type_name = _quantity_type_name(quantity_type)
    if quantity_type_name == "QUANTITY_TYPE_UNSPECIFIED":
        raise ValueError("Quantity type must be specified.")

    expected_unit = _QUANTITY_TYPE_TO_CANONICAL_UNIT.get(quantity_type_name)
    if expected_unit is None:
        raise ValueError(f"No canonical Pint unit mapping for quantity type '{quantity_type_name}'.")

    target_dimension = (1.0 * ureg.parse_units(expected_unit)).dimensionality
    if quantity.dimensionality != target_dimension:
        raise ValueError(
            f"Pint quantity dimensionality is incompatible with quantity type "
            f"'{quantity_type_name}' (expected dimensionality of '{expected_unit}')."
        )

    converted = quantity.to(ureg.parse_units(unit)) if unit else quantity
    resolved_unit = _format_units_name(converted.units)
    return _new_quantity_value(
        scalar=_magnitude_to_float(converted.magnitude),
        quantity_type=quantity_type,
        unit=resolved_unit,
    )


def _lookup_unit_type(quantity: PlainQuantity) -> str | None:
    units = "".join(str(quantity.units).split())
    for (key, value) in _QUANTITY_TYPE_TO_CANONICAL_UNIT.items():
        if units == value:
            return key
    
    return None


def convert_quantity_value_unit(
    quantity_value: QuantityValueLike,
    target_unit: str,
    registry: UnitRegistry | None = None,
) -> QuantityValueLike:
    ureg = registry or _UNIT_REGISTRY
    source = to_pint_quantity(quantity_value, registry=ureg)
    return from_pint_quantity(source, quantity_type=quantity_value.type, unit=target_unit, registry=ureg)


def _new_quantity_value(*, scalar: float, quantity_type: Any, unit: str) -> QuantityValueLike:
    try:
        from . import ares_struct_pb2
    except ImportError:
        ares_struct_pb2 = None

    if ares_struct_pb2 is not None:
        return ares_struct_pb2.QuantityValue(
            scalar=scalar,
            type=quantity_type,
            unit=unit,
        )

    return _QuantityValueRecord(
        scalar=scalar,
        type=quantity_type,
        unit=unit,
    )


def _format_units_name(unit: PlainUnit):
    unit_str = f"{unit:~}"
    unit_str = unit_str.replace("**", "^")
    unit_str = "".join(unit_str.split())
    return unit_str


def _validate_quantity_value(quantity_value: QuantityValueLike, registry: UnitRegistry | None = None) -> None:
    """
    Validate that a QuantityValue-like object's unit matches its QuantityType dimension.
    """
    ureg = registry or _UNIT_REGISTRY
    quantity_type_name = _quantity_type_name(quantity_value.type)
    if quantity_type_name == "QUANTITY_TYPE_UNSPECIFIED":
        raise ValueError("Quantity type must be specified.")

    expected_unit = _QUANTITY_TYPE_TO_CANONICAL_UNIT.get(quantity_type_name)
    if expected_unit is None:
        raise ValueError(f"No canonical Pint unit mapping for quantity type '{quantity_type_name}'.")

    source = float(quantity_value.scalar) * ureg.parse_units(quantity_value.unit)
    target = 1.0 * ureg.parse_units(expected_unit)
    if source.dimensionality != target.dimensionality:
        raise ValueError(
            f"Unit '{quantity_value.unit}' is incompatible with quantity type "
            f"'{quantity_type_name}' (expected dimensionality of '{expected_unit}')."
        )


def _quantity_type_name(quantity_type_value: Any) -> str:
    try:
        from . import ares_quantity_type_pb2

        return ares_quantity_type_pb2.QuantityType.Name(int(quantity_type_value))
    except Exception:
        if isinstance(quantity_type_value, str):
            return quantity_type_value
        return str(quantity_type_value)


def _magnitude_to_float(magnitude: Any) -> float:
    if isinstance(magnitude, complex):
        if magnitude.imag != 0:
            raise ValueError("Complex magnitude is not supported for QuantityValue scalar conversion.")
        return float(magnitude.real)
    return float(magnitude)
