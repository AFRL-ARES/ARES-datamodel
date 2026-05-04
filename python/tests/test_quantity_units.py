from ares_datamodel.ares_struct_pb2 import QuantityValue
from ares_datamodel.ares_quantity_type_pb2 import QuantityType
import pytest

from ares_datamodel.quantity_units import (
    from_pint_quantity,
    to_pint_quantity,
    to_pint_unit,
    from_pint_unit,
    infer_quantity_type,
    to_quantity_schema,
)
from ares_datamodel.quantity_validation import validate_quantity_type_match
from ares_datamodel.quantity_validation import validate_quantity_value

from pint import UnitRegistry


def test_to_pint_quantity_length():
    value = QuantityValue(scalar=250.0, unit="millimeter")

    q = to_pint_quantity(value)

    assert q.to("meter").magnitude == pytest.approx(0.25, rel=0, abs=1e-8)


def test_to_pint_quantity_electric_potential():
    value = QuantityValue(scalar=12.0, unit="volt")

    q = to_pint_quantity(value)

    assert q.to("volt").magnitude == pytest.approx(12.0, rel=0, abs=1e-8)


def test_to_pint_quantity_heat_flux():
    value = QuantityValue(scalar=500.0, unit="watt / meter ** 2")

    q = to_pint_quantity(value)

    assert q.to("watt / meter ** 2").magnitude == pytest.approx(500.0, rel=0, abs=1e-8)


def test_to_pint_quantity_rejects_missing_unit():
    value = QuantityValue(scalar=1.0, unit="")

    with pytest.raises(ValueError):
        to_pint_quantity(value)


def test_from_pint_quantity_length_to_mm():
    source = to_pint_quantity(QuantityValue(scalar=0.25, unit="meter"))

    converted = from_pint_quantity(source, unit="mm")

    assert converted.unit == "mm"
    assert converted.scalar == pytest.approx(250.0, rel=0, abs=1e-8)
    assert converted.type == QuantityType.LENGTH


def test_to_and_from_quantity_area():
    value = QuantityValue(scalar=200, unit="m^2")
    ureg = UnitRegistry()

    converted = to_pint_quantity(value)
    test = converted.to(ureg.kilometer ** 2)
    quant_val = from_pint_quantity(test)

    assert quant_val.unit == "km^2"
    assert quant_val.type == QuantityType.AREA


def test_to_and_from_quantity_acceleration():
    value = QuantityValue(scalar=9.81, unit="m/s^2")
    ureg = UnitRegistry()

    converted = to_pint_quantity(value)
    test = converted.to(ureg.kilometer / (ureg.second ** 2))
    quant_val = from_pint_quantity(test)

    assert quant_val.unit in ("km/s^2", "km/s\u00b2")
    assert quant_val.type == QuantityType.ACCELERATION


def test_validate_quantity_value_opt_in_type_match_passes():
    value = QuantityValue(scalar=1.0, unit="m/s^2")

    validate_quantity_value(value, quantity_type=QuantityType.ACCELERATION)


def test_validate_quantity_value_opt_in_type_match_rejects_mismatch():
    value = QuantityValue(scalar=1.0, unit="second")

    with pytest.raises(ValueError):
        validate_quantity_value(value, quantity_type=QuantityType.LENGTH)


def test_validate_quantity_type_match_helper():
    value = QuantityValue(scalar=5.0, unit="volt")

    validate_quantity_type_match(value, QuantityType.ELECTRIC_POTENTIAL)


def test_to_pint_unit():
    unit = to_pint_unit("meter / second^2")
    assert str(unit.dimensionality) == "[length] / [time] ** 2"


def test_from_pint_unit():
    ureg = UnitRegistry()
    unit = ureg.parse_units("kilogram * meter / second^2")
    unit_str = from_pint_unit(unit)
    assert unit_str == "kg*m/s^2"


def test_infer_quantity_type():
    assert infer_quantity_type("meter") == QuantityType.LENGTH
    assert infer_quantity_type("m/s^2") == QuantityType.ACCELERATION
    assert infer_quantity_type("degC") == QuantityType.TEMPERATURE
    assert infer_quantity_type("delta_degC") == QuantityType.TEMPERATURE_DELTA
    assert infer_quantity_type("unknown_unit") == QuantityType.QUANTITY_TYPE_UNSPECIFIED


def test_to_quantity_schema():
    schema = to_quantity_schema("mm", min_scalar_value=0.0, max_scalar_value=100.0)
    assert schema.quantity_type == QuantityType.LENGTH
    assert schema.bounds_unit == "mm"
    assert schema.min_scalar_value == 0.0
    assert schema.max_scalar_value == 100.0

