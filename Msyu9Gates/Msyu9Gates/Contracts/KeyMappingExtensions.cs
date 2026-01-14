namespace Msyu9Gates.Contracts;

public static class KeyMappingExtensions
{
    public static KeyDto ToDto(this Lib.Models.GateKey keyModel) =>
        new KeyDto(
            keyModel.Id,
            keyModel.GateId,
            keyModel.ChapterId,
            keyModel.KeyNumber,
            keyModel.KeyValue,
            keyModel.Discovered,
            keyModel.DateDiscoveredUtc
        );
    public static void ApplyFromDto(this Lib.Models.GateKey keyModel, KeyDto keyDto)
    {
        keyModel.GateId = keyDto.GateId;
        keyModel.ChapterId = keyDto.ChapterId;
        keyModel.KeyNumber = keyDto.KeyNumber;
        keyModel.KeyValue = keyDto.KeyValue;
        keyModel.Discovered = keyDto.Discovered;
        keyModel.DateDiscoveredUtc = keyDto.DateDiscoveredUtc;
    }
}
