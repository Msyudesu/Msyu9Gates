using Msyu9Gates.Data.Models;
using Msyu9Gates.Lib.Contracts;

namespace Msyu9Gates.Contracts;

public static class ChapterMappingExtensions
{
    public static ChapterDto ToDto(this Chapter model)
    {
        return new ChapterDto(
            Id: model.Id,
            GateId: model.GateId,
            ChapterNumber: model.ChapterNumber,
            DifficultyLevel: model.DifficultyLevel,
            IsCompleted: model.IsCompleted,
            IsLocked: model.IsLocked,
            DateUnlockedUtc: model.DateUnlockedUtc,
            DateCompletedUtc: model.DateCompletedUtc,
            Narrative: model.Narrative,
            RouteGuid: model.RouteGuid
        );
    }

    public static void ToModel(this Chapter chapterModel, ChapterDto dto)
    {
        chapterModel.GateId = dto.GateId;
        chapterModel.ChapterNumber = dto.ChapterNumber;
        chapterModel.DifficultyLevel = dto.DifficultyLevel;
        chapterModel.IsCompleted = dto.IsCompleted;
        chapterModel.IsLocked = dto.IsLocked;
        chapterModel.DateUnlockedUtc = dto.DateUnlockedUtc;
        chapterModel.DateCompletedUtc = dto.DateCompletedUtc;
        chapterModel.Narrative = dto.Narrative;
        chapterModel.RouteGuid = dto.RouteGuid;
    }
}
