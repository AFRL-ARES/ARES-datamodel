from __future__ import annotations

from typing import Any

from ares_datamodel.ares_struct_pb2 import QuantityValue
from ares_datamodel.quantity_validation import validate_quantity_value
from pint import Quantity, UnitRegistry
from pint.facets.plain import PlainUnit

_UNIT_REGISTRY = UnitRegistry()


def get_unit_registry() -> UnitRegistry:
    return _UNIT_REGISTRY


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
    ureg = registry or _UNIT_REGISTRY
    converted = quantity.to(ureg.parse_units(unit)) if unit else quantity
    resolved_unit = _format_units_name(converted.units)
    return QuantityValue(
        scalar=_magnitude_to_float(converted.magnitude),
        unit=resolved_unit,
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
