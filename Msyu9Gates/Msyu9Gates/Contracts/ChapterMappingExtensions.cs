using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Contracts;

public static class ChapterMappingExtensions
{
    public static ChapterDto ToDto(this ChapterModel model)
    {
        return new ChapterDto(
            Id: model.Id,
            GateId: model.GateId,
            ChapterNumber: model.ChapterNumber,
            DiffiutyLevel: model.DifficultyLevel,
            IsCompleted: model.IsCompleted,
            IsLocked: model.IsLocked,
            DateUnlockedUtc: model.DateUnlockedUtc,
            DateCompletedUtc: model.DateCompletedUtc,
            Narrative: model.Narrative,
            RouteGuid: model.RouteGuid
        );
    }

    public static void ToModel(this ChapterModel chapterModel, ChapterDto dto)
    {
        chapterModel.GateId = dto.GateId;
        chapterModel.ChapterNumber = dto.ChapterNumber;
        chapterModel.DifficultyLevel = dto.DiffiutyLevel;
        chapterModel.IsCompleted = dto.IsCompleted;
        chapterModel.IsLocked = dto.IsLocked;
        chapterModel.DateUnlockedUtc = dto.DateUnlockedUtc;
        chapterModel.DateCompletedUtc = dto.DateCompletedUtc;
        chapterModel.Narrative = dto.Narrative;
        chapterModel.RouteGuid = dto.RouteGuid;
    }
}
