using System.Text.Json.Serialization;
using Wishapp.Web.Common.Interfaces;

namespace Wishapp.Web.Admin.Features.Items.BatchImport;

public record BatchImportCatalogItemsCommand(
    List<string> Urls,
    Guid CategoryId) : ICommand<List<BatchImportItemResult>>;

public record BatchImportItemResult(
    string Url,
    BatchImportStatus Status,
    Guid? ItemId,
    List<string> MissingFields,
    string? ErrorMessage);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BatchImportStatus { Success, Partial, Failed }
