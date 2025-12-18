using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Contracts
{
    public static class GateMappingExtensions
    {
        public static GateDto ToDto(this GateModel gateModel) =>
            new GateDto(
                gateModel.Id,
                gateModel.GateNumber,
                gateModel.GateOverallDifficultyLevel,
                gateModel.IsCompleted,
                gateModel.IsLocked,
                gateModel.DateUnlocked,
                gateModel.DateCompleted,
                gateModel.Narrative,
                gateModel.Conclusion
            );

        public static void ApplyFromDto(this GateModel gateModel, GateDto gateDto)
        {
            gateModel.GateNumber = gateDto.GateNumber;
            gateModel.GateOverallDifficultyLevel = gateDto.GateOverallDifficultyLevel;
            gateModel.IsCompleted = gateDto.IsCompleted;
            gateModel.IsLocked = gateDto.IsLocked;
            gateModel.DateUnlocked = gateDto.DateUnlocked;
            gateModel.DateCompleted = gateDto.DateCompleted;
            gateModel.Narrative = gateDto.Narrative;
            gateModel.Conclusion = gateDto.Conclusion;
        }
    }
}
