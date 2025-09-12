using Google.Protobuf.WellKnownTypes;

#pragma warning disable IDE0130
namespace Ares.Datamodel.Device;

public interface IDeviceState
{
  string DeviceId { get; set; }
  Timestamp Timestamp { get; set; }
}
