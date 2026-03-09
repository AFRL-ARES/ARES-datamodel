from ares_datamodel.ares_struct_pb2 import QuantityValue
import pytest

from ares_datamodel.quantity_units import from_pint_quantity
from ares_datamodel.quantity_units import to_pint_quantity

from pint import UnitRegistry
from ares_datamodel.ares_quantity_type_pb2 import QuantityType


def test_to_pint_quantity_length():
    value = QuantityValue(scalar=250.0, type=QuantityType.LENGTH, unit="millimeter")

    q = to_pint_quantity(value)

    assert q.to("meter").magnitude == pytest.approx(0.25, rel=0, abs=1e-8)


def test_to_pint_quantity_electric_potential():
    value = QuantityValue(scalar=12.0, type=QuantityType.ELECTRIC_POTENTIAL, unit="volt")

    q = to_pint_quantity(value)

    assert q.to("volt").magnitude == pytest.approx(12.0, rel=0, abs=1e-8)


def test_to_pint_quantity_heat_flux():
    value = QuantityValue(scalar=500.0, type=QuantityType.HEAT_FLUX, unit="watt / meter ** 2")

    q = to_pint_quantity(value)

    assert q.to("watt / meter ** 2").magnitude == pytest.approx(500.0, rel=0, abs=1e-8)


def test_to_pint_quantity_rejects_dimension_mismatch():
    value = QuantityValue(scalar=1.0, type=QuantityType.LENGTH, unit="second")

    with pytest.raises(ValueError):
        to_pint_quantity(value)


def test_from_pint_quantity_length_to_mm():
    source = to_pint_quantity(QuantityValue(scalar=0.25, type=QuantityType.LENGTH, unit="meter"))

    converted = from_pint_quantity(source, quantity_type=QuantityType.LENGTH, unit="mm")

    assert converted.unit == "mm"
    assert converted.scalar == pytest.approx(250.0, rel=0, abs=1e-8)
    assert converted.type == QuantityType.LENGTH


def test_to_and_from_quantity_area():
    value = QuantityValue(scalar=200, type=QuantityType.AREA, unit="m^2")
    ureg = UnitRegistry()

    converted = to_pint_quantity(value)
    test = converted.to(ureg.kilometer ** 2)
    quant_val = from_pint_quantity(test, quantity_type=QuantityType.AREA)

    assert quant_val.unit == "km^2"


def test_to_and_from_quantity_acceleration():
    value = QuantityValue(scalar=9.81, type=QuantityType.ACCELERATION, unit="m/s^2")
    ureg = UnitRegistry()

    converted = to_pint_quantity(value)
    test = converted.to(ureg.kilometer / (ureg.second ** 2))
    quant_val = from_pint_quantity(test, quantity_type=QuantityType.ACCELERATION)

    assert quant_val.unit in ("km/s^2", "km/s²")
