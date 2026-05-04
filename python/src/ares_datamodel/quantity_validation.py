from __future__ import annotations

from ares_datamodel.ares_quantity_type_pb2 import QuantityType
from ares_datamodel.ares_struct_pb2 import QuantityValue
from pint import UnitRegistry

_UNIT_REGISTRY = UnitRegistry()

QUANTITY_TYPE_TO_CANONICAL_UNIT: dict[QuantityType, str] = {
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


def validate_quantity_value(
    quantity_value: QuantityValue,
    quantity_type: QuantityType | None = None,
    registry: UnitRegistry | None = None,
) -> None:
    ureg = registry or _UNIT_REGISTRY

    if not quantity_value.unit or not quantity_value.unit.strip():
        raise ValueError("Quantity unit must be specified.")

    source = float(quantity_value.scalar) * ureg.parse_units(quantity_value.unit)

    if quantity_type is None:
        return

    if quantity_type == QuantityType.QUANTITY_TYPE_UNSPECIFIED:
        raise ValueError("Quantity type must be specified.")

    expected_unit = QUANTITY_TYPE_TO_CANONICAL_UNIT.get(quantity_type)
    if expected_unit is None:
        quantity_type_name = QuantityType.Name(quantity_type)
        raise ValueError(f"No canonical Pint unit mapping for quantity type '{quantity_type_name}'.")

    target = 1.0 * ureg.parse_units(expected_unit)
    if source.dimensionality != target.dimensionality:
        quantity_type_name = QuantityType.Name(quantity_type)
        raise ValueError(
            f"Unit '{quantity_value.unit}' is incompatible with quantity type "
            f"'{quantity_type_name}' (expected dimensionality of '{expected_unit}')."
        )


def validate_quantity_type_match(
    quantity_value: QuantityValue,
    quantity_type: QuantityType,
    registry: UnitRegistry | None = None,
) -> None:
    validate_quantity_value(quantity_value, registry=registry, quantity_type=quantity_type)
