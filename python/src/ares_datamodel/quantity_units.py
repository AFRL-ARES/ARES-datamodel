from __future__ import annotations

from typing import Any

from ares_datamodel.ares_data_schema_pb2 import QuantitySchema
from ares_datamodel.ares_quantity_type_pb2 import QuantityType
from ares_datamodel.ares_struct_pb2 import QuantityValue
from ares_datamodel.quantity_validation import (
    QUANTITY_TYPE_TO_CANONICAL_UNIT,
    validate_quantity_value,
)
from pint import Quantity, UnitRegistry
from pint.facets.plain import PlainUnit

_UNIT_REGISTRY = UnitRegistry()


def get_unit_registry() -> UnitRegistry:
    return _UNIT_REGISTRY


def to_pint_unit(unit_str: str, registry: UnitRegistry | None = None) -> PlainUnit:
    """
    Convert a unit string to a Pint unit.
    """
    ureg = registry or _UNIT_REGISTRY
    return ureg.parse_units(unit_str)


def from_pint_unit(unit: PlainUnit) -> str:
    """
    Convert a Pint unit to a formatted unit string suitable for the ARES datamodel.
    """
    return _format_units_name(unit)


def infer_quantity_type(unit: PlainUnit | str, registry: UnitRegistry | None = None) -> QuantityType:
    """
    Infer the ARES QuantityType for a given unit based on its dimensionality.
    """
    ureg = registry or _UNIT_REGISTRY
    try:
        if isinstance(unit, str):
            unit = ureg.parse_units(unit)
    except Exception:
        return QuantityType.QUANTITY_TYPE_UNSPECIFIED

    target_dim = unit.dimensionality

    # Special case for temperature differences
    if "delta_degree_Celsius" in str(unit) or "delta_kelvin" in str(unit):
        return QuantityType.TEMPERATURE_DELTA

    for qtype, canonical_unit_str in QUANTITY_TYPE_TO_CANONICAL_UNIT.items():
        if qtype == QuantityType.QUANTITY_TYPE_UNSPECIFIED:
            continue
        try:
            canonical_unit = ureg.parse_units(canonical_unit_str)
            if canonical_unit.dimensionality == target_dim:
                return qtype
        except Exception:
            continue

    return QuantityType.QUANTITY_TYPE_UNSPECIFIED


def to_pint_quantity(quantity_value: QuantityValue, registry: UnitRegistry | None = None) -> Quantity:
    """
    Convert an ARES QuantityValue to a Pint quantity.
    """
    ureg = registry or _UNIT_REGISTRY
    quantity = quantity_value.scalar * ureg.parse_units(quantity_value.unit)
    validate_quantity_value(quantity_value, registry=ureg)
    return quantity


def from_pint_quantity(
    quantity: Quantity,
    unit: str | None = None,
    registry: UnitRegistry | None = None,
) -> QuantityValue:
    """
    Convert a Pint quantity to an ARES QuantityValue.
    """
    ureg = registry or _UNIT_REGISTRY
    converted = quantity.to(ureg.parse_units(unit)) if unit else quantity
    resolved_unit = _format_units_name(converted.units)
    return QuantityValue(
        scalar=_magnitude_to_float(converted.magnitude),
        unit=resolved_unit,
        type=infer_quantity_type(converted.units, registry=ureg),
    )


def to_quantity_schema(
    unit: PlainUnit | str,
    min_scalar_value: float | None = None,
    max_scalar_value: float | None = None,
    registry: UnitRegistry | None = None,
) -> QuantitySchema:
    """
    Create a QuantitySchema from a unit and optional bounds.
    """
    ureg = registry or _UNIT_REGISTRY
    if isinstance(unit, str):
        unit = ureg.parse_units(unit)

    return QuantitySchema(
        quantity_type=infer_quantity_type(unit, registry=ureg),
        bounds_unit=from_pint_unit(unit),
        min_scalar_value=min_scalar_value,
        max_scalar_value=max_scalar_value,
    )


def convert_quantity_value_unit(
    quantity_value: QuantityValue,
    target_unit: str,
    registry: UnitRegistry | None = None,
) -> QuantityValue:
    ureg = registry or _UNIT_REGISTRY
    source = to_pint_quantity(quantity_value, registry=ureg)
    return from_pint_quantity(source, unit=target_unit, registry=ureg)


def _format_units_name(unit: PlainUnit):
    unit_str = f"{unit:~}"
    unit_str = unit_str.replace("**", "^")
    unit_str = "".join(unit_str.split())
    return unit_str


def _magnitude_to_float(magnitude: Any) -> float:
    if isinstance(magnitude, complex):
        if magnitude.imag != 0:
            raise ValueError("Complex magnitude is not supported for QuantityValue scalar conversion.")
        return float(magnitude.real)
    return float(magnitude)
