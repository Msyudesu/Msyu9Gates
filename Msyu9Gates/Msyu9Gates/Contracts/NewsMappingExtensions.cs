using Msyu9Gates.Lib.Contracts;
using Msyu9Gates.Data.Models;

namespace Msyu9Gates.Contracts;

public static class NewsMappingExtensions
{
    public static NewsDto ToDto(this News newsModel) =>
        new NewsDto(
            newsModel.Id,
            newsModel.PublishedAtUtc,
            newsModel.Body,
            newsModel.Type
        );
    public static void ApplyFromDto(this News newsModel, NewsDto newsDto)
    {
        newsModel.PublishedAtUtc = newsDto.PublishedAtUtc;
        newsModel.Body = newsDto.Body;
        newsModel.Type = newsDto.Type;
    }
}
